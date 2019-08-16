using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class JsonGeneralResponse 
{
    
}
public class error
{
    [JsonProperty(PropertyName = "code")]
    public int Code { get; set; }

    [JsonProperty(PropertyName = "message")]
    public string Message { get; set; }
}
