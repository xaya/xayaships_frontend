using System.Collections.Generic;
using UnityEngine;

public class UserDetails
{
    public string username { get; set; }

    public string password { get; set; }

    public string domainName { get; set; }

    public string roomID { get; set; }

    public UserDetails()
    {
        Load();
    }

    public void Load()
    {
        if (PlayerPrefs.HasKey("Username"))
        {
            username = PlayerPrefs.GetString("Username");
        }
        else
        {
            username = "";
        }

        if (PlayerPrefs.HasKey("Domain"))
        {
            domainName = PlayerPrefs.GetString("Domain");
        }
        else
        {
            domainName = "";
        }

        if (PlayerPrefs.HasKey("Password"))
        {
            password = PlayerPrefs.GetString("Password");
        }
        else
        {
            password = "";
        }

        if (PlayerPrefs.HasKey("RoomID"))
        {
            roomID = PlayerPrefs.GetString("RoomID");
        }
        else
        {
            roomID = "";
        }
    }

    // Save Values
    public void Save()
    {
        PlayerPrefs.SetString("Username", username);
        PlayerPrefs.SetString("Password", password);
        PlayerPrefs.SetString("Domain", domainName);
        PlayerPrefs.SetString("RoomID", roomID);
        PlayerPrefs.Save();
    }
}
