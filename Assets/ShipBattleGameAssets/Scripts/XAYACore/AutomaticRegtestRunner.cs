using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;

namespace XAYA
{
    public class AutomaticRegtestRunner : MonoBehaviour
    {
        public static AutomaticRegtestRunner Instance;

        [HideInInspector]
        public bool isRunning = false;

        [HideInInspector]
        public bool isCleanRun = true;

        System.Diagnostics.Process myWalletDaemon;
        float waitForWalletReply = 1.0f;

        [HideInInspector]
        public int randomPort;
        public int randomRPCPort;
        public int randomDaemonPort;
        string ourAddress;
        string datadir = "";
        bool walletIsFullyPreminedAndReady = false;

        private void OnDestroy()
        {
            if (myWalletDaemon != null)
            {
                myWalletDaemon.Kill();
            }

            if (datadir != "")
            {
                try
                {
                    if (Directory.Exists(datadir))
                    {
                        Directory.Delete(datadir, true);
                    }
                }
                catch (System.Exception ex)
                {

                }
            }
        }

        public void LaunchDaemon()
        {
            isRunning = true;

            datadir = Application.streamingAssetsPath + "../../../Tests/";

            try
            {
                if (Directory.Exists(datadir))
                {
                    Directory.Delete(datadir, true);
                }
            }
            catch (System.Exception ex)
            {

            }

            if (!Directory.Exists(datadir))
            {
                Directory.CreateDirectory(datadir);
            }
            else
            {
                Debug.LogError("Test folder already exists. Least clean up failed? In any case, aborting tests.");
                return;
            }

           

            randomPort = Random.Range(10000, 65535);
            randomRPCPort = Random.Range(10000, 65535);
            randomDaemonPort = Random.Range(10000, 65535);

            string daemonDatadir = datadir + "daemondatadir";
            Directory.CreateDirectory(daemonDatadir);
            string configFilePath = datadir + "/daemondatadir/xaya.conf";

            FileStream StreamF = File.Create(configFilePath);
            StreamF.Close();

            string confText = "[regtest]\n";
            confText += "rpcuser=xayagametest\n";
            confText += "rpcpassword=xayagametest\n";
            confText += "rpcport=" + randomPort + "\n";
            confText += "zmqpubgameblocks=tcp://127.0.0.1:" + randomRPCPort + "\n";
            confText += "zmqpubgamepending=tcp://127.0.0.1:" + randomRPCPort + "\n";

            File.WriteAllText(configFilePath, confText);

            string daemonPath = Application.streamingAssetsPath + "/Daemon/xayad.exe";
            string daemonParams = "-reindex-chainstate -regtest -server -wallet=game.dat -fallbackfee=0.0005 -datadir=\"" + daemonDatadir + "\"";

            myWalletDaemon = new System.Diagnostics.Process();
            myWalletDaemon.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            myWalletDaemon.StartInfo.CreateNoWindow = true;
            myWalletDaemon.StartInfo.UseShellExecute = false;
            myWalletDaemon.StartInfo.FileName = daemonPath;

            myWalletDaemon.StartInfo.Arguments = System.Environment.ExpandEnvironmentVariables(daemonParams);
            myWalletDaemon.EnableRaisingEvents = true;
            myWalletDaemon.Start();

            /*When clean run, we recreate everything from scratch, without using preloaded regtest data,
             * starting from simply verifying, are we good to go*/

            if(isCleanRun)
            {
                waitForWalletReply = 1.0f;
            }
        }

        private JObject GenerateRequest(List<object> data, bool isNotification = false)
        {
            JObject requestObject = new JObject();

            requestObject.Add(new JProperty("jsonrpc", "2.0"));

            if (isNotification == false)
            {
                requestObject.Add(new JProperty("id", Random.Range(1,1000000)));
            }

            requestObject.Add(new JProperty("method", data[0]));
            if (data.Count > 1)
            {
                requestObject.Add(new JProperty("params", data[1]));
            }

            return requestObject;
        }

        // Start is called before the first frame update
        void Start()
        {
            Instance = this;
        }

        private string WalletRequest(JObject job)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://" + "127.0.0.1" + ":" + randomPort + "/");

            request.ConnectionGroupName = "xaya";
            request.ServicePoint.ConnectionLimit = 60;

            string requestString = JsonConvert.SerializeObject(job, Formatting.None);

            request.Method = "POST";
            request.ContentType = "application/json-rpc";
            request.Credentials = new NetworkCredential("xayagametest", "xayagametest");
            
            byte[] byteArray = Encoding.UTF8.GetBytes(requestString);
            request.ContentLength = byteArray.Length;

            bool networkError = false;

            try
            {
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
            }
            catch
            {
                //No connection?
                networkError = true;
            }

            if (networkError)
            {
                return "";
            }

            WebResponse webResponse = null;

            try
            {
                webResponse = request.GetResponse();
            }
            catch (WebException ex)
            {
                Debug.Log("Exception: " + ex.ToString() + "|" + requestString);
            }

            if (webResponse == null)
            {
                return "";
            }

            StreamReader reader = new StreamReader(webResponse.GetResponseStream());
            string response = reader.ReadToEnd();
            return response;
        }

        string AdvanceOneBlock()
        {
            List<object> requestData2 = new List<object>();
            requestData2.Add("generatetoaddress");

            JObject container = new JObject();
            JProperty v1 = new JProperty("nblocks", 1);
            JProperty v2 = new JProperty("address", ourAddress);
            container.Add(v1);
            container.Add(v2);

            requestData2.Add(container);

            return this.WalletRequest(GenerateRequest(requestData2));
        }

        void PremineCoinsAndRegisterDefaultName()
        {
            List<object> requestData = new List<object>();
            requestData.Add("getnewaddress");
            ourAddress = this.WalletRequest(GenerateRequest(requestData));

            if (ourAddress != "")
            {
                JObject resultAddr = JObject.Parse(ourAddress);
                ourAddress = resultAddr["result"].ToString();

                List<object> requestData2 = new List<object>();
                requestData2.Add("generatetoaddress");

                JObject container = new JObject();
                JProperty v1 = new JProperty("nblocks", 400);
                JProperty v2 = new JProperty("address", ourAddress);
                container.Add(v1);
                container.Add(v2);

                requestData2.Add(container);

                string generationResult = this.WalletRequest(GenerateRequest(requestData2));

                if(generationResult == "")
                {
                    Debug.LogError("Failed to execute generationResult");
                }
                else
                {
                    List<object> requestData3 = new List<object>();
                    requestData3.Add("getbalance");

                    string balanceReply = this.WalletRequest(GenerateRequest(requestData3));

                    if(balanceReply != "")
                    {
                        float balance = 0;
                        JObject resultBlc = JObject.Parse(balanceReply);
                        float.TryParse(resultBlc["result"].ToString(), out balance);

                        if(balance != 0)
                        {
                            List<object> requestData4 = new List<object>();
                            requestData2.Add("name_register");

                            JObject container2 = new JObject();
                            JProperty v3 = new JProperty("name", "p/regtestplayer");
                            JProperty v4 = new JProperty("value", "{}");
                            container2.Add(v3);
                            container2.Add(v4);

                            requestData4.Add(container2);

                            string nameRegisterResult = this.WalletRequest(GenerateRequest(requestData4));

                            if(nameRegisterResult != "")
                            {
                                if (AdvanceOneBlock() != "")
                                {
                                    walletIsFullyPreminedAndReady = true;

                                    XAYASettings.launchXAMPPbroadcastService = false;
                                    XAYASettings.launchChat = false;
                                    XAYASettings.LoginMode = LoginMode.Advanced;

                                    XAYADummyUI.Instance.LanchDaemonIfNotRunningAlready();
                                    XAYADummyUI.Instance.WaitForTheDaemonToSync();
                                }
                                else
                                {
                                    Debug.LogError("Failed to execute AdvanceOneBlock");
                                }
                            }
                            else
                            {
                                Debug.LogError("Failed to execute nameRegisterResult");
                            }
                        }
                        else
                        {
                            Debug.LogError("balanceReply is zero?");
                        }
                    }
                    else
                    {
                        Debug.LogError("Failed to execute balanceReply");
                    }
                }
            }
            else
            {
                Debug.LogError("Failed to execute getnewaddress");
            }
        }

        void TestForXayaDaemonReply()
        {
            List<object> requestData = new List<object>();
            requestData.Add("getnetworkinfo");

            string result = this.WalletRequest(GenerateRequest(requestData));

            if(result.Contains("subversion"))
            {
                PremineCoinsAndRegisterDefaultName();
            }
            else
            {
                Debug.Log("Wallet is still loading, or failed...");
                waitForWalletReply = 1.0f;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (isRunning == false) return;

            if(waitForWalletReply > 0)
            {
                waitForWalletReply -= Time.deltaTime;

                if(waitForWalletReply <= 0)
                {
                    TestForXayaDaemonReply();
                }
            }
        }
    }
}
