using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CielaSpike;
using UnityEngine.Networking;

public class ChannelDeamonManager : MonoBehaviour
{
    // Start is called before the first frame update
    public static ChannelDeamonManager instance;

    void Start()
    {
        
    }

    public void StartChannelService(string channelId)
    {
        StartCoroutine(StartService(channelId));
    }
    public void StopChannelService()
    {
        StartCoroutine( StopService());
    }


    IEnumerator StartService(string channelId)
    {
        Task task;        
        this.StartCoroutineAsync(StartServiceAsync(channelId), out task);
        yield return StartCoroutine(task.Wait());
        if(task.State==TaskState.Error)
        {
            Debug.Log(task.Exception);
        }
    }
    IEnumerator StopService()
    {
        Task task;
        this.StartCoroutineAsync(StopServiceAsync(), out task);
        yield return StartCoroutine(task.Wait());
        if (task.State == TaskState.Error)
        {
            Debug.Log(task.Exception);
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    IEnumerator StartServiceAsync(string channelId)
    {
        yield return Ninja.JumpToUnity;
        string channelServiceExePath = Application.streamingAssetsPath + "/shipsd/ships-channel.exe";
        string channelServiceStr = " --xaya_rpc_url=\"" + GlobalData.gSettingInfo.GetServerUrl() + "\" --gsp_rpc_url=\"" +
            GlobalData.gSettingInfo.GSPIP + "\" --broadcast_rpc_url=\"http://seeder.xaya.io:10042\" --rpc_port=\"" + "29060" + "\" --playername=" +
            GlobalData.gPlayerName.Substring(2) + " --channelid=\"" + channelId + "\" -alsologtostderr";
        channelServiceStr += " --v=1";

        //=================================================================================================================//

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        yield return Ninja.JumpBack;
        if (!XAYABitcoinLib.Utils.StartService("cmd.exe", "/c " + channelServiceExePath + channelServiceStr, false))
        {
            yield return Ninja.JumpToUnity;
            GlobalData.ErrorPopup("Channel service is not running.");
            yield return Ninja.JumpBack;
        }
        //Debug.Log(channelServiceExePath + channelServiceStr);
#else
        yield return Ninja.JumpBack;
         if (!XAYABitcoinLib.Utils.StartService("/bin/bash", "-c 'ships-channel " + channelServiceStr+"'", false))
        {
            yield return Ninja.JumpToUnity;
            GlobalData.ErrorPopup("Channel service is not running.");
            Debug.Log("-c 'ships-channel " + channelServiceStr + "'");
            yield return Ninja.JumpBack;

        }        
#endif       
    }
    IEnumerator StopServiceAsync()
    {
        yield return Ninja.JumpBack;
        string cmdstr = "{\"jsonrpc\":\"2.0\", \"method\":\"stop\"}";
        bool bLiveChannel = false;
        Debug.Log("stop Start" + GlobalData.bLiveChannel);
        foreach (System.Diagnostics.Process proc in System.Diagnostics.Process.GetProcessesByName("ships-channel"))
        {
            bLiveChannel = true;
            //Debug.LogError("live Channel:"+ bLiveChannel);
        }
        Debug.Log("stop after1 check bLive" + bLiveChannel);

        //System.Diagnostics.Process[] prs = System.Diagnostics.Process.GetProcesses();
        //foreach (System.Diagnostics.Process pr in prs)
        //{
        //    if (pr.ProcessName == "ships-channel")
        //    {
        //        bLiveChannel = true;
        //    }

        //}
        //Debug.LogError("stop after2 check bLive" + bLiveChannel);

        GlobalData.bLiveChannel = bLiveChannel;
        yield return Ninja.JumpToUnity;
        Debug.Log("stop after check" + GlobalData.bLiveChannel);

        if (bLiveChannel)
        {
            //================  open flag set  =========================//
            GlobalData.bOpenedChannel = false;
            //================  fixed port =====================//
            int port = 29060;
            //==================================================//
            string url = GlobalData.gSettingInfo.GetShipChannelUrl() + ":" + port;
            UnityWebRequest www = UnityWebRequest.Put(url, cmdstr);
            www.method = UnityWebRequest.kHttpVerbPOST;
            www.SetRequestHeader("Content-Type", "application/json"); www.SetRequestHeader("Accept", "application/json");

            Debug.Log(url+ " cmd:  "+cmdstr );
            Debug.Log("channelService stop-request-time:" + Time.timeSinceLevelLoad);
            yield return www.SendWebRequest();

            yield return Ninja.JumpBack;
            while (bLiveChannel)
            {
                bLiveChannel = false;
                foreach (System.Diagnostics.Process proc in System.Diagnostics.Process.GetProcessesByName("ships-channel"))
                {
                    bLiveChannel = true;
                }
            }
            yield return Ninja.JumpToUnity;
            Debug.Log("channelService stop-final-time:" + Time.timeSinceLevelLoad);
            yield return new WaitForSeconds(0.01f);
            GlobalData.bLiveChannel = false;
        }
    }
}
