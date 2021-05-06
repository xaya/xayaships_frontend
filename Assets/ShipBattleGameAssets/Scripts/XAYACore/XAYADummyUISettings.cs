using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace XAYA
{
    public class XAYADummyUISettings : MonoBehaviour
    {
        public XAYADummyUI dummyUI;

        public InputField WalletIPAddress;
        public InputField WalletPort;
        public InputField WalletUserName;
        public InputField WalletPassword;

        public InputField GSPIPAddress;
        public InputField GSPPort;

        public GameObject buttonSimpleMode;
        public GameObject buttonAdvancedMode;


        void OnEnable()
        {
            string dataPrefix = "";

            if (XAYASettings.LoginMode == LoginMode.Simple)
            {
                dataPrefix = XAYASettings.gameID + "electrum";

                buttonSimpleMode.SetActive(false);
                buttonAdvancedMode.SetActive(true);
            }
            else
            {
                dataPrefix = XAYASettings.gameID + "advanced";

                buttonSimpleMode.SetActive(true);
                buttonAdvancedMode.SetActive(false);
            }

            WalletIPAddress.text = PlayerPrefs.GetString(dataPrefix + "_ip", "127.0.0.1");
            WalletPort.text = PlayerPrefs.GetString(dataPrefix + "_port", "8396");
            WalletUserName.text = PlayerPrefs.GetString(dataPrefix + "_username", "");
            WalletPassword.text = PlayerPrefs.GetString(dataPrefix + "_password", "");

            GSPIPAddress.text = PlayerPrefs.GetString(dataPrefix + "_ipGSP", "127.0.0.1");
            GSPPort.text = PlayerPrefs.GetString(dataPrefix + "_portGSP", "8610");
        }

        public void ApplyAndClose()
        {
            string dataPrefix = "";

            if (XAYASettings.LoginMode == LoginMode.Simple)
            {
                dataPrefix = XAYASettings.gameID + "electrum";
            }
            else
            {
                dataPrefix = XAYASettings.gameID + "advanced";
            }

            PlayerPrefs.SetString(dataPrefix + "_ip", WalletIPAddress.text);
            PlayerPrefs.SetString(dataPrefix + "_port", WalletPort.text);
            PlayerPrefs.SetString(dataPrefix + "_username", WalletUserName.text);
            PlayerPrefs.SetString(dataPrefix + "_password", WalletPassword.text);
            PlayerPrefs.SetString(dataPrefix + "_ipGSP", GSPIPAddress.text);
            PlayerPrefs.SetString(dataPrefix + "_portGSP", GSPPort.text);
            PlayerPrefs.Save();

            dummyUI.CloseSettingsInnerPanel();
        }

        public void Close()
        {
            dummyUI.CloseSettingsInnerPanel();
        }

        public void SwitchWalletMode()
        {
            if(XAYASettings.LoginMode == LoginMode.Simple)
            {
                PlayerPrefs.SetString("walletMode", "advanced");
                PlayerPrefs.Save();

                XAYAWalletAPI.Instance.RestartGameCompletely(0);
            }

            if (XAYASettings.LoginMode == LoginMode.Advanced)
            {
                PlayerPrefs.SetString("walletMode", "simple");
                PlayerPrefs.Save();

                XAYAWalletAPI.Instance.RestartGameCompletely(0);
            }
        }
    }
}
