using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// BGM 타입 (Resources/Sounds/BGM/ 폴더 이름과 일치해야 함)
public enum BgmType
{
    Lobby,
    Main,
    GameOver,
    Ending // 엔딩/크레딧을 위한 BGM 타입 추가
}

// SFX 타입 (Resources/Sounds/SFX/ 폴더 이름과 일치해야 함)
public enum SfxType
{
    ButtonClick,
    Jump,
    Land,
    Walk,
    Run,
   
}

public enum NarrationType
{
    Intro,
    Tutorial,
    Story,
    Ending,
    Narration1,
    Narration2,
}

public class SoundManager : MonoBehaviour
{
    private AudioSource bgmSource;
    private AudioSource sfxSource;
    private AudioSource narrationSource;

    private Dictionary<BgmType, List<AudioClip>> bgmClips = new();
    private Dictionary<SfxType, List<AudioClip>> sfxClips = new();
    private Dictionary<NarrationType, AudioClip> narrationClips = new(); // 나레이션 클립을 저장할 딕셔너리


    // Awake에서는 자신의 내부 컴포넌트만 준비합니다.
    private void Awake()
    {
        // 자식 오브젝트에서 BGM, SFX용 AudioSource를 찾아 할당합니다.
        var audioSources = GetComponentsInChildren<AudioSource>();
        if (audioSources.Length >= 2)
        {
            bgmSource = audioSources[0];
            sfxSource = audioSources[1];
            narrationSource = audioSources[2];
            bgmSource.loop = true; // BGM은 반복 재생
        }
        else
        {
            Debug.LogError("[SoundManager] 자식 오브젝트에 AudioSource 2개가 필요합니다!", this.gameObject);
        }
    }

    // Start에서 다른 매니저와 소통하고 초기 설정을 진행합니다.
    private void Start()
    {
        // GameManager에 자신을 등록합니다.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterSoundManager(this);
        }

        // 오디오 클립을 로드하고, 저장된 볼륨 설정을 적용합니다.
        LoadSounds();
        LoadAndApplyVolume();
    }


    // --- 이벤트 핸들러 ---
    private void HandlePlayerJump() => PlaySFX(SfxType.Jump);
    private void HandlePlayerLanded() => PlaySFX(SfxType.Land);
    public void PlayButtonClickSFX() => PlaySFX(SfxType.ButtonClick);


    // --- Public API ---

    /// <summary>
    /// 지정된 타입의 BGM을 재생합니다. GameManager에 의해 호출됩니다.
    /// </summary>
    public void PlayBGM(BgmType bgmType)
    {
        if (!bgmClips.ContainsKey(bgmType) || bgmClips[bgmType].Count == 0)
        {
            Debug.LogError($"[SoundManager] 재생할 '{bgmType}' BGM 클립이 없습니다! Resources 폴더를 확인해주세요.");
            return;
        }
        // 폴더 내의 클립 중 하나를 랜덤으로 재생합니다.
        List<AudioClip> clips = bgmClips[bgmType];
        AudioClip clip = clips[Random.Range(0, clips.Count)];

        bgmSource.clip = clip;
        bgmSource.Play();
    }

    /// <summary>
    /// 지정된 타입의 SFX를 재생합니다.
    /// </summary>
    public void PlaySFX(SfxType sfxType)
    {
        if (!sfxClips.ContainsKey(sfxType) || sfxClips[sfxType].Count == 0)
        {
            Debug.LogError($"[SoundManager] 재생할 '{sfxType}' SFX 클립이 없습니다! Resources 폴더를 확인해주세요.");
            return;
        }
        List<AudioClip> clips = sfxClips[sfxType];
        AudioClip clip = clips[Random.Range(0, clips.Count)];

        sfxSource.PlayOneShot(clip);
    }
    /// <summary>
    /// 이름으로 나레이션 클립을 찾아 재생합니다.
    /// </summary>
    /// <param name="clipName">재생할 오디오 클립의 파일 이름</param>
    public void PlayNarration(string clipName)
    {       
        if (string.IsNullOrEmpty(clipName)) return;

        // 1. 전달받은 문자열(string)을 NarrationType(enum)으로 변환합니다.
        if (System.Enum.TryParse(clipName, out NarrationType type))
        {
            // 2. 변환에 성공했다면, 이제 올바른 모양의 열쇠(type)로 딕셔너리에서 클립을 찾습니다.
            if (narrationClips.TryGetValue(type, out AudioClip clipToPlay))
            {
                narrationSource.Stop(); // 이전 나레이션 중지
                narrationSource.clip = clipToPlay;
                narrationSource.Play();
                Debug.Log($"[SoundManager] 나레이션 재생: {clipName}");
            }
            else
            {
                Debug.LogWarning($"[SoundManager] '{clipName}' 타입의 나레이션 클립을 딕셔너리에서 찾을 수 없습니다.");
            }
        }
        else
        {
            // 3. 만약 문자열을 enum으로 변환하는 것 자체를 실패했다면 경고를 보냅니다.
            Debug.LogWarning($"[SoundManager] '{clipName}'은(는) 유효한 NarrationType이 아닙니다.");
        }   
    }
    public void StopBGM() => bgmSource.Stop();


    // --- 이벤트 구독/해제 (GameManager가 호출) ---

    /// <summary>
    /// GameManager가 플레이어의 InputReader 이벤트를 구독하라고 명령할 때 호출됩니다.
    /// </summary>
    public void SubscribeToPlayerEvents(InputReader inputReader)
    {
        if (inputReader == null) return;
        //inputReader.onJumpPerformed += HandlePlayerJump; 전에쓰던 인풋리더에서 불러오는 점프
        // 점프 이벤트는 애니메이션 이벤트로 처리하므로 여기서 구독하지 않음
        // inputReader.onJumpPerformed += HandlePlayerJump;
        
        // TODO: PlayerController에서 OnLanded 이벤트가 구현되면 여기에 구독 코드를 추가합니다.
        // playerController.OnLanded += HandlePlayerLanded;
        Debug.Log("[SoundManager] Player 이벤트 구독 완료.");
    }

    /// <summary>
    /// GameManager가 이전 플레이어의 이벤트 구독을 해제하라고 명령할 때 호출됩니다.
    /// </summary>
    public void UnsubscribeFromPlayerEvents(InputReader inputReader)
    {
        if (inputReader == null) return;
        //inputReader.onJumpPerformed -= HandlePlayerJump; 전에쓰던 점프 인풋리더 불러오기
        // 점프 이벤트는 애니메이션 이벤트로 처리하므로 여기서 구독 해제하지 않음
        // inputReader.onJumpPerformed -= HandlePlayerJump;
        
        // playerController.OnLanded -= HandlePlayerLanded;
        Debug.Log("[SoundManager] Player 이벤트 구독 해제.");
    }


    // --- 볼륨 제어 (SettingsMenu의 요청을 GameManager가 중개) ---

    public void SetMasterVolume(float volume) => AudioListener.volume = Mathf.Clamp01(volume);
    public void SetBgmVolume(float volume)
    {
        if (bgmSource != null) bgmSource.volume = Mathf.Clamp01(volume);
    }
    public void SetSfxVolume(float volume)
    {
        if (sfxSource != null) sfxSource.volume = Mathf.Clamp01(volume);
    }

    // --- Private Helper 함수들 ---
    private void LoadSounds()
    {
        foreach (BgmType type in System.Enum.GetValues(typeof(BgmType)))
        {
            bgmClips[type] = Resources.LoadAll<AudioClip>($"Sounds/BGM/{type}").ToList();
        }
        foreach (SfxType type in System.Enum.GetValues(typeof(SfxType)))
        {
            sfxClips[type] = Resources.LoadAll<AudioClip>($"Sounds/SFX/{type}").ToList();
        }
        foreach(NarrationType type in System.Enum.GetValues(typeof(NarrationType)))
{          
            AudioClip clip = Resources.Load<AudioClip>($"Sounds/Narration/{type}");
            if (clip != null)
            {
                narrationClips[type] = clip;
            }         
        }
    }

    private void LoadAndApplyVolume()
    {
        // PlayerPrefs에서 값을 불러와서
        float master = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float bgm = PlayerPrefs.GetFloat("BGMVolume", 0.8f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 0.8f);

        // 즉시 적용합니다.
        SetMasterVolume(master);
        SetBgmVolume(bgm);
        SetSfxVolume(sfx);
    }
}
