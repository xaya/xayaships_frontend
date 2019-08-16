using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeaBattleGameManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    public void OnStartGame()
    {
        GlobalData.bPlaying = true;
    } 
}
