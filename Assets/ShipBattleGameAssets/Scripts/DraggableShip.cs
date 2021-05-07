using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using BattleShip.BLL.Requests;
using BattleShip.BLL.GameLogic;
using BattleShip.BLL.Responses;
using BattleShip.BLL.Ships;

public class DraggableShip : MonoBehaviour,IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler {

    Vector2 m_oldPos;
    [SerializeField]
    GameObject m_boardBGObj;
    [SerializeField]
    BattleShip.BLL.Ships.ShipType shipType;
    
    Ship m_ownShip=null;
    int m_ownIndex = -1;
    Vector3 initialPos;
    Vector2 initSizeV;

    // Use this for initialization
    void Start ()
    {
        m_oldPos = GetComponent<RectTransform>().position;
        initialPos = GetComponent<RectTransform>().position;
        initSizeV = GetComponent<RectTransform>().sizeDelta;
    }
	
    public void OnDrag(PointerEventData eventData)
    {             
        if (GlobalData.bPlaying) return;
        Vector2 mousePoint = Input.mousePosition;
        GetComponent<RectTransform>().position = mousePoint;     
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //============= Ship's Position Check===========//
        if (GlobalData.bPlaying) return;
        Vector2 mousePoint = Input.mousePosition;        
        RectTransform rect = GetComponent<RectTransform>();
       
        float gridWidth = (m_boardBGObj.transform.position.x - m_boardBGObj.transform.GetChild(0).position.x);
        Vector2 p = ChangeToCoordinate(Input.mousePosition, m_boardBGObj.transform.position, gridWidth);

        if (p.x > 8 || p.x < 1 || p.y > 8 || p.y < 1)
        {
            rect.position = m_oldPos;
            return;
        }
      

        if (m_ownShip==null)
        {
            PlaceShipRequest ShipToPlace = new PlaceShipRequest();
            ShipToPlace.Direction = ShipDirection.Left;
            ShipToPlace.Coordinate = new Coordinate((int)p.x, (int)p.y);
            ShipToPlace.ShipType = shipType;
            GlobalData.gGameControl.gameMyBoard.PlaceShip(ShipToPlace, out m_ownShip);
            if (m_ownShip == null)
            {
                rect.position = m_oldPos;
                return;
            }
            m_ownIndex = GlobalData.gGameControl.gameMyBoard.GetCurrnetShipIndex();
        }
        else
        {
            if (m_ownShip.SetShipPositionsOnly(new Coordinate((int)p.x, (int)p.y)) != ShipPlacement.Ok)
            {
                rect.position = m_oldPos;
                return;
            }
        }
        //------------------------ to change (row, col) pos  ------------------------------------//
        rect.position = ChangeToPosition(p, m_boardBGObj.transform.position, gridWidth);
    }
    public void Init()
    {
        
    }
    public Vector2 ChangeToCoordinate(Vector2 mousePos,Vector2 boardCenterPos, float boardWidth )
    {
        Vector2 p =boardCenterPos;
        p = new Vector2(mousePos.x - p.x, p.y - mousePos.y);
        p = p * 4 / boardWidth;
        p = p + new Vector2(4.5f, 4.5f);
        p = new Vector2(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y));               
        return p;
    }

    public Vector2 ChangeToPosition(Vector2 rowCol, Vector2 boardCenterPos, float boardWidth)
    {

        Vector2 p = rowCol-new Vector2(4.5f, 4.5f);
        p = p * boardWidth /4;
        return new Vector2(p.x+ boardCenterPos.x,boardCenterPos.y - p.y);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (GlobalData.bPlaying) return;
        m_oldPos = GetComponent<RectTransform>().position;
    }

    public void OnPointerClick(PointerEventData eventData) // 3
    {
        if (GlobalData.bPlaying) return;

        if (eventData.clickCount < 2) return;
        if (m_ownShip == null) return;

        //if (!GetComponent<RectTransform>().rect.Contains(e.mousePosition)) return;                
        if (m_ownShip.SwapDirectionOnly() == ShipPlacement.Ok)
        {
            GetComponent<RectTransform>().localEulerAngles = new Vector3(0, 0, (int)m_ownShip.direction * 90);
        }

    }

    private void OnGUI()
    {
        Event e= Event.current;
    }

    public Coordinate[] GetPositions()
    {
        if (m_ownShip == null) return null;
        return m_ownShip.BoardPositions;
    }

    public void InitPos()
    {
        GetComponent<RectTransform>().position = initialPos;
        GetComponent<RectTransform>().sizeDelta = initSizeV;
        GetComponent<RectTransform>().localEulerAngles = Vector3.zero;
        m_ownShip = null;
    }
}
