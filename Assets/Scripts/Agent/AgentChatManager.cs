using System;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AgentChatManager : MonoBehaviour
{
    AgentInstructionManager _instructionMng;

    [SerializeField]
    TMP_InputField _inputField;
    [SerializeField]
    Button _submitButton;

    [SerializeField]
    Transform _chatHistory;

    [SerializeField]
    GameObject _chatBoxAgent;

    [SerializeField]
    GameObject _chatBoxPlayer;


    void Awake()
    {
        _instructionMng = GetComponent<AgentInstructionManager>();

        _inputField.onSubmit.AddListener(SubmitInput);
        _submitButton.onClick.AddListener(SubmitInput);
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

        GameObject obj = Instantiate(_chatBoxPlayer, _chatHistory);
        obj.GetComponent<ChatBox>().SetText(input);
        _instructionMng.Chat(input, Instantiate(_chatBoxAgent, _chatHistory));
    }
}
