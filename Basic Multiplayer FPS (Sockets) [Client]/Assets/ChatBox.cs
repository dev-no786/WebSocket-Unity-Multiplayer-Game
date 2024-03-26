using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChatBox : MonoBehaviour
{
    [SerializeField] private Text chatText;
    [SerializeField] private InputField msgBoxInput;
    [SerializeField] private int slowmodeTimer = 3;
    [SerializeField] private bool isSlowModeActive;

    private static bool _isClientChatting;
    public static bool IsClientChatting
    {
        get { return _isClientChatting; }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        //NetworkManager.Instance.OnGameJoin += ShowMenu;
    }

    private void Update()
    {
        if (msgBoxInput.isFocused)
        {
            _isClientChatting = true;
        }
        else
        {
            _isClientChatting = false;
        }
    }

    public void ShowMenu()
    {
        gameObject.SetActive(true);
    }
    
    // add the the received new one from server 
    public void GetNewChatLine(string message,string name)
    {
        chatText.text += "\n" + name + ":" + message;
    }

    public void SendNewChatLine(string message)
    {
        string from = NetworkManager.Instance.playerName;
        ChatJson json = new ChatJson(from, message);
        NetworkManager.Instance.SendChatMsg(json);
    }
    
    public void SendNewChatLine()
    {
        string from = NetworkManager.Instance.playerName;
        string message = msgBoxInput.text;
        if (string.IsNullOrEmpty(message) || string.IsNullOrWhiteSpace(message))
            return;
        
        ChatJson json = new ChatJson(from, msgBoxInput.text);
        NetworkManager.Instance.SendChatMsg(json);
        msgBoxInput.text = "";
    }

    [System.Serializable]
    public class ChatJson
    {
        [SerializeField] public string name;
        [SerializeField] public string chatMsg;

        public ChatJson(string _name,string _chatMsg)
        {
            name = _name;
            chatMsg = _chatMsg;
        }
    }
}
