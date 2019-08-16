using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class BlockchainManager : MonoBehaviour
{
    [SerializeField]
    UnityEngine.UI.Text m_text;
    // Start is called before the first frame update
    void Start()
    {
        //GlobalData.gSettingInfo = SettingInfo.getSettingFromJson();

        //string rpcServerPath = GlobalData.gSettingInfo.userName + ":" + GlobalData.gSettingInfo.userPassword + "@" + GlobalData.gSettingInfo.portNumber;
        //StartCoroutine(xayaRpcCommand("{\"method\":\"name_list\"}"));
    }

    // Update is called once per frame
    void Update()
    {
                    
    }

    

    private IEnumerator xayaRpcCommand(string requestJsonStr, Action<string> callback)
    {
        //string resultJsonStr = null;
        string tempStr = "";
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
}
