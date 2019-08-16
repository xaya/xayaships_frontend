using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockChainStatusView : MonoBehaviour
{

    UnityEngine.UI.Text blockchainStatusText;
    
    // Start is called before the first frame update
    void Start()
    {
        if (GetComponent<UnityEngine.UI.Text>())
            blockchainStatusText = GetComponent<UnityEngine.UI.Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (blockchainStatusText != null)
            blockchainStatusText.text = GlobalData.gblockHeight.ToString() + " " + GlobalData.gblockStatusStr; 
        //if()
    }
}
