using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TokenResourceBarUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] TMP_Text _tokenText;
    [SerializeField] TMP_Text _remainingTimeText;
    [SerializeField] TokenManager _tokenManager;
    [SerializeField, Min(0.1f)] float _remainingTimeVisibleSeconds = 3f;

    float _hideRemainingTimeAt;
    bool _isSubscribed;

    private void Awake()
    {
        if (_tokenManager == null)
        {
            _tokenManager = TokenManager.Instance != null
                ? TokenManager.Instance
                : FindFirstObjectByType<TokenManager>();
        }

        if (_tokenText == null)
        {
            _tokenText = GetComponentInChildren<TMP_Text>();
        }
    }

    private void OnEnable()
    {
        Subscribe();
        SetRemainingTimeVisible(false);
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Update()
    {
        if (_remainingTimeText == null || !_remainingTimeText.gameObject.activeSelf)
        {
            return;
        }

        UpdateRemainingTimeText();

        if (Time.unscaledTime >= _hideRemainingTimeAt)
        {
            SetRemainingTimeVisible(false);
        }
    }

    public void SetReferences(TMP_Text tokenText, TMP_Text remainingTimeText, TokenManager tokenManager)
    {
        Unsubscribe();
        _tokenText = tokenText;
        _remainingTimeText = remainingTimeText;
        _tokenManager = tokenManager;
        Subscribe();
        OnTokenChanged(_tokenManager.CurrentToken, _tokenManager.MaxTokenCount);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ShowRemainingTime();
    }

    public void ShowRemainingTime()
    {
        _hideRemainingTimeAt = Time.unscaledTime + _remainingTimeVisibleSeconds;
        SetRemainingTimeVisible(true);
        UpdateRemainingTimeText();
    }

    private void OnTokenChanged(int currentToken, int maxToken)
    {
        if (_tokenText != null)
        {
            _tokenText.SetText($"{currentToken}/{maxToken}");
        }

        UpdateRemainingTimeText();
    }

    private void UpdateRemainingTimeText()
    {
        if (_remainingTimeText == null || _tokenManager == null)
        {
            return;
        }

        TimeSpan remainingTime = TimeSpan.FromSeconds(Mathf.CeilToInt(_tokenManager.RemainingRecoverySeconds));
        _remainingTimeText.SetText("{0:00}:{1:00}:{2:00}", (int)remainingTime.TotalHours, remainingTime.Minutes, remainingTime.Seconds);
    }

    private void SetRemainingTimeVisible(bool visible)
    {
        if (_remainingTimeText != null && _remainingTimeText.gameObject.activeSelf != visible)
        {
            _remainingTimeText.gameObject.SetActive(visible);
        }
    }

    private void Subscribe()
    {
        if (_isSubscribed || _tokenManager == null)
        {
            return;
        }

        _tokenManager.TokenChanged += OnTokenChanged;
        _isSubscribed = true;
        OnTokenChanged(_tokenManager.CurrentToken, _tokenManager.MaxTokenCount);
    }

    private void Unsubscribe()
    {
        if (!_isSubscribed || _tokenManager == null)
        {
            return;
        }

        _tokenManager.TokenChanged -= OnTokenChanged;
        _isSubscribed = false;
    }
}
