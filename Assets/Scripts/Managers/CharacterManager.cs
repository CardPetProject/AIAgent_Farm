using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance { get; private set; }

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
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[CharacterManager] Multiple CharacterManager instances found. Using the latest one.", this);
        }

        Instance = this;
    }

    private void Start()
    {
        if (refreshOnStart)
        {
            Characterrefresh();
        }
    }

    public void SetCharacterID(int newCharacterID)
    {
        characterID = newCharacterID;
        Characterrefresh();
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
