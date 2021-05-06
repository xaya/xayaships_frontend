using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGameManager : MonoBehaviour
{
    [SerializeField]
    GameObject[] startActiveObjects;
    [SerializeField]
    GameObject[] startDeactiveObjects;
    // Start is called before the first frame update
    void Start()
    {
    
        foreach(GameObject g in startDeactiveObjects)
        {
            g.SetActive(false);
        }
        foreach (GameObject g in startActiveObjects)
        {
            g.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
