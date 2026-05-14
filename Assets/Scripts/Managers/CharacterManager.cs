using System;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance { get; private set; }
    public static event Action<int> CharacterChanged;
    public GameObject[] focus;
    public GameObject UI;
    public Sprite[] characterIcons;
    public Sprite[] characterHeads;
    [SerializeField, TextArea(3, 8)]
    private string[] characterPersonaPrompts = CreateDefaultPersonaPrompts();

    [SerializeField]
    private int characterID;
    [SerializeField]
    private Animator targetAnimator;
    [SerializeField]
    private RuntimeAnimatorController[] characterAnimators;
    [SerializeField]
    private bool refreshOnStart = true;

    public int CharacterID => characterID;

    public string CurrentPersonaPrompt => GetPersonaPrompt(characterID);
    

    private void Awake()
    {
        Instance = this;
    }
    public void ShowUI()
    {
        focus[characterID].gameObject.SetActive(true);
        UI.gameObject.SetActive(true);
    }

    private void Start()
    {
        if (refreshOnStart)
        {
            Characterrefresh();
        }
    }
    public void SetCharacterID(int ID)
    {
        characterID = ID;

        if (focus != null)
        {
            for (int index = 0; index < focus.Length; index++)
            {
                if (focus[index] != null)
                {
                    focus[index].SetActive(false);
                }
            }
        }

        if (focus != null && ID >= 0 && ID < focus.Length && focus[ID] != null)
        {
            focus[ID].SetActive(true);
        }

        // Characterrefresh();
        // CharacterChanged?.Invoke(characterID);
    }

    public void SetCharacterID()
    {
        Characterrefresh();
        CharacterChanged?.Invoke(characterID);
    }

    public void Characterrefresh()
    {
        if (targetAnimator == null)
        {
            Debug.LogWarning("[CharacterManager] Target Animator is missing.", this);
            return;
        }

        if (characterAnimators == null || characterAnimators.Length == 0)
        {
            Debug.LogWarning("[CharacterManager] Character animator list is empty.", this);
            return;
        }

        if (characterID < 0 || characterID >= characterAnimators.Length)
        {
            Debug.LogWarning($"[CharacterManager] Character ID {characterID} is out of range. Animator count: {characterAnimators.Length}.", this);
            return;
        }

        RuntimeAnimatorController nextAnimator = characterAnimators[characterID];
        if (nextAnimator == null)
        {
            Debug.LogWarning($"[CharacterManager] Animator for character ID {characterID} is missing.", this);
            return;
        }

        if (targetAnimator.runtimeAnimatorController == nextAnimator)
        {
            return;
        }

        targetAnimator.runtimeAnimatorController = nextAnimator;
        targetAnimator.Rebind();
        targetAnimator.Update(0f);
    }

    public string GetPersonaPrompt(int ID)
    {
        EnsurePersonaPrompts();

        if (characterPersonaPrompts == null || characterPersonaPrompts.Length == 0)
        {
            return string.Empty;
        }

        if (ID < 0 || ID >= characterPersonaPrompts.Length)
        {
            return characterPersonaPrompts[0];
        }

        return characterPersonaPrompts[ID] ?? string.Empty;
    }

    [ContextMenu("Reset Persona Prompts To Defaults")]
    private void ResetPersonaPromptsToDefaults()
    {
        characterPersonaPrompts = CreateDefaultPersonaPrompts();
    }

    private void EnsurePersonaPrompts()
    {
        if (characterPersonaPrompts == null
            || characterPersonaPrompts.Length < 2
            || string.IsNullOrWhiteSpace(characterPersonaPrompts[0])
            || string.IsNullOrWhiteSpace(characterPersonaPrompts[1])
            || characterPersonaPrompts[0].Contains("착하고")
            || characterPersonaPrompts[1].Contains("까칠하고"))
        {
            characterPersonaPrompts = CreateDefaultPersonaPrompts();
        }
    }

    private static string[] CreateDefaultPersonaPrompts()
    {
        return new[]
        {
            "Character 0 persona:\n" +
            "- Personality: kind, gentle, friendly, and supportive.\n" +
            "- Tone: warm, polite, soft, and encouraging.\n" +
            "- Behavior: help the user like a reliable farm companion.\n" +
            "- In Korean, use friendly and caring expressions.",
            "Character 1 persona:\n" +
            "- Personality: prickly, blunt, sharp-tongued, and rude.\n" +
            "- Tone: curt, sarcastic, impatient, and not overly kind.\n" +
            "- Behavior: still understand and execute the user's request correctly.\n" +
            "- In Korean, use blunt expressions like '뭐, 알겠어', '하...', '귀찮지만 해줄게', or '그 정도는 해줄 수 있어'.\n" +
            "- Do not become warm, sweet, apologetic, or overly supportive.\n" +
            "- Do not use severe profanity, hate speech, or abusive threats."
        };
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (characterID < 0)
        {
            characterID = 0;
        }

        EnsurePersonaPrompts();
    }
#endif
}
