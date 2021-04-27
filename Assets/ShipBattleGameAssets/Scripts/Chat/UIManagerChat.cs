using Matrix;
using Matrix.Xmpp.Client;
using Matrix.Xmpp.Muc;
using Matrix.Xmpp.Muc.Admin;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManagerChat: MonoBehaviour
{
    #region Public Variables

    public GameObject HomeScreenObj;
    public HomeScreen HS;

    public GameObject LoginScreen;

    public UserDetails userDetailsModel;

    public List<string> memberList = new List<string>();

    #endregion

    #region Private Variables

    private MucManager mucManager;

    public static UIManagerChat Instance;

    #endregion

    #region Unity Methods

    private void Start()
    {
        Instance = this;
        userDetailsModel = new UserDetails();
        HS.Start();
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    private void OnApplicationQuit()
    {
        userDetailsModel.Save();
    }

    #endregion

    #region Common Methods

    public void OffAllScreen()
    {
        HomeScreenObj.SetActive(false);
    }

    public void LogOut()
    {
        if (mucManager != null)
        {
            mucManager.ExitRoom(userDetailsModel.roomID, userDetailsModel.username);
        }

        if (HomeScreen.Instance != null)
        {
            HomeScreen.Instance.MainChatPrefab.gameObject.SetActive(false);
            HomeScreen.Instance.DestroyAllTabs();
            HomeScreen.Instance.DestroyAllMemberButtons();
            HomeScreenObj.SetActive(false);
        }
        //PlayerPrefs.DeleteAll();
    }

    public string GetRoomMembers()
    {
        mucManager.RequestMemberList(userDetailsModel.roomID);

        return mucManager.ToString();
    }

    #endregion

    #region HomeScreen

    public void OpenHomeScreen()
    {
        HomeScreen.Instance.SetSelectedTabJid(userDetailsModel.roomID);
        HomeScreenObj.SetActive(true);

        StartCoroutine(WaitForSecond());

    }

    public IEnumerator WaitForSecond()
    {
        yield return new WaitForSeconds(2);

        // Direct join group chat
        mucManager = new MucManager(XMPPConnection.Instance.xmppClient);
        mucManager.EnterRoom("xs@muc.chat.xaya.io", UIManagerChat.Instance.userDetailsModel.username);
        userDetailsModel.roomID = "xs@muc.chat.xaya.io";

        OpenChatScreen();
    }

    public void HomeBackButton()
    {
        HomeScreenObj.SetActive(false);
        LoginScreen.gameObject.SetActive(true);
    }

    public void SetPresence(Presence presence)
    {
        HomeScreen.Instance.SetPresence(presence);
    }

    public void OpenChatScreen()
    {
        HomeScreen.Instance.SetSelectedTabJid(userDetailsModel.roomID);
        HomeScreenObj.SetActive(true);
        HomeScreen.Instance.InstantiateGroupChatBox();
    }

    public void RecvMsgFromXmpp(Message message)
    {
        if (message.Body == null)
            return;

        if (message.Type.ToString() == ConstantsChat.GroupChat || message.Type.ToString() == ConstantsChat.groupchat)
        {
            HomeScreen.Instance.SetGroupLayout(message);
        }
        if (message.Type.ToString() == ConstantsChat.Chat || message.Type.ToString() == ConstantsChat.chat)
        {

            if (message.Subject != null && message.Body.Equals(ConstantsChat.inviteReason) && message.Subject.Contains(ConstantsChat.adrates))
            {
                MUCHandler.Instance.JionRoomByInvitationMsg(message.Subject);
                return;
            }

            HomeScreen.Instance.SetSingleLayout(message.From.Bare, message.Body, message.Type.ToString());
        }
    }




    #endregion
}
