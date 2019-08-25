using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisputeDisp : MonoBehaviour
{
    [SerializeField]
    Text heightText;
    [SerializeField]
    Text remainHeightText;

    [SerializeField]
    GameObject[] blocks;
    [SerializeField]
    GameObject disputePopup;
    [SerializeField]
    Text issuserText;
    // Start is called before the first frame update
    void Start()
    {
    //    if (heightText != null && GlobalData.disputeStatus!=null)
    //    heightText.text = "BLOCK HEIGHT: "+GlobalData.disputeStatus.height.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        long diff = 0;
        if(issuserText!=null)issuserText.text = "";

        if (heightText != null && GlobalData.disputeStatus != null)
        {
            heightText.text = "BLOCK HEIGHT: " + GlobalData.disputeStatus.height.ToString();
            diff = GlobalData.gChannelHeight - GlobalData.disputeStatus.height;
            remainHeightText.text = diff.ToString()+"/10";
            if(issuserText!=null)
            {
                issuserText.text = "ISSUER: ";
                if(GlobalData.disputeStatus.whoseturn==GlobalData.gPlayerIndex)
                {
                    issuserText.text += GlobalData.gOpponentName;
                }
                else
                {
                    issuserText.text += GlobalData.gPlayerName.Substring(2);
                }
            }
        }

        if (diff < 10)
        {
            for (int i = 0; i < blocks.Length; i++)
            {
                if (i < diff) blocks[i].SetActive(true);
                else blocks[i].SetActive(false);
            }
        }

        if ( GlobalData.disputeStatus == null || (GlobalData.disputeStatus != null && GlobalData.disputeStatus.canresolve) )
        {
            for (int i = 0; i < blocks.Length; i++)
            {
                blocks[i].SetActive(false);
            }
            remainHeightText.text = "";
            issuserText.text = "";
            heightText.text = "BLOCK HEIGHT:";
            disputePopup.SetActive(false);
        }

               
            
    }
    void OnEnable()
    {
        //if (heightText != null && GlobalData.disputeStatus != null)
        //    heightText.text = "BLOCK HEIGHT: " + GlobalData.disputeStatus.height.ToString();
    }
}
