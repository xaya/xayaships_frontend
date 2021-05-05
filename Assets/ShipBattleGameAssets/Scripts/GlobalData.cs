using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using BattleShip.BLL.GameLogic;
using Newtonsoft.Json;
using XAYA;

public class GlobalData : MonoBehaviour {

    public static bool bPlaying = false;
    public static bool bOpenedChannel = false;
    public static bool bLogin = false;
    public static bool bLiveChannel = false;
    public static bool bFinished = false;
    public static bool bRunonce = false;
    public static string resultJsonStr = "";
    public static GameControl gGameControl = new GameControl();  

    public static string gOpponentName = "";

    public static GameObject gErrorBox;
    public static UnityEngine.UI.Text gErrorText;

    public static string gblockhash;
    public static long gblockHeight;
    public static long gChannelHeight;
    public static string gblockStatusStr;
    public static string gcurrentPlayedChannedId;

    public static bool gbTurn;
    public static bool gbSumitPosition=false;

    public static List<ChannelInfo> ggameChannelList= new List<ChannelInfo>();
    public static List<ChannelInfo> ggameLobbyChannelList = new List<ChannelInfo>();
    public static List<string> ggameIgnoredChannelIDs = new List<string>();


    public static List<LeaderInfo> ggameLeaderList = new List<LeaderInfo>();

    public static int gWinner = -1;

    public static int gPlayerIndex = 0;
    public static DisputeStatus disputeStatus=new DisputeStatus();

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public static void Init()
    {

    }
    public static string GetSaveSettingPath()
    {
        return System.IO.Path.Combine(Application.persistentDataPath, "setting.json");
    }

    public static void ErrorPopup(string errorStr)
    {
        gErrorBox.SetActive(false);
        gErrorBox.SetActive(true);
        gErrorText.text = errorStr;        
    }
    public static int GetFreeChannelPort()
    {
        int portNumber = XAYASettings.gameChannelDefaultPort;
        foreach(ChannelInfo c in ggameChannelList)
        {
            if (c.port > portNumber) portNumber = c.port;
        }
        return ++portNumber;
    }
    public static ChannelInfo GetChannel(string id)
    {
        
        foreach (ChannelInfo c in ggameChannelList)
        {
            if (c.id == id) return c;
        }
        return null;
    }
    public static void InitGameChannelData()
    {
        bPlaying = false;
        gcurrentPlayedChannedId = null;
        gbTurn = false;
        gOpponentName = null;
        gPlayerIndex = 0;
        gWinner = -1;
        disputeStatus = null;
        gGameControl= new GameControl();
        //bOpenedChannel = false;        
    }
    public static void AddChannel(ChannelInfo c)
    {
        //===== add channel in case that channel id do not exist in ignorelist.
        if(!ggameIgnoredChannelIDs.Contains(c.id))
            ggameChannelList.Add(c);
    }
    public static bool IsIgnoreChannel(string strId)
    {
        return ggameIgnoredChannelIDs.Contains(strId);
    }
    public static bool IsOpenedChannel()
    {
        foreach(ChannelInfo c in ggameChannelList)
        {
            foreach(string s in c.userNames)
            {
                //-----------   if  channel that Player created or joined  exists,   ------------------//
                if (s == XAYASettings.playerName) return true;
            }
        }
        return false;
    }
}

public class UserInfo
{
    public string name;
    public string gameCount;
    public string gameWon;    
}
public enum CHANNEL_STATUE
{
    Opened, Playing, Ready, Closed
}
public class ChannelInfo
{
    public string id;
    public string[] userNames;
    public int port;
    public CHANNEL_STATUE status;
    public string statusText;
    public bool bignored;
}
public class LeaderInfo
{
    public string playerName;
    public int playedCount;
    public int winCount; 
}
public class Channel
{
    public string id;
    public ChannelMeta meta;    
}

public class Participant
{
    public string address;
    public string name;
}
public class ChannelMeta
{
    public Participant[] participants;
    public string proto;
    public string reinit;
}

public class CError
{
    public string code { get; set; }
    public string message { get; set; }
}
public class NameRegisterResult
{
    public string result { get; set; }
    public CError error { get; set; }
    public int id { get; set; }
    //public static NameRegisterResult LoadJsonStr(string jsonStr)
    //{
    //    var  rpcResponse = JsonConvert.DeserializeObject<JsonRpcResponse<NameRegisterResult>>(jsonStr);
    //    return rpcResponse;
    //    //return JsonUtility.FromJson<NameRegisterResult>(jsonStr);
    //}
}

