using Matrix.Xmpp.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MUCHandler : Singleton<MUCHandler>
{
    #region Private Variables

    private MucManager mucManager;

    #endregion

    #region Unity Methods

    private void Start()
    {
        mucManager = new MucManager(XMPPConnection.Instance.xmppClient);
        mucManager.OnInvite += MucManager_OnInvite;
        mucManager.OnInvite += MucManager_OnDeclineInvite;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    #endregion

    #region MUC CallBack Methods

    private void MucManager_OnDeclineInvite(object sender, MessageEventArgs e)
    {
        Debug.Log("OnDeclineInvite : " + e.ToString());
    }

    private void MucManager_OnInvite(object sender, MessageEventArgs e)
    {
        Debug.Log("OnInvite : " + e.Message);
    }

    #endregion


    #region User Define Methods

    // Join Room
    public IEnumerator DirectJoinGroup()
    {
        yield return new WaitForSeconds(2);

        mucManager.EnterRoom(ConstantsChat.defaultroomID, UIManagerChat.Instance.userDetailsModel.username);
        mucManager.GrantAdminPrivileges(ConstantsChat.defaultroomID, UIManagerChat.Instance.userDetailsModel.username);
        UIManagerChat.Instance.userDetailsModel.roomID = ConstantsChat.defaultroomID;

        UIManagerChat.Instance.OpenChatScreen();
    }

    // Invite Memebr
    public void InviteMember(string username, string roomID)
    {
        mucManager.DirectInvite(username, roomID, ConstantsChat.inviteReason, UIManagerChat.Instance.userDetailsModel.password);
    }

    public void CreateNewRoom(string roomId, string username)
    {
        mucManager.EnterRoom(roomId, username);
        mucManager.GrantAdminPrivileges(roomId, username);
        UIManagerChat.Instance.userDetailsModel.roomID = roomId;

    }

    // Join room by invitation message
    public void JionRoomByInvitationMsg(string roomID)
    {
        mucManager.EnterRoom(roomID, UIManagerChat.Instance.userDetailsModel.username);
        UIManagerChat.Instance.userDetailsModel.roomID = roomID;
    }

    #endregion
}
