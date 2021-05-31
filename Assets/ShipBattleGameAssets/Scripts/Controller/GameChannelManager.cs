using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace XAYA
{
    public class GameChannelManager : MonoBehaviour
    {
        RPCRequest request;
        System.Diagnostics.Process gameChannelProcess;

        public static GameChannelManager Instance;

        void Start()
        {
            Instance = this;
            request = new RPCRequest();
        }

        public void CreateGameChannel()
        {
            string address = request.GetNewAddress(XAYASettings.playerName);
            string value = "{\"g\":{\""+XAYASettings.gameID+"\":{\"c\":{\"addr\":\"" + address + "\"}}}}";
            JObject dataVal = JObject.Parse(value);
            string texid = request.XAYANameUpdateDirect(XAYASettings.playerName, dataVal); 
        }

        public void OnApplicationQuit()
        {
            if (gameChannelProcess != null)
            {
                gameChannelProcess.Kill();
            }
        }

        public void RunChannelService(string channelId)
        {
            gameChannelProcess = new System.Diagnostics.Process();
            gameChannelProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            gameChannelProcess.StartInfo.CreateNoWindow = true;
            gameChannelProcess.StartInfo.UseShellExecute = false;
            gameChannelProcess.StartInfo.FileName = Application.streamingAssetsPath + "/Daemon/" + XAYASettings.channelsDaemonName;

            if (XAYASettings.LoginMode == LoginMode.Simple)
            {
                gameChannelProcess.StartInfo.Arguments = System.Environment.ExpandEnvironmentVariables(" -noxaya_rpc_legacy_protocol --xaya_rpc_url=\"" + XAYASettings.GetServerUrl() + "\" --gsp_rpc_url=\"" + XAYASettings.GSPIP() + "\" --broadcast_rpc_url=\"" + XAYASettings.XAMPPbroadcastURL + ":" + XAYASettings.XAMPPbroadcastPORT + "\" --rpc_port=\"" + XAYASettings.gameChannelDefaultPort + "\" --playername=\"" + XAYASettings.playerName + "\" --channelid=\"" + channelId + "\"" + " --v=1 --alsologtostderr=1 --log_dir=\"%appdata%/XAYA-Electron/" + XAYASettings.DaemonName + "data/\"");
            }
            else
            {
                gameChannelProcess.StartInfo.Arguments = System.Environment.ExpandEnvironmentVariables(" --xaya_rpc_url=\"" + XAYASettings.GetServerUrl() + "\" --gsp_rpc_url=\"" + XAYASettings.GSPIP() + "\" --broadcast_rpc_url=\"" + XAYASettings.XAMPPbroadcastURL + ":" + XAYASettings.XAMPPbroadcastPORT + "\" --rpc_port=\"" + XAYASettings.gameChannelDefaultPort + "\" --playername=\"" + XAYASettings.playerName + "\" --channelid=\"" + channelId + "\"" + " --v=1 --alsologtostderr=1 --log_dir=\"%appdata%/XAYA-Electron/" + XAYASettings.DaemonName + "data/\"");
            }

            Debug.Log("SHIPS CHANNEL STARTS WITH ARGUMENTS: " + gameChannelProcess.StartInfo.Arguments);

            gameChannelProcess.EnableRaisingEvents = true;
            gameChannelProcess.Start();
        }

        public void JoinGameChannel(string channelId)
        {
            string address = request.GetNewAddress(XAYASettings.playerName);

            string value = "{\"g\":{\"" + XAYASettings.gameID + "\":{\"j\":{\"id\":\"" + channelId + "\", \"addr\":\"" + address + "\"}}}}";
            JObject dataVal = JObject.Parse(value);
            string texid = request.XAYANameUpdateDirect(XAYASettings.playerName, dataVal);
        }

        public void CloseGameChannel(string channelId)
        {
            string value = "{\"g\":{\"" + XAYASettings.gameID + "\":{\"a\":{\"id\":\"" + channelId + "\"}}}}";
            JObject dataVal = JObject.Parse(value);
            string texid = request.XAYANameUpdateDirect(XAYASettings.playerName, dataVal);
        }

        public string GetCurrentStateDaemon()
        {
            return request.GetCurrentDaemonState();
        }

        public string GetCurrentStateChannel()
        {
            return request.GetCurrentChannelState();
        }

        public bool KillChannel()
        {
            if (gameChannelProcess != null)
            {
                gameChannelProcess.Kill();
                return true;
            }
            return false;
        }
    }
}