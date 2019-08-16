using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class RPCUtils
{    
    public static IEnumerator NotificationRPC(string strUrl, string notificationStr)
    {
        string requestJsonStr = "{\"jsonrpc\":\"2.0\", \"method\":\""+ notificationStr+"\"}";
        UnityWebRequest www = UnityWebRequest.Put(strUrl, requestJsonStr);
        www.method = UnityWebRequest.kHttpVerbPOST;
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Accept", "application/json");
        www.downloadHandler = new DownloadHandlerBuffer();
        yield return www.SendWebRequest();
    }
}
