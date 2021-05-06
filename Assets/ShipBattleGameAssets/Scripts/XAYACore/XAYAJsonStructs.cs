using Newtonsoft.Json;
using System.Collections.Generic;

namespace XAYA
{
    public class PlayerXIDSIgner
    {
        [JsonProperty("addresses")]
        public List<string> addresses { get; set; }
    }

    public class Username
    {

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("ismine")]
        public bool ismine;
    }

    public class PlayerXIDData
    {
        [JsonProperty("addresses")]
        public Dictionary<string, string> addresses { get; set; }

        [JsonProperty("signers")]
        public List<PlayerXIDSIgner> signers { get; set; }
    }

    public class PlayerXIDResult
    {
        [JsonProperty("blockhash")]
        public string blockhash { get; set; }

        [JsonProperty("chain")]
        public string chain { get; set; }

        [JsonProperty("gameid")]
        public string gameid { get; set; }

        [JsonProperty("state")]
        public string state { get; set; }

        [JsonProperty("height")]
        public int height { get; set; }

        [JsonProperty("data")]
        public PlayerXIDData data { get; set; }
    }

    public class PlayerXID
    {
        [JsonProperty("id")]
        public int id { get; set; }

        [JsonProperty("jsonrpc")]
        public string jsonrpc { get; set; }

        [JsonProperty("result")]
        public PlayerXIDResult result { get; set; }
    }
}