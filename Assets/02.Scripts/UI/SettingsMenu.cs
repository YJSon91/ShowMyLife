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

    // --- 슬라이더 값 변경 시 호출될 함수들 ---
    // 실제 기능 연동은 다른 파트 리팩토링 후 진행합니다. 지금은 로그만 출력합니다.

    public void OnCameraSensitivityChanged()
    {
        if (GameManager.Instance?.CameraManager != null)
        {
            float sensitivity = _cameraSensitivitySlider.value;
            // CameraManager의 Sensitivity 프로퍼티에 직접 값 할당
            GameManager.Instance.CameraManager.Sensitivity = sensitivity;
        }
    }
    public void OnMasterVolumeChanged() => Debug.Log("마스터 볼륨 변경 시도: " + _masterVolumeSlider.value);
    public void OnBGMVolumeChanged() => Debug.Log("배경음악 볼륨 변경 시도: " + _bgmVolumeSlider.value);
    public void OnSFXVolumeChanged() => Debug.Log("효과음 볼륨 변경 시도: " + _sfxVolumeSlider.value);

    // --- 하단 버튼 함수들 ---

    /// <summary>
    /// 현재 슬라이더의 값들을 PlayerPrefs에 저장합니다.
    /// </summary>
    public void OnApplyButton()
    {
        FullScreenMode mode = FullScreenMode.Windowed;
        switch (_currentDisplayMode)
        {
            case DisplayMode.FullScreen:
                mode = FullScreenMode.ExclusiveFullScreen;
                break;
            case DisplayMode.Borderless:
                mode = FullScreenMode.FullScreenWindow;
                break;
            case DisplayMode.Windowed:
                mode = FullScreenMode.Windowed;
                break;
        }
        Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, mode);
        // PlayerPrefs에 현재 설정 값을 저장합니다.
        PlayerPrefs.SetInt("DisplayMode", (int)_currentDisplayMode);
        PlayerPrefs.SetFloat("CameraSensitivity", _cameraSensitivitySlider.value);
        PlayerPrefs.SetFloat("MasterVolume", _masterVolumeSlider.value);
        PlayerPrefs.SetFloat("BGMVolume", _bgmVolumeSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", _sfxVolumeSlider.value);

        // 변경사항을 즉시 디스크에 저장합니다.
        PlayerPrefs.Save();

        Debug.Log("설정이 적용되었습니다.");

        // TODO: 변경된 값을 즉시 게임에 적용하는 로직 호출 (예: GameManager.Instance.SoundManager.SetMasterVolume(...))
    }
    /// <summary>
    /// 설정을 적용하고 창을 닫습니다.
    /// </summary>
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
