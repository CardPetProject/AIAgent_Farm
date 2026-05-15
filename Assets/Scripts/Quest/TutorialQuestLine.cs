using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Quest/Tutorial Quest Line")]
public class TutorialQuestLine : ScriptableObject
{
    [SerializeField] private List<TutorialQuestData> quests = new();

    public IReadOnlyList<TutorialQuestData> Quests => quests;
}
