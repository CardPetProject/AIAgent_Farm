using System;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private TutorialQuestLine tutorialQuestLine;

    [Header("UI")]
    [SerializeField] private QuestUI questUI;
    [SerializeField] private GoldManager goldManager;

    [Header("Runtime State")]
    public int currentQuestID;
    public int currentQuestProgress_now;
    public int currentQuestProgress_max;
    public QuestType type;

    private int currentQuestIndex;
    private TutorialQuestData currentQuest;
    private bool isCurrentQuestClear;
    private bool isTutorialActive;

    public TutorialQuestData CurrentQuest => currentQuest;
    public bool IsTutorialFinished { get; private set; }
    public bool IsCurrentQuestClear => isCurrentQuestClear;
    public int CurrentQuestIndex => currentQuestIndex;

    public event Action<TutorialQuestData, int, int> QuestRefreshed;
    public event Action TutorialFinished;

    private void Awake()
    {
        if (questUI == null)
        {
            questUI = FindFirstObjectByType<QuestUI>(FindObjectsInactive.Include);
        }

        if (goldManager == null)
        {
            goldManager = FindFirstObjectByType<GoldManager>();
        }

        DeactivateForLobby();
    }

    public void StartTutorial()
    {
        currentQuestIndex = 0;
        isTutorialActive = true;
        IsTutorialFinished = false;
        RefreshCurrentQuest();
    }

    public void DeactivateForLobby()
    {
        isTutorialActive = false;
        questUI?.Hide();

        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }

    public QuestStateDto CreateState()
    {
        return new QuestStateDto
        {
            currentQuestIndex = Mathf.Max(0, currentQuestIndex),
            currentQuestID = currentQuestID,
            currentQuestProgressNow = Mathf.Max(0, currentQuestProgress_now),
            currentQuestProgressMax = Mathf.Max(0, currentQuestProgress_max),
            currentQuestType = type.ToString(),
            isCurrentQuestClear = isCurrentQuestClear,
            isTutorialFinished = IsTutorialFinished
        };
    }

    public void InitializeFromBackend(QuestStateDto state)
    {
        if (state == null)
        {
            StartTutorial();
            return;
        }

        if (state.isTutorialFinished)
        {
            currentQuest = null;
            currentQuestIndex = Mathf.Max(0, state.currentQuestIndex);
            currentQuestID = state.currentQuestID;
            currentQuestProgress_now = Mathf.Max(0, state.currentQuestProgressNow);
            currentQuestProgress_max = Mathf.Max(0, state.currentQuestProgressMax);
            isCurrentQuestClear = false;
            isTutorialActive = true;
            IsTutorialFinished = true;
            questUI?.Hide();
            return;
        }

        if (tutorialQuestLine == null || tutorialQuestLine.Quests.Count == 0)
        {
            StartTutorial();
            return;
        }

        currentQuestIndex = Mathf.Clamp(state.currentQuestIndex, 0, tutorialQuestLine.Quests.Count - 1);
        currentQuest = tutorialQuestLine.Quests[currentQuestIndex];

        if (currentQuest == null)
        {
            StartTutorial();
            return;
        }

        currentQuestID = currentQuest.QuestId;
        currentQuestProgress_max = currentQuest.RequiredProgress;
        currentQuestProgress_now = Mathf.Clamp(state.currentQuestProgressNow, 0, currentQuestProgress_max);
        type = currentQuest.QuestType;
        isCurrentQuestClear = state.isCurrentQuestClear || currentQuestProgress_now >= currentQuestProgress_max;
        isTutorialActive = true;
        IsTutorialFinished = false;

        RefreshQuestUI();
    }

    public void ReportQuestProgress(QuestType questType, string targetId = "", int amount = 1)
    {
        if (!isTutorialActive || IsTutorialFinished || currentQuest == null || isCurrentQuestClear || amount <= 0)
        {
            Debug.Log($"[QuestManager] Report ignored. active:{isTutorialActive}, finished:{IsTutorialFinished}, quest:{currentQuest != null}, clear:{isCurrentQuestClear}, amount:{amount}", this);
            return;
        }

        if (!currentQuest.Matches(questType, targetId))
        {
            Debug.Log($"[QuestManager] Report ignored. current:{currentQuest.QuestType}/{currentQuest.TargetId}, reported:{questType}/{targetId}", this);
            return;
        }

        currentQuestProgress_now = Mathf.Min(currentQuestProgress_now + amount, currentQuestProgress_max);

        if (currentQuestProgress_now >= currentQuestProgress_max)
        {
            isCurrentQuestClear = true;
        }

        RefreshQuestUI();
    }

    public void ReportMove(string targetId = "", int amount = 1)
    {
        ReportQuestProgress(QuestType.Move, targetId, amount);
    }

    public void ReportPlant(string targetId = "", int amount = 1)
    {
        ReportQuestProgress(QuestType.Plant, targetId, amount);
    }

    public void ReportChat(string targetId = "", int amount = 1)
    {
        ReportQuestProgress(QuestType.Chat, targetId, amount);
    }

    public void ReportHarvest(string targetId = "", int amount = 1)
    {
        ReportQuestProgress(QuestType.Harvest, targetId, amount);
    }

    public bool TryClaimCurrentQuestReward()
    {
        if (!isTutorialActive || IsTutorialFinished || currentQuest == null || !isCurrentQuestClear)
        {
            return false;
        }

        if (goldManager != null && currentQuest.RewardGold > 0)
        {
            goldManager.AddGold(currentQuest.RewardGold);
        }

        CompleteCurrentQuest();
        return true;
    }

    private void CompleteCurrentQuest()
    {
        currentQuestIndex++;

        if (tutorialQuestLine == null || currentQuestIndex >= tutorialQuestLine.Quests.Count)
        {
            FinishTutorial();
            return;
        }

        RefreshCurrentQuest();
    }

    private void RefreshCurrentQuest()
    {
        if (tutorialQuestLine == null || tutorialQuestLine.Quests.Count == 0)
        {
            FinishTutorial();
            return;
        }

        currentQuest = tutorialQuestLine.Quests[currentQuestIndex];
        if (currentQuest == null)
        {
            FinishTutorial();
            return;
        }

        currentQuestID = currentQuest.QuestId;
        currentQuestProgress_now = 0;
        currentQuestProgress_max = currentQuest.RequiredProgress;
        type = currentQuest.QuestType;
        isCurrentQuestClear = false;

        RefreshQuestUI();
    }

    private void RefreshQuestUI()
    {
        if (questUI == null)
        {
            questUI = FindFirstObjectByType<QuestUI>(FindObjectsInactive.Include);
        }

        if (questUI == null)
        {
            Debug.LogWarning("[QuestManager] QuestUI reference is missing. Quest progress changed, but UI could not refresh.", this);
            return;
        }

        questUI?.UIRefresh(currentQuest, currentQuestProgress_now, currentQuestProgress_max, isCurrentQuestClear);
        QuestRefreshed?.Invoke(currentQuest, currentQuestProgress_now, currentQuestProgress_max);
    }

    private void FinishTutorial()
    {
        currentQuest = null;
        isCurrentQuestClear = false;
        isTutorialActive = true;
        IsTutorialFinished = true;
        questUI?.Hide();
        TutorialFinished?.Invoke();
    }
}

[Serializable]
public class QuestStateDto
{
    public int currentQuestIndex;
    public int currentQuestID;
    public int currentQuestProgressNow;
    public int currentQuestProgressMax;
    public string currentQuestType;
    public bool isCurrentQuestClear;
    public bool isTutorialFinished;
}
