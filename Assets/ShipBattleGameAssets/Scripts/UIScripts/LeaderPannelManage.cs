using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderPannelManage : MonoBehaviour
{    
    List<GameObject> pannelList = new List<GameObject>();
    [SerializeField]
    GameObject scrollParentPanel;
    [SerializeField]
    Vector2 panelSize = new Vector2(160, 27);
    [SerializeField]
    int panelTopOffset = 0;
    [SerializeField]
    int panelGap = 0;
    [SerializeField]
    GameObject m_prefabPanel;
    // Use this for initialization
    void Start()
    {
        if (scrollParentPanel == null) scrollParentPanel = gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        //=====================Add or Delete Pannel  =======================//
        if (scrollParentPanel.transform.childCount > GlobalData.ggameLeaderList.Count)
        {
            for (int i = GlobalData.ggameLeaderList.Count; i < scrollParentPanel.transform.childCount; i++)
                GameObject.Destroy(scrollParentPanel.transform.GetChild(i).gameObject);
        }
        else
        {
            for (int i = scrollParentPanel.transform.childCount; i < GlobalData.ggameLeaderList.Count; i++)
            {
                CreatePanel(i);
            }
        }

    }
    public void CreatePanel(int index)
    {
        //int index = pannelList.Count;
        GameObject onePanel = null;
        onePanel = Instantiate(m_prefabPanel, new Vector3(0, 0, 0), Quaternion.identity);
        onePanel.transform.parent = scrollParentPanel.transform;
        onePanel.transform.localScale = new Vector3(1, 1, 1);
        //onePanel.transform.GetComponent<RectTransform>().localPosition = new Vector3(0, -panelTopOffset - (panelSize.y * (index) + panelGap * index), 0);
        //onePanel.GetComponent<RectTransform>().sizeDelta = panelSize;
        onePanel.GetComponent<LeaderPanelInfo>().Id = index;
        pannelList.Add(onePanel);
    }
}
