using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{   
    public void OnNewGameButton()
    {
        // GameManager에게 게임 시작을 요청합니다.
        GameManager.Instance.UpdateGameState(GameManager.GameState.Playing);
    }

    public void OnContinueButton()
    {
        // 이어하기 기능은 MVP 이후 구현이므로, 우선 새 게임과 동일하게 처리합니다.
        OnNewGameButton();
    }

    public void OnSettingsButton()
    {
        // GameManager를 통해 UIManager에게 설정창을 열도록 요청합니다.
        // UIManager가 null이 아닐 때만 실행되도록 안전장치를 추가할 수 있습니다.
        if (GameManager.Instance != null && GameManager.Instance.UIManager != null)
        {
            GameManager.Instance.UIManager.ShowSettingsPanel(true);
        }
        else
        {
            Debug.LogError("UIManager가 GameManager에 등록되지 않았습니다!");
        }
    }

    public void OnQuitGameButton()
    {
        Application.Quit();
        Debug.Log("게임 종료 버튼 클릭됨! (빌드에서만 작동)");
    }

    public void OnCreditButton()
    {
        Debug.Log("크레딧 버튼 클릭됨!");
    }
}
