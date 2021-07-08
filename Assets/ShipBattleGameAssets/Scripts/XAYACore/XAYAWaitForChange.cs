using UnityEngine;
using CielaSpike;
using System.Collections;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Net;
using System;
using UnityEngine.UI;
using System.Diagnostics;

namespace XAYA
{
    public interface IXAYAWaitForChange
    {
        /*Wait for pending transaction to pass into memory pool*/
        void OnWaitForChangeNewBlock();

        void OnWaitForChangeTID(string latestPendingData);

        bool SerializedPendingIsDifferent(string latestPendingData);

        void OnWaitForChangeGameChannel();
    }

    public interface IXAYAWaitBase
    { }


    public class XAYAWaitForChange : MonoBehaviour
    {
        string lastWaitForChangeResult = "";
        int lastWaitForPendingChangeResult = -1;
        int lastWaitForChangeResultChannels = -1;
        int lastBlockHeight;
        string lastPendingData = "";
        bool hasPendingData = false;

        [HideInInspector]
        public List<MonoBehaviour> objectsRegisteredForWaitForChange;
        [HideInInspector]
        public List<IXAYAWaitForChange> objectsRegisteredForWaitForChangeIntefaceOnly = new List<IXAYAWaitForChange>();

        public static XAYAWaitForChange Instance;

        [HideInInspector]
        public bool readyToAcceptPendingCalls = false;

        void Start()
        {
            Instance = this;
        }

        // Use this for initialization
        public void StartRunning(bool forGame, bool forPending, bool forChannels)
        {
            if (forPending)
            {
                Task task;
                this.StartCoroutineAsync(WaitForChangePendings(), out task);
            }

            if (forGame)
            {
                Task task2;
                this.StartCoroutineAsync(WaitForChange(), out task2);
            }

            if(forChannels)
            {
                Task task3;
                this.StartCoroutineAsync(WaitForChangeChannels(), out task3);
            }
        }

        IEnumerator WaitForChangeChannels()
        {
            while (true)
            {
                yield return Ninja.JumpBack;
                RPCRequest r = new RPCRequest();
                string reply = r.XAYAWaitForChangeGameChannels(lastWaitForChangeResultChannels);

                if (reply != "")
                {
                    JObject result = JObject.Parse(reply);
                    string versionString = result["result"]["version"].ToString();

                    int oldVersion = 0;
                    int.TryParse(versionString, out oldVersion);

                    yield return Ninja.JumpToUnity;

                    if (lastWaitForChangeResultChannels != oldVersion)
                    {
                        lastWaitForChangeResultChannels = oldVersion;

                        for (int s = 0; s < objectsRegisteredForWaitForChange.Count; s++)
                        {
                            IXAYAWaitForChange wfc = (IXAYAWaitForChange)objectsRegisteredForWaitForChange[s].GetComponent(typeof(IXAYAWaitForChange));
                            if (wfc != null)
                            {
                                if (objectsRegisteredForWaitForChange[s].gameObject != null && objectsRegisteredForWaitForChange[s].gameObject.activeInHierarchy)
                                {
                                    wfc.OnWaitForChangeGameChannel();
                                }
                            }
                        }
                    }
                }
            }
        }

        IEnumerator WaitForChange()
        {
            while (true)
            {
                RPCRequest r = new RPCRequest();
                string waitForChange = r.XAYAWaitForChange(lastWaitForChangeResult);
                string knownHash = "";

                if (waitForChange != "")
                {
                    JObject resultJobjectWF = JObject.Parse(waitForChange);
                    if (resultJobjectWF["result"] != null)
                    {
                        knownHash = resultJobjectWF["result"].ToString();
                    }
                }

                yield return Ninja.JumpToUnity;
                yield return new WaitForSeconds(0.1f);

                if (lastWaitForChangeResult != knownHash)
                {
                    lastWaitForChangeResult = knownHash;

                    try
                    {
                        string result = r.XAYAGetBlockCount();

                        if (result.Contains("result"))
                        {
                            JObject resultJobject = JObject.Parse(result);

                            if (resultJobject["result"] != null)
                            {
                                string reply = "";
                                if (XAYASettings.isElectrum() == false)
                                {
                                    reply = resultJobject["result"].ToString();
                                }

                                if (XAYASettings.gspHeightFetched == false)
                                {
                                    int blockCount = 0;
                                    RPCRequest r2 = new RPCRequest();
                                    r2.WalletIsConnected(out blockCount);
                                    lastBlockHeight = blockCount;
                                }
                                else
                                {
                                }

                                for (int s = 0; s < objectsRegisteredForWaitForChange.Count; s++)
                                {
                                    IXAYAWaitForChange wfc = (IXAYAWaitForChange)objectsRegisteredForWaitForChange[s].GetComponent(typeof(IXAYAWaitForChange));
                                    if (wfc != null)
                                    {
                                        if (objectsRegisteredForWaitForChange[s].gameObject != null && objectsRegisteredForWaitForChange[s].gameObject.activeInHierarchy)
                                        {
                                            wfc.OnWaitForChangeNewBlock();
                                        }
                                    }
                                }

                                for (int s = 0; s < objectsRegisteredForWaitForChangeIntefaceOnly.Count; s++)
                                {
                                    IXAYAWaitForChange wfc = (IXAYAWaitForChange)objectsRegisteredForWaitForChangeIntefaceOnly[s];
                                    if (wfc != null)
                                    {
                                        wfc.OnWaitForChangeNewBlock();
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                    }
                }
                else
                {
                }

                yield return new WaitForSeconds(0.1f);
                yield return Ninja.JumpBack;
            }
        }

        public bool HasPendingData()
        {
            return hasPendingData;
        }

        public string GetLastPendingData()
        {
            return lastPendingData;
        }

        IEnumerator WaitForChangePendings()
        {
            while (readyToAcceptPendingCalls == false)
            {
                yield return new WaitForSeconds(0.1f);
            }

            while (true)
            {
                yield return Ninja.JumpBack;
                RPCRequest r = new RPCRequest();
                string reply = r.XAYAWaitForChangePending(lastWaitForPendingChangeResult);

                if (reply != "")
                {
                    JObject result = JObject.Parse(reply);
                    JToken tokenRS = result["result"];
                    if (tokenRS.HasValues)
                    {
                        string versionString = result["result"]["version"].ToString();

                        int oldVersion = 0;
                        int.TryParse(versionString, out oldVersion);

                        yield return Ninja.JumpToUnity;

                        if (lastWaitForPendingChangeResult != oldVersion)
                        {
                            lastWaitForPendingChangeResult = oldVersion;
                            for (int s = 0; s < objectsRegisteredForWaitForChange.Count; s++)
                            {
                                if (objectsRegisteredForWaitForChange[s] == null)
                                {
                                    objectsRegisteredForWaitForChange.RemoveAt(s);
                                    s = -1;
                                }
                            }

                            string pendingData = result["result"]["pending"].ToString();
                            if (this.PendingDataEquals(pendingData) == false)
                            {
                                for (int s = 0; s < objectsRegisteredForWaitForChange.Count; s++)
                                {
                                    IXAYAWaitForChange wfc = (IXAYAWaitForChange)objectsRegisteredForWaitForChange[s].GetComponent(typeof(IXAYAWaitForChange));
                                    if (wfc != null)
                                    {

                                            if (objectsRegisteredForWaitForChange[s].gameObject != null && objectsRegisteredForWaitForChange[s].gameObject.activeInHierarchy)
                                            {
                                                wfc.OnWaitForChangeTID(pendingData);
                                            }
                                            else if (objectsRegisteredForWaitForChange[s].gameObject == null)
                                            {
                                                wfc.OnWaitForChangeTID(pendingData);
                                            }
                                    }
                                }

                                for (int s = 0; s < objectsRegisteredForWaitForChangeIntefaceOnly.Count; s++)
                                {
                                    IXAYAWaitForChange wfc2 = (IXAYAWaitForChange)objectsRegisteredForWaitForChangeIntefaceOnly[s];
                                    if (wfc2 != null)
                                    {
                                        wfc2.OnWaitForChangeTID(pendingData);
                                    }
                                }

                                hasPendingData = true;
                                lastPendingData = pendingData;
                            }
                        }
                    }
                }
                else
                {
                    hasPendingData = false;
                }
            }
        }

        private bool PendingDataEquals(string currentPendingData)
        {
            return currentPendingData == lastPendingData;
        }

        void OnDestroy()
        {
            if (XAYASettings.isRegtestMode == false)
            {
                ServicePoint pt1 = ServicePointManager.FindServicePoint(new System.Uri(XAYASettings.GameServerAddress));
                ServicePoint pt2 = ServicePointManager.FindServicePoint(new System.Uri(XAYASettings.WalletServerAddress));
                ServicePoint pt3 = ServicePointManager.FindServicePoint(new System.Uri(XAYASettings.XIDServerAddress));

                pt1.CloseConnectionGroup("xaya");
                pt2.CloseConnectionGroup("xaya");
                pt3.CloseConnectionGroup("xaya");
            }
        }
    }
}