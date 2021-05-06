using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CielaSpike;
using UnityEngine.Networking;using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using XAYA;

public class DisputeStatus
{

    public bool canresolve;
    public long height;
    public int whoseturn;
    public DisputeStatus()
    {
        this.height = 0;
        this.canresolve = true;
    }
}

public class ShipSDClient : MonoBehaviour, IXAYAWaitForChange
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

    public GameShootManager gameShootManager;
    public GameShootManager ourBoardManager;
    GameUserManager gameUserManager;

    public static ShipSDClient Instance;

    public void OnWaitForChangeNewBlock()
    {
        GetCurrentStateFromFreshBlock();
    }

    public void OnWaitForChangeTID(PendingStateData latestPendingData)
    {
    }

    public bool SerializedPendingIsDifferent(PendingStateData latestPendingData)
    {
        return false;
    }

    void Start()
    {
        gameUserManager = GetComponent<GameUserManager>();
        Instance = this;    
    }

    public void RegisterForWaitForChange()
    {
        XAYAWaitForChange.Instance.objectsRegisteredForWaitForChange.Add(this);
    }

    public void CreateGameChannel()
    {
        if (GlobalData.IsOpenedChannel())
        {
            gameUserManager.ShowInfo("You already have opened channel.");
            return;
        }

        GameChannelManager.Instance.CreateGameChannel();

        gameUserManager.ShowInfo("CREATE CHANNEL. Please wait...");
    }

    public void JoinGameChannel(string channelId)
    {
        GameChannelManager.Instance.JoinGameChannel(channelId);
        GetComponent<GameUserManager>().ShowInfo("JOIN CHANNEL.(" + GlobalData.GetChannel(channelId).userNames[0] + ")" + ". Please wait...");
    }

    public void CloseGameChannel(string channelId)
    {
        GameChannelManager.Instance.CloseGameChannel(channelId);
        GetComponent<GameUserManager>().ShowInfo("CLOSE CHANNEL. Please wait...");
    }

    public void InitGameboard()
    {
        foreach (Transform tShip in gameUserManager.MyshipObjs.transform)
        {
            tShip.GetComponent<DraggableShip>().InitPos();
        }

        gameShootManager.ClearMarker();
        ourBoardManager.ClearMarker();
        GlobalData.InitGameChannelData();

        //==================  Hide Gameboard  =====================//
        gameUserManager.InitGameBoard();
        //=======================================//

        StopForceChannel();
    }

    public void SetShipPostionSubmit()
    {
        char[] positionstr = new char[64];
        string strPos = "";
        for (int i = 0; i < 64; i++)
            positionstr[i] = '.';

        bool bSetPos = true;

        int[][] matrixShipsIndex = new int[8][];
        for (int i = 0; i < 8; i++)
            matrixShipsIndex[i] = new int[8];
        for (int i = 0; i < 8; i++)
            for (int j = 0; j < 8; j++)
            {
                matrixShipsIndex[i][j] = -1;
            }

        foreach (Transform tShip in GetComponent<GameUserManager>().MyshipObjs.transform)
        {
            if (tShip.GetComponent<DraggableShip>().GetPositions() == null)
            {
                //--------- some ships is not seted position-------------------------//             
                bSetPos = false;
                continue;
                //----------------------------///
            }

            foreach (BattleShip.BLL.Requests.Coordinate c in tShip.GetComponent<DraggableShip>().GetPositions())
            {
                positionstr[(c.YCoordinate - 1) * 8 + (c.XCoordinate - 1)] = 'x';

            }
        }

        if (!bSetPos)
        {
            GetComponent<GameUserManager>().ShowInfo("You must position your ships!");
            return;
        }

        for (int i = 0; i < 64; i++)
            strPos += positionstr[i];
        Debug.Log(GlobalData.gcurrentPlayedChannedId);
        SetPositionRequest(GlobalData.gcurrentPlayedChannedId, strPos);
        GlobalData.gbSumitPosition = true;
        GlobalData.bPlaying = true;
    }

    void SetDisputeStatus(JObject jsonDispute)
    {

        if (jsonDispute != null)
        {
            string jsonString = jsonDispute.ToString();
            DisputeStatus disputeStatus = JsonConvert.DeserializeObject<DisputeStatus>(jsonString);
            GlobalData.disputeStatus = disputeStatus;

            if (!disputeStatus.canresolve)
            {
                if (GlobalData.gbTurn)
                    gameUserManager.ShowInfo("There is dispute!");
                gameUserManager.DisputeDisplay();
            }
            else
            {
                gameUserManager.DisputeDisplay(false);
            }
        }
        else
        {
            if (GlobalData.disputeStatus != null)
                GlobalData.disputeStatus.canresolve = true;
            gameUserManager.DisputeDisplay(false);
        }
    }

    void SetShootStatus(JArray jGuesses)
    {
        if (jGuesses == null && jGuesses.Count < 2) return;

        string playerGuesses1;
        string playerGuesses2;
        gameShootManager.ClearMarker();
        ourBoardManager.ClearMarker();

        if (GlobalData.gPlayerIndex == 1)
        {
            playerGuesses1 = jGuesses[0].ToString();
            playerGuesses2 = jGuesses[1].ToString();
        }
        else
        {
            playerGuesses1 = jGuesses[1].ToString();
            playerGuesses2 = jGuesses[0].ToString();
        }

        for (int i = 0; i < playerGuesses1.Length; i++)
        {
            string ch = playerGuesses1.Substring(i, 1);
            int row = i % 9 + 1;
            int col = i / 9 + 1;
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

    }

    public void SetShootSubmit(Vector2 v)
    {
        string cmdstr = "{\"jsonrpc\":\"2.0\", \"method\":\"shoot\",\"params\":{ \"row\":" + (int)v.x + ", \"column\":" + (int)v.y + "}}";

        RPCRequest request = new RPCRequest();
        request.ChannelXayaReqDirect(cmdstr);
    }

    public void RevealPositionRequest(string channelId)
    {
        string cmdstr = "{\"jsonrpc\":\"2.0\", \"method\":\"revealposition\",\"params\":[]}";

        RPCRequest request = new RPCRequest();
        request.ChannelXayaReqDirect(cmdstr);
    }

    public void SetPositionRequest(string channelId, string strPos)
    {
        string cmdstr = "{\"jsonrpc\":\"2.0\", \"method\":\"setposition\",\"params\":[\"" + strPos + "\"]}";

        RPCRequest request = new RPCRequest();
        request.ChannelXayaReqDirect(cmdstr);
    }

    public void StopForceChannel()
    {
        GlobalData.bOpenedChannel = false;
        GameChannelManager.Instance.KillChannel();
        GlobalData.bLiveChannel = false;
    }

    public bool IsSetPosition()
    {
        Debug.Log(GlobalData.gGameControl.gameMyBoard.GetCurrnetShipIndex());
        return GlobalData.gGameControl.gameMyBoard.GetCurrnetShipIndex() < 7;
    }
    public void DisputeRequest()
    {
        string cmdstr = "{\"jsonrpc\":\"2.0\", \"method\":\"filedispute\", \"id\":0}";

        RPCRequest request = new RPCRequest();
        request.ChannelXayaReqDirect(cmdstr);
    }

    public void SetGameChannelSateFromJson(string channelId, string result)
    {
        JObject jresult = JObject.Parse(result) as JObject;
        jresult = jresult["result"] as JObject;
        Debug.Log(jresult.ToString());

        if (jresult["existsonchain"] != null && (jresult["existsonchain"].ToString() == "false" || jresult["existsonchain"].ToString() == "False"))
        {
            if (!GlobalData.bFinished)
            {
                if (GlobalData.gPlayerIndex == GlobalData.gWinner)
                    GlobalData.ErrorPopup("The game was finished. You have won.");
                else
                    GlobalData.ErrorPopup("The game was finished. You have lost.");

                Debug.Log("beforeInit:" + GlobalData.bLiveChannel);
                InitGameboard();
                Debug.Log("Init game! live:" + GlobalData.bLiveChannel);
                GlobalData.bFinished = true;
            }
            return;
            //}
        }


        JArray participants = jresult["current"]["meta"]["participants"] as JArray;

        //Debug.Log("parti="+participants.ToString());

        if (XAYASettings.playerName == participants[0]["name"].ToString())
        {
            GlobalData.gOpponentName = participants[1]["name"].ToString();
            GlobalData.gPlayerIndex = 0;
        }
        else
        {
            GlobalData.gOpponentName = participants[0]["name"].ToString();
            GlobalData.gPlayerIndex = 1;
        }
        bool bOldTurn = GlobalData.gbTurn;
        //=======================================   Whos turn? ======================//
        if (jresult["current"]["state"]["whoseturn"].ToString() == "1" && XAYASettings.playerName == participants[1]["name"].ToString())
        {
            GlobalData.gbTurn = true;
            GetComponent<GameUserManager>().imgTurnIcon.color = new Color32(0, 0, 255, 255);
        }
        if (jresult["current"]["state"]["whoseturn"].ToString() == "0" && XAYASettings.playerName == participants[0]["name"].ToString())
        {
            GlobalData.gbTurn = true;
            GetComponent<GameUserManager>().imgTurnIcon.color = new Color32(0, 0, 255, 255);
        }
        if (jresult["current"]["state"]["whoseturn"].ToString() == "0" && XAYASettings.playerName == participants[1]["name"].ToString())
        {
            GlobalData.gbTurn = false;
            GetComponent<GameUserManager>().imgTurnIcon.color = new Color32(255, 0, 0, 255);
        }

        if (jresult["current"]["state"]["whoseturn"].ToString() == "1" && XAYASettings.playerName == participants[0]["name"].ToString())
        {
            GlobalData.gbTurn = false;
            GetComponent<GameUserManager>().imgTurnIcon.color = new Color32(255, 0, 0, 255);
        }

        if (jresult["current"]["state"]["whoseturn"].ToString() == null || jresult["current"]["state"]["whoseturn"].ToString() == "" || jresult["current"]["state"]["whoseturn"].ToString() == "NULL")
        {
            GlobalData.gbTurn = false;
            GetComponent<GameUserManager>().imgTurnIcon.color = new Color32(200, 200, 200, 255);
        }

        if (!bOldTurn && GlobalData.gbTurn)
        {
            GetComponent<GameUserManager>().ShowInfo("YOUR TURN!");
        }

        if (jresult["height"] != null)
        {
            GlobalData.gChannelHeight = long.Parse(jresult["height"].ToString());
        }

        //================= ****** Dispute  *****      ============================//
        SetDisputeStatus(jresult["dispute"] as JObject);
        //================= **** Shooting result **** ===================================//
        JObject jParsed = jresult["current"]["state"] as JObject;
        jParsed = jParsed["parsed"] as JObject;
        Debug.Log(jParsed.ToString());
        //if (jParsed == null || jParsed["phase"].ToString()!="shoot") return;
        if (jParsed == null) return;
        JArray jGuesses = jParsed["guesses"] as JArray;
        Debug.Log(jGuesses.ToString());
        if (jGuesses != null) SetShootStatus(jGuesses);

        //============== ******* Winner state ****** =================//
        if (jParsed["winner"] != null && jParsed["phase"].ToString() == "finished")
        {
            GlobalData.gWinner = int.Parse(jParsed["winner"].ToString());
            Debug.Log("Game Finished!");
            string strInfo = "You have won.";
            if (GlobalData.gPlayerIndex != GlobalData.gWinner)
                strInfo = "You have lost.";
            GetComponent<GameUserManager>().ShowInfo("GAME FINISHED! " + strInfo);
        }
    }

    // Start is called before the first frame update
    public void SetUpShipClient()
    {
        GetCurrentStateFromFreshBlock();
        runTime = Time.time;
        RegisterForWaitForChange();
    }

    // Update is called once per frame
    void Update()
    {
        if (XAYASettings.playerLoggedIn)
        {
            if (!bCurrentLive && (Time.time - runTime) > 15 && GetComponent<GameUserManager>() && gspRunnungToggle.isOn)
            {
                Debug.Log("GSP restart!!");
                KillGSP();
                GlobalData.ErrorPopup("GSP failed to start.");
            }
        }
    }

    public void GetCurrentStateFromFreshBlock()
    {
        string currentState = GameChannelManager.Instance.GetCurrentStateOnly();
        retStatusJson ret = JsonConvert.DeserializeObject<retStatusJson>(currentState);

        if (ret != null)
        {
            GlobalData.gblockhash = ret.result.blockhash;
            GlobalData.gblockHeight = ret.result.height;
            GlobalData.gblockStatusStr = ret.result.state;
            SetGameSateFromJson(currentState);
        }
    }

    public void SetGameSateFromJson(string result)
    {
        JObject jresult = JObject.Parse(result) as JObject;

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

            //For now, we simply ignore multiple opened channels/dropped in a middle games
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
                    (channelInfo.userNames[0] != XAYASettings.playerName &&
                    channelInfo.userNames[1] != XAYASettings.playerName))
                    continue;

                string opponentName = channelInfo.userNames[0];
                if (channelInfo.userNames[0] == XAYASettings.playerName)
                    opponentName = channelInfo.userNames[1];
                //=========================    ==============================//                    
                if(!XAYADummyUI.Instance.IsExistName(opponentName))    //=== case  only other user====//
                {
                    Debug.Log("liveFlag:" + GlobalData.bLiveChannel);
                      if (!GlobalData.bLiveChannel && !GlobalData.IsIgnoreChannel(channelInfo.id))
                    //   if (!GlobalData.bLiveChannel)
                        {
                        GlobalData.InitGameChannelData();
                        GlobalData.bOpenedChannel = true;
                        //============= Create  Channe
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
    public void GetCurrentInitialState()
    {
        string currentState = GameChannelManager.Instance.GetCurrentStateOnly();
        retStatusJson ret = JsonConvert.DeserializeObject<retStatusJson>(currentState);

        GlobalData.gblockhash = ret.result.blockhash;
        GlobalData.gblockHeight = ret.result.height;
        GlobalData.gblockStatusStr = ret.result.state;
        bCurrentLive = true;
        SetGameSateFromJson(currentState);
    }

    public bool KillGSP()
    {
        gspRunnungToggle.isOn = false;
        try
        {
            foreach (System.Diagnostics.Process p in System.Diagnostics.Process.GetProcessesByName(XAYASettings.DaemonName))
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

