using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XAYAChat
{
    public class ConstantsChat
    {
        public const string HEX_CHARS = "0123456789" + "abcdef";
        public const string SIMPLE_NAME_CHARS = "0123456789" + "abcdefghijklmnopqrstuvwxyz";
        public const string xabberandroidOU1xyto0 = "xabber-android-OU1xyto0";

        // Static values
        public const string muc = "@muc.";
        public const string adrates = "@";
        public const string chatBtn = "@chatBtn";

        // Commands
        public const string tell = "/tell";
        public const string create = "/create";
        public const string invite = "/invite";
        public const string seperator = "/";
        // Filter
        public const string GroupChat = "GroupChat";
        public const string groupchat = "groupchat";
        public const string Chat = "Chat";
        public const string chat = "chat";

        // Invite reason
        public const string inviteReason = "Join Room";

        // Default tab
        public const string defaultTab = "Default";
        public const string defaultTabText = "LOBBY";

        // Text color
        public const string systemTxtColorBoldStart = "<b> <color=#0065ba>";
        public const string systemTxtColorBoldEnd = "</color> </b> ";
        public const string systemTxtColorNormalStart = "<color=#0065ba>";
        public const string systemTxtColorNormalEnd = "</color>";

        public const string userTxtColorBoldStart = "<b> <color=#dfff00>";
        public const string userTxtColorBoldEnd = "</color> :</b> ";

        public const string memberTxtColorBoldStart = "<b> <color=#ffffff>";
        public const string memberTxtColorBoldEnd = "</color> :</b> ";

        // System message
        public const string leaveChannel = "leaves the channel";
        public const string joinChannel = "joined the channel";
    }
}
