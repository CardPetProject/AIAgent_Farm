using TMPro;
using UnityEngine;

public class NoticeManager : MonoBehaviour
{
    public TMP_Text title;
    public TMP_Text desc;

    [SerializeField] private string emptyNoticeTitle = "공지사항";
    [SerializeField] private string emptyNoticeContent = "현재 등록된 공지사항이 없습니다.";
    [SerializeField] private string errorNoticeTitle = "공지사항";
    [SerializeField] private string errorNoticeContent = "공지사항을 불러오지 못했습니다.";

    public void OnClickNoticeButton()
    {
        LoadAndOpen();
    }

    public void LoadAndOpen()
    {
        gameObject.SetActive(true);
        AudioManager.Instance.PlaySFX(SfxType.Click);

        APIController.Notice.GetLatest(
            onSuccess: Init,
            onError: error =>
            {
                Debug.LogError($"[NoticeManager] Failed to load notice: {error}", this);
                Init(errorNoticeTitle, errorNoticeContent);
            });
    }

    public void Init(NoticeResponse notice)
    {
        string noticeTitle = notice != null ? notice.title : string.Empty;
        string noticeContent = notice != null ? notice.content : string.Empty;

        if (string.IsNullOrWhiteSpace(noticeTitle) && string.IsNullOrWhiteSpace(noticeContent))
        {
            Init(emptyNoticeTitle, emptyNoticeContent);
            return;
        }

        Init(noticeTitle, noticeContent);
    }

    public void Init(string noticeTitle, string noticeContent)
    {
        gameObject.SetActive(true);

        if (title != null)
        {
            title.SetText(string.IsNullOrWhiteSpace(noticeTitle) ? emptyNoticeTitle : noticeTitle.Trim());
        }

        if (desc != null)
        {
            desc.SetText(string.IsNullOrWhiteSpace(noticeContent) ? emptyNoticeContent : noticeContent.Trim());
        }
    }

    public void Close()
    {
        gameObject.SetActive(false);
        AudioManager.Instance.PlaySFX(SfxType.Click);
    }
}
