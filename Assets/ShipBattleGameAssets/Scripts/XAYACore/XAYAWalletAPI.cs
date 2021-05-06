using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace XAYA
{
    /*Support utility to encode XID names*/
    public class HexadecimalEncoding
    {
        public static string ToHexString(string str)
        {
            Regex regex = new Regex("^[A-z0-9]+$");
            Match match = regex.Match(str);

            if (match.Success)
            {
                if (str.ToLower() == str)
                {
                    return str;
                }
            }

            var sb = new StringBuilder();

            var bytes = Encoding.UTF8.GetBytes(str);
            foreach (var t in bytes)
            {
                if (t.ToString("X2") != "00")
                {
                    sb.Append(t.ToString("X2"));
                }
            }

            return "x-" + sb.ToString();
        }

        public static string FromHexString(string hexString)
        {
            if (hexString.Contains("x-"))
            {
                hexString = hexString.Replace("x-", "");
            }
            else
            {
                return hexString;
            }

            try
            {
                var bytes = new byte[hexString.Length / 2];
                for (var i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
                }

                return Encoding.UTF8.GetString(bytes); // returns: "Hello world" for "48656C6C6F20776F726C64"
            }
            catch
            {
                return hexString;
            }
        }
    }

    public class XAYAWalletAPI : Singleton<XAYAWalletAPI>
    {
        private Process XAMPPServerDaemon;

        public bool xidIsSolved = false;
        private bool retryXIDTest = false;
        private bool registeringXIDName = false;
        private bool XIDNameIsRegistered = false;

        private Text informationFeedbackLastKnow;

        void OnApplicationQuit()
        {
            StopAndQuitAllDaemons();
        }

        void StopAndQuitAllDaemons()
        {
            RPCRequest r = new RPCRequest();
            r.StopGSP();

            if(XAMPPServerDaemon != null)
            {
                XAMPPServerDaemon.Kill();
            }
        }

        public void SetElectronWallet()
        {
            XAYASettings.LoginMode = LoginMode.Advanced;
        }

        public void SetElectrumWallet()
        {
            XAYASettings.LoginMode = LoginMode.Simple;
        }

        public void RestartGameCompletely(int startingSceneIndex)
        {
            SceneManager.LoadScene(startingSceneIndex, LoadSceneMode.Single);
        }

        /* Construct connection string from the data variables we gathered in some way using game UI or PlayerPrefs*/
        public void ConstructConnectionStrings()
        {
            XAYASettings.GameServerAddress = "http://" + XAYASettings.GameDaemonIP + ":" + XAYASettings.GameDaemonPort + "/";
            XAYASettings.WalletServerAddress = "http://" + XAYASettings.ElectronWalletIPAddress + ":" + XAYASettings.ElectronWalletPort + "/";
            XAYASettings.XIDServerAddress = "http://127.0.0.1:8400/"; //todo: load from settings too

            if (XAYASettings.LoginMode == LoginMode.Simple)
            {
                XAYASettings.WalletServerAddress = "http://" + XAYASettings.ElectronWalletIPAddress + ":" + XAYASettings.ElectrumWalletPort + "/";
                XAYASettings.XIDServerAddress = "http://127.0.0.1:8602/"; //todo: load from settings too
            }
        }

        /* The way it works now, is that config file for ELECTRUM needs to be pre-populated inside directory //... Electrum-CHI ...//
         * during the game installation. Its just a file 'config', no extension, containing:
         *     "rpcpassword": "password",
               "rpcport": 8999,
               "rpcuser": "user"
         and here this function locates it and loads it. This does not imply secuity risk and workaroujnd certain bug
         during wallet creation. rpcpassword, rpcport, rpsuser can be any other value, we just load them here */
        public bool TryAndResolveElectrumConfig(out string userName, out string password, out string port)
        {
            string userDirPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string workingDir = userDirPath + "/Electrum-CHI";
            string electronConfigPath = workingDir + "/config";

            userName = PlayerPrefs.GetString("electrum_" + "userName", "");
            password = PlayerPrefs.GetString("electrum_" + "password", "");
            port = PlayerPrefs.GetString("electrum_" + "port", "");

            if (File.Exists(electronConfigPath))
            {
                string fileData = "";
                StreamReader reader = new StreamReader(electronConfigPath);
                fileData = reader.ReadToEnd();
                reader.Close();

                JObject result = JObject.Parse(fileData);

                userName = result["rpcuser"].ToString();
                password = result["rpcpassword"].ToString();
                port = result["rpcport"].ToString();

                PlayerPrefs.SetString("electrum_" + "userName", userName);
                PlayerPrefs.SetString("electrum_" + "password", password);
                PlayerPrefs.SetString("electrum_" + "port", port);

                XAYASettings.ElectrumWalletUsername = userName;
                XAYASettings.ElectrumWalletPassword = password;
                XAYASettings.ElectrumWalletPort = port;

                return true;
            }
            else
            {
                SignalWalletError("Fail to load electrum config file, this will break everything now");
                return false;
            }
        }

        public void GetUserNameAndPasswordFromCookies(out string rpc_username, out string rpc_password)
        {
            /*Lets see, if we can fetch cookies automatically first*/
            string cookiePath = Application.persistentDataPath + "/../../../Roaming/Xaya/.cookie";
            string cookiePathTestnet = Application.persistentDataPath + "/../../../Roaming/Xaya/testnet/.cookie";
            string cookiePathRegtest = Application.persistentDataPath + "/../../../Roaming/Xaya/regtest/.cookie";

            if (File.Exists(cookiePath))
            {
                string[] lines = File.ReadAllLines(cookiePath);
                string[] expl = lines[0].Split(':');

                rpc_username = expl[0];
                rpc_password = expl[1];
            }
            else if (File.Exists(cookiePathTestnet))
            {
                string[] lines = File.ReadAllLines(cookiePathTestnet);
                string[] expl = lines[0].Split(':');

                rpc_username = expl[0];
                rpc_password = expl[1];
            }
            else if (File.Exists(cookiePathRegtest))
            {
                string[] lines = File.ReadAllLines(cookiePathRegtest);
                string[] expl = lines[0].Split(':');

                rpc_username = expl[0];
                rpc_password = expl[1];
            }
            else
            {
                rpc_username = "unresolved";
                rpc_password = "unresolved";
            }
        }

        /*Main enumerator for launching daemon exe files*/
        /* Example: (LaunchDaemonLocally(Application.dataPath, Application.persistentDataPath, Application.isEditor)*/
        /* yields progress, which can br processed to show so response text to the users UI */
        public IEnumerator LaunchDaemonLocally(string path, string persistent, bool isEditor, System.Action<string> callback)
        {
            if (XAYASettings.LoginMode == LoginMode.Advanced)
            {
                callback("smcd launching");

                Process[] pname = Process.GetProcessesByName(XAYASettings.DaemonName);
                if (pname.Length != 0)
                {
                    // We probably have daemon running already, so no need to worry about it
                }
                else
                {
                    //Lets launch the daemon

                    try
                    {
                        Process myProcessDaemon = new Process();
                        myProcessDaemon.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        myProcessDaemon.StartInfo.CreateNoWindow = true;
                        myProcessDaemon.StartInfo.UseShellExecute = false;
                        myProcessDaemon.StartInfo.FileName = Application.streamingAssetsPath + "/Daemon/"+ XAYASettings.DaemonName+".exe";

                        myProcessDaemon.StartInfo.Arguments = Environment.ExpandEnvironmentVariables("--xaya_rpc_url=\"http://" + XAYASettings.ElectronWalletUsername + ":" + XAYASettings.ElectronWalletPassword + "@" + XAYASettings.GameDaemonIP + ":" + XAYASettings.ElectronWalletPort + "\" --enable_pruning=1000 --game_rpc_port=" + XAYASettings.GameDaemonPort + " --datadir=\"%appdata%/XAYA-Electron/"+ XAYASettings.DaemonName+"data/\"" + " --v=1 --alsologtostderr=1 --log_dir=\"%appdata%/XAYA-Electron/"+ XAYASettings.DaemonName+"data/\"");
                        myProcessDaemon.EnableRaisingEvents = true;
                        myProcessDaemon.Start();
                    }
                    catch (Exception e)
                    {
                        SignalWalletError(e.ToString());             
                    }
                }
            }
            else
            {
                float progress = 0;
                yield return progress;
                callback("Starting...");

                string username = "";
                string password = "";
                string port = "";

                string userDirPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string workingDir = userDirPath + "/Electrum-CHI";

                if (TryAndResolveElectrumConfig(out username, out password, out port))
                {
                    yield return null;
                    RPCRequest r = new RPCRequest();

                    string dParams = "";
                    string ip = XAYASettings.ElectrumWalletPort;

                    dParams = "daemon";

                    progress = 0.01f;
                    yield return progress;

                    Process[] pname = Process.GetProcessesByName("electrum");
                    if (pname.Length != 0)
                    {
                        // We probably have daemon running already, so no need to worry about it
                    }
                    else
                    {
                        //Lets launch the daemon

                        try
                        {
                            Process myProcessDaemon = new Process();
                            myProcessDaemon.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                            myProcessDaemon.StartInfo.CreateNoWindow = true;
                            myProcessDaemon.StartInfo.UseShellExecute = false;
                            myProcessDaemon.StartInfo.FileName = Application.streamingAssetsPath + "/Daemon/electrum.exe";
                            myProcessDaemon.StartInfo.WorkingDirectory = workingDir;
                            myProcessDaemon.StartInfo.Arguments = dParams;
                            myProcessDaemon.EnableRaisingEvents = true;
                            myProcessDaemon.Start();
                        }
                        catch (Exception e)
                        {
                            SignalWalletError(e.ToString());
                        }
                    }

                    progress = 0.1f;
                    yield return progress;
                    callback("Launching XID...");

                    Process[] pname2 = Process.GetProcessesByName("xid-light");
                    if (pname2.Length != 0)
                    {
                        // We probably have daemon running already, so no need to worry about it
                    }
                    else
                    {
                        try
                        {
                            Process myProcessDaemonXID = new Process();
                            myProcessDaemonXID.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                            myProcessDaemonXID.StartInfo.CreateNoWindow = true;
                            myProcessDaemonXID.StartInfo.UseShellExecute = false;
                            myProcessDaemonXID.StartInfo.Arguments = "--game_rpc_port=8602 --rest_endpoint=\"https://xid.xaya.io\" --cafile=\"" + Application.streamingAssetsPath + "/Daemon/letsencrypt.pem\"";
                            myProcessDaemonXID.StartInfo.FileName = Application.streamingAssetsPath + "/Daemon/xid-light.exe";
                            myProcessDaemonXID.StartInfo.WorkingDirectory = workingDir;

                            UnityEngine.Debug.Log(myProcessDaemonXID.StartInfo.FileName + " as " + myProcessDaemonXID.StartInfo.Arguments);

                            myProcessDaemonXID.EnableRaisingEvents = true;
                            myProcessDaemonXID.Start();
                        }
                        catch (Exception e)
                        {
                            SignalWalletError(e.ToString());
                        }
                    }


                    bool walletCraetedAndResponding2 = false;
                    while (walletCraetedAndResponding2 == false)
                    {
                        progress = 0.15f;
                        yield return progress;
                        callback("Executing exe files...");

                        try
                        {
                            string loadingInfp = r.ElectrumGetInfo();

                            if (loadingInfp.Contains("result"))
                            {
                                JObject result = JObject.Parse(loadingInfp);
                                if (result["result"]["connected"].ToString() == "false" || result["result"]["default_wallet"].ToString() == "")
                                {
                                    progress = 0.3f;
                                    callback("Connecting to electrum network...");
                                }
                                else
                                {
                                    walletCraetedAndResponding2 = true;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            callback("Erorr: " + e.ToString());
                        }

                        yield return progress;

                        if (walletCraetedAndResponding2 == false)
                        {
                            yield return new WaitForSeconds(1.0f);
                        }
                    }

                    progress = 0.4f;
                    yield return progress;
                    callback("loading-wallet");

                    // Lets track synch status

                    //Do we have electron wallet?

                    string electronWalletPathPath = workingDir + "/wallets/default_wallet";

                    bool walletLoadedJustFine = false;
                    int tries = 0;
                    if (!File.Exists(electronWalletPathPath))
                    {
                        bool walletCraetedAndResponding = false;
                        while (walletCraetedAndResponding == false)
                        {
                            try
                            {
                                string loadingInfp = r.ElectrumGetInfo();

                                if (loadingInfp.Contains("result"))
                                {
                                    JObject result = JObject.Parse(loadingInfp);

                                    string res = "";

                                    if (result["result"]["connected"].ToString() == "false" && result["result"]["default_wallet"].ToString() == "")
                                    {
                                        progress = 0.6f;
                                        callback("Synching, current header is at: " + result["result"]["blockchain_height"]);
                                    }
                                    else
                                    {
                                        if (File.Exists(electronWalletPathPath))
                                        {
                                            walletCraetedAndResponding = true;
                                        }
                                        else
                                        {
                                            res = r.ElectrumWalletCreate();
                                        }
                                    }
                                }
                                else
                                {
                                    if (tries == 30)
                                    {
                                        callback("Erorr: Failed to get electrum info");
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                callback("Erorr: " + e.ToString());
                            }

                            tries++;
                            yield return new WaitForSeconds(1.0f);

                            progress = 0.4f;
                            yield return progress;
                        }

                        string rRes = r.ElectrumLoadWallet();

                        if (rRes.Contains("true"))
                        {
                            walletLoadedJustFine = true;
                        }
                        else
                        {
                            callback("Erorr: Failed to load the wallet");
                        }
                    }
                    else
                    {
                        try
                        {
                            string rRes = r.ElectrumLoadWallet();

                            if (rRes.Contains("true"))
                            {
                                walletLoadedJustFine = true;
                            }
                            else
                            {
                                callback("Erorr: Failed to load the wallet");
                            }
                        }
                        catch (Exception e)
                        {
                            callback("Erorr: " + e.ToString());
                        }
                    }

                    if (walletLoadedJustFine)
                    {
                        progress = 1.0f;
                        yield return progress;
                        callback("Confirming Electrum...");
                    }
                    else
                    {
                        callback("Erorr: Failed to load the wallet");
                    }

                }
                else
                {
                    callback("Erorr: Failed to resolve configuration file.");
                }
            }

            yield return -1.0f;
        }

        /*This function takes care of launching Charon, which is an art of its own, because it needs to
         * also poroperly sign username with XID, hence function looks a little bit bulky*/
        public IEnumerator LaunchCharon(string username, System.Action<string> callback)
        {
            bool xidNameResolved = false;
            bool waitingForReply = false;

            RPCRequest r = new RPCRequest();

            PlayerXIDResult xidResults = null;
            int tries = 0;
            while (xidNameResolved == false)
            {

                try
                {
                    /*Ok, first thing first, we want to get chat names states*/
                    r = new RPCRequest();
                    xidResults = r.XIDNameState(username).result;

                    if (xidResults.data.signers.Count != 0)
                    {
                        /*We have no XID name registered, so we must don it first*/
                        xidNameResolved = true;
                    }
                    else
                    {
                        if (waitingForReply == false)
                        {
                            r = new RPCRequest();
                            callback("NAME NOT REGISTERED");

                            string newAddresss = r.GetNewAddressForXIDChat();

                            JObject result = JObject.Parse(newAddresss);
                            string resAddress = result["result"].ToString();

                            JObject data = JObject.Parse("{\"g\":{\"id\":{\""+ XAYASettings.gameID + "\":{\"g\":[\"" + resAddress + "\"]}}}}");
                            r.XAYANameUpdateDirect(username, data);

                            waitingForReply = true;
                            tries++;
                        }
                    }

                    if (tries >= 3 && waitingForReply == false)
                    {
                        callback("Error: " + "XID FAILED TO RESOLVE");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    callback("Error: " + ex.ToString());
                }

                yield return new WaitForSeconds(10.0f);
            }

            /*Now that we have our name registered, and signer address retrieved, we are good to proceed logging in*/

            string userDirPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string workingDir = userDirPath + "/Electrum-CHI";
            string stubs = Application.streamingAssetsPath + "/Daemon/" + XAYASettings.rpcCommandJsonFile;

            string passwordD = "fillemeplease";
            string authMessage = r.AuthWithWallet(username, XAYASettings.chatDomainName, out passwordD);

            bool signSolved = false;
            string singResult = "";


            /*If name was transferred, we might need to iterate the lists and resign the mssage*/

            for (int f = 0; f < xidResults.data.signers[0].addresses.Count; f++)
            {
                singResult = r.SingMessage(xidResults.data.signers[0].addresses[f], authMessage);

                if (singResult != "")
                {
                    signSolved = true;
                }

                yield return null;
            }

            if (signSolved == false)
            {
                int previousCount = xidResults.data.signers.Count;
                xidNameResolved = false;
                tries = 0;

                while (xidNameResolved == false)
                {
                    try
                    {
                        /*Ok, first thing first, we want to get chat names states*/

                        r = new RPCRequest();
                        xidResults = r.XIDNameState(username).result;

                        if (xidResults.data.signers.Count != previousCount)
                        {
                            /*We have no XID name registered, so we must don it first*/
                            xidNameResolved = true;
                        }
                        else
                        {
                            //connectionStatus.text = "Waiting for XID to register the name...";

                            if (waitingForReply == false)
                            {
                                r = new RPCRequest();

                                callback("NAME NOT REGISTERED");
                                string newAddresss = r.GetNewAddressForXIDChat();

                                JObject result = JObject.Parse(newAddresss);
                                string resAddress = result["result"].ToString();

                                JObject data = JObject.Parse("{\"g\":{\"id\":{\"" + XAYASettings.gameID + "\":{\"g\":[\"" + resAddress + "\"]}}}}");
                                r.XAYANameUpdateDirect(username, data);

                                waitingForReply = true;
                                tries++;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        callback("Error: " + ex.ToString());
                    }

                    if (tries >= 3 && waitingForReply == false)
                    {
                        callback("Error: " + "XID FAILED TO RESOLVE");
                        break;
                    }

                    yield return new WaitForSeconds(10.0f);
                }

                signSolved = false;
                singResult = "";

                for (int f = 0; f < xidResults.data.signers[0].addresses.Count; f++)
                {
                    try
                    {
                        singResult = r.SingMessage(xidResults.data.signers[0].addresses[f], authMessage);
                    }
                    catch (Exception ex)
                    {
                        callback("Error: " + ex.ToString());
                    }

                    if (singResult != "")
                    {
                        signSolved = true;
                    }

                    yield return null;
                }

            }
            /*at this point, we are probably good co tontinue*/

            if (signSolved == false)
            {
                //connectionStatus.text = "Failed to resolve XID signing...";
            }
            else
            {
                Process myProcessDaemonCharon = new Process();

                try
                {
                    string ourXIDpassword = r.SetAuthStignature(passwordD, singResult);
                    string ourXIDLogin = HexadecimalEncoding.ToHexString(username) + "@" + XAYASettings.chatDomainName;

                    XAYASettings.XIDAuthPassword = ourXIDpassword;

                    myProcessDaemonCharon.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    myProcessDaemonCharon.StartInfo.CreateNoWindow = true;
                    myProcessDaemonCharon.StartInfo.UseShellExecute = false;
                    myProcessDaemonCharon.StartInfo.Arguments = Environment.ExpandEnvironmentVariables("--server_jid "+XAYASettings.chatID+"@"+XAYASettings.chatDomainName+" --client_jid " + ourXIDLogin + " --password \"" + ourXIDpassword + "\" --waitforchange --waitforpendingchange --backend_version \"0.3\" --port=" + XAYASettings.GameDaemonPort + " --methods_json_spec=\"" + stubs + "\" --alsologtostderr");

                    myProcessDaemonCharon.StartInfo.FileName = Application.streamingAssetsPath + "/Daemon/charon-client.exe";
                    myProcessDaemonCharon.StartInfo.WorkingDirectory = workingDir;
                    myProcessDaemonCharon.EnableRaisingEvents = true;
                }
                catch (Exception ex)
                {
                    callback("Error: " + ex.ToString());
                }

                if (myProcessDaemonCharon.Start())
                {
                    callback("WAITING TO SOLVE GSP");
                }
                else
                {
                    callback("FAIL");
                }

            }
        }

        public void LaunchXMPPServer()
        {
            string ourXIDpassword = XAYASettings.XIDAuthPassword;
            string ourXIDLogin = HexadecimalEncoding.ToHexString(XAYASettings.playerName) + "@" + XAYASettings.chatDomainName;

            string userDirPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string workingDir = userDirPath + "/Electrum-CHI";

            XAMPPServerDaemon = new Process();
            XAMPPServerDaemon.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            XAMPPServerDaemon.StartInfo.CreateNoWindow = true;
            XAMPPServerDaemon.StartInfo.UseShellExecute = false;
            XAMPPServerDaemon.StartInfo.Arguments = Environment.ExpandEnvironmentVariables("--game_id \"" + XAYASettings.gameID + "\" --jid " + ourXIDLogin + " --password " + ourXIDpassword + " --muc \"muc."+XAYASettings.chatDomainName+"\" --port "+ XAYASettings.XAMPPbroadcastPORT);

            XAMPPServerDaemon.StartInfo.FileName = Application.dataPath + "/StreamingAssets/Daemon/xmpp-broadcast-rpc-server.exe";
            XAMPPServerDaemon.StartInfo.WorkingDirectory = workingDir;
            XAMPPServerDaemon.EnableRaisingEvents = true;

            XAMPPServerDaemon.Start();
        }

        IEnumerator TestIfXIDNamePresent(Text informationFeedback)
        {
            yield return null;

            if (registeringXIDName == false)
            {
                informationFeedback.text = "Testing, if XID is present";
            }

            try
            {
                string username = XAYASettings.playerName;

                RPCRequest r = new RPCRequest();
                PlayerXID nameData = r.XIDNameState(username);

                if (nameData == null || (nameData.result.data.addresses.Count == 0 && nameData.result.data.signers.Count == 0))
                {
                    if (registeringXIDName == false)
                    {
                        registeringXIDName = true;
                        string newAddresss = r.GetNewAddressForXIDChat();

                        JObject result = JObject.Parse(newAddresss);
                        string resAddress = result["result"].ToString();

                        JObject data = JObject.Parse("{\"g\":{\"id\":{\"s\":{\"g\":[\"" + resAddress + "\"]}}}}");
                        r.XAYANameUpdateDirect(username, data);

                        informationFeedback.text = "Issued TX to register XID, please wait for block confirmations...";
                    }
                }
                else
                {
                    registeringXIDName = false;
                    XIDNameIsRegistered = true;
                }
            }
            catch (Exception ex)
            {
                informationFeedback.text = ex.ToString() + ", retrying...";
            }

            yield return null;
        }

        public IEnumerator EnsureXIDIsRegistered(Text informationFeedback)
        {
            informationFeedbackLastKnow = informationFeedback;

            xidIsSolved = false;
            retryXIDTest = false;

            if (XIDNameIsRegistered == false)
            {
                StartCoroutine(TestIfXIDNamePresent(informationFeedback));
                retryXIDTest = true;
            }
            else
            {
                if (XAYASettings.LoginMode == LoginMode.Advanced)
                {
                    informationFeedback.text = "Authenticating XID...";

                    CoroutineWithData<string> coroutine = null;

                    AsynchroniousRequests requestAS = new AsynchroniousRequests();
                    coroutine = new CoroutineWithData<string>(requestAS.AuthWithWallet(XAYASettings.playerName));
                    yield return coroutine.Coroutine; while (coroutine.result == null) { yield return new WaitForEndOfFrame(); }
                    yield return new WaitForSeconds(0.5f);

                    if (coroutine != null && coroutine.result == "Call failed")
                    {
                        informationFeedback.text = "Error authenticating. Failed to register XID name registered in XAYA wallet?";
                    }
                    else
                    {
                        string password = "";
                        try
                        {
                            JObject result = JObject.Parse(coroutine.result);
                            password = result["result"]["data"].ToString();
                            XAYASettings.XIDAuthPassword = password;
                            xidIsSolved = true;
                        }
                        catch
                        {
                            retryXIDTest = true;
                        }
                    }
                }
            }
        }

        public void SignalWalletError(string error)
        {
            //ConnectionStatusSolver.Instance.IndicatorXaya.color = new Color32(255, 161, 0, 255);
            UnityEngine.Debug.LogError(error);
        }

        void EnsureNoLiteModeDaemonsAreRunning()
        {
            //Lets try closing all lite-model related processed, if they are running

            Process[] pname2 = Process.GetProcessesByName("xid-light");
            if (pname2.Length != 0)
            {
                pname2[0].Kill();
            }

            Process[] pname3 = Process.GetProcessesByName("electrum");
            if (pname3.Length != 0)
            {
                pname3[0].Kill();
            }

            Process[] pname4 = Process.GetProcessesByName("charon-client");
            if (pname4.Length != 0)
            {
                pname4[0].Kill();
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            EnsureNoLiteModeDaemonsAreRunning();
        }

        // Update is called once per frame
        void Update()
        {
            if(retryXIDTest)
            {
                retryXIDTest = false;
                StartCoroutine(EnsureXIDIsRegistered(informationFeedbackLastKnow));
            }
        }
    }
}