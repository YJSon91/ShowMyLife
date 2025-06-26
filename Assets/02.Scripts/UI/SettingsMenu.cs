using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : UiBase
{
    [Header("탭 버튼 및 콘텐츠 패널")]
    [SerializeField] private GameObject _gameplaySettingsPanel;
    [SerializeField] private GameObject _volumeSettingsPanel;

    [Header("게임플레이 설정 슬라이더")]
    [SerializeField] private Slider _mouseSensitivitySlider;
    [SerializeField] private Slider _cameraSensitivitySlider;

    [Header("볼륨 설정 슬라이더")]
    [SerializeField] private Slider _masterVolumeSlider;
    [SerializeField] private Slider _bgmVolumeSlider;
    [SerializeField] private Slider _sfxVolumeSlider;

    public override void Init()
    {
        // 3. UIManager에 자기 자신을 등록합니다.
        //    이때, 자신의 클래스 타입을 제네릭으로 넘겨줍니다.
        GameManager.Instance.UIManager.Add<SettingsMenu>(this);
    }

    // 설정창이 켜질 때마다 호출되는 Unity 생명주기 함수입니다.
    private void OnEnable()
    {
        // 1. 저장된 설정 값을 불러와서 UI에 반영합니다.
        LoadSettings();
        // 2. 기본적으로 게임플레이 탭을 보여줍니다.
        ShowGameplayTab();
    }

    // 설정 값을 불러와 UI에 적용하는 함수
    private void LoadSettings()
    {
        // PlayerPrefs에서 값을 불러오고, 저장된 값이 없으면 기본값(예: 50)을 사용합니다.
        _mouseSensitivitySlider.value = PlayerPrefs.GetFloat("MouseSensitivity", 50f);
        _cameraSensitivitySlider.value = PlayerPrefs.GetFloat("CameraSensitivity", 50f);

        _masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
        _bgmVolumeSlider.value = PlayerPrefs.GetFloat("BGMVolume", 1f);
        _sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);

        Debug.Log("저장된 설정을 불러왔습니다.");
    }

    // --- 탭 전환 함수들 ---
    // 이 함수들을 각 탭 버튼의 OnClick() 이벤트에 연결합니다.
    public void ShowGameplayTab()
    {
        _gameplaySettingsPanel.SetActive(true);
        _volumeSettingsPanel.SetActive(false);
    }

    public void ShowVolumeTab()
    {
        _gameplaySettingsPanel.SetActive(false);
        _volumeSettingsPanel.SetActive(true);
    }

    // --- 슬라이더 값 변경 시 호출될 함수들 ---
    // 각 슬라이더의 OnValueChanged() 이벤트에 연결합니다.
    public void OnMouseSensitivityChanged()
    {
        // TODO: 실제 마우스 감도 조절 로직 연결
        Debug.Log("마우스 감도: " + _mouseSensitivitySlider.value);
    }

    public void OnCameraSensitivityChanged()
    {
        // TODO: 실제 카메라 감도 조절 로직 연결
        Debug.Log("카메라 감도: " + _cameraSensitivitySlider.value);
    }

    public void OnMasterVolumeChanged()
    {
        // TODO: 오디오 믹서 등을 통해 실제 전체 볼륨 조절
        Debug.Log("마스터 볼륨: " + _masterVolumeSlider.value);
    }

    public void OnBGMVolumeChanged()
    {
        // TODO: 배경음악 볼륨 조절
        Debug.Log("배경음악 볼륨: " + _bgmVolumeSlider.value);
    }

    public void OnSFXVolumeChanged()
    {
        // TODO: 효과음 볼륨 조절
        Debug.Log("효과음 볼륨: " + _sfxVolumeSlider.value);
    }

    // --- 하단 버튼 함수들 ---
    // 각 하단 버튼의 OnClick() 이벤트에 연결합니다.
    public void OnApplyButton()
    {
        // 현재 UI의 값들을 PlayerPrefs에 저장합니다.
        PlayerPrefs.SetFloat("MouseSensitivity", _mouseSensitivitySlider.value);
        PlayerPrefs.SetFloat("CameraSensitivity", _cameraSensitivitySlider.value);

        PlayerPrefs.SetFloat("MasterVolume", _masterVolumeSlider.value);
        PlayerPrefs.SetFloat("BGMVolume", _bgmVolumeSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", _sfxVolumeSlider.value);

        // PlayerPrefs에 변경사항을 즉시 디스크에 저장합니다.
        PlayerPrefs.Save();

        Debug.Log("설정이 적용되었습니다.");
    }

    public void OnOKButton()
    {
        OnApplyButton();
        // 4. 자기 자신을 닫을 때도 UIManager를 통해 요청합니다.
        GameManager.Instance.UIManager.Show<SettingsMenu>(false);
    }

    public void OnCancelButton()
    {
        // 아무것도 저장하지 않고 그냥 창을 닫습니다.
        // 다음에 창을 다시 열면 OnEnable에서 기존에 저장된 값을 다시 불러오므로 '취소' 효과가 납니다.
        GameManager.Instance.UIManager.Show<SettingsMenu>(false);
    }
}
