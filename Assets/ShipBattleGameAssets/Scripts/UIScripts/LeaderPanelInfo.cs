using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderPanelInfo : MonoBehaviour
{

    public int Id = 0;
    [SerializeField]
    Text noText;
    [SerializeField]
    Text playerNameText;
    [SerializeField]
    Text playedCountText;
    [SerializeField]
    Text winCountText;    
        
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("ChannelInfo: " + Id + GlobalData.ggameLeaderList.Count);
        if (Id < GlobalData.ggameLeaderList.Count)
        {
            playerNameText.text = GlobalData.ggameLeaderList[Id].playerName;                        
            playedCountText.text = GlobalData.ggameLeaderList[Id].playedCount.ToString();
            winCountText.text= GlobalData.ggameLeaderList[Id].winCount.ToString();
            if (noText) noText.text =(Id+1).ToString();
        }
    }    
}
