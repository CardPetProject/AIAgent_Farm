using UnityEngine;
using TMPro;

public class InputFieldFixMiddle : MonoBehaviour
{
    TMP_InputField _inputField;
    [SerializeField]
    RectTransform _textRectTransform;
    [SerializeField]
    RectTransform _caretRectTransform;

    void Start()
    {
        _inputField = GetComponent<TMP_InputField>();

        _textRectTransform = _inputField.textComponent.GetComponent<RectTransform>();
        _caretRectTransform = _textRectTransform.parent.GetChild(0).GetComponent<RectTransform>();
        _inputField.onValueChanged.AddListener(OnTextChanged);
    }

    void OnTextChanged(string text)
    {
        if (string.IsNullOrEmpty(text) || !text.Contains("\n"))
        {
            Vector2 pos = _textRectTransform.anchoredPosition;

            if (pos.y != 0f)
            {
                pos.y = 0f;
                _textRectTransform.anchoredPosition = pos;

                Vector2 caretPos = _caretRectTransform.anchoredPosition;
                caretPos.y = 0f;
                _caretRectTransform.anchoredPosition = caretPos;
            }
        }
    }

    void OnDestroy()
    {
        _inputField.onValueChanged.RemoveListener(OnTextChanged);
    }
}