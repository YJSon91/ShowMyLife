using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("UI Panels")]
    [Tooltip("활성화시킬 설정 화면 패널을 연결합니다.")]
    [SerializeField] private GameObject _settingsPanel; // 설정 화면 Panel 오브젝트

    [Header("Scene Names")]
    [Tooltip("새 게임 시작 시 불러올 씬의 이름입니다.")]
    [SerializeField] private string _startSceneName = "Test_Scene"; // "KindergartenScene" 등 실제 씬 이름으로 변경

    private void Start()
    {
        // 시작할 때 설정 화면은 꺼져 있도록 합니다.
        if (_settingsPanel != null)
        {
            _settingsPanel.SetActive(false);
        }
    }

    public void OnNewGameButton()
    {
        // 지정된 시작 씬을 불러옵니다.
        SceneManager.LoadScene(_startSceneName);
    }

    // '이어하기' 버튼에 연결될 함수 (MVP에서는 새 게임과 동일하게 처리)
    public void OnContinueButton()
    {
        // 우선은 새 게임과 같은 기능을 하도록 합니다.
        OnNewGameButton();
    }

    public void OnSettingsButton()
    {
        // 설정 화면이 있다면 활성화합니다.
        if (_settingsPanel != null)
        {
            _settingsPanel.SetActive(true);
        }
        else
        {
            Debug.Log("설정 화면이 연결되지 않았습니다.");
        }
    }

    public void OnQuitGameButton()
    {
        Application.Quit();
        // 에디터 테스트용 로그
        Debug.Log("게임 종료 버튼 클릭됨! (빌드에서만 작동)");
    }
    public void OnCreditButton()
    {
        // MVP 단계에서는 우선 로그만 출력합니다.
        Debug.Log("크레딧 버튼 클릭됨!");
    }
}
