using UnityEngine;

[CreateAssetMenu(menuName = "Quest/Tutorial Quest Data")]
public class TutorialQuestData : ScriptableObject
{
    [Header("Quest")]
    [SerializeField] private int questId;
    [SerializeField] private string title_kr;
    [SerializeField, TextArea] private string description_kr;
    [SerializeField] private string title_eng;
    [SerializeField, TextArea] private string description_eng;

    [Header("Condition")]
    [SerializeField] private QuestType questType;
    [SerializeField] private string targetId;
    [SerializeField, Min(1)] private int requiredProgress = 1;

    [Header("Reward")]
    [SerializeField, Min(0)] private int rewardGold;

    public int QuestId => questId;
    public string Title => GetTitle();
    public string Description => GetDescription();
    public QuestType QuestType => questType;
    public string TargetId => targetId;
    public int RequiredProgress => requiredProgress;
    public int RewardGold => rewardGold;

    public string GetTitle()
    {
        if (AgentLanguageUtility.IsEnglish && !string.IsNullOrWhiteSpace(title_eng))
        {
            return title_eng;
        }

        return title_kr;
    }

    public string GetDescription()
    {
        if (AgentLanguageUtility.IsEnglish && !string.IsNullOrWhiteSpace(description_eng))
        {
            return description_eng;
        }

        return description_kr;
    }

    public bool Matches(QuestType reportedType, string reportedTargetId)
    {
        if (questType != reportedType)
        {
            return false;
        }

        return string.IsNullOrEmpty(targetId) || targetId == reportedTargetId;
    }
}
