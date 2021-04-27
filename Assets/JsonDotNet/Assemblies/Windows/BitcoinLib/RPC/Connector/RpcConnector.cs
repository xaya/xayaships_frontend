﻿// Copyright (c) 2014 - 2016 George Kimionis
// See the accompanying file LICENSE for the Software License Aggrement

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using BitcoinLib.Auxiliary;
using BitcoinLib.ExceptionHandling.Rpc;
using BitcoinLib.RPC.RequestResponse;
using BitcoinLib.RPC.Specifications;
using BitcoinLib.Services.Coins.Base;
using Newtonsoft.Json;

namespace BitcoinLib.RPC.Connector
{
    public sealed class RpcConnector : IRpcConnector
    {
        private readonly ICoinService _coinService;
        public int id = 0;

        public RpcConnector(ICoinService coinService)
        {
            _coinService = coinService;
        }

        public T MakeRequest<T>(RpcMethods rpcMethod, params object[] parameters)
        {
            id++;
            var jsonRpcRequest = new JsonRpcRequest(id, rpcMethod.ToString(), parameters);
            var webRequest = (HttpWebRequest)WebRequest.Create(_coinService.Parameters.SelectedDaemonUrl);
            byte[] byteArray;

            if (rpcMethod == RpcMethods.stop) /* Dirty workaround to properly support json notification syntax, lets merge this into library later properly*/
            {
                var jsonRpcRequestNotification = new JsonRpcRequestNotification(rpcMethod.ToString(), parameters);
                SetBasicAuthHeader(webRequest, _coinService.Parameters.RpcUsername, _coinService.Parameters.RpcPassword);
                webRequest.Credentials = new NetworkCredential(_coinService.Parameters.RpcUsername, _coinService.Parameters.RpcPassword);

                webRequest.ContentType = "application/json-rpc";
                webRequest.Method = "POST";
                webRequest.Proxy = null;
                webRequest.Timeout = _coinService.Parameters.RpcRequestTimeoutInSeconds * GlobalConstants.MillisecondsInASecond;
                byteArray = jsonRpcRequestNotification.GetBytes();
                webRequest.ContentLength = jsonRpcRequestNotification.GetBytes().Length;
            }
            else if (rpcMethod == RpcMethods.waitforchange) /* Latest XAYA library addition pases waitforchange arguments as array, this also needs special handling*/
            {
                string[] aRR = new string[parameters.Length];

                for (int s = 0; s < parameters.Length; s++)
                {
                    aRR[s] = parameters[s].ToString();
                }

                var jsonRpcRequestNotification = new JsonRpcRequestArray(id, rpcMethod.ToString(), aRR);
                SetBasicAuthHeader(webRequest, _coinService.Parameters.RpcUsername, _coinService.Parameters.RpcPassword);
                webRequest.Credentials = new NetworkCredential(_coinService.Parameters.RpcUsername, _coinService.Parameters.RpcPassword);

                webRequest.ContentType = "application/json-rpc";
                webRequest.Method = "POST";
                webRequest.Proxy = null;
                webRequest.Timeout = _coinService.Parameters.RpcRequestTimeoutInSeconds * GlobalConstants.MillisecondsInASecond;
                byteArray = jsonRpcRequestNotification.GetBytes();
                webRequest.ContentLength = jsonRpcRequestNotification.GetBytes().Length;
            }
            else
            {
                SetBasicAuthHeader(webRequest, _coinService.Parameters.RpcUsername, _coinService.Parameters.RpcPassword);
                webRequest.Credentials = new NetworkCredential(_coinService.Parameters.RpcUsername, _coinService.Parameters.RpcPassword);

                webRequest.ContentType = "application/json-rpc";
                webRequest.Method = "POST";
                webRequest.Proxy = null;
                webRequest.Timeout = _coinService.Parameters.RpcRequestTimeoutInSeconds * GlobalConstants.MillisecondsInASecond;
                byteArray = jsonRpcRequest.GetBytes();
                webRequest.ContentLength = jsonRpcRequest.GetBytes().Length;
            }

            if (rpcMethod == RpcMethods.waitforchange) /* Dirty workaround to properly support json notification syntax, lets merge this into library later properly*/
            {
                webRequest.Timeout = int.MaxValue;
            }

            try
            {
                using (var dataStream = webRequest.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    dataStream.Dispose();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("There was a problem sending the request to the wallet", exception);
                return default(T);
            }

            try
            {
                string json;

                using (var webResponse = webRequest.GetResponse())
                {
                    using (var stream = webResponse.GetResponseStream())
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            var result = reader.ReadToEnd();
                            reader.Dispose();
                            json = result;
                        }
                    }
                }

                if (json != "")
                {
                    var rpcResponse = JsonConvert.DeserializeObject<JsonRpcResponse<T>>(json);
                    return rpcResponse.Result;
                }
                else
                {
                    return default(T);
                }
            }
            catch (WebException webException)
            {
                #region RPC Internal Server Error (with an Error Code)

                var webResponse = webException.Response as HttpWebResponse;

                if (webResponse != null)
                {
                    switch (webResponse.StatusCode)
                    {
                        case HttpStatusCode.InternalServerError:
                            {
                                using (var stream = webResponse.GetResponseStream())
                                {
                                    if (stream == null)
                                    {
                                        throw new RpcException("The RPC request was either not understood by the server or there was a problem executing the request", webException);
                                    }

                                    using (var reader = new StreamReader(stream))
                                    {
                                        var result = reader.ReadToEnd();
                                        reader.Dispose();

                                        try
                                        {
                                            var jsonRpcResponseObject = JsonConvert.DeserializeObject<JsonRpcResponse<object>>(result);

                                            var internalServerErrorException = new RpcInternalServerErrorException(jsonRpcResponseObject.Error.Message, webException)
                                            {
                                                RpcErrorCode = jsonRpcResponseObject.Error.Code
                                            };

                                            throw internalServerErrorException;
                                        }
                                        catch (JsonException)
                                        {
                                            throw new RpcException(result, webException);
                                        }
                                    }
                                }
                            }

                        default:
                            throw new RpcException("The RPC request was either not understood by the server or there was a problem executing the request", webException);
                    }
                }

                #endregion

                #region RPC Time-Out

                if (webException.Message == "The operation has timed out")
                {
                    throw new RpcRequestTimeoutException(webException.Message);
                }

                #endregion

                return default(T);
            }
            catch (JsonException jsonException)
            {
                throw new RpcResponseDeserializationException("There was a problem deserializing the response from the wallet", jsonException);
            }
            catch (ProtocolViolationException protocolViolationException)
            {
                throw new RpcException("Unable to connect to the server", protocolViolationException);
            }
            catch (Exception exception)
            {
                var queryParameters = jsonRpcRequest.Parameters.Cast<string>().Aggregate(string.Empty, (current, parameter) => current + (parameter + " "));
                throw new Exception($"A problem was encountered while calling MakeRpcRequest() for: {jsonRpcRequest.Method} with parameters: {queryParameters}. \nException: {exception.Message}");
            }
        }

        private static void SetBasicAuthHeader(WebRequest webRequest, string username, string password)
        {
            var authInfo = username + ":" + password;
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            webRequest.Headers["Authorization"] = "Basic " + authInfo;
        }
    }
}