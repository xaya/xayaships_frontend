using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;

namespace XAYA
{
    /*Special class for dealing with dara, returned from coroutines, easily*/
    public class CoroutineWithData<T>
    {
        private IEnumerator _target;
        public T result;
        public Coroutine Coroutine { get; private set; }
        public CoroutineWithData(IEnumerator target_)
        {
            _target = target_;
            Coroutine = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<AsyncCoroutineParent>().StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            while (_target.MoveNext())
            {
                try
                {
                    result = (T)_target.Current;

                    if (result == null)
                    {
                        Debug.LogError("Should NEVER get there!!!");
                    }
                }
                catch
                {
                }
                yield return result;
            }
        }
    }

    /// <summary>
    /// Deals with sending JSONRPC requests for each game action in asynchronious manner
    /// </summary>
    public class AsynchroniousRequests
    {

        /// <summary>
        /// RPC request from XID GSP
        /// </summary>
        private IEnumerator HTTPXayaXID(JObject job, bool ignoreDebugLog = false, bool doNotDeleteEmptySpace = false)
        {
            string requestString = JsonConvert.SerializeObject(job, Formatting.None);

            if (doNotDeleteEmptySpace == false)
            {
                requestString = requestString.Replace("\\r\\n", string.Empty).Replace(" ", string.Empty);
                requestString = requestString.Replace("\\n", string.Empty).Replace("\\r", string.Empty).Replace(" ", string.Empty);
            }

            if (ignoreDebugLog == false)
            {
                Debug.Log(requestString.Replace("\r\n", string.Empty).Replace(" ", string.Empty));
            }

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(XAYASettings.XIDServerAddress);
            webRequest.ContentType = "application/json";
            webRequest.Method = "POST";

            byte[] byteArray = Encoding.UTF8.GetBytes(requestString);
            webRequest.ContentLength = byteArray.Length;
            Stream dataStream = webRequest.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            WebResponse webResponse = null;

            try
            {
                webResponse = webRequest.GetResponse();
            }
            catch (WebException ex)
            {
                webResponse = ex.Response;

                if (webResponse != null)
                {
                    WebHeaderCollection header = webResponse.Headers;

                    var encoding = Encoding.ASCII;
                    string responseText = null;
                    using (var read = new StreamReader(webResponse.GetResponseStream(), encoding))
                    {
                        responseText = read.ReadToEnd();
                    }

                    Debug.Log("Exception: " + responseText);
                }
            }

            if (webResponse != null)
            {
                StreamReader reader = new StreamReader(webResponse.GetResponseStream());
                string response = reader.ReadToEnd();
                yield return response;
            }
            else
            {
                yield return "Call failed";
            }
        }

        public IEnumerator XidIsConnected()
        {
            List<object> requestData = new List<object>();
            JArray j = new JArray();

            requestData.Add("getnullstate");
            requestData.Add(j);

            CoroutineWithData<string> coroutine = new CoroutineWithData<string>(this.HTTPXayaXID(this.CreateJObject(requestData)));
            yield return coroutine.Coroutine; while (coroutine.result == null) { yield return new WaitForEndOfFrame(); }

            yield return coroutine.result;
        }

        /// <summary>
        /// Get chat password from the wallet XID, this is for locally running daemon only
        /// </summary>
        /// <param name="paramsSize">The size of the <c>params</c> array in the RPC request</param>
        /// <returns>Returns a Player object containing the requested player</returns>
        public IEnumerator AuthWithWallet(string username)
        {
            List<object> requestData = new List<object>();
            JObject container = new JObject();

            JProperty usernameP = new JProperty("name", username);
            JProperty applicationP = new JProperty("application", "chat.xaya.io");
            JProperty dataP = new JProperty("data", new JObject());

            container.Add(usernameP);
            container.Add(applicationP);
            container.Add(dataP);

            requestData.Add("authwithwallet");
            requestData.Add(container);

            CoroutineWithData<string> coroutine = new CoroutineWithData<string>(this.HTTPXayaXID(this.CreateJObject(requestData), false, true));
            yield return coroutine.Coroutine; while (coroutine.result == null) { yield return new WaitForEndOfFrame(); }

            yield return coroutine.result;
        }

        /// <summary>
        /// Creates a JObject containing the data to be sent in a JSONRPC HTTP request
        /// </summary>
        /// <param name="data">The data to be formatted into a JObject</param>
        /// <returns>Returns the inputted data as a JObject</returns>
        private JObject CreateJObject(List<object> data)
        {
            JObject requestObject = new JObject();

            requestObject.Add(new JProperty("jsonrpc", "2.0"));
            requestObject.Add(new JProperty("id", 1));


            requestObject.Add(new JProperty("method", data[0]));

            if (data.Count > 1)
            {
                requestObject.Add(new JProperty("params", data[1]));
            }

            return requestObject;
        }
    }
}