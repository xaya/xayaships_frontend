using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using SeaBattleControl;
using BattleShip.BLL.GameLogic;
public class PlayboardControl : MonoBehaviour {


    //public SeaBattleGame seabattleGame; 
    //public SeaBattleControl.Ship[] ships;

    GameControl gameControl = null;
	// Use this for initialization
	void Start () {
        gameControl = GlobalData.gGameControl;        
	}
	
	// Update is called once per frame
	void Update () {
		
	}

}
