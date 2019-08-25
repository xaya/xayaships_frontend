using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateChannelBtn : MonoBehaviour {

    [SerializeField]
    GameObject m_prefabLobbyCannelPanel;
    List<GameObject> channelPannelList=new List<GameObject>();
    [SerializeField]
    GameObject scrollParentPanel;
    [SerializeField]
    Vector2 panelSize=new Vector2(160,27);
    [SerializeField]
    int panelTopOffset = 0;
    [SerializeField]
    int panelGap = 0;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update() {
        //=====================Add or Delete Pannel  =======================//

        if (scrollParentPanel.transform.childCount > GlobalData.ggameLobbyChannelList.Count)
        {
            for (int i = GlobalData.ggameLobbyChannelList.Count; i < scrollParentPanel.transform.childCount; i++)
                GameObject.Destroy(scrollParentPanel.transform.GetChild(i).gameObject);
        }
        else
        {
            for (int i = scrollParentPanel.transform.childCount; i < GlobalData.ggameLobbyChannelList.Count; i++)
            {
                CreateCannel(i);
            }
        }
        //ReCreatePanels();
    }
    public void CreateCannel(int index)
    {
        //int index = channelPannelList.Count;
        GameObject onePanel = null;
        onePanel = Instantiate(m_prefabLobbyCannelPanel, new Vector3(0, 0, 0), Quaternion.identity);
        onePanel.transform.parent = scrollParentPanel.transform;
        onePanel.transform.localScale = new Vector3(1, 1, 1);
        //onePanel.transform.GetComponent<RectTransform>().localPosition = new Vector3(0, -panelTopOffset - (panelSize.y * (index) + panelGap * index), 0);
        //onePanel.GetComponent<RectTransform>().sizeDelta = panelSize;
        onePanel.GetComponent<CannelPannelIno>().Id = index;

        channelPannelList.Add(onePanel);
    }
    public void ReCreatePanels()
    {
        foreach (Transform t in scrollParentPanel.transform)
        {
            GameObject.Destroy(t.gameObject);
        }
        for (int i = 0; i < GlobalData.ggameLobbyChannelList.Count; i++)
        {
            CreateCannel(i);
        }
    }
    
}
