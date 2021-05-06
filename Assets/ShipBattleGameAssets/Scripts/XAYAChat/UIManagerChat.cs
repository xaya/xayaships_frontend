using Matrix;
using Matrix.Xmpp.Client;
using Matrix.Xmpp.Muc;
using Matrix.Xmpp.Muc.Admin;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XAYA;

namespace XAYAChat
{
    public class UIManagerChat : MonoBehaviour
    {
        public GameObject HomeScreenObj;
        public HomeScreen HS;
        public UserDetails userDetailsModel;
        public List<string> memberList = new List<string>();

        private MucManager mucManager;

        public static UIManagerChat Instance;

        public void LaunchChat()
        {
            string encodeduser = HexadecimalEncoding.ToHexString(XAYASettings.playerName).ToLower();

            userDetailsModel.username = encodeduser;
            userDetailsModel.password = XAYASettings.XIDAuthPassword;

            XMPPConnection.Instance.Connect(encodeduser, XAYASettings.XIDAuthPassword);
        }

        private void Start()
        {
            Instance = this;
            userDetailsModel = new UserDetails();
            HS.Start();
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
        }

        public string GetRoomMembers()
        {
            mucManager.RequestMemberList(userDetailsModel.roomID);
            return mucManager.ToString();
        }

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
            mucManager.EnterRoom(XAYASettings.chatID + "@muc." + XAYASettings.chatDomainName, UIManagerChat.Instance.userDetailsModel.username);
            userDetailsModel.roomID = XAYASettings.chatID + "@muc." + XAYASettings.chatDomainName;

            OpenChatScreen();
        }

        public void HomeBackButton()
        {
            HomeScreenObj.SetActive(false);
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
    }
}