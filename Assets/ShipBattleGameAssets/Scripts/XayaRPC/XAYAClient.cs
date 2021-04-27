using UnityEngine;
using BitcoinLib.Services.Coins.XAYA;
using System.Collections.Generic;
using BitcoinLib.Responses;
using System.Collections;
using UnityEngine.Networking;

using BitcoinLib.Auxiliary;
using BitcoinLib.ExceptionHandling.Rpc;
using BitcoinLib.Services.Coins.Base;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using System.IO;
using System;

public class XAYAClient : MonoBehaviour 
{
    // We could send RPC calls via libxayagame.However, we're presenting this class here 
    // because the Game State Processor does not need to be integrated into Unity at all, 
    // and when it isn't, we need an in-house RPC implementation, e.g. this XAYAClient class.

    public bool connected = false;
    string cmdResult;

    //public XAYAConnector connector;

    [HideInInspector]
    public IXAYAService xayaService;


    private static ICoinService xayaCoinService;

    private void Start()
    {
        
    }
    public bool Connect()
    {
        xayaService = new XAYAService("http://"+GlobalData.gSettingInfo.xayaURL, GlobalData.gSettingInfo.rpcUserName,GlobalData.gSettingInfo.rpcUserPassword, "", 10);
        
        if (xayaService.GetConnectionCount() > 0)
        {
            // We are not tracking connection drops or anything
            // here for the same of simplicity.We just assume
            // that once we are connected, then we are always fine.

            //connector.SubscribeForBlockUpdates();
            
            
           connected = true;
           return true;
        }
        else
        {
            Debug.Log("Failed to connect with XAYAService.");
        }
        return false;
    }

    public int GetTotalBlockCount()
    {
        if (xayaService == null) return 0;
        return (int)xayaService.GetBlockCount();
    }

    private string QueryXID(JObject job)
    {
        string requestString = JsonConvert.SerializeObject(job, Formatting.None);

        string address = GlobalData.XIDServerAddress;
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
        return response;
    }

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

    public IEnumerator XIDAuthWithWallet(string username)
    {
        List<object> requestData = new List<object>();
        JObject container = new JObject();

        JProperty usernameP = new JProperty("name", username);
        JProperty applicationP = new JProperty("application", "chat.xaya.io");
        JProperty dataP = new JProperty("data", new JObject());

        container.Add(usernameP);
        container.Add(applicationP);
        container.Add(dataP);

        requestData.Add("authwithwallet");
        requestData.Add(container);

        string result = "";

        try
        {
            result = this.QueryXID(this.CreateJObject(requestData));
        }
        catch (WebException ex)
        {

        }

        yield return result;
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
            result = this.QueryXID(this.CreateJObject(requestData));
            PlayerStats = JsonConvert.DeserializeObject<PlayerXID>(result);
        }
        catch (WebException ex)
        {

        }

        return PlayerStats;
    }

    public List<string> GetNameList()
    {
        List<string> allMyNames = new List<string>();

        // We are not doing any error checking here for the sake of simplicity.
        // We just assume that all goes well.

       List < GetNameListResponse> nList = xayaService.GetNameList();

        foreach(var nname in nList)
        {
            if (nname.ismine == true)
            {                
                allMyNames.Add(nname.name);
                
            }
        }

        return allMyNames;
    }
    public Dictionary<string,string> GetNameAndValues()
    {
        //List<string> allMyNames = new List<string>();
        Dictionary<string, string> allNameAndValues = new Dictionary<string, string>();
        // We are not doing any error checking here for the sake of simplicity.
        // We just assume that all goes well.

        List<GetNameListResponse> nList = xayaService.GetNameList();

        foreach (var nname in nList)
        {
            if (nname.ismine == true)
            {
                allNameAndValues.Add(nname.name, nname.value);
                //allMyNames.Add(nname.name);

            }
        }

        return allNameAndValues;
    }

    public string GetBalance()
    {
        return xayaService.GetBalance().ToString();        
    }
    public string RegisterUserName(string newName)
    {
        //string rpcCmdRegister="";
        //return rpcCmdRegister;
        //BitcoinLib.Services;
        //ICoinService iCoinService;

        //string r = xayaCoinService.RegisterName(newName, "{}", new object());

        return xayaService.RegisterName(newName, "{}", new object());


        //rpcCmdRegister = "\"method\":\"name_register\",\"params\":[\""+ newName +"\",\"{}\"]";

        //CoroutineWithData cd = new CoroutineWithData(this, xayaRpcCommand(rpcCmdRegister));
        //yield return cd.coroutine;
        //return cd.result.ToString();

        // StartCoroutine(xayaRpcCommand(rpcCmdRegister));
        //return false;
    }

    IEnumerator IRegisterUserName(string rpcCmdRegister)
    {
        yield return new WaitForSeconds(0.1f);
    }


    public string ExecuteMove(string playername, string direction, string distance)
	{   
         return xayaService.NameUpdate(playername, "{\"g\":{\"mv\":{\"d\":\"" + direction + "\",\"n\":" + distance + "}}}", new object()); 		
	}

    IEnumerator xayaRpcCommand(string requestJsonStr, System.Action<string> resultCallback)
    {

        UnityWebRequest www = UnityWebRequest.Put(GlobalData.gSettingInfo.GetServerUrl(), requestJsonStr);

        www.method = UnityWebRequest.kHttpVerbPOST;
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Accept", "application/json");
        www.downloadHandler = new DownloadHandlerBuffer();
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            yield return www.error;
            Debug.Log(www.error);
        }
        else
        {
            //resultJsonStr = www.downloadHandler.text;
            GlobalData.resultJsonStr = www.downloadHandler.text;
            resultCallback(www.downloadHandler.text);
            yield return www.downloadHandler.text;
            //Debug.Log(www.downloadHandler.text);
        }
    }

    public string forceCommand(string requestJsonStr)
    {
        cmdResult = "";
        StartCoroutine(xayaRpcCommand(requestJsonStr, (cmdResult) => {
        }));
       
        return cmdResult;
    }
    public string GetNewAddress(string userName, string address = "")
    {
        return xayaService.GetNewAddress(GlobalData.gPlayerName, address);
        //return xayaService.GetAccountAddress(userName);
    }
    public string NameUpdate(string playerName, string Value)
    {
        string method = "name_update";
        string nameUpdateCmd = "{\"method\":\""+method+"\",\"params\":[\""+playerName+"\",\"\"]} ";
        //string resultStr= forceCommand(nameUpdateCmd);
        //return resultStr;
        return xayaService.NameUpdate(playerName, Value, new object());
    }
    public bool IsRunningXayaServer()
    {
        try
        {
            foreach (System.Diagnostics.Process p in System.Diagnostics.Process.GetProcesses())
            {
                try
                {
                    if (p.ProcessName == "xayad")
                    {
                        //print("shipsd is already locally running.");
                        return true;
                    }
                    //print(p.ToString());
                }
                catch (System.Exception e)
                {
                    print("Mini exception when trying to list the name " + e);
                }
            }
        }
        catch (System.Exception e)
        {
            print("Exception caught " + e);
        }
        return false;
    }
}

public class CoroutineWithData
{
    public Coroutine coroutine { get; private set; }
    public object result;
    private IEnumerator target;
    public CoroutineWithData(MonoBehaviour owner, IEnumerator target)
    {
        this.target = target;
        this.coroutine = owner.StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        while (target.MoveNext())
        {
            result = target.Current;
            yield return result;
        }
    }
}