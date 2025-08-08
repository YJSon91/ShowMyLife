using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 일시정지 메뉴 UI를 제어하는 스크립트입니다. UiBase를 상속받습니다.
/// </summary>
public class PauseMenu : UiBase
{
    /// <summary>
    /// UIManager에 자기 자신을 등록하여 초기화합니다.
    /// </summary>
    public override void Init()
    {
        // GameManager를 통해 UIManager에 접근하여, 이 UI를 등록합니다.
        if (GameManager.Instance != null && GameManager.Instance.UIManager != null)
        {
            GameManager.Instance.UIManager.Add<PauseMenu>(this);
        }
        else
        {
            Debug.LogError("[PauseMenu] GameManager 또는 UIManager가 준비되지 않았습니다!");
        }
    }

    // '이어하기(Resume)' 버튼에 연결될 함수
    public void OnResumeButton()
    {
        // 직접 Time.timeScale을 조작하는 대신, GameManager에 상태 변경을 '요청'합니다.        
        GameManager.Instance.UpdateGameState(GameManager.GameState.Playing);
        // GameManager가 상태를 변경하면 UIManager가 자동으로 이 UI를 숨깁니다.
        GameManager.Instance.UIManager.Hide<PauseMenu>();
    }

    // '재시작(Restart)' 버튼에 연결될 함수
    public void OnRestartButton()
    {
        // 재시작 전에는 반드시 시간을 다시 흐르게 해야 합니다.
        //Time.timeScale = 1f;
        GameManager.Instance.UIManager.Hide<PauseMenu>();
        SceneManager.LoadScene("IntroScene"); 
        GameManager.Instance.UpdateGameState(GameManager.GameState.Start);
    }

    // '설정(Settings)' 버튼에 연결될 함수
    public void OnSettingsButton()
    {
        // GameManager를 통해 UIManager에게 SettingsMenu를 보여달라고 요청합니다.
        GameManager.Instance.UIManager.Hide<PauseMenu>(); // 현재 메뉴를 숨깁니다.
        GameManager.Instance.UIManager.Show<SettingsMenu>(true);
    }

    // '게임 종료(Quit)' 버튼에 연결될 함수
    public void OnQuitGameButton()
    {
        //Debug.Log("게임 종료 버튼 클릭됨!");

#if UNITY_EDITOR
        // Unity 에디터에서 실행했을 경우, 에디터의 플레이 모드를 중지시킵니다.
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 빌드된 게임에서 실행했을 경우, 어플리케이션을 종료합니다.
        Application.Quit();
#endif
    }
}
