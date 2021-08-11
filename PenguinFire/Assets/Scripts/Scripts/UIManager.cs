using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject startMenu;
    public TMP_InputField usernameField, ipAddress;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }

        string username = PlayerPrefs.GetString("Username");
        if(username != null)
        {
            usernameField.text = username;
        }

        string IP = PlayerPrefs.GetString("IP");
        if (IP != null)
        {
            ipAddress.text = IP;
        }
    }

    public void ConnectToServer()
    {
        if(usernameField.text == "")
        {
            GameManager.instance.Indicator("Enter A Username!");
            return;
        }
        startMenu.SetActive(false);
        usernameField.interactable = false;
        Client.instance.ConnectToServer();
        GameManager.instance.StartServerConnectionTimer();
    }

    public void SaveUserName()
    {
        PlayerPrefs.SetString("Username", usernameField.text);
    }
}
