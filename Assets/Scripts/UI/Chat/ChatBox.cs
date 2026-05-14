using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatBox : MonoBehaviour
{
    [SerializeField]
    TMP_Text _name;
    [SerializeField]
    TMP_Text _time;
    [SerializeField]
    TMP_Text _text;
    [SerializeField]
    Image icon;

    private void OnEnable()
    {
        CharacterManager.CharacterChanged += RefreshCharacterHead;
        RefreshCharacterHead();
    }

    private void OnDisable()
    {
        CharacterManager.CharacterChanged -= RefreshCharacterHead;
    }

    public void SetText(string str) { _text.SetText(str); }
    public string GetText() { return _text.text; }

    public void SetName(string str) { _name.SetText(str); }
    public string GetName() { return _name.text; }

    public void SetTime(string str) { _time.SetText(str); }
    public string GetTime() { return _time.text; }

    public void RefreshCharacterHead()
    {
        if (icon == null)
        {
            return;
        }

        CharacterManager characterManager = CharacterManager.Instance;
        if (characterManager == null || characterManager.characterHeads == null)
        {
            return;
        }

        int characterID = characterManager.CharacterID;
        if (characterID < 0 || characterID >= characterManager.characterHeads.Length)
        {
            Debug.LogWarning($"[ChatBox] Character ID {characterID} is out of head icon range. Head count: {characterManager.characterHeads.Length}.", this);
            return;
        }

        Sprite head = characterManager.characterHeads[characterID];
        if (head != null)
        {
            icon.sprite = head;
        }
    }

    private void RefreshCharacterHead(int characterID)
    {
        RefreshCharacterHead();
    }
}
