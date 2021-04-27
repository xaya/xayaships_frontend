using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginScreen : MonoBehaviour
{

    #region Public Variables

    [Header("InputField")]
    public TMP_InputField usernameInputField;
    public TMP_InputField passwordInputField;

    [Header("Button")]
    public Button loginButton;

    #endregion

    #region Private Variable

    [Header("String")]
    private string username = string.Empty;
    private string password = string.Empty;

    #endregion

    #region Unity Methods

    private void Start()
    {
        //usernameInputField.text = ConstantsChat.user2;
        //passwordInputField.text = ConstantsChat.password2;
    }

    #endregion

    #region User Define Methods

    public void OnUsernameInputFieldEditEnded(string usernameString)
    {
        if (!string.IsNullOrEmpty(usernameString))
        {
            username = usernameString;
        }
    }

    public void OnPasswordInputFieldEditEnded(string passwordString)
    {
        if (!string.IsNullOrEmpty(passwordString))
        {
            password = passwordString;
        }
    }

    public void OnLogin()
    {
        string emailStr = usernameInputField.text.Trim();
        string passwordStr = passwordInputField.text.Trim();

        // Check Username or Password is empty or not
        if (string.IsNullOrEmpty(emailStr) || string.IsNullOrEmpty(passwordStr))
        {
            Debug.Log("USERNAME OR PASSWORD IS EMPTY");
            ToastManager.Show("USERNAME OR PASSWORD IS EMPTY");

            // For show toast in pc
            if (Application.platform == RuntimePlatform.WindowsPlayer)
                SSTools.ShowMessage("USERNAME OR PASSWORD IS EMPTY", SSTools.Position.bottom, SSTools.Time.twoSecond);

            return;
        }

        // Set values in UserDetails class
        UIManagerChat.Instance.userDetailsModel.username = emailStr;
        UIManagerChat.Instance.userDetailsModel.password = passwordStr;

        XMPPConnection.Instance.Connect(emailStr, passwordStr);

        ToastManager.Show("Login..." + emailStr);

        // For show toast in pc
        if (Application.platform == RuntimePlatform.WindowsPlayer)
            SSTools.ShowMessage("Login..", SSTools.Position.bottom, SSTools.Time.twoSecond);
    }

    #endregion
}
