using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XAYA
{
    /* Simple mode runs using electrum wallet, advanced runs using default XAYA electron wallet*/
    public enum LoginMode { Simple, Advanced };

    public class XAYASettings
    {
        //Main Game
        public static LoginMode LoginMode = LoginMode.Simple;
        public static string gameID = "xs"; //xs goes for Xayaships
        public static string simpleRPCToTestConnection = "getcurrentstate"; //(without any params)
        public static string DaemonName = "shipsd"; // exe file name of game daemon
        public static string rpcCommandJsonFile = "channel-gsp-rpc.json";

        //Lite mode
        public static bool localCharonServer = false;
        public static string rpcCommandJsonFileCharonServer = "gsp.json";
        public static string additionalBackend = ""; //ususaly empty, for games like SME can be --backend_version "0.3"

        //XID
        public static string chatID = "sme"; //xampp chat server prefix, xmpptest1 for testing
        public const string chatLicense = @"eJxkkcluwjAURX8FsUWtCYMglbHKECKmEAoEyM7EJjIZHDwQkq8vKi0surs65+m9Kz04ZwFNJa3ckjiVvSoO3yQ/qRwL+hE/VBVBV3CiAzUhaIGVYDcIXgSuNE4VUwUyIHhmONRS8YQKBB2cUNRPQxrzypDHXLKEQ/BD4ZAnGU4L1NeKpzzhWlZ2XMREVuaKQPCnoZVgFiOckuLzhgv8zu4bHuw+9Ly0zQhW1LplTNDRPaFGvWEYDaMOwT8F1yxMsdKCormmrjPYh9moZnYmJrMv4pytDl+0xa1SeK45EBmRneZhv2IiX0Vtq5wU9cwvvcTJLSteTsHIHR83rObNgD4JQwyWwTpvLuSlHXpXw+tYvnTW5k77MmgsZ3EnKsOuvdle+45j1bvsFIDcvowWl2g55bOj0So3oFZySYLJwC71mHC99s/X/TbqQfDqDcHv89C3AA==";
        public const string chatDomainName = "chat.xaya.io";
        public const string defaultChatRoomID = "sme@muc.chat.xaya.io";
        public static bool launchChat = true; //true if we want dummy UI to also launch chat

        //Boradcaster
        public static bool launchXAMPPbroadcastService = true; //we use this for games with gamechannels
        public static string XAMPPbroadcastURL = "127.0.0.1";
        public static string XAMPPbroadcastPORT = "10042";
        public static int gameChannelDefaultPort = 29060;
        public static string gameChannelURL = "127.0.0.1";
        public static string channelsDaemonName = "ships-channel.exe";

        //Other
        public static bool ignoreChatDebugLog = true;

        //Values below are filled automatically
        public static string GameServerAddress = ""; //game's GSP address 
        public static string WalletServerAddress = ""; //electron or electrum wallet address
        public static string XIDServerAddress = ""; //address of XID, which is either integrated into electron, or xid-light running alongside electrum.exe
        public static string ElectronWalletIPAddress, ElectronWalletPort, ElectronWalletUsername, ElectronWalletPassword, GameDaemonIP, GameDaemonPort, ElectrumWalletIPAddress, ElectrumWalletUsername, ElectrumWalletPassword, ElectrumWalletPort, XIDAuthPassword = "";
        public static bool playerLoggedIn = false;
        public static string playerName = "";
        public static bool gspHeightFetched = false; //We set this tur true on the first valid fetch
        public static bool isRegtestMode = false;
        public static int lastBlockHeight = 0;

        //LiteMode (filled automatically

        public static string litePassword;
        public static string liteSigned;
        public static string liteAuthMessage;

        public static string GetUserName()
        {
            if (LoginMode == LoginMode.Simple)
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

        public static string GetChannelDaemonUrl()
        {
            if (isElectrum() == false)
            {
                return "http" + "://" + ElectronWalletUsername + ":" + ElectronWalletPassword + "@" + GSPIP();
            }
            else
            {
                return "http" + "://" + ElectrumWalletUsername + ":" + ElectrumWalletPassword + "@" + GSPIP();
            }
        }

        public static string GetChannelUrl()
        {
            if (isElectrum() == false)
            {
                return "http" + "://" + ElectronWalletUsername + ":" + ElectronWalletPassword + "@" + gameChannelURL;
            }
            else
            {
                return "http" + "://" + ElectrumWalletUsername + ":" + ElectrumWalletPassword + "@" + gameChannelURL;
            }
        }

        public static string GSPIP()
        {
            return GameDaemonIP + ":" + GameDaemonPort;
        }

        public static string GetServerUrl()
        {
            if (isElectrum() == false)
            {
                return "http://" + ElectronWalletUsername + ":" + ElectronWalletPassword + "@" + ElectronWalletIPAddress + ":" + ElectronWalletPort + "/wallet/game.dat";
            }
            else
            {
                return "http://" + ElectrumWalletUsername + ":" + ElectrumWalletPassword + "@" + XAYASettings.ElectronWalletIPAddress + ":" + XAYASettings.ElectrumWalletPort + "/xaya/compatibility";
            }
        }

        public static void ResetConnectionSettings()
        {
            string dataPrefix = "";
            dataPrefix = gameID + "electrum";

            PlayerPrefs.SetString(dataPrefix + "_ip", "127.0.0.1");
            PlayerPrefs.SetString(dataPrefix + "_port", "8397");
            PlayerPrefs.SetString(dataPrefix + "_ipGSP", "127.0.0.1");
            PlayerPrefs.SetString(dataPrefix + "_portGSP", "8610");

            dataPrefix = gameID + "advanced";

            PlayerPrefs.SetString(dataPrefix + "_ip", "127.0.0.1");
            PlayerPrefs.SetString(dataPrefix + "_password", "");

            PlayerPrefs.Save();
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

            if (PlayerPrefs.HasKey(dataPrefix + "_ip"))
            {
                if (LoginMode == LoginMode.Simple)
                {
                    ElectrumWalletIPAddress = PlayerPrefs.GetString(dataPrefix + "_ip", "127.0.0.1");
                    ElectrumWalletPort = PlayerPrefs.GetString(dataPrefix + "_port", "8397");
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

                if (LoginMode == LoginMode.Simple)
                {
                    ElectrumWalletPort = "8397";
                }
                else
                {
                    ElectronWalletPort = "8396";
                }

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