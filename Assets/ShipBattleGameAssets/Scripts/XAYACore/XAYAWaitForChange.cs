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
    public class PendingStateData
    {
    }

    public interface IXAYAWaitForChange
    {
        /*Wait for pending transaction to pass into memory pool*/
        void OnWaitForChangeNewBlock();

        void OnWaitForChangeTID(PendingStateData latestPendingData);

        bool SerializedPendingIsDifferent(PendingStateData latestPendingData);
    }

    public class XAYAWaitForChange : MonoBehaviour
    {
        string lastWaitForChangeResult = "";
        string lastWaitForChangeResultChannels = "";
        int lastWaitForPendingChangeResult = -1;
        int lastBlockHeight;
        PendingStateData lastPendingData;

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
            if(forGame && forChannels)
            {
                UnityEngine.Debug.LogError("Fatal error, can't listen to both, no reason to do so");
                return;
            }

            if (forGame)
            {
                Task task;
                this.StartCoroutineAsync(WaitForChangePendings(), out task);
            }

            if (forPending)
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
                RPCRequest r = new RPCRequest();
                string waitForChange = r.XAYAWaitForChangeGameChannels(lastWaitForChangeResultChannels);
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

                if (lastWaitForChangeResultChannels != knownHash)
                {
                    lastWaitForChangeResultChannels = knownHash;


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

                yield return new WaitForSeconds(0.1f);
                yield return Ninja.JumpBack;
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

        IEnumerator WaitForChangePendings()
        {
            while (readyToAcceptPendingCalls == false)
            {
                yield return new WaitForSeconds(0.1f);
            }

            bool skipFirst = true;

            while (true)
            {
                yield return Ninja.JumpBack;
                RPCRequest r = new RPCRequest();
                string reply = r.XAYAWaitForChangePending(lastWaitForPendingChangeResult);

                if (reply != "" && !reply.Contains("null")) //Could happen, if charon nut loaded yet in light mode, also first block is null all the time
                {
                    JObject result = JObject.Parse(reply);
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

                        PendingStateData pendingData = JsonConvert.DeserializeObject<PendingStateData>(result["result"]["pending"].ToString());

                        if (this.PendingDataEquals(pendingData) == false)
                        {
                            for (int s = 0; s < objectsRegisteredForWaitForChange.Count; s++)
                            {
                                IXAYAWaitForChange wfc = (IXAYAWaitForChange)objectsRegisteredForWaitForChange[s].GetComponent(typeof(IXAYAWaitForChange));
                                if (wfc != null)
                                {
                                    if (skipFirst == false)
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
                            }

                            for (int s = 0; s < objectsRegisteredForWaitForChangeIntefaceOnly.Count; s++)
                            {
                                IXAYAWaitForChange wfc2 = (IXAYAWaitForChange)objectsRegisteredForWaitForChangeIntefaceOnly[s];
                                if (wfc2 != null)
                                {
                                    wfc2.OnWaitForChangeTID(pendingData);
                                }
                            }
                        }
                    }
                }

                skipFirst = false;
            }
        }

        private bool PendingDataEquals(PendingStateData currentPendingData)
        {
            string curSer = JsonConvert.SerializeObject(currentPendingData);
            string storedData = JsonConvert.SerializeObject(lastPendingData);

            if (storedData != curSer)
            {
                return false;
            }

            lastPendingData = currentPendingData;

            return true;
        }

        void OnDestroy()
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