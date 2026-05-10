using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

// 1. 관리할 SFX 종류를 Enum으로 정의합니다.
// 게임에 필요한 효과음이 생길 때마다 여기에 추가해주세요.
public enum SfxType
{
    Bite,
    EnergyLow,
    EnergyCharged,
    Plant_Harvest,
    RobotEffect,
    Walk0,
    Walk1,
    Chat,
    Click,
    Store0,
    Store1,
}

[System.Serializable]
public class AudioData
{
    public SfxType sfxType; // string key 대신 Enum 사용
    public AudioClip clip;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    [Header("BGM Settings")]
    [SerializeField] private AudioSource bgmSource;
    [Tooltip("랜덤으로 재생할 BGM 리스트를 넣으세요.")]
    [SerializeField] private List<AudioClip> bgmList = new List<AudioClip>();
    private int _currentBgmIndex = -1;

    [Header("SFX Settings")]
    [Tooltip("초기에 생성할 SFX AudioSource의 개수")]
    [SerializeField] private int sfxPoolSize = 5;
    [Tooltip("인스펙터에서 SFX 타입과 오디오 클립을 등록하세요.")]
    [SerializeField] private List<AudioData> sfxList = new List<AudioData>();
    [SerializeField] AudioSource windEffectSource; // 바람 효과음용 AudioSource
    [SerializeField] AudioSource birdEffectSource; // 새 효과음용 AudioSource
    
    private List<AudioSource> sfxSources = new List<AudioSource>();
    
    // Dictionary의 Key 타입도 SfxType으로 변경
    private Dictionary<SfxType, AudioClip> sfxDictionary = new Dictionary<SfxType, AudioClip>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeSfxSystem();
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
        if (bgmSource != null && !bgmSource.isPlaying && bgmList.Count > 0)
        {
            PlayNextBGM();
        }
    }

    // --- BGM 로직 ---

    private void PlayNextBGM()
    {
        if (bgmList.Count == 0) return;

        int nextIndex = 0;

        if (bgmList.Count == 1)
        {
            nextIndex = 0;
        }
        else
        {
            nextIndex = Random.Range(0, bgmList.Count);

            if (nextIndex == _currentBgmIndex)
            {
                nextIndex = (nextIndex + 1) % bgmList.Count;
            }
        }

        _currentBgmIndex = nextIndex;
        bgmSource.clip = bgmList[_currentBgmIndex];
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    // --- SFX 로직 (Enum 사용) ---

    private void InitializeSfxSystem()
    {
        foreach (var sfx in sfxList)
        {
            // 동일한 Enum 키가 중복으로 등록되는 것을 방지 (TryAdd는 C# 7.0 이상 지원)
            sfxDictionary.TryAdd(sfx.sfxType, sfx.clip);
        }

        for (int i = 0; i < sfxPoolSize; i++)
        {
            CreateNewSfxSource();
        }
    }

    private AudioSource CreateNewSfxSource()
    {
        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        newSource.outputAudioMixerGroup = sfxMixerGroup;
        newSource.playOnAwake = false;
        sfxSources.Add(newSource);
        return newSource;
    }

    private AudioSource GetAvailableSfxSource()
    {
        foreach (var source in sfxSources)
        {
            if (!source.isPlaying) return source;
        }
        return CreateNewSfxSource(); 
    }

    // 매개변수로 string 대신 SfxType Enum을 받습니다.
    public void PlaySFX(SfxType sfxType)
    {
        if (sfxDictionary.TryGetValue(sfxType, out AudioClip clip))
        {
            AudioSource source = GetAvailableSfxSource();
            source.clip = clip;
            source.Play();
        }
        else
        {
            Debug.LogWarning($"[AudioManager] SFX Type '{sfxType}'가 등록되지 않았습니다.");
        }
    }

    public void PlayEffect()
    {
        windEffectSource.Play();
        birdEffectSource.Play();
    }

    public void StopEffect()
    {
        windEffectSource.Stop();
        birdEffectSource.Stop();
    }

    // --- 볼륨 조절 메서드 ---
    
    public void SetMasterVolume(float volume) => audioMixer.SetFloat("Master", VolumeToDB(volume));
    public void SetBgmVolume(float volume) => audioMixer.SetFloat("BGM", VolumeToDB(volume));
    public void SetSfxVolume(float volume) => audioMixer.SetFloat("SFX", VolumeToDB(volume));

    private float VolumeToDB(float volume)
    {
        return Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f;
    }
}