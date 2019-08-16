using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunkShipManage : MonoBehaviour
{
    public Sprite m_sActive;
    public Sprite m_sSunk;
    // Start is called before the first frame update
    void Start()
    {
        if (m_sActive == null) m_sActive = GetComponent<UnityEngine.UI.Image>().sprite;   
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void InitSprite()
    {
        GetComponent<UnityEngine.UI.Image>().sprite = m_sActive;
    }
    public void SetSunkShip()
    {
        GetComponent<UnityEngine.UI.Image>().sprite = m_sSunk;
    }
}
