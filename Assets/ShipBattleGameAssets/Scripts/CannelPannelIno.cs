using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XAYA;

public class CannelPannelIno : MonoBehaviour {

    public int Id = 0;
    [SerializeField]
    Text playerNameText;
    [SerializeField]
    Text statusText;
    [SerializeField]
    Text playerCountText;
    [SerializeField]
    GameObject actBtn;

	
	// Update is called once per frame
	void Update ()
    {
		if(Id<GlobalData.ggameLobbyChannelList.Count)
        {
            playerNameText.text =GlobalData.ggameLobbyChannelList[Id].id.Substring(0,7)+": ("+  GlobalData.ggameLobbyChannelList[Id].userNames[0];
            if (GlobalData.ggameLobbyChannelList[Id].userNames.Length == 2)
            {
                playerNameText.text += ",  " + GlobalData.ggameLobbyChannelList[Id].userNames[1];
                transform.Find("adctionBtn").Find("Text").GetComponent<Text>().text = "Playing";
                transform.Find("adctionBtn").GetComponent<Image>().color = new Color32(7, 47, 106, 255);
            }
            playerNameText.text += ")  ";
            playerCountText.text = GlobalData.ggameLobbyChannelList[Id].userNames.Length.ToString();
            statusText.text = GlobalData.ggameLobbyChannelList[Id].statusText;

            //group prefix is removed, and compare====//
            if (XAYASettings.playerName == GlobalData.ggameLobbyChannelList[Id].userNames[0] && GlobalData.ggameLobbyChannelList[Id].userNames.Length == 1)
            {
                transform.Find("adctionBtn").Find("Text").GetComponent<Text>().text = "Close";
                transform.Find("adctionBtn").GetComponent<Image>().color = new Color32(115,11, 10, 255);
            }

            if (XAYASettings.playerName != GlobalData.ggameLobbyChannelList[Id].userNames[0] && GlobalData.ggameLobbyChannelList[Id].userNames.Length == 1)
            {
                transform.Find("adctionBtn").Find("Text").GetComponent<Text>().text = "Join";
                transform.Find("adctionBtn").GetComponent<Image>().color = new Color32(19, 136, 16, 255);             
            }

        }
	}

    public void JoinChannel()
    {
        if (transform.Find("adctionBtn").Find("Text").GetComponent<Text>().text != "Join") return;

        //====== case in player created channel, expire  ========================//
        if (GlobalData.ggameLobbyChannelList[Id].userNames[0] == XAYASettings.playerName) return;

        ShipSDClient.Instance.JoinGameChannel(GlobalData.ggameLobbyChannelList[Id].id);
    }
    public void CloseChannel()
    {
        if (transform.Find("adctionBtn").Find("Text").GetComponent<Text>().text != "Close") return;
        ShipSDClient.Instance.CloseGameChannel(GlobalData.ggameLobbyChannelList[Id].id);
    }
}
