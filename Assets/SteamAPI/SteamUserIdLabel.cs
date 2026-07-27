using TMPro;
using UnityEngine;

public class SteamUserIdLabel : MonoBehaviour
{
    [SerializeField] private TMP_Text targetText;
    [SerializeField] private string loadingText = "Steam user loading...";
    [SerializeField] private string unavailableText = "Steam user unavailable";

    private void Awake()
    {
        if (targetText == null)
        {
            targetText = GetComponent<TMP_Text>();
        }
    }

    private void Start()
    {
        RefreshLabel();
    }

    public void RefreshLabel()
    {
        if (targetText == null)
        {
            Debug.LogWarning("SteamUserIdLabel requires a TMP_Text reference.", this);
            return;
        }

        SteamService steamService = SteamService.Instance;
        if (steamService == null)
        {
            targetText.text = loadingText;
            return;
        }

        steamService.RefreshUserData();

        if (!steamService.IsInitialized || string.IsNullOrEmpty(steamService.PersonaName))
        {
            targetText.text = unavailableText;
            return;
        }

        targetText.text = steamService.PersonaName;
    }
}
