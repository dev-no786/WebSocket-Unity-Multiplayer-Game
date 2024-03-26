using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class JoinGameMenuUI : MonoBehaviour
{
    [SerializeField] private InputField nameInput;
    [SerializeField] private Button submitButton;
    public void Start()
    {
        submitButton.onClick.AddListener(OnSubmitButton);
        
    }

    public void CheckNameText(string txt)
    {
        Debug.Log("input "+txt);
        if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
            submitButton.interactable = false;
        else
            submitButton.interactable = true;
    }
    
    private void OnSubmitButton()
    {
        NetworkManager.Instance.playerName = nameInput.text;
        NetworkManager.Instance.JoinGameServer();
        HideMenu();
    }

    public void HideMenu()
    {
        nameInput.text = "";
        gameObject.SetActive(false);
    }
}