using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpDownMenuBtn : MonoBehaviour {

    [SerializeField]
    UnityEngine.Sprite down=null;
    [SerializeField]
    UnityEngine.Sprite up=null;

    [SerializeField]
    GameObject menuBody; 
    bool m_bActive = false;
    // Use this for initialization
    void Start () {
        if (menuBody == null)
            menuBody = transform.GetChild(1).gameObject;

    }
	
	// Update is called once per frame
	void Update () {
		
	}
    public void OnBtnClick()
    {
        if (GetComponent<UnityEngine.UI.Image>() == null || down == null || up == null) return;
        m_bActive = !m_bActive;
        if (m_bActive)
        {
            GetComponent<UnityEngine.UI.Image>().sprite = up;
            if (transform.GetChild(0) == null) return;
            transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().color = new Color32(255,255,255,255);
            menuBody.gameObject.SetActive(true);
        }
        else
        {
            GetComponent<UnityEngine.UI.Image>().sprite = down;
            if (transform.GetChild(0) == null) return;
            transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().color = new Color32(255, 255, 255, 100);
            menuBody.gameObject.SetActive(false);
        }
    }
    public void OnMenubodyHide()
    {
        if (GetComponent<UnityEngine.UI.Image>() == null || down == null || up == null) return;
        m_bActive = false;        
        GetComponent<UnityEngine.UI.Image>().sprite = down;
        if (transform.GetChild(0) == null) return;
        transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().color = new Color32(255, 255, 255, 100);
        menuBody.gameObject.SetActive(false);
        
    }
}
