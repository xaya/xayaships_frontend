using Matrix;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MemberMatrix : MonoBehaviour, IPointerClickHandler
{
    #region Public Variables

    public Jid Jid;

    #endregion

    #region User Define Methods

    public void SetProperties(Jid jid)
    {
        Jid = jid;

        if (jid.ToString().Contains('@'))
            GetComponentInChildren<Text>().text = jid.ToString().Split('@').First();
        else
            GetComponentInChildren<Text>().text = jid;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        HomeScreen handler = HomeScreen.Instance;
        handler.ShowTab(Jid, Matrix.Xmpp.MessageType.Chat);
    }

    #endregion
}