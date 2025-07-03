using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 게임의 각종 설정을 관리하는 UI입니다. UiBase를 상속받습니다.
/// </summary>
public class SettingsMenu : UiBase
{
    [Header("탭 콘텐츠 패널")]
    [Tooltip("게임플레이 설정 UI 패널")]
    [SerializeField] private GameObject _gameplaySettingsPanel;
    [Tooltip("볼륨 설정 UI 패널")]
    [SerializeField] private GameObject _volumeSettingsPanel;
    [Tooltip("조작 설정 UI 패널")]
    [SerializeField] private GameObject _controlSettingsPanel;

    [Header("게임플레이 설정 슬라이더")]
    [SerializeField] private Slider _cameraSensitivitySlider;

    [Header("볼륨 설정 슬라이더")]
    [SerializeField] private Slider _masterVolumeSlider;
    [SerializeField] private Slider _bgmVolumeSlider;
    [SerializeField] private Slider _sfxVolumeSlider;

    [Header("비디오 설정")]
    [SerializeField] private GameObject _videoSettingsPanel;
    [SerializeField] private TextMeshProUGUI _displayModeText;

    public enum DisplayMode { FullScreen, Borderless, Windowed }
    private DisplayMode _currentDisplayMode;

    /// <summary>
    /// UIManager에 자기 자신을 등록하여 초기화합니다.
    /// </summary>
    public override void Init()
    {
        // GameManager를 통해 UIManager에 접근하여, 이 UI를 등록합니다.
        if (GameManager.Instance != null && GameManager.Instance.UIManager != null)
        {
            GameManager.Instance.UIManager.Add<SettingsMenu>(this);
        }
    }
    /// <summary>
    /// 설정창이 활성화될 때마다 호출됩니다.
    /// </summary>
    private void OnEnable()
    {
        // 1. 저장된 설정 값을 불러와서 UI에 반영합니다.
        LoadSettings();
        // 2. 기본적으로 게임플레이 탭을 보여줍니다.
        ShowGameplayTab();
    }
    /// <summary>
    /// PlayerPrefs에 저장된 설정 값을 불러와 각 슬라이더에 적용합니다.
    /// </summary>
    private void LoadSettings()
    {
        _cameraSensitivitySlider.value = PlayerPrefs.GetFloat("CameraSensitivity", 50f);
        _masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
        _bgmVolumeSlider.value = PlayerPrefs.GetFloat("BGMVolume", 0.8f);
        _sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
        _currentDisplayMode = PlayerPrefs.HasKey("DisplayMode")
            ? (DisplayMode)PlayerPrefs.GetInt("DisplayMode", (int)DisplayMode.FullScreen)
            : DisplayMode.FullScreen;

        Debug.Log("저장된 설정을 불러왔습니다.");
    }
    // --- 탭 전환 함수들 ---

    /// <summary>
    /// 게임플레이 설정 탭을 보여줍니다.
    /// </summary>
    public void ShowGameplayTab()
    {
        _gameplaySettingsPanel.SetActive(true);
        _volumeSettingsPanel.SetActive(false);
        _controlSettingsPanel.SetActive(false);
        _videoSettingsPanel.SetActive(false);
    }

    /// <summary>
    /// 볼륨 설정 탭을 보여줍니다.
    /// </summary>
    public void ShowVolumeTab()
    {
        _gameplaySettingsPanel.SetActive(false);
        _volumeSettingsPanel.SetActive(true);
        _controlSettingsPanel.SetActive(false);
        _videoSettingsPanel.SetActive(false);
    }
    public void ShowVideoTab()
    {
        _gameplaySettingsPanel.SetActive(false);
        _volumeSettingsPanel.SetActive(false);
        _controlSettingsPanel.SetActive(false);
        _videoSettingsPanel.SetActive(true);
    }
    public void ShowControlTab()
    {
        _gameplaySettingsPanel.SetActive(false);
        _volumeSettingsPanel.SetActive(false);
        _controlSettingsPanel.SetActive(true);
        _videoSettingsPanel.SetActive(false);
    }
    public void OnCameraSensitivityChanged()
    {
        if (GameManager.Instance?.CameraManager != null)
        {
            float sensitivity = _cameraSensitivitySlider.value;
            // CameraManager의 Sensitivity 프로퍼티에 직접 값 할당하여 실시간 적용
            GameManager.Instance.CameraManager.Sensitivity = sensitivity;
        }
    }
    public void OnMasterVolumeChanged()
    {
        float volume = _masterVolumeSlider.value;
        // GameManager를 통해 SoundManager의 볼륨 설정 함수를 호출합니다.
        if (GameManager.Instance?.SoundManager != null)
        {
            GameManager.Instance.SoundManager.SetMasterVolume(volume);
        }
        // PlayerPrefs에 저장하는 로직도 OnApplyButton에 있어야 합니다.
    }
    public void OnBGMVolumeChanged()
    {
        float volume = _bgmVolumeSlider.value;
        // GameManager를 통해 SoundManager의 BGM 볼륨 설정 함수를 호출합니다.
        if (GameManager.Instance?.SoundManager != null)
        {
            GameManager.Instance.SoundManager.SetBgmVolume(volume);
        }
    }
        // PlayerPrefs에 저장하는 로직도 OnApplyButton에 있어야 합니다.}
    public void OnSFXVolumeChanged()
    {
        float volume = _sfxVolumeSlider.value;
        // GameManager를 통해 SoundManager의 SFX 볼륨 설정 함수를 호출합니다.
        if (GameManager.Instance?.SoundManager != null)
        {
            GameManager.Instance.SoundManager.SetSfxVolume(volume);
        }
    }
    // --- 하단 버튼 함수들 ---

    /// <summary>
    /// 현재 UI의 값들을 PlayerPrefs에 저장하고, 게임에 즉시 적용합니다.
    /// </summary>
    public void OnApplyButton()
    {
        // 1. 현재 슬라이더의 값들을 PlayerPrefs에 저장합니다.
        PlayerPrefs.SetFloat("CameraSensitivity", _cameraSensitivitySlider.value);
        PlayerPrefs.SetFloat("MasterVolume", _masterVolumeSlider.value);
        PlayerPrefs.SetFloat("BGMVolume", _bgmVolumeSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", _sfxVolumeSlider.value);
        PlayerPrefs.Save();
              
        // 2. 저장된 값을 바탕으로, 실제 게임에 즉시 적용합니다.
        //    (슬라이더를 움직이지 않고 '적용'만 눌렀을 경우를 대비)
        OnCameraSensitivityChanged();
        OnMasterVolumeChanged();
        OnBGMVolumeChanged();
        OnSFXVolumeChanged();
       
        Debug.Log("변경된 설정이 게임에 즉시 적용되었습니다.");
    }
    /// <summary>
    /// 설정을 적용하고 창을 닫습니다.
    /// </summary>
    public void OnOKButton()
    {
        OnApplyButton();
        CloseSettingsMenu(); // 창을 닫는 로직을 공통 함수로 분리
    }
    /// <summary>
    /// 변경사항을 저장하지 않고 창을 닫습니다.
    /// </summary>
    public void OnCancelButton()
    {
        CloseSettingsMenu(); // 창을 닫는 로직을 공통 함수로 분리
    }
    public void OnDisplayModeNext()
    {
        _currentDisplayMode++;
        if (_currentDisplayMode > DisplayMode.Windowed) _currentDisplayMode = DisplayMode.FullScreen;
        UpdateDisplayModeText();
    }
    public void OnDisplayModePrevious()
    {
        _currentDisplayMode--;
        if (_currentDisplayMode < DisplayMode.FullScreen) _currentDisplayMode = DisplayMode.Windowed;
        UpdateDisplayModeText();
    }
    private void UpdateDisplayModeText()
    {
        _displayModeText.text = _currentDisplayMode.ToString();
    }
    /// <summary>
    /// 설정창을 닫고, 이전 메뉴로 돌아가는 로직을 처리합니다.
    /// </summary>
    private void CloseSettingsMenu()
    {
        // 1. 먼저 설정창을 숨깁니다.
        GameManager.Instance.UIManager.Hide<SettingsMenu>();

        // 2. GameManager의 현재 게임 상태를 확인합니다.
        if (GameManager.Instance.CurrentState == GameManager.GameState.MainMenu)
        {
            // 3a. 게임 상태가 '메인 메뉴'였다면, 메인 메뉴 UI를 다시 보여줍니다.
            GameManager.Instance.UIManager.Show<MainMenu>(true);
        }
        else
        {
            // 3b. 그 외의 상태(Paused 등)였다면, 일시정지 메뉴 UI를 다시 보여줍니다.
            GameManager.Instance.UIManager.Show<PauseMenu>(true);
        }
    }
}    
