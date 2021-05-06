using CielaSpike;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

/*MonoBehaviour helper itility, which monitors status of Wallet, GSP, XID
 * and reports back, other scripts like wallet launchers uses that info
 * to take proper decision*/

namespace XAYA
{
    public class ConnectionStatusSolver : MonoBehaviour
    {
        public GameObject ConnectionStatusHandler;
        public int currentBlockNumber;

        public bool charonAllowsToTest = false;
        public bool electrumAllowsToTest = false;
        public bool testWalletConnection = false;
        float testIfXayaWalletIsAlive = 0.05f;

        bool testGspConnection = true;
        float testIfGspIsAlive = 0.07f;

        bool testXIDConnection = true;
        float testIfXIDIsAlive = 0.09f;

        bool electrumSolved = false;

        [HideInInspector]
        public bool walletSolved = false;

        [HideInInspector]
        public bool xidSolver = false;

        [HideInInspector]
        public bool gspSolved = false;

        [HideInInspector]
        public bool forceRetest = false;

        [HideInInspector]
        public bool solverReadyToRun = false;

        public static ConnectionStatusSolver Instance;

        public string lastReportedGSPError = "";

        IEnumerator CheckXayaWalletConnection()
        {
            while (XAYASettings.WalletServerAddress == "")
            {
                yield return null;
            }

            if (XAYASettings.isElectrum() && electrumAllowsToTest == false)
            {
                yield return null;
            }
            else
            {
                if (testWalletConnection == false)
                {
                    yield return Ninja.JumpToUnity;

                    walletSolved = false;
                    testIfXayaWalletIsAlive = 5.0f;
                    testWalletConnection = true;
                }
                else
                {
                    testWalletConnection = false;
                    testIfXayaWalletIsAlive = 5.0f;

                    int blockCount = 0;

                    RPCRequest r = new RPCRequest();
                    bool result = r.WalletIsConnected(out blockCount);

                    yield return Ninja.JumpToUnity;

                    currentBlockNumber = blockCount;
                    bool currentSolveStatus = walletSolved;

                    if (result)
                    {
                        if (XAYASettings.isElectrum() && electrumSolved == false)
                        {
                            bool electrumInSynch = false;

                            while (electrumInSynch == false)
                            {
                                r = new RPCRequest();
                                string res = r.GetNetworkInfo();

                                if (res.Contains("blockchain_height"))
                                {
                                    JObject result2 = JObject.Parse(res);
                                    string isConnected = result2["result"]["connected"].ToString();

                                    if (isConnected == "true" || isConnected == "True")
                                    {
                                        electrumInSynch = true;
                                    }
                                    else
                                    {
                                    }
                                }
                                else
                                {
                                }

                                if (electrumInSynch == false)
                                {
                                    /*SOME UI FEDDBACK CAN GO THERE, LIKE 'CHECKING FOR CONNECTION'*/
                                    yield return new WaitForSeconds(5.0f);
                                }
                                else
                                {

                                }
                            }
                        }

                        walletSolved = true;
                        electrumSolved = true;
                    }
                    else
                    {
                        walletSolved = false;
                    }

                    if (walletSolved == false)
                    {
                         /*Maybe connection settings are not good? Changed? Here is good palce to test for that*/
                    }
                    else
                    {
                        if (currentSolveStatus != walletSolved)
                        {

                        }

                        testIfXayaWalletIsAlive = 10.0f;
                    }

                    testWalletConnection = true;
                }
            }
        }

        IEnumerator CheckGspConnection()
        {
            while (XAYASettings.GameServerAddress == "")
            {
                yield return null;
            }

            if (XAYASettings.isElectrum() && charonAllowsToTest == false)
            {
                if (walletSolved)
                {
                    yield return Ninja.JumpToUnity;
                }
            }
            else
            {
                if (testGspConnection == false)
                {
                    yield return Ninja.JumpToUnity;
                    gspSolved = false;
                    testGspConnection = true;
                    testIfGspIsAlive = 5.0f;
                }
                else
                {
                    bool currentSolveStatus = gspSolved;

                    if (walletSolved)
                    {
                        testGspConnection = false;
                        testIfGspIsAlive = 5.0f;

                        RPCRequest r = new RPCRequest();
                        bool result = r.GspIsConnected();

                        yield return Ninja.JumpToUnity;

                        if (result)
                        {
                            gspSolved = true;
                        }
                        else
                        {
                            gspSolved = false;
                            lastReportedGSPError = "[...failed...]";
                        }

                        if (gspSolved == false)
                        {

                        }
                        else
                        {
                            if (currentSolveStatus != gspSolved)
                            {
                            }

                            testIfGspIsAlive = 10.0f;
                        }
                        testGspConnection = true;
                    }
                    else
                    {
                        testGspConnection = true;
                        testIfGspIsAlive = 5.0f;
                        yield return null;
                    }
                }
            }
        }

        IEnumerator CheckXIDConnection()
        {
            while (XAYASettings.XIDServerAddress == "")
            {
                yield return null;
            }

            if (testXIDConnection == false)
            {
                yield return Ninja.JumpToUnity;

                xidSolver = false;
                testXIDConnection = true;
                testIfXIDIsAlive = 5.0f;
            }
            else
            {
                if (walletSolved)
                {
                    testXIDConnection = false;
                    testIfXIDIsAlive = 5.0f;

                    RPCRequest r = new RPCRequest();
                    bool result = r.XIDIsConnected();

                    yield return Ninja.JumpToUnity;

                    if (result)
                    {
                        xidSolver = true;
                    }
                    else
                    {
                        xidSolver = false;
                    }

                    if (xidSolver == false)
                    {

                    }
                    else
                    {
                        testIfXIDIsAlive = 10.0f;
                    }

                    testXIDConnection = true;
                }
                else
                {
                    testXIDConnection = true;
                    testIfXIDIsAlive = 5.0f;
                    yield return null;
                }
            }
        }

        // Use this for initialization
        void Awake()
        {
            Instance = this;
        }

        // Update is called once per frame
        void Update()
        {
            if (solverReadyToRun == false) return;

            if (walletSolved && gspSolved && xidSolver && forceRetest == false)
            {
                return;
            }

            testIfXayaWalletIsAlive -= Time.deltaTime;

            if (testIfXayaWalletIsAlive <= 0)
            {
                this.StartCoroutineAsync(CheckXayaWalletConnection());
            }

            testIfGspIsAlive -= Time.deltaTime;

            if (testIfGspIsAlive <= 0)
            {
                this.StartCoroutineAsync(CheckGspConnection());
            }

            testIfXIDIsAlive -= Time.deltaTime;

            if (testIfXIDIsAlive <= 0)
            {
                this.StartCoroutineAsync(CheckXIDConnection());
            }

            forceRetest = false;
        }
    }
}
