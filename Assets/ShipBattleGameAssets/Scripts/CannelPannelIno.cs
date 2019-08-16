using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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


    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        //Debug.Log("ChannelInfo: "+ Id + GlobalData.ggameChannelList.Count);

		if(Id<GlobalData.ggameChannelList.Count)
        {
            playerNameText.text =GlobalData.ggameChannelList[Id].id.Substring(0,7)+": ("+  GlobalData.ggameChannelList[Id].userNames[0];
            if (GlobalData.ggameChannelList[Id].userNames.Length == 2)
            {
                playerNameText.text += ",  " + GlobalData.ggameChannelList[Id].userNames[1];
                transform.Find("adctionBtn").Find("Text").GetComponent<Text>().text = "Playing";
                transform.Find("adctionBtn").GetComponent<Image>().color = new Color32(7, 47, 106, 255);
            }
            playerNameText.text += ")  ";
            playerCountText.text = GlobalData.ggameChannelList[Id].userNames.Length.ToString();

            //statusText.text = GlobalData.ggameChannelList[Id].status.ToString();
            statusText.text = GlobalData.ggameChannelList[Id].statusText;

            //group prefix is removed, and compare====//
            if (GlobalData.gPlayerName.Substring(2) == GlobalData.ggameChannelList[Id].userNames[0] && GlobalData.ggameChannelList[Id].userNames.Length == 1)
            {
                transform.Find("adctionBtn").Find("Text").GetComponent<Text>().text = "Close";
                transform.Find("adctionBtn").GetComponent<Image>().color = new Color32(115,11, 10, 255);
                //=============================  if channel is opened and channel is not running, create game channel =======================================================//
                //if(!GameObject.Find("Manager").GetComponent<GameChannelManager>().IsRunningChannel(29060))
                //{
                //    GameObject.Find("Manager").GetComponent<GameChannelManager>().RunChannelService(GlobalData.ggameChannelList[Id].id);
                //    GameObject.Find("Manager").GetComponent<GameChannelManager>().StartChannelWaiting(GlobalData.ggameChannelList[Id].id);
                //}
                //==================================================================================//
            }

            if (GlobalData.gPlayerName.Substring(2) != GlobalData.ggameChannelList[Id].userNames[0] && GlobalData.ggameChannelList[Id].userNames.Length == 1)
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
        if (GlobalData.ggameChannelList[Id].userNames[0] == GlobalData.gPlayerName.Substring(2)) return;

        GameObject.Find("Manager").GetComponent<GameChannelManager>().JoinGameChannel(GlobalData.ggameChannelList[Id].id);
    }
    public void CloseChannel()
    {
        if (transform.Find("adctionBtn").Find("Text").GetComponent<Text>().text != "Close") return;

        //====== case in player created channel, expire  ========================//
        //if (GlobalData.ggameChannelList[Id].userNames[0] != GlobalData.gPlayerName.Substring(2)) return;

        GameObject.Find("Manager").GetComponent<GameChannelManager>().CloseGameChannel(GlobalData.ggameChannelList[Id].id);
    }
}
