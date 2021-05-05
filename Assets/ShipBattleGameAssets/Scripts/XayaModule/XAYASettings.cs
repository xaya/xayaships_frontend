using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XAYA
{
    /* Simple mode runs using electrum wallet, advanced runs using default XAYA electron wallet*/
    public enum LoginMode { Simple, Advanced };

    public class XAYASettings
    {
        public static LoginMode LoginMode = LoginMode.Simple;
        public static string gameID = "xs"; //xs goes for Xayaships
        public static string DaemonName = "shipsd"; // exe file name of game daemon
        public static string rpcCommandJsonFile = "channel.json";

        public static bool launchXAMPPbroadcastService = true; //we use this for games with gamechannels
        public static string XAMPPbroadcastURL = "127.0.0.1";
        public static string XAMPPbroadcastPORT = "10042";
        public static int gameChannelDefaultPort = 29060;

        public static bool ignoreChatDebugLog = true;

        public static bool launchChat = true; //true if we want dummy UI to also launch chat

        [HideInInspector]
        public static string GameServerAddress = ""; //game's GSP address 

        [HideInInspector]
        public static string WalletServerAddress = ""; //electron or electrum wallet address

        [HideInInspector]
        public static string XIDServerAddress = ""; //address of XID, which is either integrated into electron, or xid-light running alongside electrum.exe

        public static string ElectronWalletIPAddress, ElectronWalletPort, ElectronWalletUsername, ElectronWalletPassword, GameDaemonIP, GameDaemonPort, ElectrumWalletIPAddress,  ElectrumWalletUsername, ElectrumWalletPassword, ElectrumWalletPort, XIDAuthPassword = "";

        public static bool playerLoggedIn = false;
        public static string playerName = "";

        public static string GetUserName()
        {
            if(LoginMode == LoginMode.Simple)
            {
                return ElectrumWalletUsername;
            }
            else
            {
                return ElectronWalletUsername;
            }
        }

        public static string GetPassword()
        {
            if (LoginMode == LoginMode.Simple)
            {
                return ElectrumWalletPassword;
            }
            else
            {
                return ElectronWalletPassword;
            }
        }

        public static bool isElectrum()
        {
            return LoginMode == LoginMode.Simple;
        }

        public static string GetSDUrl()
        {
            return "http" + "://" + ElectronWalletUsername + ":" + ElectronWalletPassword + "@" + GSPIP();
        }

        public static string GetChannelUrl()
        {
            return "http" + "://" + ElectronWalletUsername + ":" + ElectronWalletPassword + "@" + XAMPPbroadcastURL;
        }

        public static string GSPIP()
        {
            return GameDaemonIP + ":" + GameDaemonPort;
        }

        public static string GetServerUrl()
        {
            return "http://" + ElectronWalletUsername + ":" + ElectronWalletPassword + "@" + ElectronWalletIPAddress + ":" + ElectronWalletPort + "/wallet/game.dat";
        }

        public static void FillAllConnectionSettings()
        {
            //If we have connection settings stored in PlayerPrefs, we are going to use them,
            //else we will use default values

            string dataPrefix = "";

            if (LoginMode == LoginMode.Simple)
            {
                dataPrefix = gameID + "electrum";
            }
            else
            {
                dataPrefix = gameID + "advanced";
            }

            string savedUsername = "";
            string savedPassword = "";

            if(PlayerPrefs.HasKey(dataPrefix + "_ip"))
            {
                if (LoginMode == LoginMode.Simple)
                {
                    ElectrumWalletIPAddress = PlayerPrefs.GetString(dataPrefix + "_ip", "127.0.0.1");
                    ElectrumWalletPort = PlayerPrefs.GetString(dataPrefix + "_port", "8396");
                }
                else
                {
                    ElectronWalletIPAddress = PlayerPrefs.GetString(dataPrefix + "_ip", "127.0.0.1");
                    ElectronWalletPort = PlayerPrefs.GetString(dataPrefix + "_port", "8396");

                    savedUsername = PlayerPrefs.GetString(dataPrefix + "_username", "");
                    savedPassword = PlayerPrefs.GetString(dataPrefix + "_password", "");
                }

                GameDaemonIP = PlayerPrefs.GetString(dataPrefix + "_ipGSP", "127.0.0.1");
                GameDaemonPort = PlayerPrefs.GetString(dataPrefix + "_portGSP", "8610");
            }
            else
            {
                ElectronWalletIPAddress = "127.0.0.1";
                ElectronWalletPort = "8396";
                GameDaemonIP = "127.0.0.1";
                GameDaemonPort = "8610";
            }

            //For the electrum, we resolve username/password later between launching the wallet, here we fill advanced wallet values

            if (LoginMode == LoginMode.Advanced)
            {
                if (savedUsername == "" && savedPassword == "")
                {
                    string rpc_username, rpc_password = "";
                    XAYAWalletAPI.Instance.GetUserNameAndPasswordFromCookies(out rpc_username, out rpc_password);

                    ElectronWalletUsername = rpc_username;
                    ElectronWalletPassword = rpc_password;
                }
                else
                {
                    ElectronWalletUsername = savedUsername;
                    ElectronWalletPassword = savedPassword;
                }
            }

            XAYAWalletAPI.Instance.ConstructConnectionStrings();
        }

    }
}