using System;
using System.Globalization;
using TMPro;
using UnityEngine;

public class BanManager : MonoBehaviour
{
    public TMP_Text reason_Text;
    public TMP_Text date_Text;

    public void Init(string reason, string endsAt)
    {
        gameObject.SetActive(true);

        if (reason_Text != null)
        {
            string resolvedReason = string.IsNullOrWhiteSpace(reason) ? "제재 사유가 제공되지 않았습니다." : reason.Trim();
            reason_Text.SetText(resolvedReason);
        }

        if (date_Text != null)
        {
            date_Text.SetText(FormatEndDate(endsAt));
        }
    }

    public void GameOff()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private static string FormatEndDate(string endsAt)
    {
        if (string.IsNullOrWhiteSpace(endsAt))
        {
            return "영구 제재";
        }

        if (DateTimeOffset.TryParse(
                endsAt,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal,
                out DateTimeOffset parsedEndDate))
        {
            return parsedEndDate.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
        }

        return endsAt;
    }
}
