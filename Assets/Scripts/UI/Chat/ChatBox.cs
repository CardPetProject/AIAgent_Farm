using TMPro;
using UnityEngine;

public class ChatBox : MonoBehaviour
{
    [SerializeField]
    TMP_Text _text;

    public void SetText(string str) { _text.SetText(str); }
    public string GetText() { return _text.text; }
}
