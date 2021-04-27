using CielaSpike;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;

/*Special class for dealing with dara, returned from coroutines, easily*/
public class CoroutineWithData<T>
{
    private IEnumerator _target;
    public T result;
    public Coroutine Coroutine { get; private set; }
    public CoroutineWithData(IEnumerator target_, MonoBehaviour EnumParent)
    {
        _target = target_;
        Coroutine = EnumParent.StartCoroutine(Run());
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
    /// Get chat password from the wallet XID, this is for locally running daemon only
    /// </summary>
    /// <param name="paramsSize">The size of the <c>params</c> array in the RPC request</param>
    /// <returns>Returns a Player object containing the requested player</returns>
    public IEnumerator AuthWithWallet(string username, MonoBehaviour monoParent)
    {
        CoroutineWithData<string> coroutine = new CoroutineWithData<string>(GameUserManager.Instance.xayaClient.XIDAuthWithWallet(username), monoParent);
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