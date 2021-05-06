using Matrix;
using Matrix.Xmpp;
using Matrix.Xmpp.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XAYA;

namespace XAYAChat
{
    public class HomeScreen : MonoBehaviour
    {

        #region Public Variables

        public GameObject MainChatPrefab;

        public readonly Dictionary<string, TabMatrix> Tabs = new Dictionary<string, TabMatrix>();

        public readonly Dictionary<string, MemberMatrix> membersMatrix = new Dictionary<string, MemberMatrix>();

        public Text CurrentChannelText;

        public Toggle TabToggleToInstantiate;

        public Transform tabContent;

        public GameObject chatNotificationIcon;

        [Header("InputField")]
        public TMP_InputField roomIDInputField;
        public InputField ChatInputField;

        [Header("Button")]
        public Button joinRoomButton;
        public Button MemberButtonToInstantiate;

        #endregion

        #region Private Variables

        private string roomIDq;

        private Jid selectedTabJid;

        //private string SIMPLE_NAME_CHARS = string.digits + string.ascii_lowercase'

        public static HomeScreen Instance;

        #endregion

        #region Unity Methods

        public void Start()
        {
            Instance = this;

            roomIDInputField.text = XAYASettings.defaultChatRoomID;
            TabToggleToInstantiate.gameObject.SetActive(false);
            MemberButtonToInstantiate.gameObject.SetActive(false);
        }

        #endregion

        #region User Define Methods

        public void OnRoomIDInputFieldEditEnded(string roomIDString)
        {
            if (!string.IsNullOrEmpty(roomIDString))
            {
                roomIDq = roomIDString;
            }

            CheckAndEnableJoinRoomButton();
        }

        private void CheckAndEnableJoinRoomButton()
        {
            bool isRoomIDEnter = !string.IsNullOrEmpty(roomIDq);

            joinRoomButton.interactable = (isRoomIDEnter);
        }

        public void OnJoinRoom()
        {
            string roomID = roomIDInputField.text.Trim();

            // Check that RoomID is not null
            if (string.IsNullOrEmpty(roomID))
            {
                Debug.Log("ROOM ID IS EMPTY");
                ToastManager.Show("Room ID IS EMPTY");

                // For show toast in pc
                if (Application.platform == RuntimePlatform.WindowsPlayer)
                    SSTools.ShowMessage("Room ID IS EMPTY", SSTools.Position.bottom, SSTools.Time.oneSecond);
                return;
            }
        }

        public void SetSelectedTabJid(string roomID)
        {
            selectedTabJid = roomID;
        }

        // Active main chat prefab
        public void InstantiateGroupChatBox()
        {
            MainChatPrefab.gameObject.SetActive(true);
        }

        // Set Presence
        public void SetPresence(Presence presence)
        {
            if (presence.To.Resource == presence.From.Resource)
            {
                return;
            }

            if (presence.Type == PresenceType.Available)
            {
                InstantiateMemberButton(presence.From);
            }
            else if (presence.Type == PresenceType.Unavailable)
            {
                string xayaName = DecodeXmppName(presence.From.Resource);

                if (xayaName == null)
                    xayaName = presence.From.Resource;

                TabMatrix tempTab = null;

                // Check group chat tab exist
                if (this.Tabs.TryGetValue(presence.From.Bare.Split('@')[0], out tempTab))
                {
                    tempTab.GetComponent<Toggle>().isOn = false;

                    tempTab.Add(xayaName, ConstantsChat.systemTxtColorBoldStart + xayaName + ConstantsChat.systemTxtColorBoldEnd + ConstantsChat.systemTxtColorNormalStart + ConstantsChat.leaveChannel + ConstantsChat.systemTxtColorNormalEnd);

                    if (!MainChatPrefab.activeInHierarchy)
                        chatNotificationIcon.gameObject.SetActive(true);
                    else
                        chatNotificationIcon.gameObject.SetActive(false);

                    if (selectedTabJid != null && (selectedTabJid.Equals(presence.From.Bare) || selectedTabJid.Equals(xayaName.Split('@')[0])))
                    {
                        CurrentChannelText.text = tempTab.ToStringMessages();
                    }

                    // Remove member button
                    membersMatrix.Remove(xayaName + presence.From.Bare.Split('@')[0]);
                    tempTab.RemoveMember(xayaName + presence.From.Bare.Split('@')[0]);

                    // Destroy button
                    foreach (Transform t in tempTab.memberParentTransform)
                    {
                        if (t.name.Equals(xayaName + presence.From.Bare.Split('@')[0]))
                            Destroy(t.gameObject);
                    }
                }
            }
        }

        // Create member button
        private void InstantiateMemberButton(Jid jid)
        {
            // check jid is equal username
            if (jid.Resource != null && (jid.Resource == UIManagerChat.Instance.userDetailsModel.roomID || jid.Resource == UIManagerChat.Instance.userDetailsModel.username || jid.Resource.StartsWith(ConstantsChat.xabberandroidOU1xyto0)))
            {
                return;
            }

            if (jid.Resource != null)
            {
                MemberMatrix member = null;

                string xayaName = DecodeXmppName(jid.Resource);

                if (xayaName == null)
                    xayaName = jid.Resource;

                // Check that member button is not exist
                if (!this.membersMatrix.TryGetValue(xayaName + jid.Bare.Split('@')[0], out member))
                {
                    // Create button
                    Button cbtn = (Button)GameObject.Instantiate(MemberButtonToInstantiate);
                    cbtn.gameObject.name = xayaName + jid.Bare.Split('@')[0];
                    cbtn.gameObject.SetActive(true);
                    cbtn.GetComponentInChildren<MemberMatrix>().SetProperties(xayaName);

                    member = cbtn.GetComponentInChildren<MemberMatrix>();

                    // Add member button
                    membersMatrix.Add(xayaName + jid.Bare.Split('@')[0], member);

                    TabMatrix tempTab = null;

                    //Check that groupchat tab exist 
                    if (this.Tabs.TryGetValue(jid.Bare.Split('@')[0], out tempTab))
                    {
                        tempTab.GetComponent<Toggle>().isOn = false;

                        // Add message that new member join chat
                        tempTab.Add(xayaName, ConstantsChat.systemTxtColorBoldStart + xayaName + ConstantsChat.systemTxtColorBoldEnd + ConstantsChat.systemTxtColorNormalStart + ConstantsChat.joinChannel + ConstantsChat.systemTxtColorNormalEnd);

                        tempTab.AddMember(cbtn.gameObject);

                        if (!MainChatPrefab.activeInHierarchy)
                            chatNotificationIcon.gameObject.SetActive(true);
                        else
                            chatNotificationIcon.gameObject.SetActive(false);

                        if (selectedTabJid != null && (selectedTabJid.Equals(jid.Bare) || selectedTabJid.Equals(jid.Bare.Split('@')[0])))
                        {
                            CurrentChannelText.text = tempTab.ToStringMessages();

                            cbtn.transform.SetParent(tempTab.memberParentTransform, false);
                        }
                    }
                }
                else
                {
                    TabMatrix tempTab = null;

                    //Check that groupchat tab exist 
                    if (this.Tabs.TryGetValue(jid.Bare.Split('@')[0], out tempTab))
                    {
                        tempTab.GetComponent<Toggle>().isOn = false;

                        // Add message that existing member join chat
                        tempTab.Add(xayaName, ConstantsChat.systemTxtColorBoldStart + xayaName + ConstantsChat.systemTxtColorBoldEnd + ConstantsChat.systemTxtColorNormalStart + ConstantsChat.joinChannel + ConstantsChat.systemTxtColorNormalEnd);

                        if (!tempTab.members.Contains(member.gameObject))
                        {
                            tempTab.AddMember(member.gameObject);
                        }


                        if (!MainChatPrefab.activeInHierarchy)
                            chatNotificationIcon.gameObject.SetActive(true);
                        else
                            chatNotificationIcon.gameObject.SetActive(false);

                        if (selectedTabJid != null && (selectedTabJid.Equals(jid.Bare) || selectedTabJid.Equals(jid.Bare.Split('@')[0])))
                        {
                            CurrentChannelText.text = tempTab.ToStringMessages();
                        }
                    }
                }
            }
            else
            {
                TabMatrix tab = GetTab(jid, MessageType.GroupChat);
                tab.GetComponent<Toggle>().isOn = false;

            }
        }

        // Decode name
        public string DecodeXmppName(string name)
        {
            if (name.Length == 0)
            {
                return null;
            }
            else
            {
                if (name.Contains("x-"))
                {
                    var c = name.Substring(2).ToCharArray();
                    foreach (var a in c)
                    {
                        if (!ConstantsChat.SIMPLE_NAME_CHARS.Contains(a))
                        {
                            Debug.Log("Invalide xmpp name : " + name);
                            return null;
                        }
                    }

                    var hexPart = c;
                    foreach (var h in hexPart)
                    {
                        if (!ConstantsChat.HEX_CHARS.Contains(h))
                        {
                            Debug.Log("Invalide xmpp name : " + name);
                            return null;
                        }
                    }

                    if (hexPart.Length % 2 != 0)
                    {
                        Debug.Log("Odd length hex part : " + name);
                        return null;
                    }

                    string tempname = name.Substring(2);

                    byte[] bytes = Enumerable.Range(0, tempname.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(tempname.Substring(x, 2), 16)).ToArray();

                    name = System.Text.Encoding.UTF8.GetString(bytes);
                }
            }

            return name;
        }

        // Set group chat layout
        public void SetGroupLayout(Message message)
        {
            Jid tabJid = message.From.Bare;

            TabMatrix tab = GetTab(tabJid, MessageType.GroupChat);

            if (!tab.GetComponent<Toggle>().isOn)
                tab.notificationIcon.SetActive(true);

            if (tab.Jid.Equals(ConstantsChat.defaultTab))
                tab.gameObject.transform.SetAsFirstSibling();

            if (!MainChatPrefab.activeInHierarchy)
                chatNotificationIcon.gameObject.SetActive(true);
            else
                chatNotificationIcon.gameObject.SetActive(false);

            string xayaName = DecodeXmppName(message.From.Resource);

            if (xayaName == null)
                xayaName = message.From.Resource;

            // Add message in tab
            if (message.From.Resource.Equals(UIManagerChat.Instance.userDetailsModel.username))
                tab.Add(tabJid, ConstantsChat.userTxtColorBoldStart + xayaName + ConstantsChat.userTxtColorBoldEnd + message.Body);
            else
                tab.Add(tabJid, ConstantsChat.memberTxtColorBoldStart + xayaName + ConstantsChat.memberTxtColorBoldEnd + message.Body);

            // Add message to current select tab
            if (selectedTabJid.Equals(tabJid) || (selectedTabJid.Equals(UIManagerChat.Instance.userDetailsModel.roomID) || selectedTabJid.Equals(tabJid.Bare.Split('@')[0])))
            {
                CurrentChannelText.text = tab.ToStringMessages();
            }
        }

        // Set one to one chat layout
        public void SetSingleLayout(string from, string msg, string type)
        {
            string xayaName = DecodeXmppName(from);

            if (xayaName == null)
                xayaName = from;

            Jid tabJid = xayaName;

            TabMatrix tab = GetTab(tabJid, MessageType.Chat);

            tab.GetComponent<Toggle>().isOn = false;

            //tab.closeButton.SetActive(true);

            if (!MainChatPrefab.activeInHierarchy)
                chatNotificationIcon.gameObject.SetActive(true);

            if (!tab.GetComponent<Toggle>().isOn)
                tab.notificationIcon.SetActive(true);



            if (xayaName.ToString().Contains("@"))
            {
                tab.Add(tabJid, ConstantsChat.memberTxtColorBoldStart + xayaName.Split('@').First() + ConstantsChat.memberTxtColorBoldEnd + msg);
            }
            else
            {
                tab.Add(tabJid, ConstantsChat.memberTxtColorBoldStart + xayaName + ConstantsChat.memberTxtColorBoldEnd + msg);
            }

            if (selectedTabJid != null && selectedTabJid.Equals(tabJid.Bare.Split('@')[0]))
            {
                CurrentChannelText.text = tab.ToStringMessages();
            }
        }

        public void OnClickSend()
        {
            if (ChatInputField != null)
            {
                SendChatMessage(ChatInputField.text);
                ChatInputField.text = "";
            }
        }

        public void OnEnterSend()
        {
            if (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter))
            {
                SendChatMessage(ChatInputField.text);
                ChatInputField.text = "";
                ChatInputField.ActivateInputField();

            }
        }

        // Set chat message 
        private void SendChatMessage(string inputLine)
        {
            if (string.IsNullOrEmpty(inputLine))
            {
                return;
            }

            TabMatrix tab = null;

            bool found = Tabs.TryGetValue(selectedTabJid.Bare.Split('@')[0], out tab);

            if (!found)
            {
                Debug.Log("ShowTab failed to find channel: " + selectedTabJid);
                return;
            }

            // Check that message start with "/tell" and message type group chat
            if (inputLine.StartsWith(ConstantsChat.tell))
            {
                string[] words = inputLine.Split(' ');

                // Get username
                string username = words[1];

                // Get msg
                var tempList = new List<string>(words);
                tempList.Remove(words.ElementAt(0));
                tempList.Remove(words.ElementAt(1));
                words = tempList.ToArray();
                string finalMsg = string.Join(" ", words);

                // Send msg to person
                XMPPConnection.Instance.SendMessageTOPerson((username + ConstantsChat.adrates + UIManagerChat.Instance.userDetailsModel.domainName), finalMsg);

                // Set one to one chat layout by user
                SetSingleLayoutFromUser(UIManagerChat.Instance.userDetailsModel.username, username + ConstantsChat.adrates + UIManagerChat.Instance.userDetailsModel.domainName, finalMsg, MessageType.Chat.ToString());

            }
            // Message start with "/room"
            else if (inputLine.StartsWith(ConstantsChat.create))
            {
                string[] words = inputLine.Split(' ');

                // Get roomid
                string newRoomID = words[1];

                List<string> keyList = new List<string>(Tabs.Keys);

                foreach (string key in keyList)
                {
                    // Open needed tab
                    if (key.Equals(newRoomID.Split('@')[0]))
                    {
                        ToastManager.Show("Room alredy exist");

                        // For show toast in pc
                        if (Application.platform == RuntimePlatform.WindowsPlayer)
                            SSTools.ShowMessage("Room alredy exist", SSTools.Position.bottom, SSTools.Time.twoSecond);
                        return;
                    }
                }

                // Create room
                MUCHandler.Instance.CreateNewRoom(newRoomID, UIManagerChat.Instance.userDetailsModel.username);
            }
            // Message start with "/invite"
            else if (tab.messageType == MessageType.GroupChat && inputLine.StartsWith(ConstantsChat.invite))
            {
                string[] words = inputLine.Split(' ');

                // Get username
                string username = words[1];

                // Check member alredy added or not
                bool isMemberExist = tab.CheckMemberExist(username + tab.Jid);

                if (isMemberExist)
                {
                    ToastManager.Show("Member alredy exist");

                    // For show toast in pc
                    if (Application.platform == RuntimePlatform.WindowsPlayer)
                        SSTools.ShowMessage("Member alredy exist", SSTools.Position.bottom, SSTools.Time.twoSecond);

                    return;
                }

                // Send invite message 
                XMPPConnection.Instance.SendInviteMessageTOPerson((username + ConstantsChat.adrates + UIManagerChat.Instance.userDetailsModel.domainName), ConstantsChat.inviteReason, (tab.Jid + ConstantsChat.muc + UIManagerChat.Instance.userDetailsModel.domainName));

            }
            else
            {
                if (inputLine.StartsWith(ConstantsChat.seperator))
                {
                    ToastManager.Show("Invalide command!");

                    // For show toast in pc
                    if (Application.platform == RuntimePlatform.WindowsPlayer)
                        SSTools.ShowMessage("Invalide command!", SSTools.Position.bottom, SSTools.Time.twoSecond);

                    return;
                }

                if (tab.messageType == MessageType.Chat)
                {
                    // Send msg to person

                    if (tab.Jid.ToString().Contains("@"))
                        XMPPConnection.Instance.SendMessageTOPerson(tab.Jid, inputLine);
                    else
                        XMPPConnection.Instance.SendMessageTOPerson(tab.Jid + ConstantsChat.adrates + UIManagerChat.Instance.userDetailsModel.domainName, inputLine);

                    SetSingleLayoutFromUser(UIManagerChat.Instance.userDetailsModel.username, tab.Jid, inputLine, MessageType.Chat.ToString());
                }
                else
                {
                    // Send msg to room
                    XMPPConnection.Instance.SendMessageToRoom(inputLine, tab.Jid + ConstantsChat.muc + UIManagerChat.Instance.userDetailsModel.domainName);
                }
            }
        }

        // Set messge layout of one to one by user
        public void SetSingleLayoutFromUser(string from, string to, string msg, string type)
        {
            Jid tabJid = to;

            TabMatrix tab = GetTab(tabJid, MessageType.Chat);

            ShowTab(tabJid, MessageType.Chat);

            tab.closeButton.SetActive(true);

            string xayaName = DecodeXmppName(from);

            if (xayaName == null)
                xayaName = from;

            if (xayaName.ToString().Contains("@"))
            {
                tab.Add(tabJid, ConstantsChat.userTxtColorBoldStart + xayaName.Split('@').First() + ConstantsChat.userTxtColorBoldEnd + msg);
            }
            else
            {
                tab.Add(tabJid, ConstantsChat.userTxtColorBoldStart + xayaName + ConstantsChat.userTxtColorBoldEnd + msg);
            }

            if (selectedTabJid.Equals(tabJid.Bare.Split('@')[0]))
            {
                CurrentChannelText.text = tab.ToStringMessages();
            }
        }

        // Get tab with message type
        private TabMatrix GetTab(Jid jid, MessageType messageType)
        {
            TabMatrix tab = null;

            // Check that tab not exist
            if (!this.Tabs.TryGetValue(jid.Bare.Split('@')[0], out tab))
            {
                // create tab
                Toggle cbtn = (Toggle)GameObject.Instantiate(this.TabToggleToInstantiate);
                cbtn.gameObject.SetActive(true);
                cbtn.GetComponentInChildren<TabMatrix>().SetChannel(jid, messageType);
                cbtn.transform.SetParent(tabContent.transform, false);
                tab = cbtn.GetComponentInChildren<TabMatrix>();

                Tabs.Add(jid.Bare.Split('@')[0], tab);
            }
            return tab;
        }

        private MemberMatrix GetMember(Jid jid)
        {
            MemberMatrix member = null;

            //Create new button for show indivusial chat member
            if (!this.membersMatrix.TryGetValue(jid + ConstantsChat.chatBtn, out member))
            {
                // Create button
                Button cbtn = (Button)GameObject.Instantiate(MemberButtonToInstantiate);
                cbtn.gameObject.name = jid + ConstantsChat.chatBtn;
                cbtn.gameObject.SetActive(true);
                cbtn.GetComponentInChildren<MemberMatrix>().SetProperties(jid + ConstantsChat.chatBtn);
                member = cbtn.GetComponentInChildren<MemberMatrix>();

                membersMatrix.Add(jid + ConstantsChat.chatBtn, member);
            }
            return member;
        }

        // Show tab by message type
        public void ShowTab(Jid jid, MessageType messageType)
        {
            if (string.IsNullOrEmpty(jid) || jid.Resource == UIManagerChat.Instance.userDetailsModel.roomID)
            {
                return;
            }

            TabMatrix tab = GetTab(jid, messageType);

            selectedTabJid = jid.Bare.Split('@')[0];

            CurrentChannelText.text = tab.ToStringMessages();

            if (tab.messageType == MessageType.Chat)
            {
                tab.closeButton.SetActive(true);


                MemberMatrix member = GetMember(jid.Bare.Split('@')[0]);

                tab.AddMember(member.gameObject);

                if (selectedTabJid != null && (selectedTabJid.Equals(jid.Bare.Split('@')[0]) || selectedTabJid.Equals(jid.Bare.Split('@')[0])))
                {
                    member.transform.SetParent(tab.memberParentTransform, false);
                }
            }

            tab.notificationIcon.SetActive(false);

            chatNotificationIcon.gameObject.SetActive(false);

            List<string> keyList = new List<string>(Tabs.Keys);

            foreach (Transform t in tab.memberParentTransform)
            {
                t.transform.gameObject.SetActive(false);
            }

            foreach (string key in keyList)
            {
                // Open needed tab
                if (key.Equals(jid.Bare.Split('@')[0]))
                {
                    Tabs[jid.Bare.Split('@')[0]].GetComponent<Toggle>().isOn = true;
                    Tabs[jid.Bare.Split('@')[0]].GetComponent<TabMatrix>().ShowMember();
                }

                // CLose other tabs
                else
                {
                    Tabs[key].GetComponent<Toggle>().isOn = false;
                }
            }
        }

        public void BackButton()
        {
            UIManagerChat.Instance.HomeBackButton();
        }

        public void MinimizeChatBox()
        {
            if (MainChatPrefab.activeInHierarchy)
            {
                MainChatPrefab.gameObject.SetActive(false);
                chatNotificationIcon.gameObject.SetActive(false);
            }
            else
            {
                MainChatPrefab.gameObject.SetActive(true);
                chatNotificationIcon.gameObject.SetActive(false);
            }
        }

        public void LogOutButton()
        {
            XMPPConnection.Instance.CloseConnection();
            UIManagerChat.Instance.LogOut();
        }

        public void DestroyAllTabs()
        {
            foreach (Transform t in TabToggleToInstantiate.transform.parent)
            {
                if (t.GetComponent<TabMatrix>().Jid != null && Tabs.ContainsKey(t.GetComponent<TabMatrix>().Jid))
                    Destroy(t.gameObject);
            }
            Tabs.Clear();
        }

        public void DestroyAllMemberButtons()
        {
            foreach (Transform m in MemberButtonToInstantiate.transform.parent)
            {
                if (m.GetComponent<MemberMatrix>().Jid != null && membersMatrix.ContainsKey(m.GetComponent<MemberMatrix>().Jid))
                    Destroy(m.gameObject);
            }
            membersMatrix.Clear();
        }

        #endregion
    }
}