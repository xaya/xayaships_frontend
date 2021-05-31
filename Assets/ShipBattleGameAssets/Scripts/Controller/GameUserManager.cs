using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
//using System.Diagnostics;

using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.Text;
using System.Diagnostics;
using XAYA;

public class PlayerXIDSIgner
{
    [JsonProperty("addresses")]
    public List<string> addresses { get; set; }
}

public class PlayerXIDData
{
    [JsonProperty("addresses")]
    public Dictionary<string, string> addresses { get; set; }

    [JsonProperty("name")]
    public string name { get; set; }

    [JsonProperty("signers")]
    public List<PlayerXIDSIgner> signers { get; set; }
}

public class PlayerXIDResult
{
    [JsonProperty("blockhash")]
    public string blockhash { get; set; }

    [JsonProperty("chain")]
    public string chain { get; set; }

    [JsonProperty("gameid")]
    public string gameid { get; set; }

    [JsonProperty("state")]
    public string state { get; set; }

    [JsonProperty("height")]
    public int height { get; set; }

    [JsonProperty("data")]
    public PlayerXIDData data { get; set; }

}

public class PlayerXID
{
    [JsonProperty("id")]
    public int id { get; set; }

    [JsonProperty("jsonrpc")]
    public string jsonrpc { get; set; }

    [JsonProperty("result")]
    public PlayerXIDResult result { get; set; }
}

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

public class GameUserManager : MonoBehaviour
{
    [SerializeField]
    UnityEngine.UI.Text m_uiBalanceText = null;
    [SerializeField]
    GameObject m_uiLeaderBoardScrollView;
    [SerializeField]
    GameObject m_uiLeaderBoardPanelPrfab;
    [SerializeField]
    GameObject m_gamePlayboard;

    [SerializeField]
    InputField inputXayaURL;
    [SerializeField]
    InputField inputRpcUserName;
    [SerializeField]
    InputField inputRpcUserPassword;
    [SerializeField]
    InputField inputXayashipsGSPIP;
    [SerializeField]
    GameObject startUI;
    [SerializeField]
    GameObject userSelectUI;
    [SerializeField]
    Text errorText;
    [SerializeField]
    InputField inputGSPIP;

    [SerializeField]
    Dropdown userSelectDropdown;
    [SerializeField]
    InputField inputBalance;
    [SerializeField]
    Text newUserNameText;
    [SerializeField]
    GameObject waitingPanel;

    [SerializeField]
     Toggle checkLocalGSP;
    [SerializeField]
    GameObject errorPopup;
    ShipSDClient shipsdClient;

    public GameObject MyshipObjs;
    public GameObject OpponentShips;

    public Image imgTurnIcon;

    [SerializeField]
    Text txtPlayerName;
    [SerializeField]
    Text txtOpponentName;

    [SerializeField]
    GameObject messageInform;

    [SerializeField]
    GameObject menuObjects;

    [SerializeField]
    GameObject errorCloseBtn;
    [SerializeField]
    GameObject disputeGroup;

    public static GameUserManager Instance;


    private Process myProcessDaemonCharon;
    private bool runAsLiteMode = false;

    public bool runLocalCharonTestServer = true;

    Process electrumDaemon;
    Process xidLightDaemon;

    RPCRequest request;

    private void OnDestroy()
    {
        if(myProcessDaemonCharon != null)
        {
            myProcessDaemonCharon.Kill();
        }
    }

    public void SetRunAsLiteMode(bool newVal)
    {
        runAsLiteMode = newVal;
    }

    void Start()
    {
        Instance = this;
    }

    public void UserManagerStart()
    {
        request = new RPCRequest();
        shipsdClient = GetComponent<ShipSDClient>();
        GlobalData.gErrorBox = errorPopup;
        GlobalData.gErrorText= errorText;
    }
    
    public void CreateLeaderPannel(KeyValuePair<string, string> userInfo)
    {
        //int index =  .Count;
        GameObject onePanel = null;
        onePanel = Instantiate(m_uiLeaderBoardPanelPrfab, new Vector3(0, 0, 0), Quaternion.identity);
        onePanel.transform.Find("no").GetComponent<UnityEngine.UI.Text>().text = (m_uiLeaderBoardScrollView.transform.childCount + 1).ToString();
        onePanel.transform.Find("playerName").GetComponent<UnityEngine.UI.Text>().text = userInfo.Key.Substring(2);
        onePanel.transform.parent = m_uiLeaderBoardScrollView.transform;        
        onePanel.transform.localScale = new Vector3(1, 1, 1);
    }

    // Update is called once per frame
    void Update()
    {
        if (txtPlayerName != null && XAYASettings.playerName != null && XAYASettings.playerName.Length > 2) txtPlayerName.text = XAYASettings.playerName;
        if (txtOpponentName != null  && GlobalData.gOpponentName!=null && GlobalData.gOpponentName.Length>2)
            txtOpponentName.text = GlobalData.gOpponentName;
    }


    public void OnConnectClick()
    {
        //Lite mode?

        if (runAsLiteMode)
        {
            // run electrum
            //StartCoroutine(LaunchElectrum());

        }
        else
        {
            // Check Running Xaya Server ===========================//

            /*
            if (ConnectClient())
            {
                UnityEngine.Debug.Log("Connection OK");
                startUI.SetActive(false);
                userSelectUI.SetActive(true);
                //shipsdClient.GetCurrentStateAndWaiting();

                GlobalData.gSettingInfo.SaveToJson();

                //GetComponent<GameChannelManager>().KillIsChannel();
            }
            else
            {
                GlobalData.ErrorPopup("Xaya Server is not running!");
            }*/
        }
    }

    public void DisplayConnectionUI()
    {
        startUI.SetActive(true);
    }

    public void OnGoBtn()
    {
        //StartCoroutine(TestIfXIDNamePresent());
    }

    public void OnSubmitPositions()
    {      
        if (GlobalData.bPlaying)
        {
            ShowInfo("You already submited ship's positions.");
        }
        
        if (GlobalData.gGameControl.gameMyBoard.CountOfShips()< 7)
        {
            ShowInfo("Some ships do not point!\nYou can't submit ship's postitions.");
            return;
        }
           
       bool bValidate=GlobalData.gGameControl.gameMyBoard.ValidatePositions();
        if(!bValidate)
        {
            ShowInfo("Positions of ships do not validate!\nYou can't submit ship's postitions.");
            return;
        }

        ShipSDClient.Instance.SetShipPostionSubmit();
    }

    public void StartGameByChannel(string channelId)
    {
        UnityEngine.Debug.Log(GlobalData.bOpenedChannel);

        GlobalData.gcurrentPlayedChannedId = channelId;
        GetComponent<GameChannelManager>().RunChannelService(channelId);
        ShipSDClient.Instance.GetCurrentInitialState();

        m_gamePlayboard.SetActive(false);
        m_gamePlayboard.SetActive(true);        
        ShowInfo("START GAME.\n ARRANGE YOUR SHIPS!");
    }
  
    public void InitGameBoard()
    {
        m_gamePlayboard.SetActive(false);        
    } 
    public void InitMenu()
    {
        if(menuObjects)
        {
            foreach(Transform t in menuObjects.transform)
            {
                t.GetChild(0).GetComponent<UpDownMenuBtn>().OnMenubodyHide();
            }
        }
    }
    public void ShowError(string errorStr)
    {        
        //errorText.text = errorStr;
    }

    public void ShowInfo(string messageStr)
    {
        if (messageInform)
        {
            messageInform.GetComponent<Text>().text = messageStr;
            messageInform.SetActive(false);
            messageInform.SetActive(true);
        }
    }

    public void DisputeDisplay(bool bShow=true)
    {
        if (bShow)
        {
            disputeGroup.SetActive(false);
            disputeGroup.SetActive(true);
        }
        else
            disputeGroup.SetActive(false);
    }
    public void OnDisputeBtn()
    {
        ShowInfo("You started a dispute!");
        ShipSDClient.Instance.DisputeRequest();
    }
}
