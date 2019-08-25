using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using BitcoinLib.RPC.RequestResponse;
//using System.Diagnostics;

using UnityEngine.UI;

public class GameUserManager : MonoBehaviour
{
    // Start is called before the first frame update
    List<string> m_userNameList;
    Dictionary<string, string> m_userNameAndValues;
    XAYAClient xayaClient;
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

    //[SerializeField]
    //CreateChannelBtn m_channelPannelManager;

    [SerializeField]
    GameObject menuObjects;

    [SerializeField]
    GameObject errorCloseBtn;
    [SerializeField]
    GameObject disputeGroup;

    void Start()
    {
        xayaClient = GetComponent<XAYAClient>();
        shipsdClient = GetComponent<ShipSDClient>();
        GlobalData.gErrorBox = errorPopup;
        GlobalData.gErrorText= errorText;
        GlobalData.gSettingInfo = SettingInfo.getSettingFromJson();
        //============ get user info from cookie ========================//
        Debug.Log(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData));
        if (File.Exists(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "\\xaya\\.cookie"))
        {
            string cookieStr = File.ReadAllText(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "\\xaya\\.cookie");
            string[] userInfo = cookieStr.Split(':');
            if(userInfo!=null && userInfo.Length>1)
            {
                GlobalData.gSettingInfo.rpcUserName = userInfo[0];
                GlobalData.gSettingInfo.rpcUserPassword = userInfo[1];
            }
        }
        //===============================================================//

        inputXayaURL.text = GlobalData.gSettingInfo.xayaURL;
        inputRpcUserName.text = GlobalData.gSettingInfo.rpcUserName;
        inputRpcUserPassword.text = GlobalData.gSettingInfo.rpcUserPassword;
        inputGSPIP.text = GlobalData.gSettingInfo.GSPIP;

        //---------------------------------------------//
        if(shipsdClient.IsRunningGSPServer())
        {
            checkLocalGSP.isOn = true;
            checkLocalGSP.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
        }
        
        //------------- check running xayad-------------------------//
/*        
        bool running = false;
        
            try
            {
            foreach (System.Diagnostics.Process p in System.Diagnostics.Process.GetProcessesByName("xayad"))
            {
                running = true;
            }
#if UNITY_STANDALONE_LINUX
            foreach (System.Diagnostics.Process p in System.Diagnostics.Process.GetProcessesByName("xaya-qt"))
            {
                running = true;
            }
            foreach (System.Diagnostics.Process p in System.Diagnostics.Process.GetProcessesByName("./xaya-qt"))
            {
                running = true;
            }
#endif  
        }
        catch
        {
          

        }
       
        if (!running)
        {
            GlobalData.ErrorPopup("Xaya Service is not running.\nYou must run xaya and restart application.");
            errorCloseBtn.GetComponent<Button>().onClick.AddListener(delegate {
                UnityEngine.SceneManagement.SceneManager.UnloadScene(0);
                Application.Quit();
            });
            //new WaitForSeconds(2);
            
        }
        */
        //----------------------------------------------------------//
        //---------------- Kill live Channel-----------------------------//
        GetComponent<GameChannelManager>().KillIsChannel();
        
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
        if (txtPlayerName != null && GlobalData.gPlayerName != null && GlobalData.gPlayerName.Length > 2) txtPlayerName.text = GlobalData.gPlayerName.Substring(2);
        if (txtOpponentName != null  && GlobalData.gOpponentName!=null && GlobalData.gOpponentName.Length>2)
            txtOpponentName.text = GlobalData.gOpponentName;
    }
    //------------------Submit buttion Clicking--------------//
    public void UpdateSettingInfo()
    {
        GlobalData.gSettingInfo.xayaURL = inputXayaURL.text;
        GlobalData.gSettingInfo.rpcUserName = inputRpcUserName.text;
        GlobalData.gSettingInfo.rpcUserPassword = inputRpcUserPassword.text;
        GlobalData.gSettingInfo.GSPIP = inputGSPIP.text ;

    }
    public void OnConnectClick()
    {
        GlobalData.gSettingInfo.xayaURL = inputXayaURL.text;
        GlobalData.gSettingInfo.rpcUserName =inputRpcUserName.text;
        GlobalData.gSettingInfo.rpcUserPassword = inputRpcUserPassword.text;

        // Check Running Xaya Server ===========================//

        if (ConnectClient())
        {
            Debug.Log("Connection OK");
            startUI.SetActive(false);
            userSelectUI.SetActive(true);
            //shipsdClient.GetCurrentStateAndWaiting();

            GlobalData.gSettingInfo.SaveToJson();

            //GetComponent<GameChannelManager>().KillIsChannel();
        }
        else
        {
            GlobalData.ErrorPopup("Xaya Server is not running!");
        }
    }
    public void DisplayConnectionUI()
    {
        startUI.SetActive(true);
    }

    public void OnGoBtn()
    {
        GlobalData.gPlayerName = userSelectDropdown.captionText.text;
        m_uiBalanceText.text = inputBalance.text;
        GameChannelManager channelManager = GetComponent<GameChannelManager>();        

        userSelectUI.SetActive(false);
        GlobalData.bLogin = true;       
                
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
        Debug.Log(bValidate);
        //GlobalData.ErrorPopup(bValidate.ToString());

        GetComponent<GameChannelManager>().SetShipPostionSubmit();
    }

    public void OnSubmitBtn()
    {

        string newUserName = newUserNameText.text;
        string errorStr = "";
        bool bError = false;
        //if (m_userNameList.Contains(newUserName))
        //{
        //    bError = true;         
        //    errorStr = "this name exists already.";
        //}
        if(!XAYABitcoinLib.Utils.IsValidName(newUserName,"p/"))
        {
            bError = true;
            errorStr = "User name is invalid. Namespace of user don't consist of lower-case letters only or can not exist.";
        }

        if (bError)
        {
            //errorText.text = errorStr;
            //errorText.gameObject.SetActive(false);
            //errorText.transform.gameObject.SetActive(true);
            GlobalData.ErrorPopup(errorStr);
            return;

        }
        string responsestr = xayaClient.RegisterUserName(newUserName);
        var responseObject = new NameRegisterResult();
        
        JsonConvert.PopulateObject(responsestr, responseObject);
        Debug.Log(responsestr);

        
        if(responseObject.error!=null)
        {
            //errorText.text = responseObject.error.message;
            //errorText.transform.gameObject.SetActive(true);
            GlobalData.ErrorPopup(responseObject.error.message);
            return;
        }

        StartCoroutine(waitCreatingName(newUserName));

        //string cmdstr="{\"method\":\"name_register\",\"params\":[\""+ newUserName +"\",\"{}\"]}" ;
        /*
        StartCoroutine(xayaRpcCommand(cmdstr, (status)=> {
            //NameRegisterResult result = NameRegisterResult.LoadJsonStr(status);
            JsonRpcResponse<string> rpcResponse = JsonConvert.DeserializeObject<JsonRpcResponse<string>>(status);

            //Debug.Log(rpcResponse.Result +",  errormessage:"+ rpcResponse.Error.Message);
        }));
        */
        //FillNameList();
        //xayaClient.na
    }
    private string status;
    IEnumerator waitCreatingName(string newName)
    {
        waitingPanel.SetActive(true);
        while (!m_userNameList.Contains(newName))
        {
            yield return new WaitForSeconds(0.1f);
            m_userNameList= xayaClient.GetNameList();            
        }
        userSelectDropdown.ClearOptions();
        userSelectDropdown.AddOptions(m_userNameList);
        waitingPanel.SetActive(false);
        
    }

    public void StartGameByChannel(string channelId)
    {

        Debug.Log(GlobalData.bOpenedChannel);
        //if (GlobalData.bOpenedChannel) return;
        GlobalData.gcurrentPlayedChannedId = channelId;

        GetComponent<GameChannelManager>().RunChannelService(channelId);
        //WaitForSeconds w= new WaitForSeconds(2)
               
        GetComponent<GameChannelManager>().StartChannelWaiting(channelId);

        m_gamePlayboard.SetActive(false);
        m_gamePlayboard.SetActive(true);        
        ShowInfo("START GAME.\n ARRANGE YOUR SHIPS!");

    }
    private IEnumerator xayaRpcCommand(string requestJsonStr, Action<string> callback)
    {
        //string resultJsonStr = null;
        string tempStr = "";
        //Debug.Log(GlobalData.gSettingInfo.GetServerUrl());
        //Debug.Log(requestJsonStr);
        UnityWebRequest www = UnityWebRequest.Put(GlobalData.gSettingInfo.GetServerUrl(), requestJsonStr);
        //UnityWebRequest www = UnityWebRequest.Put("http://admin:admin123$@127.0.0.1:8396/wallet/game.dat", m_text.text);

        www.method = UnityWebRequest.kHttpVerbPOST;
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Accept", "application/json");
        www.downloadHandler = new DownloadHandlerBuffer();
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
            tempStr = www.error;
        }
        else
        {
            //resultJsonStr = www.downloadHandler.text;
            GlobalData.resultJsonStr = www.downloadHandler.text;
            tempStr = www.downloadHandler.text;
            Debug.Log(www.downloadHandler.text);
        }
        callback(tempStr);
    }
    public bool ConnectClient()
    {
        if (!xayaClient.connected)
        {
            try
            {
                if (xayaClient.Connect())
                {
                    FillNameList();
                    inputBalance.text= xayaClient.GetBalance();
                    //FillNameAndValues();
                    return true;
                }
            }
            catch (Exception e)
            {
                
                ShowError(e.ToString());
                return false;
            }
        }
        else
        {
            try
            {
                //if ()
                //{
                    
                //}
                //else
                //{
                //    ShowError("No name selected.");
                //}
            }
            catch (Exception e)
            {
                //return false;
                ShowError(e.ToString());
            }
        }
        return false;
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
    public void FillNameList()
    {

        //inputBalance.text = xayaClient.GetBalance();
        m_userNameList = xayaClient.GetNameList();

        userSelectDropdown.ClearOptions();
        userSelectDropdown.AddOptions(m_userNameList);

        if (m_userNameList.Count > 0 )
        {
            GlobalData.gPlayerName = m_userNameList[0];
        }
        

    }
    public void FillNameAndValues()
    {
        m_userNameAndValues = xayaClient.GetNameAndValues();
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

    public bool IsExistName(string namestr)
    {
        return m_userNameList.Contains(namestr);
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
        GetComponent<GameChannelManager>().DisputeRequest();
    }
}
