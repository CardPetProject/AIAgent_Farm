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

    [SerializeField]
    private int characterID;
    [SerializeField]
    private Animator targetAnimator;
    [SerializeField]
    private RuntimeAnimatorController[] characterAnimators;
    [SerializeField]
    private bool refreshOnStart = true;

    public int CharacterID => characterID;
    

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

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (characterID < 0)
        {
            characterID = 0;
        }
    }
#endif
}
