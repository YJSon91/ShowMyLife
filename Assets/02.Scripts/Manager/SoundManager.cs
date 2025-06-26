using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

// 사용 방법 예시
// 배경음 재생: SoundManager.Instance.PlayBGM(BgmType.Lobby, -1);
// 효과음 재생: SoundManager.Instance.PlaySFX(SfxType.Click, -1);
// 볼륨 설정: SoundManager.Instance.SetMasterVolume(0.8f);
// BGM 정지: SoundManager.Instance.StopBGM();

// BGM (Resources/Sounds/BGM 폴더에 폴더(이름) 배치 필요)
public enum BgmType
{
    Lobby,      // 로비 씬 배경음
    Main,       // 메인 씬 배경음
    GameOver,   // 게임 오버 씬 배경음
}

// SFX (Resources/Sounds/SFX 폴더에 폴더(이름) 배치 필요)
public enum SfxType
{
    // Click, Jump, Hit 등 필요에 따라 추가
}

public class SoundManager : MonoBehaviour
{

    
    
    

    private AudioSource bgmSource; // 배경음 재생용 AudioSource
    private AudioSource sfxSource; // 효과음 재생용 AudioSource

    private Dictionary<BgmType, List<AudioClip>> bgm = new(); // BGM 오디오 클립 저장소
    private Dictionary<SfxType, List<AudioClip>> sfx = new(); // SFX 오디오 클립 저장소

    public float MasterVolume { get; private set; } // 전체 볼륨
    public float BgmVolume { get; private set; }    // 배경음 볼륨
    public float SfxVolume { get; private set; }    // 효과음 볼륨

    // PlayerPrefs 키값 정의
    private const string MASTER_VOLUME_KEY = "MasterVolume";
    private const string BGM_VOLUME_KEY = "BgmVolume";
    private const string SFX_VOLUME_KEY = "SfxVolume";

    // 오브젝트 생성 시 실행
    private void Awake()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterSoundManager(this);
        }
        bgmSource = gameObject.AddComponent<AudioSource>(); // 배경음 전용 오디오 소스 추가
        sfxSource = gameObject.AddComponent<AudioSource>(); // 효과음 전용 오디오 소스 추가
        Init(bgmSource, sfxSource);                         // AudioSource 설정 초기화

        SceneManager.sceneLoaded += OnSceneLoaded;          // 씬 전환 시 BGM 처리 연결
    }

    // 첫 실행 시 사운드 로드 및 볼륨 적용
    private void Start()
    {
        LoadSounds();             // Resources에서 BGM/SFX 로드
        LoadVolumeSettings();     // 저장된 볼륨 불러오기

        SetMasterVolume(MasterVolume);
        SetBgmVolume(BgmVolume);
        SetSfxVolume(SfxVolume);

        PlayBGM(BgmType.Lobby, -1); // 기본 BGM 재생
    }

    // AudioSource 설정
    public void Init(AudioSource bgmSource, AudioSource sfxSource)
    {
        this.bgmSource = bgmSource;
        this.sfxSource = sfxSource;
        this.bgmSource.loop = true; // BGM은 반복 재생
    }

    // Resources 폴더에서 BGM 및 SFX 클립을 로드
    private void LoadSounds()
    {
        // BGM 클립 로드
        foreach (BgmType bgmType in System.Enum.GetValues(typeof(BgmType)))
        {
            string path = $"Sounds/BGM/{bgmType}";
            AudioClip[] clips = Resources.LoadAll<AudioClip>(path).OrderBy(c => c.name).ToArray();
            if (clips.Length > 0)
            {
                bgm.Add(bgmType, new List<AudioClip>(clips));
            }
        }

        // SFX 클립 로드
        foreach (SfxType sfxType in System.Enum.GetValues(typeof(SfxType)))
        {
            string path = $"Sounds/SFX/{sfxType}";
            AudioClip[] clips = Resources.LoadAll<AudioClip>(path).OrderBy(c => c.name).ToArray();
            if (clips.Length > 0)
            {
                sfx.Add(sfxType, new List<AudioClip>(clips));
            }
        }
    }

    // 배경음 재생
    public void PlayBGM(BgmType bgmType, int index)
    {
        if (!bgm.ContainsKey(bgmType)) return;
        List<AudioClip> clips = bgm[bgmType];
        if (clips.Count == 0) return;

        // index < 0 이면 랜덤 재생
        AudioClip clip = (index < 0) ? clips[Random.Range(0, clips.Count)] : clips[index];

        bgmSource.clip = clip;
        bgmSource.volume = BgmVolume;
        bgmSource.Play();
    }

    // 배경음 정지
    public void StopBGM()
    {
        bgmSource.Stop();
    }

    // 효과음 재생
    public void PlaySFX(SfxType sfxType, int index)
    {
        if (!sfx.ContainsKey(sfxType)) return;
        List<AudioClip> clips = sfx[sfxType];
        if (clips.Count == 0) return;

        // index < 0 이면 랜덤 재생
        AudioClip clip = (index < 0) ? clips[Random.Range(0, clips.Count)] : clips[index];

        sfxSource.clip = clip;
        sfxSource.PlayOneShot(clip, SfxVolume);
    }

    // 전체 볼륨 설정
    public void SetMasterVolume(float volume)
    {
        MasterVolume = Mathf.Clamp01(volume);
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, MasterVolume);
    }

    // 배경음 볼륨 설정
    public void SetBgmVolume(float volume)
    {
        BgmVolume = Mathf.Clamp01(volume);
        bgmSource.volume = volume;
        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, BgmVolume);
    }

    // 효과음 볼륨 설정
    public void SetSfxVolume(float volume)
    {
        SfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = volume;
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, SfxVolume);
    }

    // 저장된 볼륨 값 불러오기
    private void LoadVolumeSettings()
    {
        MasterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1.0f);
        BgmVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 1.0f);
        SfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1.0f);
    }

    // 씬 전환 시 자동으로 BGM 바꾸고 싶을 때 사용
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 예시:
        // switch (scene.name)
        // {
        //     case "LobbyScene":
        //         StopBGM();
        //         PlayBGM(BgmType.Lobby, -1);
        //         break;
        //     case "MainScene":
        //         StopBGM();
        //         PlayBGM(BgmType.Main, -1);
        //         break;
        // }
    }

    // 해제 시 이벤트 연결 해제
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}

