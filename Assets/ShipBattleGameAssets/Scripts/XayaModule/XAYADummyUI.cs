using CielaSpike;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

namespace XAYA
{
    public class XAYADummyUI : MonoBehaviour
    {
        public GameObject WalletTypeSelectionPanel;
        public GameObject WalletLoadingInformationPanel;
        public GameObject SettingsPanel;
        public GameObject SettingsPanelInner;
        public GameObject UsernameSelectionScreen;

        public Text WalletLoadingInformationalText;
        public Dropdown usernameSelectionList;

        public GameObject UsernameLoadingInformationPanel;
        public Text UsernameLoadingInformationalText;

        /*Advanced mode GSP synch track vars*/
        private bool waitingToSynch = false;
        private float waitingToSynchTimer = -1.0f;

        private RPCRequest request;

        private bool waitingForXIDToSolve = false;

        private List<string> existingNamesFiltered;
        public static XAYADummyUI Instance;

        void Start()
        {
            Instance = this;
            request = new RPCRequest();
            WalletTypeSelectionPanel.SetActive(true);
            SettingsPanel.SetActive(true);

            if(PlayerPrefs.HasKey("walletMode"))
            {
                string wm = PlayerPrefs.GetString("walletMode", "");

                if(wm == "simple")
                {
                    WalletSimpleModeClick();
                }

                if(wm == "advanced")
                {
                    WalletAdvancedModeClick();
                }
            }
        }

        public void Update()
        {
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
        }

        public void WalletSimpleModeClick()
        {
            XAYAWalletAPI.Instance.SetElectrumWallet();
            WalletTypeSelectionPanel.SetActive(false);
            WalletLoadingInformationPanel.SetActive(true);

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
                    BringNameSelectionDialog();
                }
                else
                {
                    int currentGspBlockSynched = 0;
                    int.TryParse(resultOBJ["result"]["height"].ToString(), out currentGspBlockSynched);
                    waitingToSynch = true;
                    waitingToSynchTimer = 1.0f;
                    yield return Ninja.JumpToUnity;
                    WalletLoadingInformationalText.text = "Synching GSP, current block: " + currentGspBlockSynched;
                }
            }
            else if (result == "")
            {
                yield return Ninja.JumpToUnity;
                XAYASettings.FillAllConnectionSettings();
                LanchDaemonIfNotRunningAlready();
                waitingToSynch = true;
                waitingToSynchTimer = 1.0f;
            }
            else
            {
                yield return Ninja.JumpToUnity;
                XAYASettings.FillAllConnectionSettings();
                LanchDaemonIfNotRunningAlready();
                waitingToSynch = true;
                waitingToSynchTimer = 1.0f;
            }
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
            SettingsPanel.SetActive(false);
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
        }

        public void ContinueLogingAfterXIDVerification()
        {
            if (XAYASettings.LoginMode == LoginMode.Advanced)
            {
                if (XAYASettings.launchXAMPPbroadcastService == true)
                {
                    XAYAWalletAPI.Instance.LaunchXMPPServer();
                }

                if(XAYASettings.launchChat == true)
                {
                    UIManagerChat.Instance.LaunchChat();
                }

                UsernameSelectionScreen.SetActive(false);
                UsernameLoadingInformationPanel.SetActive(false);
                GlobalData.bLogin = true;

                ShipSDClient.Instance.SetUpShipClient();
            }
            else
            {
                //StartCoroutine(LaunchCharon());
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

        /*
        IEnumerator LaunchCharon()
        {
            if (launchingCharon)
            {

            }
            else
            {
                loadingText.text = "Launching Charon...";
                launchingCharon = true;
                StartCoroutine(XAYAWalletAPI.Instance.LaunchCharon(usernameHardcoded, (myReturnValue) =>
                {
                    if (myReturnValue.Contains("Error"))
                    {
                        LaunchingCharonError(myReturnValue);
                    }

                    if (myReturnValue == "NAME NOT REGISTERED")
                    {
                        chatNameIsRegistered = false;
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
            loadingText.text = "Waiting Charon...";
            ConnectionStatusSolver ss = GameObject.FindObjectOfType<ConnectionStatusSolver>();
            ss.charonAllowsToTest = true;

            while (ss.gspSolved == false)
            {
                yield return null;
            }

            LaunchMainGameLoadCycle();
        }*/

        /*

        void LaunchMainGameLoadCycle()
        {
            //All done mostly, but lets see, if our name properly existed with XID

            if (chatNameIsRegistered == false)
            {
                StartCoroutine(TestIfXIDNamePresent());
            }
            else
            {
                Done();
            }
        }

        void Done()
        {
            //We can start calling the game RPCs now to fetch game data and go on
            loadingText.text = "Done.";

            if (XAYASettings.isElectrum)
            {
                dummyWallet.gameObject.SetActive(true);
            }
        }*/

        void LanchDaemonIfNotRunningAlready()
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
