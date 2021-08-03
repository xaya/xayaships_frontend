using CielaSpike;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using XAYAChat;

namespace XAYA
{
    public class XAYADummyUI : MonoBehaviour
    {
        public bool useRegtestMode = false;

        public static XAYADummyUI Instance;

        public GameObject WalletTypeSelectionPanel;
        public GameObject WalletLoadingInformationPanel;
        public GameObject SettingsPanel;
        public GameObject SettingsPanelInner;
        public GameObject UsernameSelectionScreen;
        public GameObject CreateAccountButton;

        public Text WalletLoadingInformationalText;
        public Dropdown usernameSelectionList;

        public GameObject UsernameLoadingInformationPanel;
        public Text UsernameLoadingInformationalText;

        //Lite wallet
        public GameObject liteWalletPanel;
        public GameObject liteWalletButton;
        public GameObject liteWalletInner;

        [HideInInspector]
        public float waitingForNewName = -1.0f;

        /*Advanced mode GSP synch track vars*/
        private bool waitingToSynch = false;
        private float waitingToSynchTimer = -1.0f;

        private RPCRequest request;

        private bool waitingForXIDToSolve = false;
        private bool launchingCharon = false;

        private List<string> existingNamesFiltered;


        public void HideLiteWallet()
        {
            liteWalletButton.SetActive(true);
            liteWalletInner.SetActive(false);
        }

        public void LiteWalletButtonShow()
        {
            liteWalletPanel.SetActive(true);
            liteWalletButton.SetActive(true);
        }

        public void LiteWalletButtonClick()
        {
            liteWalletButton.SetActive(false);
            liteWalletInner.SetActive(true);
        }

        void Start()
        {
            Instance = this;
            request = new RPCRequest();

            if (useRegtestMode == false)
            {
                WalletTypeSelectionPanel.SetActive(true);
                SettingsPanel.SetActive(true);

                if (PlayerPrefs.HasKey("walletMode"))
                {
                    string wm = PlayerPrefs.GetString("walletMode", "");

                    if (wm == "simple")
                    {
                        WalletSimpleModeClick();
                    }

                    if (wm == "advanced")
                    {
                        WalletAdvancedModeClick();
                    }
                }
            }
            else
            {
                WalletTypeSelectionPanel.SetActive(false);
                WalletLoadingInformationPanel.SetActive(true);

                PlayerPrefs.SetString("walletMode", "advanced");
                PlayerPrefs.Save();
            }
        }

        public void Update()
        {
            if(Input.GetKeyUp(KeyCode.F12))
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
                ToastManager.Show("Player prefs erased");
            }

            if(useRegtestMode)
            {
                useRegtestMode = false;
                XAYASettings.isRegtestMode = true;
                AutomaticRegtestRunner.Instance.LaunchDaemon();
            }

            if (waitingToSynch)
            {
                if (waitingToSynchTimer > 0)
                {
                    waitingToSynchTimer -= Time.deltaTime;

                    if (waitingToSynchTimer <= 0)
                    {
                        waitingToSynch = false;
                        waitingToSynchTimer = 1.0f;
                        this.StartCoroutineAsync(WaitForDaemonSynch());
                    }
                }
            }

            if(waitingForXIDToSolve)
            {
                if(XAYAWalletAPI.Instance.xidIsSolved)
                {
                    waitingForXIDToSolve = false;
                    ContinueLogingAfterXIDVerification();
                }
            }

            if(waitingForNewName > 0)
            {
                CreateAccountButton.GetComponent<Button>().interactable = false;

                waitingForNewName -= Time.deltaTime;

                if(waitingForNewName <= 0)
                {
                    int namesInListNow = existingNamesFiltered.Count - 1; //minus 1, as one in the list is empty

                    List<string> existingNames = request.XAYAGetNameList();

                    if (existingNames.Count != namesInListNow)
                    {
                        existingNamesFiltered = new List<string>();
                        existingNamesFiltered.Add("");

                        foreach (string name in existingNames)
                        {
                            if (name != "")
                            {
                                existingNamesFiltered.Add(name.Remove(0, 2));
                            }
                        }

                        usernameSelectionList.ClearOptions();
                        usernameSelectionList.AddOptions(existingNamesFiltered);

                        if (XAYASettings.LoginMode == LoginMode.Advanced)
                        {
                            if (usernameSelectionList.options.Count == 1)
                            {
                                waitingForNewName = 5.0f;
                            }
                        }
                    }
                    else
                    {
                        waitingForNewName = 5.0f;
                    }
                }
            }
            else
            {
                CreateAccountButton.GetComponent<Button>().interactable = true;
            }
        }

        public void WalletSimpleModeClick()
        {
            XAYAWalletAPI.Instance.SetElectrumWallet();
            WalletTypeSelectionPanel.SetActive(false);
            WalletLoadingInformationPanel.SetActive(true);

            LiteWalletButtonShow();

            PlayerPrefs.SetString("walletMode", "simple");
            PlayerPrefs.Save();

            LoadTheWallet();
        }

        public void WalletAdvancedModeClick()
        {
            XAYAWalletAPI.Instance.SetElectronWallet();
            WalletTypeSelectionPanel.SetActive(false);
            WalletLoadingInformationPanel.SetActive(true);
            LoadTheWallet();

            PlayerPrefs.SetString("walletMode", "advanced");
            PlayerPrefs.Save();

        }

        public void SettingsButtonClick()
        {
            SettingsPanelInner.gameObject.SetActive(true);
        }

        public void CloseSettingsInnerPanel()
        {
            SettingsPanelInner.gameObject.SetActive(false);
        }

        void LoadTheWallet()
        {
            XAYASettings.FillAllConnectionSettings();

            if (XAYASettings.LoginMode == LoginMode.Simple)
            {
                StartCoroutine(LaunchDaemonLocally(Application.dataPath, Application.persistentDataPath, Application.isEditor, WalletLoadingInformationalText));
            }
            else
            {
                ConnectionStatusSolver connectionSolver = GameObject.FindObjectOfType<ConnectionStatusSolver>();
                connectionSolver.solverReadyToRun = true;

                WalletLoadingInformationalText.text = "Loading GSP...";
                LanchDaemonIfNotRunningAlready();
                this.StartCoroutineAsync(WaitForDaemonSynch());
            }
        }

        /*Using NINJA here and start it via UPDATE, as had problems with stacking coroutine loops*/

        IEnumerator WaitForDaemonSynch()
        {
            yield return new WaitForSeconds(0.1f);
            RPCRequest r = new RPCRequest();
            string result = r.XAYAGetGameStateNull();

            if (result.Contains("state"))
            {
                JObject resultOBJ = JObject.Parse(result);

                if (resultOBJ["result"]["state"].ToString() == "up-to-date")
                {
                    yield return Ninja.JumpToUnity;

                    if (XAYASettings.isRegtestMode == false)
                    {
                        BringNameSelectionDialog();
                    }
                    else
                    {
                        GamePrelaunchRoutines();
                    }
                }
                else
                {
                    int currentGspBlockSynched = 0;
                    int.TryParse(resultOBJ["result"]["height"].ToString(), out currentGspBlockSynched);
                    WaitForTheDaemonToSync();
                    yield return Ninja.JumpToUnity;
                    WalletLoadingInformationalText.text = "Synching GSP, current block: " + currentGspBlockSynched;
                }
            }
            else if (result == "")
            {
                yield return Ninja.JumpToUnity;

                if (XAYASettings.isRegtestMode == false)
                {
                    XAYASettings.FillAllConnectionSettings();
                    LanchDaemonIfNotRunningAlready();
                }

                WaitForTheDaemonToSync();
            }
            else
            {
                yield return Ninja.JumpToUnity;

                if (XAYASettings.isRegtestMode == false)
                {
                    XAYASettings.FillAllConnectionSettings();
                }

                LanchDaemonIfNotRunningAlready();
                WaitForTheDaemonToSync();
            }
        }

        public void WaitForTheDaemonToSync()
        {
            waitingToSynch = true;
            waitingToSynchTimer = 1.0f;
        }

        IEnumerator ConfirmElectrumState()
        {
            ConnectionStatusSolver connectionSolver = GameObject.FindObjectOfType<ConnectionStatusSolver>();
            connectionSolver.electrumAllowsToTest = true;
            connectionSolver.solverReadyToRun = true;

            bool electrumNotSolved = true;

            while (electrumNotSolved)
            {
                if (connectionSolver.walletSolved)
                {
                    electrumNotSolved = false;
                }

                yield return new WaitForSeconds(0.25f);
            }

            BringNameSelectionDialog();
            yield return null;
        }

        public bool IsExistName(string namestr)
        {
            return existingNamesFiltered.Contains(namestr);
        }

        void BringNameSelectionDialog()
        {
            WalletLoadingInformationPanel.SetActive(false);
            UsernameSelectionScreen.SetActive(true);

            List<string> existingNames = request.XAYAGetNameList();
            existingNamesFiltered = new List<string>();
            existingNamesFiltered.Add("");

            foreach (string name in existingNames)
            {
                if (name != "")
                {
                    existingNamesFiltered.Add(name.Remove(0, 2));
                }
            }

            usernameSelectionList.ClearOptions();
            usernameSelectionList.AddOptions(existingNamesFiltered);

            if(XAYASettings.LoginMode == LoginMode.Advanced)
            {
                if(usernameSelectionList.options.Count == 1)
                {
                    waitingForNewName = 5.0f;
                }
            }
        }

        private void GamePrelaunchRoutines()
        {
            if (XAYASettings.launchXAMPPbroadcastService == true)
            {
                XAYAWalletAPI.Instance.LaunchXMPPServer();
            }

            if (XAYASettings.launchChat == true)
            {
                UIManagerChat.Instance.LaunchChat();
            }

            UsernameSelectionScreen.SetActive(false);
            UsernameLoadingInformationPanel.SetActive(false);

            //This code is only relevant to XAYAHIPS, commented for other games
            GlobalData.bLogin = true;
            GameUserManager.Instance.UserManagerStart();
            ShipSDClient.Instance.SetUpShipClient();

            XAYAWaitForChange.Instance.StartRunning(true, true, true);
            XAYAWaitForChange.Instance.readyToAcceptPendingCalls = true;

        }

        public void ContinueLogingAfterXIDVerification()
        {
            if (XAYASettings.LoginMode == LoginMode.Advanced)
            {
                GamePrelaunchRoutines();
            }
            else
            {
                StartCoroutine(LaunchCharon());
            }
        }

        public void OnExistingNameSelection(int nameIndex)
        {
            if (usernameSelectionList.options[nameIndex].text != "")
            {
                UsernameLoadingInformationPanel.SetActive(true);
                XAYASettings.playerName = usernameSelectionList.options[nameIndex].text;
                StartCoroutine(XAYAWalletAPI.Instance.EnsureXIDIsRegistered(UsernameLoadingInformationalText));
                waitingForXIDToSolve = true;
            }
        }

        
        IEnumerator LaunchCharon()
        {
            if (launchingCharon)
            {

            }
            else
            {
                UsernameLoadingInformationalText.text = "Launching Charon...";
                launchingCharon = true;
                StartCoroutine(XAYAWalletAPI.Instance.LaunchCharon(XAYASettings.playerName, (myReturnValue) =>
                {
                    if (myReturnValue.Contains("Error"))
                    {
                        LaunchingCharonError(myReturnValue);
                    }

                    if (myReturnValue == "WAITING TO SOLVE GSP")
                    {
                        StartCoroutine(WaitForGSPToSolve());
                    }
                }
                ));
            }

            yield return null;
        }

        IEnumerator WaitForGSPToSolve()
        {
            UsernameLoadingInformationalText.text = "Waiting Charon...";
            ConnectionStatusSolver ss = GameObject.FindObjectOfType<ConnectionStatusSolver>();
            ss.charonAllowsToTest = true;

            while (ss.gspSolved == false)
            {
                //Lets additionally test, if charon client died for whatever reason. This could be certificate error,
                //and then we can heuristially try and reauthenticate XID

                if(XAYAWalletAPI.Instance.myProcessDaemonCharon.HasExited)
                {
                    UsernameLoadingInformationalText.text = "Charon client failed, trying to re-authenticate...";
                    StartCoroutine(XAYAWalletAPI.Instance.EnsureXIDIsRegistered(UsernameLoadingInformationalText, true));
                    break;
                }

                yield return null;
            }

            if (ss.gspSolved == true)
            {
                GamePrelaunchRoutines();
            }
        }

        public void LanchDaemonIfNotRunningAlready()
        {
            Process[] pname = Process.GetProcessesByName(XAYASettings.DaemonName);

            if (pname.Length != 0)
            {
                return;
            }

            StartCoroutine(LaunchDaemonLocally(Application.dataPath, Application.persistentDataPath, Application.isEditor, WalletLoadingInformationalText));
        }

        IEnumerator LaunchDaemonLocally(string path, string persistent, bool isEditor, Text loadingText)
        {
            StartCoroutine(XAYAWalletAPI.Instance.LaunchDaemonLocally(path, persistent, isEditor, (myReturnValue) =>
            {
                if (myReturnValue == "smcd launching")
                {
                    UpdateProgress(1.0f, XAYASettings.DaemonName + " launching...");
                }

                if (myReturnValue == "Starting...")
                {
                    UpdateProgress(0, "Starting...");
                }

                if (myReturnValue == "Launching XID...")
                {
                    UpdateProgress(0.01f, "Launching XID...");
                    loadingText.text = "Launching XID...";
                }

                if (myReturnValue == "Executing exe files...")
                {
                    UpdateProgress(0.1f, "Executing exe files...");
                    loadingText.text = "Executing exe files...";
                }

                if (myReturnValue == "Connecting to electrum network...")
                {
                    UpdateProgress(0.3f, "Connecting to electrum network...");
                    loadingText.text = "Connecting to electrum network...";
                }

                if (myReturnValue == "loading-wallet")
                {
                    UpdateProgress(0.4f, "loading-wallet" + "...");
                    loadingText.text = "loading-wallet" + "...";
                }

                if (myReturnValue.Contains("Synching, current header is at: "))
                {
                    UpdateProgress(0.6f, myReturnValue);
                    loadingText.text = myReturnValue;
                }

                if (myReturnValue == "Confirming Electrum...")
                {
                    UpdateProgress(0.9f, "Confirming Electrum...");
                    loadingText.text = "Confirming Electrum...";
                    StartCoroutine(ConfirmElectrumState());
                }

                if (myReturnValue.Contains("Error: "))
                {
                    LoadingWalletError(myReturnValue);
                }
            }
            ));

            yield return null;
        }

        /*Ideally here we hook into some UI to give proper feddback*/
        void UpdateProgress(float progress, string text)
        {
            UnityEngine.Debug.Log(progress + ":" + text);
        }

        /*Here goes proper error handling, which might restart cycle,
         * or wait for additional input, e.t.c*/
        void LoadingWalletError(string error)
        {
            UnityEngine.Debug.LogError(error);
        }

        void LaunchingCharonError(string error)
        {
            UnityEngine.Debug.LogError(error);
        }

        void NameRegistrationError(string error)
        {
            UnityEngine.Debug.LogError(error);
        }
    }
}
