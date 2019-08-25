using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CielaSpike;
using UnityEngine.Networking;using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

public class ShipSDClient : MonoBehaviour
{
    [SerializeField]
    UnityEngine.UI.Text shipsdPathPrefix;
    [SerializeField]
    UnityEngine.UI.Text shipsdPathBackfix;

    [SerializeField]
    UnityEngine.UI.Toggle gspRunnungToggle;
    //[SerializeField]
      //UnityEngine.UI.InputField inputGSPStatus;

    GameObject m_errorBox = null;

    int errorCounter = 0;
    bool bCurrentLive = false;
    float runTime = 0;
    // Start is called before the first frame update
    void Start()
    {
        //m_errorBox = GameObject.Find("errorBox");
        if (IsRunningGSPServer())
        {            
            GetCurrentStateAndWaiting();
            runTime = Time.time;
            //return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("GSP live" + bCurrentLive);
        if(!bCurrentLive && (Time.time - runTime)>15 && GetComponent<GameUserManager>() && gspRunnungToggle.isOn )
        {
            Debug.Log("GSP restart!!");
            KillGSP();
            GlobalData.ErrorPopup("GSP failed to start.");
            //GetComponent<GameUserManager>().DisplayConnectionUI();
        }        
    }
    public void startGSPServer()
    {
        //--------Checking Local Server running state-----------//
        if (IsRunningGSPServer())
        {
            //GetCurrentStateAndWaiting();
            return;
        }
        //------------------------------------------------------//
        string strCmdText;
        GetComponent<GameUserManager>().UpdateSettingInfo();

        if (shipsdPathBackfix == null || shipsdPathPrefix == null) return;
        string strShipSDPath = shipsdPathPrefix.text + GlobalData.gSettingInfo.rpcUserName + ":" + GlobalData.gSettingInfo.rpcUserPassword + "@"+ GlobalData.gSettingInfo.xayaURL+"\" --game_rpc_port="+29050+ shipsdPathBackfix.text;
        strCmdText = Application.streamingAssetsPath+ "/" + strShipSDPath;
        //strCmdText+= " -log_dir=\"datadir\" --v=1";

        strCmdText += " --v=1";
        string envPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
        //Debug.Log(envPath);
        string strLinuxParams= " shipsd --xaya_rpc_url=\"http://"+ GlobalData.gSettingInfo.rpcUserName + ":" +GlobalData.gSettingInfo.rpcUserPassword + "@"+GlobalData.gSettingInfo.xayaURL+"\" --game_rpc_port=29050 --datadir=\""+envPath+"/.xayagame\" -alsologtostderr --v=1";

        //System.Diagnostics.Process.Start("cmd.exe", "/c " +strCmdText); //Start cmd process
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
           XAYABitcoinLib.Utils.StartService("cmd.exe", "/c" + strCmdText);
#else
        XAYABitcoinLib.Utils.StartService("/bin/bash", "-c '" + strLinuxParams+"'");
        
#endif
        Debug.Log("Win GSP path" + strCmdText);
        //Debug.Log("Linux GSP path:"+ strLinuxParams);
        GetCurrentStateAndWaiting();
        runTime = Time.time;
        Debug.Log(runTime);
    }

    public void GetCurrentStateAndWaiting()
    {
        string cmdstr= "{\"jsonrpc\":\"2.0\", \"method\":\"getcurrentstate\", \"id\":0}";

        StartCoroutine(shipsdRpcCommand(cmdstr, (status) => {
            //JObject jsonObject = JObject.Parse(status);            
            //print(status);
            retStatusJson ret = JsonConvert.DeserializeObject<retStatusJson>(status);
            //Debug.Log(ret.result.height);
            //Debug.Log(ret.result.blockhash);
            GlobalData.gblockhash = ret.result.blockhash;
            GlobalData.gblockHeight = ret.result.height;
            GlobalData.gblockStatusStr = ret.result.state;
            SetGameSateFromJson(status);
            WaitChange(GlobalData.gblockhash); 
            
        }));
    }
    public void SetGameSateFromJson(string result)
    {
        //string json = @"{  CPU: 'Intel',  Drives: [    'DVD read/writer',    '500 gigabyte hard drive'  ]}";

        JObject jresult = JObject.Parse(result) as JObject;

        //Debug.Log(jresult["result"]["gamestate"]["channels"]);

        //================  Get channel Info  ==============//
        JObject jChannels= jresult["result"]["gamestate"]["channels"] as JObject;
        Dictionary<string, JObject> dictObj = jChannels.ToObject<Dictionary<string, JObject>>();

        List<ChannelInfo> oldChannelList = new List<ChannelInfo>();
        oldChannelList = GlobalData.ggameChannelList;
        if(GlobalData.ggameChannelList!=null)
            GlobalData.ggameChannelList.Clear();
        if (GlobalData.ggameLobbyChannelList != null)
            GlobalData.ggameLobbyChannelList.Clear();

        foreach (KeyValuePair<string, JObject> item in dictObj)
        {
            ChannelInfo channelInfo = new ChannelInfo();
            channelInfo.id= item.Key;
            JArray a = item.Value["meta"]["participants"] as JArray;

            ChannelInfo oldChannel = null;
            foreach (ChannelInfo c in oldChannelList) if (c.id == channelInfo.id) oldChannel = c;            
            
            channelInfo.userNames = new string[a.Count];
            channelInfo.statusText =  item.Value["state"]["parsed"]["phase"].ToString();

            int index = 0;
            foreach(JObject j in a)
            {
                channelInfo.userNames[index++] = j["name"].ToString();
            }
            if(!GlobalData.bRunonce && a.Count>1)
            {
                channelInfo.bignored = true;
                GlobalData.ggameIgnoredChannelIDs.Add(channelInfo.id);
                continue;
            }
            
            GlobalData.AddChannel(channelInfo);

            if (a.Count == 1) GlobalData.ggameLobbyChannelList.Add(channelInfo);


            if (!GlobalData.bLogin) continue;

            
            //====== case new channel, load channel service =============//
            if (channelInfo.userNames.Length > 1)
            {
                if (GlobalData.bOpenedChannel || 
                    (channelInfo.userNames[0] != GlobalData.gPlayerName.Substring(2) &&
                    channelInfo.userNames[1] != GlobalData.gPlayerName.Substring(2)))
                    continue;

                string opponentName = channelInfo.userNames[0];
                if (channelInfo.userNames[0] == GlobalData.gPlayerName.Substring(2))
                    opponentName = channelInfo.userNames[1];
                //=========================    ==============================//                    
                if(!GetComponent<GameUserManager>().IsExistName("p/"+opponentName))    //=== case  only other user====//
                {
                    Debug.Log("liveFlag:" + GlobalData.bLiveChannel);
                    if (!GlobalData.bLiveChannel && !GlobalData.IsIgnoreChannel(channelInfo.id))
                    {
                        GlobalData.bOpenedChannel = true;
                        //if (!GlobalData.bLogin)
                        //    GlobalData.gcurrentPlayedChannedId = channelInfo.id;                  
                        //else
                        GetComponent<GameUserManager>().StartGameByChannel(channelInfo.id);
                        //case menu is active, collapse  menu 
                        GetComponent<GameUserManager>().InitMenu();
                        GlobalData.bLiveChannel = true;
                        GlobalData.bFinished = false;
                        GlobalData.bPlaying = false;
                    }
                    //Debug.Log(channelInfo.id);
                }                    

            }
            
        }
        //============================================================//
        if (!GlobalData.bRunonce)
        {            
            GlobalData.bRunonce = true;
        }

        //============================= Get Leader Info ===============================//
        JObject jstatss = jresult["result"]["gamestate"]["gamestats"] as JObject;
        //Debug.Log(jstatss.ToString());
        if (jstatss == null) return;
        dictObj = jstatss.ToObject<Dictionary<string, JObject>>();

        if (GlobalData.ggameLeaderList != null)
            GlobalData.ggameLeaderList.Clear();

        foreach (KeyValuePair<string, JObject> item in dictObj)
        {
            LeaderInfo leaderInfo = new LeaderInfo();
            leaderInfo.playerName = item.Key;
            leaderInfo.winCount= int.Parse(item.Value["won"].ToString());
            leaderInfo.playedCount = leaderInfo.winCount + int.Parse(item.Value["lost"].ToString());
            GlobalData.ggameLeaderList.Add(leaderInfo);
        }
        //============================================================//

    }
    public void GetCurrentStateOnly()
    {
        string cmdstr = "{\"jsonrpc\":\"2.0\", \"method\":\"getcurrentstate\", \"id\":0}";

        StartCoroutine(shipsdRpcCommand(cmdstr, (status) => {
            //JObject jsonObject = JObject.Parse(status);            
            //print(status);
            try
            {
                retStatusJson ret = JsonConvert.DeserializeObject<retStatusJson>(status);
                //Debug.Log("getstate:" + ret.result.height + " hash" + ret.result.blockhash);
                //Debug.Log(ret.result.blockhash);
                GlobalData.gblockhash = ret.result.blockhash;
                GlobalData.gblockHeight = ret.result.height;
                GlobalData.gblockStatusStr = ret.result.state;
                bCurrentLive = true;
                SetGameSateFromJson(status);
                
            }
            catch (System.Exception e)
            {
                //bCurrentLive = false;
                print(e.ToString());                
            }
           
            
        }));
    }
    public void WaitChange(string blockhash)
    {

        string cmdstr = "";
        if (blockhash == null) return;
            

        cmdstr = "{\"jsonrpc\":\"2.0\", \"method\":\"waitforchange\",  \"params\":[\"" + blockhash + "\"],\"id\":0}";

        StartCoroutine(waitforChangeBlockchain(blockhash, (status) => {
            
            //waitforchangeResult ret = JsonConvert.DeserializeObject<waitforchangeResult>(status);
            
            //Debug.Log(ret.result);
            
        }));
    }
    private IEnumerator shipsdRpcCommand(string requestJsonStr, System.Action<string> callback)
    {
        
        string tempStr = "";
        UnityWebRequest www = UnityWebRequest.Put(GlobalData.gSettingInfo.GetShipSDUrl(), requestJsonStr);
       // Debug.Log("Requesting to ShipSD!!" + requestJsonStr);
        www.method = UnityWebRequest.kHttpVerbPOST;
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Accept", "application/json");
        www.downloadHandler = new DownloadHandlerBuffer();
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
            tempStr = www.error;
            bCurrentLive = false;
        }
        else
        {
            //resultJsonStr = www.downloadHandler.text;
            GlobalData.resultJsonStr = www.downloadHandler.text;
            tempStr = www.downloadHandler.text;
            bCurrentLive = true;
            //Debug.Log(www.downloadHandler.text);
        }
        callback(tempStr);
    }

    private IEnumerator waitforChangeBlockchain(string blockhash, System.Action<string> callback)
    {
        
        string tempStr = "";
        //string requestJsonStr=;
        string requestJsonStr;
        
        while (true)
        {         

            requestJsonStr = "{\"jsonrpc\":\"2.0\", \"method\":\"waitforchange\", \"params\":[\"" + blockhash + "\"],\"id\":0}";
            //----------2, waitforchange===============//
            UnityWebRequest www = UnityWebRequest.Put(GlobalData.gSettingInfo.GetShipSDUrl(), requestJsonStr);
            //Debug.Log("Requesting to ShipSD!!" + requestJsonStr);
            www.method = UnityWebRequest.kHttpVerbPOST;
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Accept", "application/json");
            www.downloadHandler = new DownloadHandlerBuffer();
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
                tempStr = www.error;
                bCurrentLive = false;
                break;
            }
            else
            {
                //resultJsonStr = www.downloadHandler.text;
                GlobalData.resultJsonStr = www.downloadHandler.text;
                tempStr = www.downloadHandler.text;
                bCurrentLive = true;
                waitforchangeResult ret = JsonConvert.DeserializeObject<waitforchangeResult>(tempStr);

      //====       3.            ===================
                blockhash = ret.result;
                //4. call getcurentstate again ====================//
                if (blockhash != GlobalData.gblockhash)
                {
                    GlobalData.gblockhash = blockhash;
                        GetCurrentStateOnly();
                }
                //Debug.Log(www.downloadHandler.text);
            }
            callback(blockhash);
            yield return new WaitForSeconds(0.01f);
        }
    }


    public bool IsRunningGSPServer()
    {
        
        try
        {
            foreach (System.Diagnostics.Process p  in  System.Diagnostics.Process.GetProcesses())
            {
                try
                {
                    if (p.ProcessName == "shipsd")
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

    public void StopService()
    {
        StartCoroutine(StopServiceAsync());
    }

    IEnumerator StopServiceAsync()
    {
        string cmdstr = "{\"jsonrpc\":\"2.0\", \"method\":\"stop\"}";       
        string url = GlobalData.gSettingInfo.GetShipSDUrl();
        UnityWebRequest www = UnityWebRequest.Put(url, cmdstr);
        www.method = UnityWebRequest.kHttpVerbPOST;
        www.SetRequestHeader("Content-Type", "application/json"); www.SetRequestHeader("Accept", "application/json");        
        yield return www.SendWebRequest();
        
    }
    public bool KillGSP()
    {
        gspRunnungToggle.isOn = false;
        try
        {
            foreach (System.Diagnostics.Process p in System.Diagnostics.Process.GetProcessesByName("shipsd"))
            {
                try
                {                    
                        p.Kill();
                        p.WaitForExit();
                        print("ships-sd is killed.");                     
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
public class channels
{
    
}
public class gamestate
{
    public string channels;

}
public class result
{
    public string blockhash;
    public string chain;
    public string gameid;
    public long height;
    public string state;
    //public JObject gamestate;

}
public class retStatusJson
{
    public int id;
    public string jsonrpc;
    public result result;   
}
public class waitforchangeResult
{
    public int id;
    public string jsonrpc;
    public string result;
}

