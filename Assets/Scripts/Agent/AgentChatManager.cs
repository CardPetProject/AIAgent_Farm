using LLMUnity;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

public class AgentChatManager : MonoBehaviour
{
    [SerializeField]
    TMP_InputField _inputField;
    [SerializeField]
    Button _submitButton;

    [SerializeField]
    Transform _chatHistory;

    [SerializeField]
    GameObject _chatBoxAgent;
    ChatBox _curChatBoxAgent;

    [SerializeField]
    GameObject _chatBoxPlayer;

    LLMAgent _agent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _agent = GetComponent<LLMAgent>();

        _inputField.onSubmit.AddListener(SubmitInput);
        _submitButton.onClick.AddListener(SubmitInput);
    }

    void HandleReply(string replySoFar)
    {
        _curChatBoxAgent.SetText(replySoFar);
    }

    void ReplyCompleted()
    {
        _curChatBoxAgent = null;
        Debug.Log("The AI has finished replying");
    }

    private void SubmitInput()
    {
        SubmitInput(_inputField.text);
    }

    private void SubmitInput(string input)
    {
        _inputField.text = "";
        _inputField.ActivateInputField();
        _inputField.Select();
        if (input.Length < 1 || input.Trim().Length < 1) return;
        Debug.Log("Input submitted: " + input);

        GameObject obj = Instantiate(_chatBoxPlayer, _chatHistory);
        obj.GetComponent<ChatBox>().SetText(input);

        _agent.Chat(input, HandleReply, ReplyCompleted);
        _curChatBoxAgent = Instantiate(_chatBoxAgent, _chatHistory).GetComponent<ChatBox>();
        _curChatBoxAgent.SetText("생각중...");
    }
}
