using System.Collections;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TokenManager : MonoBehaviour
{
    public static TokenManager Instance { get; private set; }
    const int MaxToken = 10;
    const int QuestionCost = 1;

    [SerializeField] TMP_Text _tokenText;
    [SerializeField] bool _dontDestroyOnLoad = true;
    [SerializeField] Color _defaultTextColor = Color.white;
    [SerializeField] Color _warningTextColor = Color.red;
    [SerializeField] float _flashInterval = 0.12f;
    [SerializeField] int _flashCount = 3;
    [SerializeField, Min(1f)] float _recoveryIntervalSeconds = 60f;

    public int token = MaxToken;
    Coroutine _flashCoroutine;
    float _recoveryElapsedSeconds;

    public int CurrentToken => token;
    public int MaxTokenCount => MaxToken;
    public int QuestionTokenCost => QuestionCost;
    public float RecoveryIntervalSeconds => _recoveryIntervalSeconds;
    public float RemainingRecoverySeconds => token >= MaxToken
        ? 0f
        : Mathf.Max(0f, _recoveryIntervalSeconds - _recoveryElapsedSeconds);
    public bool IsRecovering => token < MaxToken;

    public event Action<int, int> TokenChanged;

    private void OnValidate()
    {
        _recoveryIntervalSeconds = Mathf.Max(1f, _recoveryIntervalSeconds);
    }

    private void Awake()
    {
        _recoveryIntervalSeconds = Mathf.Max(1f, _recoveryIntervalSeconds);

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (_dontDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void Start()
    {
        BindTokenResourceBarIfPresent();

        if (_tokenText != null)
        {
            _defaultTextColor = _tokenText.color;
        }

        RefreshTokenText();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            SetToken(10);
        }

        RecoverTokenOverTime();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BindTokenResourceBarIfPresent();
        RefreshTokenText();
    }

    public void SetTokenText(TMP_Text tokenText)
    {
        _tokenText = tokenText;

        if (_tokenText != null)
        {
            _defaultTextColor = _tokenText.color;
        }

        RefreshTokenText();
    }

    public void SetToken(int value)
    {
        token = Mathf.Clamp(value, 0, MaxToken);
        if (token >= MaxToken)
        {
            _recoveryElapsedSeconds = 0f;
        }

        RefreshTokenText();
    }

    public bool HasEnoughToken(int amount)
    {
        return token >= amount;
    }

    public bool AddToken(int amount)
    {
        if (token + amount < 0)
        {
            Debug.LogWarning($"토큰이 부족합니다. 현재 토큰: {token}, 요청 변화량: {amount}");
            PlayInsufficientTokenFeedback();
            return false;
        }

        token = Mathf.Clamp(token + amount, 0, MaxToken);
        if (token >= MaxToken)
        {
            _recoveryElapsedSeconds = 0f;
        }

        RefreshTokenText();
        return true;
    }

    public bool UseToken(int amount)
    {
        if (amount < 0)
        {
            amount = -amount;
        }

        return AddToken(-amount);
    }

    public bool TrySpendQuestionToken()
    {
        return UseToken(QuestionCost);
    }

    private void RefreshTokenText()
    {
        if (_tokenText != null)
        {
            _tokenText.SetText(token.ToString() + "/" + MaxToken);
        }

        TokenChanged?.Invoke(token, MaxToken);
    }

    private void RecoverTokenOverTime()
    {
        if (token >= MaxToken)
        {
            _recoveryElapsedSeconds = 0f;
            return;
        }

        _recoveryElapsedSeconds += Time.deltaTime;

        while (_recoveryElapsedSeconds >= _recoveryIntervalSeconds && token < MaxToken)
        {
            _recoveryElapsedSeconds -= _recoveryIntervalSeconds;
            token = Mathf.Clamp(token + 1, 0, MaxToken);
            RefreshTokenText();
        }

        if (token >= MaxToken)
        {
            _recoveryElapsedSeconds = 0f;
        }
    }

    private void BindTokenResourceBarIfPresent()
    {
        Canvas canvas = FindUICanvas();
        if (canvas == null)
        {
            return;
        }

        Transform existingResourceBar = canvas.transform.Find("ResourceBar_Token");
        if (existingResourceBar != null)
        {
            BindTokenResourceBar(existingResourceBar);
        }
    }

    private void BindTokenResourceBar(Transform resourceBar)
    {
        TokenResourceBarUI tokenResourceBarUI = resourceBar.GetComponent<TokenResourceBarUI>();
        if (tokenResourceBarUI == null)
        {
            tokenResourceBarUI = resourceBar.gameObject.AddComponent<TokenResourceBarUI>();
        }

        TMP_Text tokenValueText = FindChildText(resourceBar, "TokenValue");
        TMP_Text recoveryTimeText = FindChildText(resourceBar, "RecoveryTime");

        if (tokenValueText != null)
        {
            SetTokenText(tokenValueText);
        }

        tokenResourceBarUI.SetReferences(tokenValueText, recoveryTimeText, this);
    }

    private Canvas FindUICanvas()
    {
        GameObject uiCanvasObject = GameObject.Find("UICanvas");
        if (uiCanvasObject != null && uiCanvasObject.TryGetComponent(out Canvas uiCanvas))
        {
            return uiCanvas;
        }

        return FindFirstObjectByType<Canvas>();
    }

    private TMP_Text FindChildText(Transform parent, string childName)
    {
        Transform child = parent.Find(childName);
        if (child != null && child.TryGetComponent(out TMP_Text text))
        {
            return text;
        }

        return null;
    }

    private void PlayInsufficientTokenFeedback()
    {
        if (_tokenText == null)
        {
            return;
        }

        if (_flashCoroutine != null)
        {
            StopCoroutine(_flashCoroutine);
        }

        _flashCoroutine = StartCoroutine(FlashTokenText());
    }

    private IEnumerator FlashTokenText()
    {
        for (int i = 0; i < _flashCount; i++)
        {
            _tokenText.color = _warningTextColor;
            yield return new WaitForSeconds(_flashInterval);
            _tokenText.color = _defaultTextColor;
            yield return new WaitForSeconds(_flashInterval);
        }

        _tokenText.color = _defaultTextColor;
        _flashCoroutine = null;
    }
}
