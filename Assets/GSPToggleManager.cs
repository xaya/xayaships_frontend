using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GSPToggleManager : MonoBehaviour
{
    [SerializeField]
    UnityEngine.UI.Toggle gspToggle;
    [SerializeField]
    GameObject commentGSPRun;
    [SerializeField]
    GameObject bgToggleDisable;
    [SerializeField]
    ShipSDClient sdClient;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void GSPToggleChange()
    {
        bool bRun = gspToggle.isOn;
        if (bRun)
        {
            commentGSPRun.SetActive(true);
            bgToggleDisable.SetActive(true);
            sdClient.startGSPServer();
        }
        else
        {
            commentGSPRun.SetActive(false);
            bgToggleDisable.SetActive(false);
        }
    }
}
