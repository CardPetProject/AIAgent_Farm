using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatFeedback : MonoBehaviour
{
    [SerializeField]
    Transform _feedback;

    TMP_Text _title;
    TMP_Text _desc;
    Slider _slider;
    TMP_Text _sliderTime;

    const float _limitSlierTime = 10f;

    Coroutine _timerCoroutine;
    int _lastDisplayedSeconds = -1;

    void Start()
    {
        Transform eleParent = _feedback.GetChild(0);
        _title = eleParent.GetChild(0).GetComponent<TMP_Text>();
        _desc = eleParent.GetChild(1).GetComponent<TMP_Text>();

        _slider = eleParent.GetChild(4).GetComponent<Slider>();
        _sliderTime = _slider.transform.GetChild(2).GetComponent<TMP_Text>();

        if (_feedback.gameObject.activeInHierarchy)
        {
            OnButtonClick(false);
        }
    }

    public void ShowFeedbackUI(string instruct)
    {
        _desc.SetText(instruct);
        _feedback.gameObject.SetActive(true);

        // 기존에 실행 중인 타이머가 있다면 중단
        if (_timerCoroutine != null)
        {
            StopCoroutine(_timerCoroutine);
        }

        // 코루틴으로 타이머 시작
        _timerCoroutine = StartCoroutine(TimerRoutine());
    }

    private IEnumerator TimerRoutine()
    {
        float currentTime = _limitSlierTime;
        _slider.value = 1f;
        UpdateTimerText(Mathf.CeilToInt(currentTime));

        while (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            _slider.value = currentTime / _limitSlierTime;

            // 소수점 올림 처리한 '초' 계산
            int currentSeconds = Mathf.CeilToInt(currentTime);

            // 텍스트는 1초에 한 번, 값이 바뀌었을 때만 갱신 (핵심 최적화)
            if (currentSeconds != _lastDisplayedSeconds)
            {
                UpdateTimerText(currentSeconds);
            }

            yield return null; // 다음 프레임까지 대기
        }

        // 시간이 0이 되면 자동으로 닫기
        OnButtonClick(false);
    }

    // 텍스트 갱신 전용 메서드
    private void UpdateTimerText(int seconds)
    {
        _lastDisplayedSeconds = seconds;
        _sliderTime.SetText(seconds + "s");
    }

    public void OnButtonClick(bool isYes)
    {
        if (isYes)
        {
            // Yes 버튼 클릭 로직
        }

        // UI가 닫힐 때 코루틴이 계속 도는 것을 방지
        if (_timerCoroutine != null)
        {
            StopCoroutine(_timerCoroutine);
            _timerCoroutine = null;
        }

        // 초기화 및 UI 끄기
        _slider.value = 1f;
        UpdateTimerText((int)_limitSlierTime);

        _feedback.gameObject.SetActive(false);
    }
}