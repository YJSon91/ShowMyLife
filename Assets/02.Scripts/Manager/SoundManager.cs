using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

// BGM 타입 (Resources/Sounds/BGM/ 폴더 이름과 일치해야 함)
public enum BgmType
{
    Lobby,
    Main,
    GameOver,
}

// SFX 타입 (Resources/Sounds/SFX/ 폴더 이름과 일치해야 함)
public enum SfxType
{
    ButtonClick,
    Jump,
    Land
    // ... 필요에 따라 추가
}

public class SoundManager : MonoBehaviour
{
    private AudioSource bgmSource;
    private AudioSource sfxSource;

    private Dictionary<BgmType, List<AudioClip>> bgmClips = new();
    private Dictionary<SfxType, List<AudioClip>> sfxClips = new();

    public float MasterVolume { get; private set; }
    public float BgmVolume { get; private set; }
    public float SfxVolume { get; private set; }

    private const string MASTER_VOLUME_KEY = "MasterVolume";
    private const string BGM_VOLUME_KEY = "BgmVolume";
    private const string SFX_VOLUME_KEY = "SfxVolume";

    private void Awake()
    {
       
        

        // --- 수정 2: Init() 함수 내용을 Awake()로 통합하여 간소화 ---
        bgmSource = gameObject.AddComponent<AudioSource>();
        sfxSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true; // BGM은 반복 재생

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterSoundManager(this);
        }
        else
        {
            Debug.LogError("[SoundManager] SoundManager가 씬에 존재하지 않습니다!");
        }
        LoadSounds();
        LoadVolumeSettings();

        // 불러온 볼륨 값을 실제 AudioSource에 적용
        SetMasterVolume(MasterVolume);
        SetBgmVolume(BgmVolume);
        SetSfxVolume(SfxVolume);

        // --- 수정 3: 자동 BGM 재생 로직 삭제 ---
        // PlayBGM(BgmType.Lobby, -1); // 이 제어권은 GameManager에게 넘겨줍니다.
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Resources 폴더에서 오디오 클립 로드
    private void LoadSounds()
    {
        foreach (BgmType type in System.Enum.GetValues(typeof(BgmType)))
        {
            AudioClip[] clips = Resources.LoadAll<AudioClip>($"Sounds/BGM/{type}");
            if (clips.Length > 0) bgmClips.Add(type, new List<AudioClip>(clips));
        }

        foreach (SfxType type in System.Enum.GetValues(typeof(SfxType)))
        {
            AudioClip[] clips = Resources.LoadAll<AudioClip>($"Sounds/SFX/{type}");
            if (clips.Length > 0) sfxClips.Add(type, new List<AudioClip>(clips));
        }
    }

    // --- Public API 메서드 ---
    public void PlayBGM(BgmType bgmType, int index = -1)
    {
        if (!bgmClips.ContainsKey(bgmType)) return;
        List<AudioClip> clips = bgmClips[bgmType];
        if (clips.Count == 0) return;
        AudioClip clip = (index < 0) ? clips[Random.Range(0, clips.Count)] : clips[index];

        bgmSource.clip = clip;
        bgmSource.Play();
    }

    public void StopBGM() => bgmSource.Stop();

    public void PlaySFX(SfxType sfxType, int index = -1)
    {
        if (!sfxClips.ContainsKey(sfxType)) return;
        List<AudioClip> clips = sfxClips[sfxType];
        if (clips.Count == 0) return;
        AudioClip clip = (index < 0) ? clips[Random.Range(0, clips.Count)] : clips[index];

        sfxSource.PlayOneShot(clip);
    }

    // --- 수정 4: 볼륨 '적용'만 담당하도록 수정 (저장 로직 삭제) ---
    public void SetMasterVolume(float volume)
    {
        MasterVolume = Mathf.Clamp01(volume);
        AudioListener.volume = MasterVolume; // 전체 볼륨은 AudioListener를 제어
    }

    public void SetBgmVolume(float volume)
    {
        BgmVolume = Mathf.Clamp01(volume);
        bgmSource.volume = BgmVolume; // BGM 볼륨은 BGM AudioSource를 제어
    }

    public void SetSfxVolume(float volume)
    {
        SfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = SfxVolume; // SFX 볼륨은 SFX AudioSource를 제어
    }

    private void LoadVolumeSettings()
    {
        MasterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
        BgmVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 0.8f);
        SfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 0.8f);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 이 함수의 제어권은 GameManager에게 넘겨주는 것이 좋습니다.
        // GameManager가 상태에 따라 BGM 재생을 명령하기 때문입니다.
    }
}
