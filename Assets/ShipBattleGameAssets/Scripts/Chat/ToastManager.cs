using UnityEngine;

public static class ToastManager
{
    public static void Show(string message)
    {
        Debug.Log("TOAST MANAGER : " + message);
#if UNITY_ANDROID && !UNITY_EDITOR
        FantomLib.AndroidPlugin.ShowToast(message);
#endif 
    }

}
