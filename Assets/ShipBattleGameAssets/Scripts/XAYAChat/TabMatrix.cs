using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Matrix;
using System.Linq;

namespace XAYAChat
{
    public class TabMatrix : MonoBehaviour, IPointerClickHandler
    {
        #region Public Variables

        public Jid Jid;

        public Matrix.Xmpp.MessageType messageType;

        public readonly List<Jid> Members = new List<Jid>();

        public readonly List<object> Messages = new List<object>();

        public List<GameObject> members = new List<GameObject>();

        public int MessageLimit = 2000;

        public GameObject closeButton;

        public GameObject notificationIcon;

        public Transform memberParentTransform;

        #endregion

        #region User Define Methods

        public void SetChannel(string jid, Matrix.Xmpp.MessageType msgType)
        {
            Jid = jid;
            messageType = msgType;

            if (jid.Equals(ConstantsChat.defaultTab))
            {
                GetComponentInChildren<Text>().text = ConstantsChat.defaultTabText;
            }
            else
                GetComponentInChildren<Text>().text = jid;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            HomeScreen handler = HomeScreen.Instance;
            handler.ShowTab(Jid, messageType);
        }

        public void Add(string sender, object message)
        {
            this.Messages.Add(message);
            this.TruncateMessages();
        }

        public void TruncateMessages()
        {
            if (this.MessageLimit <= 0 || this.Messages.Count <= this.MessageLimit)
            {
                return;
            }

            int excessCount = this.Messages.Count - this.MessageLimit;
            this.Messages.RemoveRange(0, excessCount);
        }

        public string ToStringMessages()
        {
            StringBuilder txt = new StringBuilder();
            for (int i = 0; i < this.Messages.Count; i++)
            {
                txt.Append(string.Format("\n{0}", this.Messages[i]));
            }
            return txt.ToString();
        }

        // Destroy tab
        public void CloseTab()
        {
            List<string> keyList = new List<string>(HomeScreen.Instance.Tabs.Keys);

            foreach (string key in keyList)
            {
                if (key.Equals(Jid))
                {
                    HomeScreen.Instance.Tabs.Remove(Jid);

                    foreach (Transform t in memberParentTransform)
                    {
                        if (members.Contains(t.gameObject))
                            Destroy(t.gameObject);
                    }

                    members.Clear();
                    HomeScreen.Instance.membersMatrix.Remove(Jid + ConstantsChat.chatBtn);
                    Destroy(gameObject);
                    HomeScreen.Instance.ShowTab(ConstantsChat.defaultTab, Matrix.Xmpp.MessageType.GroupChat);
                    break;
                }
            }
        }

        public void AddMember(GameObject gameObject)
        {
            this.members.Add(gameObject);
        }

        public void ShowMember()
        {
            for (int i = 0; i < this.members.Count; i++)
            {
                if (members[i].gameObject.transform != null)
                {
                    members[i].gameObject.transform.SetParent(memberParentTransform);
                    members[i].gameObject.SetActive(true);
                }
            }
        }

        public void HideMember()
        {
            for (int i = 0; i < this.members.Count; i++)
            {
                members[i].gameObject.SetActive(false);
            }
        }

        public void RemoveMember(string name)
        {
            for (int i = 0; i < this.members.Count; i++)
            {
                if (members[i].gameObject.name.Equals(name))
                {
                    members.RemoveAt(i);
                }
            }
        }

        public bool CheckMemberExist(string name)
        {
            for (int i = 0; i < this.members.Count; i++)
            {
                if (members[i].gameObject.name.Equals(name))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}