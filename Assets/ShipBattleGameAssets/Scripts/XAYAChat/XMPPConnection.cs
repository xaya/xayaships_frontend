using Matrix;
using Matrix.License;
using Matrix.Net;
using Matrix.Xmpp.Client;
using System;
using System.Collections;
using UnityEngine;
using XAYA;
namespace XAYAChat
{
    public class XMPPConnection : MonoBehaviour
    {

        public XmppClient xmppClient = new XmppClient();
        [HideInInspector]
        public bool loggedIn = false;

        public static XMPPConnection Instance;

        public void Start()
        {
            Instance = this;

            LicenseManager.SetLicense(XAYASettings.chatLicense);

            xmppClient.OnLogin += XmppLogin;
            xmppClient.OnRegister += XmppRegister;
            xmppClient.OnValidateCertificate += XmppClient_OnValidateCertificate;
            xmppClient.OnError += XmppOnError;
            xmppClient.OnPresence += XmppOnPresence;
            xmppClient.OnMessage += xmppClient_OnMessage;
            xmppClient.OnSendBody += XmppClient_OnSendBody;
            xmppClient.OnReceiveBody += XmppClient_OnReceiveBody;
            xmppClient.OnReceiveXml += new EventHandler<TextEventArgs>(XmppClientOnReceiveXml);
            xmppClient.OnSendXml += new EventHandler<TextEventArgs>(XmppClientOnSendXml);
            xmppClient.OnClose += XmppClient_OnClose;

            if (XAYASettings.ignoreChatDebugLog == false)
            {
                Debug.Log("Setting license to: " + XAYASettings.chatLicense);
            }
        }

        private void OnApplicationQuit()
        {
            xmppClient.Close();
            UIManagerChat.Instance.LogOut();
        }

        private void XmppLogin(object sender, Matrix.EventArgs e)
        {
            if (XAYASettings.ignoreChatDebugLog == false)
            {
                Debug.Log("onLogin --- " + sender + " e :  " + e.ToString());
            }

            loggedIn = true;
            UnityMainThreadDispatcher.Instance().Enqueue(OpenHomeScreen());
        }

        private void XmppRegister(object sender, Matrix.EventArgs e)
        {
            if (XAYASettings.ignoreChatDebugLog == false)
            {
                Debug.Log("OnRegister");
            }
        }

        private void XmppClient_OnValidateCertificate(object sender, CertificateEventArgs e)
        {
            e.AcceptCertificate = true;

            if (XAYASettings.ignoreChatDebugLog == false)
            {
                Debug.Log("OnValidateCertificate --- " + sender + " e :  " + e.ToString());
            }
        }

        private void XmppOnError(object sender, ExceptionEventArgs e)
        {
            if (XAYASettings.ignoreChatDebugLog == false)
            {
                Debug.Log("OnError : " + e.ToString() + " ........... " + sender.ToString());
            }

            UnityMainThreadDispatcher.Instance().Enqueue(ShowErrorMsg());
        }

        private void XmppOnPresence(object sender, PresenceEventArgs e)
        {
            if (XAYASettings.ignoreChatDebugLog == false)
            {
                Debug.Log("OnPresence : " + e.ToString());
            }

            UnityMainThreadDispatcher.Instance().Enqueue(GetPresence(e.Presence));
        }

        void XmppClientOnReceiveXml(object sender, TextEventArgs e)
        {
            if (XAYASettings.ignoreChatDebugLog == false)
            {
                AddDebug("RECV: " + e.Text);
            }
        }

        void XmppClientOnSendXml(object sender, TextEventArgs e)
        {
            if (XAYASettings.ignoreChatDebugLog == false)
            {
                AddDebug("SEND: " + e.Text);
            }
        }

        private void xmppClient_OnMessage(object sender, MessageEventArgs e)
        {
            if (XAYASettings.ignoreChatDebugLog == false)
            {
                Debug.Log(string.Format("OnMessage from {0}", e.Message));
            }

            UnityMainThreadDispatcher.Instance().Enqueue(RecvMsgFromXmpp(e.Message));
        }

        private void XmppClient_OnSendBody(object sender, BodyEventArgs e)
        {
            if (XAYASettings.ignoreChatDebugLog == false)
            {
                Debug.Log(string.Format("OnSendBody {0}", e.Body));
            }
        }

        private void XmppClient_OnReceiveBody(object sender, BodyEventArgs e)
        {
            if (XAYASettings.ignoreChatDebugLog == false)
            {
                Debug.Log(string.Format("OnReceiveBody {0}", e.Body));
            }
        }

        private void XmppClient_OnClose(object sender, Matrix.EventArgs e)
        {
            if (XAYASettings.ignoreChatDebugLog == false)
            {
                Debug.Log("onClose --- " + e.ToString());
            }

            UnityMainThreadDispatcher.Instance().Enqueue(CloseConnectionXmpp());
        }

        void AddDebug(string debug)
        {
            if (XAYASettings.ignoreChatDebugLog == false)
            {
                Debug.Log(debug);
            }
        }

        public IEnumerator CloseConnectionXmpp()
        {
            xmppClient.Close();
            yield return null;
        }

        public void CloseConnection()
        {
            xmppClient.Close();
        }

        private IEnumerator ShowErrorMsg()
        {
            ToastManager.Show("Something went wrong");

            if (Application.platform == RuntimePlatform.WindowsPlayer)
                SSTools.ShowMessage("Something went wrong", SSTools.Position.bottom, SSTools.Time.twoSecond);

            yield return null;
        }

        public void Connect(string username, string password)
        {
            xmppClient.SetUsername(username);
            xmppClient.SetXmppDomain(XAYASettings.chatDomainName);

            UIManagerChat.Instance.userDetailsModel.domainName = XAYASettings.chatDomainName;

            xmppClient.Password = password;
            xmppClient.AutoRoster = true;
            xmppClient.Status = "I'm chatty";
            xmppClient.Show = Matrix.Xmpp.Show.Chat;

            xmppClient.Open();
        }

        public void SendMessageTOPerson(string username, string sendMsg)
        {
            if (XAYASettings.ignoreChatDebugLog == false)
            {
                Debug.Log("Sending Message");
            }

            var msg = new Matrix.Xmpp.Client.Message
            {
                Type = Matrix.Xmpp.MessageType.Chat,
                To = username,
                Body = sendMsg,
            };
            xmppClient.Send(msg);
        }

        public void SendInviteMessageTOPerson(string username, string sendMsg, string roomID)
        {
            if (XAYASettings.ignoreChatDebugLog == false)
            {
                Debug.Log("Sending Message");
            }

            var msg = new Matrix.Xmpp.Client.Message
            {
                Type = Matrix.Xmpp.MessageType.Chat,
                To = username,
                Body = sendMsg,
                Subject = roomID
            };
            xmppClient.Send(msg);
        }

        public void SendMessageToRoom(string sendMsgString, string roomID)
        {
            if (XAYASettings.ignoreChatDebugLog == false)
            {
                Debug.Log("Sending Message To Group");
            }

            var msg = new Matrix.Xmpp.Client.Message
            {
                Type = Matrix.Xmpp.MessageType.GroupChat,
                To = roomID,
                Body = sendMsgString
            };
            xmppClient.Send(msg);
        }

        public IEnumerator RecvMsgFromXmpp(Message message)
        {
            UIManagerChat.Instance.RecvMsgFromXmpp(message);
            yield return null;
        }

        public IEnumerator OpenHomeScreen()
        {
            UIManagerChat.Instance.OpenHomeScreen();
            yield return null;
        }

        public IEnumerator GetPresence(Presence presence)
        {
            UIManagerChat.Instance.SetPresence(presence);
            yield return null;
        }
    }
}
