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

    private Color normalDescriptionColor = Color.white;
    private TutorialQuestData currentQuest;
    private int currentProgress;
    private int maxProgress;
    private bool isClear;

    private void Awake()
    {
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
    }

    public void OnClickQuestButton()
    {
        questManager?.TryClaimCurrentQuestReward();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void HandleSelectedLocaleChanged(Locale locale)
    {
        if (currentQuest != null)
        {
            UIRefresh(currentQuest, currentProgress, maxProgress, isClear);
        }
    }
}
