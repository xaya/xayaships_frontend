using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HoverBtn : MonoBehaviour {

    [SerializeField]
    Color32 hoveColor;
    Color32 originColor;
	// Use this for initialization
	void Start () {
        if(GetComponent<Image>())
        originColor = GetComponent<Image>().color;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    public void OnHoverBtn()
    {
        if (GetComponent<Image>() == null) return;
        GetComponent<Image>().color = hoveColor;
    }
    public void OnLeaveBtn()
    {
        if (GetComponent<Image>() == null) return;
        GetComponent<Image>().color = originColor;
    }
}
