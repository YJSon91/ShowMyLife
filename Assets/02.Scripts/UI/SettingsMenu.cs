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
    [SerializeField] private float _minCameraSensitivity = 1f;
    [SerializeField] private float _maxCameraSensitivity = 10f;

    [Header("볼륨 설정 슬라이더")]
    [SerializeField] private Slider _masterVolumeSlider;
    [SerializeField] private Slider _bgmVolumeSlider;
    [SerializeField] private Slider _sfxVolumeSlider;

    [Header("비디오 설정")]
    [SerializeField] private GameObject _videoSettingsPanel;
    [SerializeField] private TextMeshProUGUI _displayModeText;

    public enum DisplayMode { FullScreen, Borderless, Windowed }
    private DisplayMode _currentDisplayMode;

    private void Update()
    {
        // 만약 'F1' 키가 눌렸다면
        if (Input.GetKeyDown(KeyCode.F1))
        {
            // OnDefaultsButton() 함수를 직접 호출합니다.
            OnDefaultsButton();
        }
    }

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
        LoadSettings();

        OnCameraSensitivityChanged();
        OnMasterVolumeChanged();
        OnBGMVolumeChanged();
        OnSFXVolumeChanged();
        UpdateDisplayModeText(); // 디스플레이 모드 텍스트도 업데이트     

        ShowGameplayTab();
    }
    /// <summary>
    /// PlayerPrefs에 저장된 설정 값을 불러와 각 슬라이더에 적용합니다.
    /// </summary>
    private void LoadSettings()
    {
        float savedSens = PlayerPrefs.GetFloat("CameraSensitivity", 3f);
        // (현재값 - 최소값) / (최대값 - 최소값) = 0~1 사이의 비율
        _cameraSensitivitySlider.SetValueWithoutNotify(Mathf.InverseLerp(_minCameraSensitivity, _maxCameraSensitivity, savedSens));
        _masterVolumeSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("MasterVolume", 1f));
        _bgmVolumeSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("BGMVolume", 0.8f));
        _sfxVolumeSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("SFXVolume", 0.8f));


        _currentDisplayMode = (DisplayMode)PlayerPrefs.GetInt("DisplayMode", (int)DisplayMode.Borderless);
        UpdateDisplayModeText();

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
            // --- 3. 슬라이더의 0~1 값을 실제 감도 범위로 '역정규화'합니다 ---
            // 슬라이더의 현재 비율(value)을 실제 감도 값으로 변환
            float actualSensitivity = Mathf.Lerp(_minCameraSensitivity, _maxCameraSensitivity, _cameraSensitivitySlider.value);
            GameManager.Instance.CameraManager.Sensitivity = actualSensitivity;
        }
    }

    public void OnMasterVolumeChanged()
    {
        Debug.LogWarning(">>>>> 마스터 볼륨 변경 함수 호출됨! <<<<<");
        float volume = _masterVolumeSlider.value;
        if (GameManager.Instance?.SoundManager != null)
        {
            GameManager.Instance.SoundManager.SetMasterVolume(volume);
        }
    }

    public void OnBGMVolumeChanged()
    {
        Debug.LogWarning(">>>>> BGM 볼륨 변경 함수 호출됨! <<<<<");
        float volume = _bgmVolumeSlider.value;
        if (GameManager.Instance?.SoundManager != null)
        {
            GameManager.Instance.SoundManager.SetBgmVolume(volume);
        }
    }

    public void OnSFXVolumeChanged()
    {
        Debug.LogWarning(">>>>> SFX 볼륨 변경 함수 호출됨! <<<<<");
        float volume = _sfxVolumeSlider.value;
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
        FullScreenMode mode = FullScreenMode.FullScreenWindow; // 기본값 (테두리 없는 창)
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
        // 현재 해상도를 유지하면서 화면 모드만 변경합니다.
        Screen.SetResolution(Screen.width, Screen.height, mode);
        PlayerPrefs.SetInt("DisplayMode", (int)_currentDisplayMode);
        Debug.Log($"디스플레이 모드를 '{mode}' (으)로 변경 및 저장했습니다.");

        //  현재 슬라이더의 값들을 PlayerPrefs에 저장합니다.
        float actualSensitivity = Mathf.Lerp(_minCameraSensitivity, _maxCameraSensitivity, _cameraSensitivitySlider.value);
        PlayerPrefs.SetFloat("CameraSensitivity", actualSensitivity);
        PlayerPrefs.SetFloat("MasterVolume", _masterVolumeSlider.value);
        PlayerPrefs.SetFloat("BGMVolume", _bgmVolumeSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", _sfxVolumeSlider.value);
        PlayerPrefs.Save();

        //  저장된 값을 바탕으로, 실제 게임에 즉시 적용합니다.
        //    (슬라이더를 움직이지 않고 '적용'만 눌렀을 경우를 대비)
        OnCameraSensitivityChanged();
        OnMasterVolumeChanged();
        OnBGMVolumeChanged();
        OnSFXVolumeChanged();
        GameManager.Instance.LoadAllKeybindings();

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
    /// <summary>
    /// '기본값' 버튼에 연결될 함수입니다. 모든 설정을 초기화합니다.
    /// </summary>
    public void OnDefaultsButton()
    {
        Debug.Log("모든 설정을 기본값으로 초기화합니다.");

        // 1. PlayerPrefs에 저장된 모든 설정 관련 키를 삭제합니다.
        PlayerPrefs.DeleteKey("CameraSensitivity");
        PlayerPrefs.DeleteKey("DisplayMode");
        PlayerPrefs.DeleteKey("MasterVolume");
        PlayerPrefs.DeleteKey("BGMVolume");
        PlayerPrefs.DeleteKey("SFXVolume");
        PlayerPrefs.DeleteKey("AllKeyRebinds");

        PlayerPrefs.Save();

        // 2. LoadSettings()를 다시 호출하여 UI를 기본값으로 새로고침합니다.
        // GetFloat/GetInt의 두 번째 인자인 기본값이 슬라이더에 적용됩니다.
        LoadSettings();

        // 3. 변경된 기본값을 실제 게임 시스템에 즉시 적용합니다.
        OnCameraSensitivityChanged();
        //ApplyDisplayMode(); // 
        OnMasterVolumeChanged();
        OnBGMVolumeChanged();
        OnSFXVolumeChanged();
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
        LoadSettings();

        // 2. UI뿐만 아니라 실제 게임 시스템에도 저장된 값을 다시 적용하여
        //    변경했지만 저장하지 않은 값들을 되돌립니다.
        OnCameraSensitivityChanged();
        OnMasterVolumeChanged();
        OnBGMVolumeChanged();
        OnSFXVolumeChanged();
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
