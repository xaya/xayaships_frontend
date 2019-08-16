using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
public class GameChannelManager : MonoBehaviour
{
    // Start is called before the first frame update
    XAYAClient xayaClient;
    Dictionary<string, ChannelStatus> channelPorts;

    public GameShootManager gameShootManager;
    public GameShootManager ourBoardManager;
    public ChannelDeamonManager channelDeamon;
    int curPort = 29060;
    //==========//

    void Start()
    {
        xayaClient = GetComponent<XAYAClient>();
        channelPorts = new Dictionary<string, ChannelStatus>();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.LogError("livechanel"+GlobalData.bLiveChannel);
    }
    //===================== when clicking   Create Channel button, directly call ========================//
    public void CreateGameChannel()
    {

        string address=xayaClient.GetNewAddress(GlobalData.gPlayerName);
        string value = "{\"g\":{\"xs\":{\"c\":{\"addr\":\""+address+"\"}}}}";        
        string texid=xayaClient.NameUpdate(GlobalData.gPlayerName, value);
        GetComponent<GameUserManager>().ShowInfo("CREATE CHANNEL.  Please wait a second...");

        //RunChannelService(texid);
        //=================== Waiting state ==================//
        //StartChannelWaiting(texid);
        //====================================================//
    }
    public void RunChannelService(string channelId)
    {

        //KillIsChannel();


        string channelServiceExePath = Application.streamingAssetsPath + "/shipsd/ships-channel.exe";      
        string channelServiceStr = " --xaya_rpc_url=\"" + GlobalData.gSettingInfo.GetServerUrl() + "\" --gsp_rpc_url=\"" +
            GlobalData.gSettingInfo.GSPIP + "\" --broadcast_rpc_url=\"http://seeder.xaya.io:10042\" --rpc_port=\"" + "29060" +"\" --playername=" +
            GlobalData.gPlayerName.Substring(2) + " --channelid=\"" + channelId + "\" -alsologtostderr";

        channelServiceStr += " --v=1";

        //-------test-------//
        //channelServiceStr += " -log_dir=\"\" --v=1";
        //------------------//ss

        if (!channelPorts.ContainsKey(channelId))
        {
            channelPorts.Add(channelId, new ChannelStatus(curPort++, "0"));
        }

        //=================================================================================================================//

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        if (!XAYABitcoinLib.Utils.StartService("cmd.exe", "/c " + channelServiceExePath + channelServiceStr, false))
        {
            GlobalData.ErrorPopup("Channel service is not running.");
        }
        //Debug.Log(channelServiceExePath + channelServiceStr);
#else
         if (!XAYABitcoinLib.Utils.StartService("/bin/bash", "-c 'ships-channel " + channelServiceStr+"'", false))
        {
            GlobalData.ErrorPopup("Channel service is not running.");
            Debug.Log("-c 'ships-channel " + channelServiceStr + "'");
        }
        
#endif

    }
    public void JoinGameChannel(string channelId)
    {
        string address = xayaClient.GetNewAddress(GlobalData.gPlayerName);
        
        string value = "{\"g\":{\"xs\":{\"j\":{\"id\":\""+ channelId + "\", \"addr\":\"" + address + "\"}}}}";
        string texid = xayaClient.NameUpdate(GlobalData.gPlayerName, value);

        GetComponent<GameUserManager>().ShowInfo("JOIN CHANNEL.("+ GlobalData.GetChannel(channelId).userNames[0]+")" +". Please wait a second.");
        //KillIsChannel();     
    }

    public void CloseGameChannel(string channelId)
    {
        //string address = xayaClient.GetNewAddress(GlobalData.gPlayerName);

        string value = "{\"g\":{\"xs\":{\"a\":{\"id\":\"" + channelId + "\"}}}}";
        string texid = xayaClient.NameUpdate(GlobalData.gPlayerName, value);

        GetComponent<GameUserManager>().ShowInfo("CLOSE CHANNEL. Please wait a second.");
        //=================================================================================================================//
        //WaitForSeconds(0.5f);

    }

    //======================  start channel and wait  ==========================//
    public void StartChannelWaiting(string channelId)
    {
        string cmdstr = "{\"jsonrpc\":\"2.0\", \"method\":\"getcurrentstate\", \"id\":0}";
        
        StartCoroutine(shipsdChannelRpcCommand(cmdstr, channelId, (status) => {
                        
            JObject jstatus= JObject.Parse(status) as JObject;
            //Debug.Log(jstatus["version"].ToString());
            //SetGameSateFromJson(status);
            WaitChange(channelId, jstatus["result"]["version"].ToString());

        }, 2.0f));
    }
public void WaitChange(string channelId, string version)
{

        string cmdstr = "";
        if (version == null) return;

        cmdstr = "{\"jsonrpc\":\"2.0\", \"method\":\"waitforchange\",  \"params\":[\"" + version + "\"],\"id\":0}";

        StartCoroutine(waitforChangeBlockchain(version, channelId, (status) => {


    }));
}
public void GetCurrentStateOnly(string channelId)
    {
        string cmdstr = "{\"jsonrpc\":\"2.0\", \"method\":\"getcurrentstate\", \"id\":0}";

        StartCoroutine(shipsdChannelRpcCommand(cmdstr, channelId, (status) => {
            //JObject jsonObject = JObject.Parse(status);            
            //print(status);
            try
            {
                ///JObject jstatus = JObject.Parse(status) as JObject;
                //Debug.Log(jstatus["result"]["version"].ToString());

                if (status != null)
                {
                    SetGameChannelSateFromJson(channelId, status);
                    Debug.LogError("getstate!"+Time.timeSinceLevelLoad);
                }
                else
                    Debug.LogError("getstate_error!" + Time.timeSinceLevelLoad);
            }
            catch (System.Exception e)
            {                
                print(e.ToString()+"stauts:"+status);
            }

        }));

    }
    public void SetGameChannelSateFromJson(string channelId, string result)
    {
        //string json = @"{  CPU: 'Intel',  Drives: [    'DVD read/writer',    '500 gigabyte hard drive'  ]}";

        JObject jresult = JObject.Parse(result) as JObject;
        jresult = jresult["result"] as JObject;
        //Debug.Log(jresult.ToString());

        channelPorts[channelId].version = jresult["version"].ToString();
        //Debug.Log("exist:"+  jresult["existsonchain"].ToString());

        //Debug.Log("winner" + GlobalData.gWinner);
        //Debug.Log("index" + GlobalData.gPlayerIndex);

        if (jresult["existsonchain"]!=null && (jresult["existsonchain"].ToString()=="false" || jresult["existsonchain"].ToString() == "False"))            
        {
            //if (jresult["winner"] != null || jresult["win])
            //{
            //GlobalData.bOpenedChannel = false;
            if (!GlobalData.bFinished)
            {
                if (GlobalData.gPlayerIndex == GlobalData.gWinner)
                    GlobalData.ErrorPopup("The game was finished. You have won.");
                else
                    GlobalData.ErrorPopup("The game was finished. You have lost.");

                Debug.LogError("beforeInit:" + GlobalData.bLiveChannel);                
                InitGameboard();
                Debug.LogError("Init game! live:" + GlobalData.bLiveChannel);
                GlobalData.bFinished = true;
            }
            return;
            //}
        }


        JArray participants = jresult["current"]["meta"]["participants"] as JArray;

        //Debug.Log("parti="+participants.ToString());
       
        if (GlobalData.gPlayerName.Substring(2) == participants[0]["name"].ToString())
        {
            GlobalData.gOpponentName = participants[1]["name"].ToString();
            GlobalData.gPlayerIndex = 0;
        }
        else
        {
            GlobalData.gOpponentName = participants[0]["name"].ToString();
            GlobalData.gPlayerIndex = 1;
        }
        bool bOldTurn=GlobalData.gbTurn;
        //=======================================   Whos turn? ======================//
        if (jresult["current"]["state"]["whoseturn"].ToString() == "1" && GlobalData.gPlayerName.Substring(2) == participants[1]["name"].ToString())
        {
            GlobalData.gbTurn = true;
            GetComponent<GameUserManager>().imgTurnIcon.color = new Color32(0, 0, 255, 255);
        }
        if (jresult["current"]["state"]["whoseturn"].ToString() == "0" && GlobalData.gPlayerName.Substring(2) == participants[0]["name"].ToString())
        {
            GlobalData.gbTurn = true;
            GetComponent<GameUserManager>().imgTurnIcon.color = new Color32(0, 0, 255, 255);
        }
        if (jresult["current"]["state"]["whoseturn"].ToString() == "0" && GlobalData.gPlayerName.Substring(2) == participants[1]["name"].ToString())
        {
            GlobalData.gbTurn = false;
            GetComponent<GameUserManager>().imgTurnIcon.color = new Color32(255, 0, 0, 255);
        }
            
        if (jresult["current"]["state"]["whoseturn"].ToString() == "1" && GlobalData.gPlayerName.Substring(2) == participants[0]["name"].ToString())
        {
            GlobalData.gbTurn = false;
            GetComponent<GameUserManager>().imgTurnIcon.color = new Color32(255, 0, 0, 255);
        }

        if (jresult["current"]["state"]["whoseturn"].ToString() == null || jresult["current"]["state"]["whoseturn"].ToString() == "" || jresult["current"]["state"]["whoseturn"].ToString() == "NULL")
        {
            GlobalData.gbTurn = false;
            GetComponent<GameUserManager>().imgTurnIcon.color = new Color32(200, 200, 200, 255);
        }

        if(!bOldTurn && GlobalData.gbTurn)
        {
            GetComponent<GameUserManager>().ShowInfo("YOUR TURN!");
        }
        //========================================== Shooting result ===================================//
        JObject jParsed = jresult["current"]["state"] as JObject;
        //jresult["current"]["state"]["parsed"] as JObject;

        //Debug.Log(jParsed.ToString());
        jParsed = jParsed["parsed"] as JObject;
        //Debug.Log(jParsed.ToString());        

        Debug.LogError(jParsed.ToString());

        //if (jParsed == null || jParsed["phase"].ToString()!="shoot") return;
        if (jParsed == null ) return;

        //Debug.Log(jParsed.ToString());
        JArray jGuesses= jParsed["guesses"] as JArray;        
        Debug.Log(jGuesses.ToString());

        if (jGuesses==null && jGuesses.Count < 2) return;

        string playerGuesses1 ;
        string playerGuesses2 ;
        gameShootManager.ClearMarker();
        ourBoardManager.ClearMarker();

        if(GlobalData.gPlayerIndex==1)
        {
             playerGuesses1 = jGuesses[0].ToString();
             playerGuesses2 = jGuesses[1].ToString();
        }
        else
        {
            playerGuesses1 = jGuesses[1].ToString();
            playerGuesses2 = jGuesses[0].ToString();
        }
        
        for(int i=0; i<playerGuesses1.Length;i++)
        {
            string ch = playerGuesses1.Substring(i, 1);            
            int row = i % 9+1;
            int col = i / 9+1;
            if (ch == "x")
            {
                gameShootManager.SetMarker(new Vector2(row, col), true);
            }
            if (ch == "m")
            {
                // Debug.Log("row: " + row + "col:" + col);
                gameShootManager.SetMarker(new Vector2(row, col), false);
            }
        }
        for (int i = 0; i < playerGuesses2.Length; i++)
        {
            string ch = playerGuesses2.Substring(i, 1);
            int row = i % 9 + 1;
            int col = i / 9 + 1;
            if (ch == "x")
            {
                 ourBoardManager.SetMarker(new Vector2(row, col), true);
            }
            if (ch == "m")
            {
                Debug.Log("row: " + row + "col:" + col);
                ourBoardManager.SetMarker(new Vector2(row, col), false);
            }
        }

        //============== Winner state =================//
        if (jParsed["winner"] != null && jParsed["phase"].ToString() == "finished")
        {
            GlobalData.gWinner = int.Parse(jParsed["winner"].ToString());
            Debug.Log("Game Finished!");
            string strInfo = "You have won.";
            if (GlobalData.gPlayerIndex != GlobalData.gWinner)
                strInfo = "You have lost."; 
            GetComponent<GameUserManager>().ShowInfo("GAME FINISHED! " + strInfo);

        }
        
        //==============================================================================================//

        //Debug.Log(jresult["current"]["state"]["whoseturn"].ToString());

    }
    private IEnumerator shipsdChannelRpcCommand(string requestJsonStr, string channelId, System.Action<string> callback,float delay=0.0f)
    {
        string tempStr = "";
        int port;

        if(delay>0.001f)
            yield return new WaitForSeconds(delay);

        if (channelId == null || !channelPorts.ContainsKey(channelId))
            port = 29060;
        else
            port= channelPorts[channelId].port;

        //================  fixed port =====================//
        port = 29060;
        //==================================================//
        string url = GlobalData.gSettingInfo.GetShipChannelUrl() + ":" +port;
        //Debug.Log(url);
        //Debug.Log(requestJsonStr);

        UnityWebRequest www = UnityWebRequest.Put(url, requestJsonStr);

        //Debug.Log("Requesting to ShipSD!!" + requestJsonStr);
        www.method = UnityWebRequest.kHttpVerbPOST;
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Accept", "application/json");
        www.downloadHandler = new DownloadHandlerBuffer();
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            //GlobalData.ErrorPopup(www.error);
            //GetComponent<GameUserManager>().ShowInfo("Channel State Error!");
            Debug.LogError(www.error);
            Debug.LogError(requestJsonStr);
            yield return new WaitForSeconds(3);
            tempStr = www.error;
            //GlobalData.bOpenedChannel = false;

            //GlobalData.gcurrentPlayedChannedId = null;
        }
        else
        {
            //resultJsonStr = www.downloadHandler.text;
            GlobalData.resultJsonStr = www.downloadHandler.text;
            tempStr = www.downloadHandler.text;
        }
        callback(tempStr);
    }

    private IEnumerator waitforChangeBlockchain(string version, string channelId, System.Action<string> callback)
    {

        string tempStr = "";
        //string requestJsonStr=;
        string requestJsonStr;
       

        while (GlobalData.bOpenedChannel)
        {
            //if (!GlobalData.bOpenedChannel) break;
            requestJsonStr = "{\"jsonrpc\":\"2.0\", \"method\":\"waitforchange\", \"params\":[" + version + "],\"id\":0}";
            //----------2, waitforchange===============//
            //string tempStr = "";
            int port;
            if (channelId == null || !channelPorts.ContainsKey(channelId))
                port = 29060;
            else
                port = channelPorts[channelId].port;
            //===========//
            port = 29060;

            //Debug.Log("wait version:" + version+ Time.timeSinceLevelLoad.ToString().Substring(0,5) );

            string url = GlobalData.gSettingInfo.GetShipChannelUrl() + ":" + port;
            //print(url);
            //print(requestJsonStr);
            UnityWebRequest www = UnityWebRequest.Put(url, requestJsonStr);
            //Debug.Log("Requesting to ShipSD-Channel!!" + requestJsonStr);
            www.method = UnityWebRequest.kHttpVerbPOST;
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Accept", "application/json");
            www.downloadHandler = new DownloadHandlerBuffer();
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error + " cmd:"+requestJsonStr);
                yield return new WaitForSeconds(0.01f);
                //tempStr = www.error;
                //GlobalData.bOpenedChannel = false;
                //break;
            }
            else
            {
                //resultJsonStr = www.downloadHandler.text;
                GlobalData.resultJsonStr = www.downloadHandler.text;
                tempStr = www.downloadHandler.text;

                JObject jstatus = JObject.Parse(tempStr) as JObject;

                //====       3.            ===================
                string curVersion = jstatus["result"]["version"].ToString();
                
                //4. call getcurentstate again ====================//
                if (version != curVersion)
                {
                    GetCurrentStateOnly(channelId);
                    Debug.Log("update channel state!");
                }
                version = curVersion;
                //Debug.Log(www.downloadHandler.text);
            }
            //callback(blockhash);
            //yield return new WaitForSeconds(0.01f);
        }
    }
    public bool IsRunningChannel(int port=29060)
    {

        try
        {
            foreach (System.Diagnostics.Process p in System.Diagnostics.Process.GetProcesses())
            {
                try
                {
                    if (p.ProcessName == "ships-channel")
                    {
                        print("ships-channel is already locally running.");
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
    public bool KillChannel(int port = 29060)
    {

        try
        {
            foreach (System.Diagnostics.Process p in System.Diagnostics.Process.GetProcesses())
            {
                try
                {
                    if (p.ProcessName == "ships-channel")
                    {
                        p.Kill();
                        p.WaitForExit();
                        print("ships-channel is killed.");
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
    public bool KillIsChannel()
    {

        if (channelDeamon != null)
        {
            //Debug.LogError("livechannel"+GlobalData.bLiveChannel +Time.timeSinceLevelLoad);
            channelDeamon.StopChannelService();
            //Debug.LogError("livechannel_ended" + GlobalData.bLiveChannel + Time.timeSinceLevelLoad);
            return true;
        }
        return false;        
    }
    public void StopForceChannel()
    {
        //================  open flag set  =========================//
        GlobalData.bOpenedChannel = false;
        string cmdstr = "{\"jsonrpc\":\"2.0\", \"method\":\"stop\"}";

        //================  fixed port =====================//
        int port = 29060;
        //==================================================//
        string url = GlobalData.gSettingInfo.GetShipChannelUrl() + ":" + port;
        UnityWebRequest www = UnityWebRequest.Put(url, cmdstr);
        www.method = UnityWebRequest.kHttpVerbPOST;
        www.SetRequestHeader("Content-Type", "application/json"); www.SetRequestHeader("Accept", "application/json");

        Debug.LogError(url + " cmd:  " + cmdstr);
        Debug.LogError("channelForce stop-request-time:" + Time.timeSinceLevelLoad);
        www.SendWebRequest();        
        //yield return Ninja.JumpToUnity;
        Debug.LogError("channelForce stop-final-time:" + Time.timeSinceLevelLoad);
        new WaitForSeconds(0.01f);
        GlobalData.bLiveChannel = false;
    }
    public void SetShipPostionSubmit()
    {
        //===============  For Testing =============//
        //if (GlobalData.bPlaying) return;
        //============================//
        char[] positionstr=new char[64];
        string strPos = "";
        for (int i = 0; i < 64; i++)
            positionstr[i] = '.';
        //positionstr = "";
        foreach(Transform tShip in GetComponent<GameUserManager>().MyshipObjs.transform)
        {
            if (tShip.GetComponent<DraggableShip>().GetPositions() == null) continue;
            foreach( BattleShip.BLL.Requests.Coordinate c in  tShip.GetComponent<DraggableShip>().GetPositions())
            {
                positionstr[(c.YCoordinate-1) * 8 + (c.XCoordinate-1)] = 'x';
            }
        }

        for (int i = 0; i < 64; i++)
            strPos+=positionstr[i];
        Debug.Log(GlobalData.gcurrentPlayedChannedId);
        SetPositionRequest(GlobalData.gcurrentPlayedChannedId, strPos);

        //GlobalData.bPlaying = true;
        Debug.Log(strPos);
    }
    public void SetShootSubmit(Vector2 v)
    {
        //===============  For Testing =============//
        //if (GlobalData.bPlaying) return;
        //============================//
       
        string cmdstr = "{\"jsonrpc\":\"2.0\", \"method\":\"shoot\",\"params\":{ \"row\":"+(int)v.x+", \"column\":"+(int)v.y+"}}";
        
        StartCoroutine(shipsdChannelRpcCommand(cmdstr, GlobalData.gcurrentPlayedChannedId, (status) => { }));
    }
    public void SetPositionRequest(string channelId,string strPos)
    {
        string cmdstr = "{\"jsonrpc\":\"2.0\", \"method\":\"setposition\",\"params\":[\""+strPos+"\"]}";

        StartCoroutine(shipsdChannelRpcCommand(cmdstr, channelId, (status) => {
            //JObject jsonObject = JObject.Parse(status);            
            //print(status);
            try
            {
                int nn = 0;
                //JObject jstatus = JObject.Parse(status) as JObject;
                //Debug.Log(jstatus.ToString());
                //SetGameChannelSateFromJson(channelId, status);
            }
            catch (System.Exception e)
            {
                print(e.ToString());
            }

        }));
    }
    public void InitGameboard()
    {
        foreach (Transform tShip in GetComponent<GameUserManager>().MyshipObjs.transform)
        {
            tShip.GetComponent<DraggableShip>().InitPos();           
        }
        gameShootManager.ClearMarker();
        ourBoardManager.ClearMarker();
        GlobalData.InitGameChannelData();
        //==================  Hide Gameboard  =====================//
        GetComponent<GameUserManager>().InitGameBoard();
        //=======================================//
#if UNITY_STANDALONE_LINUX        
        StopForceChannel();
        Debug.LogError("stop in linux!");      
#else
        KillIsChannel();
        //StopForceChannel();
#endif

    }
}


public class ChannelStatus
{
    public int port;
    public string version;
    public ChannelStatus(int p,string v)
    {
        port = p;
        version = v;
    }

}