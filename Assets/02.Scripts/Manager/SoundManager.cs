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
    Land,
    Walk,
    Run
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

    private void Awake()
    {
        // AudioSource를 컴포넌트에서 가져오는 방식으로 변경
        var audioSources = GetComponentsInChildren<AudioSource>();
        if (audioSources.Length >= 2)
        {
            bgmSource = audioSources[0];
            sfxSource = audioSources[1];
        }
        else
        {
            Debug.LogError("SoundManager에 AudioSource가 2개 필요합니다.");
        }

        LoadSounds();   // 오디오 클립 로드
        ApplyVolume();  // 볼륨 설정 반영
    }

    // 현재 설정된 볼륨값을 실제 AudioSource 및 AudioListener에 반영하는 함수
    private void ApplyVolume()
    {
        AudioListener.volume = MasterVolume;

        if (bgmSource != null)
            bgmSource.volume = BgmVolume;

        if (sfxSource != null)
            sfxSource.volume = SfxVolume;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Resources 폴더에서 오디오 클립 로드
    private void LoadSounds()
    {
        // Dictionary에 직접 대입하여 중복 키 예외 방지
        foreach (BgmType type in System.Enum.GetValues(typeof(BgmType)))
        {
            AudioClip[] clips = Resources.LoadAll<AudioClip>($"Sounds/BGM/{type}");
            bgmClips[type] = clips.ToList(); // 덮어쓰기 방식
        }

        foreach (SfxType type in System.Enum.GetValues(typeof(SfxType)))
        {
            AudioClip[] clips = Resources.LoadAll<AudioClip>($"Sounds/SFX/{type}");
            sfxClips[type] = clips.ToList(); // 덮어쓰기 방식
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 이 함수의 제어권은 GameManager에게 넘겨주는 것이 좋습니다.
        // GameManager가 상태에 따라 BGM 재생을 명령하기 때문입니다.
    }

    // 이벤트 구독
    private void OnEnable()
    {

    }
    // 이벤트 구독 해제
    private void OnDisable()
    {

    }

    // 점프 이벤트
    private void HandlePlayerJump()
    {
        PlaySFX(SfxType.Jump);
    }

    // 착지 이벤트
    private void HandlePlayerLanded()
    {
        PlaySFX(SfxType.Land);
    }
    // UI 버튼 클릭
    public void PlayButtonClickSFX()
    {
        PlaySFX(SfxType.ButtonClick);
    }
}
