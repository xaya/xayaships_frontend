using Matrix;
using Matrix.License;
using Matrix.Net;
using Matrix.Xmpp.Client;
using System;
using System.Collections;
using UnityEngine;

public class XMPPConnection : MonoBehaviour
{
    #region Private Variables

    public XmppClient xmppClient = new XmppClient();

    #endregion

    #region Unity Methods

    [HideInInspector]
    public bool loggedIn = false;

    public static XMPPConnection Instance;

    public void Start()
    {
        Instance = this;

        LicenseManager.SetLicense(ConstantsChat.LICENSE);

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

        Debug.Log("Setting license to: " + ConstantsChat.LICENSE);
    }

    private void OnApplicationQuit()
    {
        xmppClient.Close();
        UIManagerChat.Instance.LogOut();
    }

    #endregion

    #region XMPP CallBack Methods

    private void XmppLogin(object sender, Matrix.EventArgs e)
    {
        if (GlobalData.ignoreChatDebugLog == false)
        {
            Debug.Log("onLogin --- " + sender + " e :  " + e.ToString());
        }

        loggedIn = true;
        UnityMainThreadDispatcher.Instance().Enqueue(OpenHomeScreen());
    }

    private void XmppRegister(object sender, Matrix.EventArgs e)
    {
        if (GlobalData.ignoreChatDebugLog == false)
        {
            Debug.Log("OnRegister");
        }
    }

    private void XmppClient_OnValidateCertificate(object sender, CertificateEventArgs e)
    {
        e.AcceptCertificate = true;

        if (GlobalData.ignoreChatDebugLog == false)
        {
            Debug.Log("OnValidateCertificate --- " + sender + " e :  " + e.ToString());
        }
    }

    private void XmppOnError(object sender, ExceptionEventArgs e)
    {
        if (GlobalData.ignoreChatDebugLog == false)
        {
            Debug.Log("OnError : " + e.ToString() + " ........... " + sender.ToString());
        }

        // Close connection
        UnityMainThreadDispatcher.Instance().Enqueue(ShowErrorMsg());
    }

    private void XmppOnPresence(object sender, PresenceEventArgs e)
    {
        if (GlobalData.ignoreChatDebugLog == false)
        {
            Debug.Log("OnPresence : " + e.ToString());
        }

        UnityMainThreadDispatcher.Instance().Enqueue(GetPresence(e.Presence));
    }

    void XmppClientOnReceiveXml(object sender, TextEventArgs e)
    {
        if (GlobalData.ignoreChatDebugLog == false)
        {
            AddDebug("RECV: " + e.Text);
        }
    }

    void XmppClientOnSendXml(object sender, TextEventArgs e)
    {
        if (GlobalData.ignoreChatDebugLog == false)
        {
            AddDebug("SEND: " + e.Text);
        }
    }

    private void xmppClient_OnMessage(object sender, MessageEventArgs e)
    {
        if (GlobalData.ignoreChatDebugLog == false)
        {
            Debug.Log(string.Format("OnMessage from {0}", e.Message));
        }

        UnityMainThreadDispatcher.Instance().Enqueue(RecvMsgFromXmpp(e.Message));
    }

    private void XmppClient_OnSendBody(object sender, BodyEventArgs e)
    {
        if (GlobalData.ignoreChatDebugLog == false)
        {
            Debug.Log(string.Format("OnSendBody {0}", e.Body));
        }
    }

    private void XmppClient_OnReceiveBody(object sender, BodyEventArgs e)
    {
        if (GlobalData.ignoreChatDebugLog == false)
        {
            Debug.Log(string.Format("OnReceiveBody {0}", e.Body));
        }
    }

    private void XmppClient_OnClose(object sender, Matrix.EventArgs e)
    {
        if (GlobalData.ignoreChatDebugLog == false)
        {
            Debug.Log("onClose --- " + e.ToString());
        }

        UnityMainThreadDispatcher.Instance().Enqueue(CloseConnectionXmpp());
    }

    #endregion

    #region User Define Methods

    void AddDebug(string debug)
    {
        if (GlobalData.ignoreChatDebugLog == false)
        {
            Debug.Log(debug);
        }
    }

    public IEnumerator CloseConnectionXmpp()
    {
        xmppClient.Close();
        yield return null;
    }

    // Close connection
    public void CloseConnection()
    {
        xmppClient.Close();
    }

    private IEnumerator ShowErrorMsg()
    {
        ToastManager.Show("Something went wrong");

        // For show toast in pc
        if (Application.platform == RuntimePlatform.WindowsPlayer)
            SSTools.ShowMessage("Something went wrong", SSTools.Position.bottom, SSTools.Time.twoSecond);

        yield return null;
    }

    // Connect (Login)
    public void Connect(string username, string password)
    {
        xmppClient.SetUsername(username);
        xmppClient.SetXmppDomain(ConstantsChat.domainName);

        UIManagerChat.Instance.userDetailsModel.domainName = ConstantsChat.domainName;

        xmppClient.Password = password;
        xmppClient.AutoRoster = true;
        xmppClient.Status = "I'm chatty";
        xmppClient.Show = Matrix.Xmpp.Show.Chat;

        xmppClient.Open();
    }

    // Send message to single person
    public void SendMessageTOPerson(string username, string sendMsg)
    {
        if (GlobalData.ignoreChatDebugLog == false)
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

    // Send invite message
    public void SendInviteMessageTOPerson(string username, string sendMsg, string roomID)
    {
        if (GlobalData.ignoreChatDebugLog == false)
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

    // Join chat group
    public void JoinRoom(string roomIDD, string username)
    {

    }

    // Send msg to group
    public void SendMessageToRoom(string sendMsgString, string roomID)
    {
        if (GlobalData.ignoreChatDebugLog == false)
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

    #endregion
}
