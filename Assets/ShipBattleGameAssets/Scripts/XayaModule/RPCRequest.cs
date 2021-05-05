using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;

namespace XAYA
{
    /*Minimalistic example of RPC class. Can be expanded into singleton,
     * open connections tracking, caching e.t.c. */
    public class RPCRequest
    {
        //WARNING!!!! THIS ONE (GspIsConnected) NEEDS TO BE CHANGED, AS ITS GAME-SPECIFIC
        //RIGHT NOW THIS IS CONFIGURED FOR SOCCER MANAGER ELITE
        //BASICALLY HERE CAN GO ANY GSP RPC TO MAKE SURE, IT RETURNS VALID RESULT
        public bool GspIsConnected()
        {
            List<object> requestData = new List<object>();
            JObject container = new JObject();
            JProperty seasonID = new JProperty("season_id", 0);

            container.Add(seasonID);

            requestData.Add("get_news_feed");
            requestData.Add(container);

            string result = "";

            try
            {
                result = this.HTTPReq(this.CreateJObject(requestData), false, false);
            }
            catch (WebException ex)
            {
                Debug.Log(ex.ToString());
            }

            if (result == "") return false;
            if (result.Contains("height") && result.Contains("chain")) return true;
            return false;
        }

        /*This RPC is GSP-specific, but we are likely to see it in ANY game
         This example is for SME, but Taurion has own NULL function, idea here
        is to get game state metadata without actual state itself*/
        public string XAYAGetGameStateNull()
        {
            List<object> requestData = new List<object>();
            JArray j = new JArray();

            requestData.Add("getcurrentstate");
            requestData.Add(j);

            return this.HTTPReq(this.CreateJObject(requestData));
        }

        public bool XIDIsConnected()
        {
            List<object> requestData = new List<object>();
            JArray j = new JArray();

            if (XAYASettings.isElectrum())
            {
                requestData.Add("getnullstate");
                requestData.Add(j);
            }
            else
            {
                JObject container = new JObject();
                JProperty player_id = new JProperty("name", "dummy");
                container.Add(player_id);

                requestData.Add("getnamestate");
                requestData.Add(container);
            }

            string result = "";

            try
            {
                result = this.HTTPReq(this.CreateJObject(requestData), true, true);
            }
            catch (WebException ex)
            {
                Debug.Log(ex.ToString());
            }

            if (result == "") return false;
            if (result.Contains("height") && result.Contains("chain")) return true;
            return false;
        }


        public string ElectrumLoadWallet()
        {
            List<object> requestData = new List<object>();
            requestData.Add("load_wallet");
            return this.HTTPXayaReq(this.CreateJObject(requestData), false, true);
        }

        public string ElectrumWalletCreate()
        {
            List<object> requestData = new List<object>();
            JObject container = new JObject();
            JProperty seedType = new JProperty("seed_type", "standard");

            container.Add(seedType);

            requestData.Add("create");
            requestData.Add(container);

            return this.HTTPXayaReq(this.CreateJObject(requestData), false, true);
        }

        public string SingMessage(string addressD, string messageD)
        {
            List<object> requestData = new List<object>();
            JObject container = new JObject();

            JProperty pName = new JProperty("address", addressD);
            JProperty aApplication = new JProperty("message", messageD);

            container.Add(pName);
            container.Add(aApplication);

            requestData.Add("signmessage");
            requestData.Add(container);

            string result = this.HTTPXayaReq(this.CreateJObject(requestData), false, false, true);

            if (result == "") return "";

            JObject resultOBJ = JObject.Parse(result);
            string realREsult = resultOBJ["result"].ToString();

            return realREsult;
        }

        public string SetAuthStignature(string passwordD, string signatureD)
        {
            List<object> requestData = new List<object>();
            JObject container = new JObject();

            JProperty pPassword = new JProperty("password", passwordD);
            JProperty pSignature = new JProperty("signature", signatureD);

            container.Add(pPassword);
            container.Add(pSignature);

            requestData.Add("setauthsignature");
            requestData.Add(container);

            string result = this.HTTPReq(this.CreateJObject(requestData), true, false, false);

            JObject resultOBJ = JObject.Parse(result);
            string realREsult = resultOBJ["result"].ToString();

            return realREsult;
        }

        public bool WalletIsConnected(out int blockCount)
        {
            List<object> requestData = new List<object>();
            JArray j = new JArray();

            requestData.Add("getblockcount");
            requestData.Add(j);

            string result = "";

            try
            {
                if (XAYASettings.isElectrum())
                {
                    result = this.HTTPXayaReq(this.CreateJObject(requestData), true, false);
                }
                else
                {
                    result = this.HTTPXayaReq(this.CreateJObject(requestData), true, true);
                }
            }
            catch (WebException ex)
            {
                Debug.Log(ex.ToString());
                blockCount = -1;
                return false;
            }

            if (result == "")
            {
                blockCount = -1;
                return false;
            }

            JObject resultSS = JObject.Parse(result);
            string bCNT = resultSS["result"].ToString();

            int.TryParse(bCNT, out blockCount);

            if (result == "") return false;
            if (blockCount != -1) return true;
            return false;
        }

        public string GetNetworkInfo()
        {
            List<object> requestData = new List<object>();
            JArray j = new JArray();

            requestData.Add("getinfo");
            requestData.Add(j);

            return this.HTTPXayaReq(this.CreateJObject(requestData), true, true);
        }

        public void StopGSP()
        {
            List<object> requestData = new List<object>();

            requestData.Add("stop");

            this.HTTPReq(this.CreateJObject(requestData, true));

            if (XAYASettings.isElectrum())
            {
                this.HTTPXayaReq(this.CreateJObject(requestData, true), true, true);
                this.HTTPReq(this.CreateJObject(requestData, true), true);
            }
        }

        public string AuthWithWallet(string pNameD, string pApplicationD, out string passwordD)
        {
            List<object> requestData = new List<object>();
            JObject container = new JObject();

            JProperty pName = new JProperty("name", pNameD);
            JProperty aApplication = new JProperty("application", pApplicationD);
            JProperty pData = new JProperty("data", new JObject());


            container.Add(pName);
            container.Add(aApplication);
            container.Add(pData);


            requestData.Add("getauthmessage");
            requestData.Add(container);

            string result = this.HTTPReq(this.CreateJObject(requestData), true, false, false);

            JObject resultOBJ = JObject.Parse(result);
            string realREsult = resultOBJ["result"]["authmessage"].ToString();

            passwordD = resultOBJ["result"]["password"].ToString();

            return realREsult;
        }

        public string XAYANameUpdateDirect(string userName, JObject data)
        {
            List<object> requestData = new List<object>();

            JArray j = new JArray();

            j.Add("p/" + userName);
            j.Add("" + data + "");

            requestData.Add("name_update");
            requestData.Add(j);

            return this.HTTPXayaReq(this.CreateJObject(requestData));
        }

        public string GetNewAddressForXIDChat()
        {
            List<object> requestData = new List<object>();

            JObject data = new JObject();

            if (!XAYASettings.isElectrum())
            {
                JProperty label = new JProperty("label", "");
                JProperty type = new JProperty("address_type", "legacy");
                data.Add(label);
                data.Add(type);
            }

            requestData.Add("getnewaddress");
            requestData.Add(data);

            return this.HTTPXayaReq(this.CreateJObject(requestData));
        }

        public PlayerXID XIDNameState(string playerName)
        {
            List<object> requestData = new List<object>();
            JObject container = new JObject();
            JProperty player_id = new JProperty("name", playerName);
            container.Add(player_id);
            PlayerXID PlayerStats = null;

            requestData.Add("getnamestate");
            requestData.Add(container);

            string result = "";

            try
            {
                result = this.HTTPReq(this.CreateJObject(requestData), true, true, true);
                PlayerStats = JsonConvert.DeserializeObject<PlayerXID>(result);
            }
            catch (WebException ex)
            {
                Debug.Log("Exception: " + ex.ToString());
            }

            return PlayerStats;
        }

        public string ElectrumGetInfo()
        {
            List<object> requestData = new List<object>();
            requestData.Add("getinfo");

            string result = this.HTTPXayaReq(this.CreateJObject(requestData, false), false, true);
            return result;
        }

        private string HTTPXayaReq(JObject job, bool ignoreDebugLog = false, bool ignoreElectrumFullpath = false, bool ignoreFiltering = false)
        {
            if (!XAYASettings.isElectrum())
            {
                if (XAYASettings.ElectronWalletUsername == "" || XAYASettings.ElectronWalletPassword == "") return "";
            }
            else
            {
                if (XAYASettings.ElectrumWalletUsername == "" || XAYASettings.ElectrumWalletPassword == "") return "";
            }

            HttpWebRequest request = null;

            if (ignoreElectrumFullpath == false || XAYASettings.isElectrum() == false)
            {
                if (!XAYASettings.isElectrum())
                {
                    request = (HttpWebRequest)WebRequest.Create(XAYASettings.WalletServerAddress + "wallet/game.dat");
                }
                else
                {
                    string rSTR = "http://" + XAYASettings.ElectronWalletIPAddress + ":" + XAYASettings.ElectrumWalletPort + "/xaya/compatibility";
                    request = (HttpWebRequest)WebRequest.Create(rSTR);
                }
            }
            else
            {
                request = (HttpWebRequest)WebRequest.Create("http://" + XAYASettings.ElectronWalletIPAddress + ":" + XAYASettings.ElectrumWalletPort + "/");
            }

            request.ConnectionGroupName = "xaya";
            request.ServicePoint.ConnectionLimit = 60;

            string requestString = JsonConvert.SerializeObject(job, Formatting.None);

            request.Method = "POST";
            request.ContentType = "application/json-rpc";

            if (XAYASettings.isElectrum() == false)
            {
                request.Credentials = new NetworkCredential(XAYASettings.ElectronWalletUsername, XAYASettings.ElectronWalletPassword);
            }
            else
            {
                request.Credentials = new NetworkCredential(XAYASettings.ElectrumWalletUsername, XAYASettings.ElectrumWalletPassword);
            }

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

        /* Creates a JObject containing the data to be sent in a JSONRPC HTTP request*/
        private JObject CreateJObject(List<object> data, bool isNotification = false)
        {
            JObject requestObject = new JObject();

            requestObject.Add(new JProperty("jsonrpc", "2.0"));
            if (isNotification == false)
            {
                requestObject.Add(new JProperty("id", 1));
            }

            requestObject.Add(new JProperty("method", data[0]));
            if (data.Count > 1)
            {
                requestObject.Add(new JProperty("params", data[1]));
            }

            return requestObject;
        }

        /*Sends a JSONRPC HTTP request and receives the response*/
        private string HTTPReq(JObject job, bool isXIDRequest = false, bool ignoreDebugLog = false, bool ignoreFiltering = false)
        {
            string requestString = JsonConvert.SerializeObject(job, Formatting.None);
            string address = XAYASettings.GameServerAddress;

            if (isXIDRequest)
            {
                address = XAYASettings.XIDServerAddress;
            }

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(address);
            webRequest.ServicePoint.ConnectionLimit = 60;
            webRequest.ConnectionGroupName = "xaya";
            webRequest.ContentType = "application/json-rpc";
            webRequest.Method = "POST";

            byte[] byteArray = Encoding.UTF8.GetBytes(requestString);
            webRequest.ContentLength = byteArray.Length;

            bool networkError = false;

            try
            {
                Stream dataStream = webRequest.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
            }
            catch (Exception ex)
            {
                Debug.Log("NETWORK ERROR: " + ex.ToString());
                networkError = true;
            }

            if (networkError)
            {
                return "";
            }

            WebResponse webResponse = null;

            try
            {
                webResponse = webRequest.GetResponse();
            }
            catch (WebException ex)
            {
                Debug.Log("Exception: " + ex.ToString() + "|" + requestString);
            }

            if (webResponse == null) return "";

            StreamReader reader = new StreamReader(webResponse.GetResponseStream());
            string response = reader.ReadToEnd();

            if (address == XAYASettings.GameServerAddress)
            {
                try
                {
                    JObject resultJobject = JObject.Parse(response);
                    string ghgt = resultJobject["result"]["height"].ToString();
                    int currentBlockCount = Int32.Parse(ghgt);
                }
                catch (Exception ex)
                {
                    Debug.Log(ex.ToString());
                }
            }

            return response;
        }

        public List<string> XAYAGetNameList()
        {
            List<object> requestData = new List<object>();
            JArray j2 = new JArray();

            requestData.Add("name_list");
            requestData.Add(j2);

            string nameData = this.HTTPXayaReq(this.CreateJObject(requestData));

            JObject result = JObject.Parse(nameData);
            string namesJSON = result["result"].ToString();
            JArray namesArray = JArray.Parse(namesJSON);

            List<string> NameList = new List<string>();

            foreach (JObject j in namesArray)
            {
                string nameJSON = j.ToString();

                if (nameJSON != string.Empty)
                {
                    Username name = JsonConvert.DeserializeObject<Username>(nameJSON);

                    if (name.Name.Contains("p/") && name.ismine == true)
                    {
                        NameList.Add(name.Name);
                    }
                }
            }

            return NameList;
        }

        /*RPC down below are useful in Electrum Wallet*/
        public List<List<string>> GetAddressList()
        {
            List<object> requestData = new List<object>();

            JObject data = new JObject();

            JProperty ARG1 = new JProperty("receiving", "true");
            data.Add(ARG1);

            JProperty ARG2 = new JProperty("labels", "false");
            data.Add(ARG2);

            JProperty ARG3 = new JProperty("unused", "true");
            data.Add(ARG3);

            requestData.Add("listaddresses");
            requestData.Add(data);

            string result = this.HTTPXayaReq(this.CreateJObject(requestData), false, true, false);

            JObject resultO = JObject.Parse(result);
            string ddRes = resultO["result"].ToString();

            List<List<string>> addresses = JsonConvert.DeserializeObject<List<List<string>>>(ddRes);

            return addresses;
        }

        public string GetNewAddress(string _label = "receiveSME")
        {
            List<object> requestData = new List<object>();

            JObject data = new JObject();

            JProperty label = new JProperty("label", _label);
            data.Add(label);

            requestData.Add("getnewaddress");
            requestData.Add(data);

            string result = this.HTTPXayaReq(this.CreateJObject(requestData));

            JObject resultO = JObject.Parse(result);

            return resultO["result"].ToString();
        }

        public string ElectrumWalletGetSeed()
        {
            List<object> requestData = new List<object>();
            requestData.Add("getseed");


            string result = this.HTTPXayaReq(this.CreateJObject(requestData, false), false, true);

            JObject resultO = JObject.Parse(result);

            return resultO["result"].ToString();
        }

        public string XAYANameRegister(string userName)
        {
            List<object> requestData = new List<object>();

            JObject container = new JObject();
            JProperty nameParam = new JProperty("name", "p/" + userName);
            JProperty valueParam = new JProperty("value", "{}");

            container.Add(nameParam);
            container.Add(valueParam);


            requestData.Add("name_register");
            requestData.Add(container);

            return this.HTTPXayaReq(this.CreateJObject(requestData), false, false, true);
        }

        public void SendToAddress(string sendToAddress, double amountToSent)
        {
            List<object> requestData = new List<object>();
            JObject container = new JObject();
            JProperty addressJ = new JProperty("address", sendToAddress);
            JProperty amountJ = new JProperty("amount", amountToSent);

            container.Add(addressJ);
            container.Add(amountJ);

            requestData.Add("sendtoaddress");
            requestData.Add(container);

            this.HTTPXayaReq(this.CreateJObject(requestData), false, false);
        }
    }
}