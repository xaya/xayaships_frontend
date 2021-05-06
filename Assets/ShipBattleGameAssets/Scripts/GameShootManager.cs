﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameShootManager: MonoBehaviour, IPointerClickHandler
{
    public GameObject fireBoard;
    public GameObject parentTrans;

    public GameObject hitPoint;
    public GameObject missPoint;
        
    [SerializeField]
    bool bFireboard = true;
    List<GameObject> shoots;
    // Start is called before the first frame update
    void Start()
    {
        shoots = new List<GameObject>();   
    }

    public void OnPointerClick(PointerEventData eventData) // 3
    {
        if (!GlobalData.bPlaying) return;
        if (!GlobalData.gbTurn)
        {            
            return;
        }
        if (!bFireboard) return;

        if (eventData.clickCount < 2) return;

        float gridWidth = (fireBoard.transform.position.x - fireBoard.transform.GetChild(0).position.x);
        Vector2 p = XAYABitcoinLib.Utils.ChangeToCoordinate(Input.mousePosition, fireBoard.transform.position, gridWidth);

        ShipSDClient.Instance.SetShootSubmit(new Vector2(p.y,p.x)-new Vector2(1,1));

    }

    public void SetMarker(Vector2 coord, bool bHit)
    {
        float gridWidth = (fireBoard.transform.position.x - fireBoard.transform.GetChild(0).position.x);
        Vector2 pos = XAYABitcoinLib.Utils.ChangeToPosition(coord, fireBoard.transform.position, gridWidth);

        if (bHit)
        {
            if (hitPoint == null) return;
            GameObject g = Instantiate(hitPoint, new Vector3(0,0,0), Quaternion.identity);
            g.transform.position = new Vector3(pos.x, pos.y, 0);
            g.transform.SetParent(parentTrans.transform);
            g.transform.localScale = new Vector3(1, 1, 1);
            shoots.Add(g);
        }
        else
        {
            if (missPoint == null) return;
            GameObject g = Instantiate(missPoint, new Vector3(0, 0, 0), Quaternion.identity);
            g.transform.position = new Vector3(pos.x, pos.y, 0);
            g.transform.SetParent(parentTrans.transform);
            g.transform.localScale = new Vector3(1, 1, 1);
            shoots.Add(g);
        }
    }
    public  void ClearMarker()
    {
        foreach (GameObject g in shoots)
        {
            //if (Vector2.Distance( g.transform.position, pos) < 1)
                GameObject.Destroy(g);
        }
    }

}
