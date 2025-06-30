using UnityEngine;
using UnityEngine.SceneManagement;

// 1. MonoBehaviour 대신 UiBase를 상속받습니다.
public class MainMenu : UiBase
{
    public override void Init()
    {
        // 2. UIManager에 자기 자신을 'MainMenu' 타입으로 정확하게 등록합니다.
        GameManager.Instance.UIManager.Add<MainMenu>(this);
    }

    // '새 게임' 버튼을 위한 함수
    public void OnNewGameButton()
    {
        // GameManager에게 게임 시작을 요청합니다.
        GameManager.Instance.UpdateGameState(GameManager.GameState.Playing);
        SceneManager.LoadScene("TestMapScene"); // 실제 게임 씬 이름으로 변경해야 합니다.
    }

    // '이어하기' 버튼을 위한 함수
    public void OnContinueButton()
    {
        // MVP에서는 우선 새 게임과 동일하게 처리합니다.
        OnNewGameButton();
    }

    // '설정' 버튼을 위한 함수
    public void OnSettingsButton()
    {
        // 3. UIManager의 범용 Show<T> 함수를 사용하여 'SettingsMenu'를 보여달라고 요청합니다.
        if (GameManager.Instance != null && GameManager.Instance.UIManager != null)
        {
            GameManager.Instance.UIManager.Hide<MainMenu>(); // 현재 메뉴를 숨깁니다.
            GameManager.Instance.UIManager.Show<SettingsMenu>(true);
        }
        else
        {
            Debug.LogError("UIManager가 GameManager에 등록되지 않았습니다!");
        }
    }

    // '게임 종료' 버튼을 위한 함수
    public void OnQuitGameButton()
    {
        Application.Quit();
        Debug.Log("게임 종료 버튼 클릭됨! (빌드에서만 작동)");
    }

    // '크레딧' 버튼을 위한 함수
    public void OnCreditButton()
    {
        Debug.Log("크레딧 버튼 클릭됨!");
    }
}
