using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("BGM Playlist")]
    [SerializeField] private List<AudioClip> bgmList = new List<AudioClip>();
    
    private int _currentBgmIndex = -1;

    private void Awake()
    {
        // Singleton 패턴 구성 (씬 전환 시 유지하려면 DontDestroyOnLoad 추가)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 필요 시 주석 해제
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (bgmList.Count > 0)
        {
            PlayNextBGM();
        }
    }

    private void Update()
    {
        // BGM이 끝나면 다음 BGM을 재생
        if (bgmSource != null && !bgmSource.isPlaying && bgmList.Count > 0)
        {
            PlayNextBGM();
        }
    }

    private void PlayNextBGM()
    {
        if (bgmList.Count == 0) return;

        int nextIndex = 0;

        if (bgmList.Count == 1)
        {
            // BGM이 1개뿐이라면 같은 곡 반복
            nextIndex = 0;
        }
        else
        {
            // 랜덤 인덱스 추출
            nextIndex = Random.Range(0, bgmList.Count);

            // 이전 인덱스와 같다면 1을 더하고 리스트 크기로 나눈 나머지를 사용하여 강제로 다른 곡 선택
            if (nextIndex == _currentBgmIndex)
            {
                nextIndex = (nextIndex + 1) % bgmList.Count;
            }
        }

        _currentBgmIndex = nextIndex;
        bgmSource.clip = bgmList[_currentBgmIndex];
        bgmSource.Play();
    }

    /// <summary>
    /// 단일 SFX 재생용 메서드 (필요에 따라 Object Pooling으로 확장 가능)
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    // --- 볼륨 조절 메서드 (SoundConfigController에서 호출) ---
    
    public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat("Master", VolumeToDB(volume));
    }

    public void SetBgmVolume(float volume)
    {
        audioMixer.SetFloat("BGM", VolumeToDB(volume));
    }

    public void SetSfxVolume(float volume)
    {
        audioMixer.SetFloat("SFX", VolumeToDB(volume));
    }

    // 슬라이더 값(0~1)을 AudioMixer의 데시벨(dB) 값으로 변환 (-80 ~ 0)
    private float VolumeToDB(float volume)
    {
        // 0에 한없이 가까우면 -80dB(Mute)에 가깝게 처리
        return Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f;
    }
}