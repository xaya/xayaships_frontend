using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XAYA;

public class CreatePendingData
{
    public string address;
    public string id;
    public string name;
}

public class JoinPendingData
{
    public string address;
    public string id;
    public string name;
}

public class ChannelPendingData
{
    public string address;
    public string id;
    public string name;
}

public class PendingData
{
    [JsonProperty("abort")]
    public List<string> abort = new List<string>();

    [JsonProperty("create")]
    public List<CreatePendingData> create = new List<CreatePendingData>();

    [JsonProperty("join")]
    public List<JoinPendingData> join = new List<JoinPendingData>();

    [JsonProperty("channel")]
    public List<ChannelPendingData> channel = new List<ChannelPendingData>();
}

public class HandleChannelsPendingDataAndNewBlockData : MonoBehaviour, IXAYAWaitForChange
{
    public GameObject createButtonChannel;

    private Color origColor;
    // Start is called before the first frame update
    void Start()
    {
        XAYAWaitForChange.Instance.objectsRegisteredForWaitForChange.Add(this);
        origColor = createButtonChannel.GetComponent<Image>().color;
    }

    public void OnWaitForChangeNewBlock()
    {

    }

    public void OnWaitForChangeTID(string latestPendingData)
    {
        PendingData data = JsonConvert.DeserializeObject<PendingData>(latestPendingData);
        ShipSDClient.Instance.SetGameStateWithPendingData(data);

        bool createChannelIsPendingForUs = false;

        for(int s =0; s < data.create.Count;s++)
        {
            if(data.create[s].name == XAYASettings.playerName)
            {
                createChannelIsPendingForUs = true;
            }
        }

        if(createChannelIsPendingForUs)
        {
            createButtonChannel.GetComponent<Image>().color = new Color(255, 0, 255, 255);
            createButtonChannel.GetComponent<Button>().enabled = false;
        }
        else
        {
            createButtonChannel.GetComponent<Image>().color = origColor;
            createButtonChannel.GetComponent<Button>().enabled = true;
        }
    }

    public bool SerializedPendingIsDifferent(string latestPendingData)
    {
        return true;
    }

    public void OnWaitForChangeGameChannel()
    {

    }
}
