using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class QuestUI : MonoBehaviour
{
    public TMP_Text title;
    public TMP_Text desc;
    public TMP_Text gem_Count;

    [SerializeField] private QuestManager questManager;
    [SerializeField] private Button questButton;
    [SerializeField] private Color clearDescriptionColor = Color.green;
    [SerializeField] private Vector2 hiddenPosition = new Vector2(10000f, 10000f);

    private Color normalDescriptionColor = Color.white;
    private RectTransform rectTransform;
    private Vector2 shownPosition;
    private bool hasShownPosition;
    private TutorialQuestData currentQuest;
    private int currentProgress;
    private int maxProgress;
    private bool isClear;

    private void Awake()
    {
        CacheShownPosition();

        if (questManager == null)
        {
            questManager = FindFirstObjectByType<QuestManager>();
        }

        if (questButton == null)
        {
            questButton = GetComponent<Button>();
        }

        if (desc != null)
        {
            normalDescriptionColor = desc.color;
        }
    }

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += HandleSelectedLocaleChanged;

        if (questButton != null)
        {
            questButton.onClick.AddListener(OnClickQuestButton);
        }
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= HandleSelectedLocaleChanged;

        if (questButton != null)
        {
            questButton.onClick.RemoveListener(OnClickQuestButton);
        }
    }

    public void UIRefresh(TutorialQuestData quest, int currentProgress, int maxProgress, bool isClear)
    {
        currentQuest = quest;
        this.currentProgress = currentProgress;
        this.maxProgress = maxProgress;
        this.isClear = isClear;

        if (quest == null)
        {
            Hide();
            return;
        }

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        if (title != null)
        {
            title.text = quest.GetTitle();
        }

        if (desc != null)
        {
            desc.text = $"{quest.GetDescription()} : ({currentProgress}/{maxProgress})";
            desc.color = isClear ? clearDescriptionColor : normalDescriptionColor;
        }

        if (gem_Count != null)
        {
            gem_Count.text = quest.RewardGold.ToString();
        }

        if(isClear)
        {
            AudioManager.Instance.PlaySFX(SfxType.Success);
        }
    }

    public void OnClickQuestButton()
    {
        AudioManager.Instance.PlaySFX(SfxType.Click);
        questManager?.TryClaimCurrentQuestReward();
    }

    public void Hide()
    {
        CacheShownPosition();

        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = hiddenPosition;
        }
        // AudioManager.Instance.PlaySFX(SfxType.Click);
    }

    public void Show()
    {
        CacheShownPosition();

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = shownPosition;
        }
    }

    private void CacheShownPosition()
    {
        if (hasShownPosition)
        {
            return;
        }

        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        if (rectTransform == null)
        {
            return;
        }

        shownPosition = rectTransform.anchoredPosition;
        hasShownPosition = true;
    }

    private void HandleSelectedLocaleChanged(Locale locale)
    {
        if (currentQuest != null)
        {
            UIRefresh(currentQuest, currentProgress, maxProgress, isClear);
        }
    }
}
