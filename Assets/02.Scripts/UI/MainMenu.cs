using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

// 1. MonoBehaviour 대신 UiBase를 상속받습니다.
public class MainMenu : UiBase
{
    private CanvasGroup _mainMenuGroup;
    public override void Init()
    {
        // 2. UIManager에 자기 자신을 'MainMenu' 타입으로 정확하게 등록합니다.
        GameManager.Instance.UIManager.Add<MainMenu>(this);
        _mainMenuGroup = GetComponent<CanvasGroup>();
    }

    // '새 게임' 버튼을 위한 함수
    public void OnNewGameButton()
    {
        // GameManager에게 게임 시작을 요청합니다.        
       //GameManager.Instance.UpdateGameState(GameManager.GameState.Tutorial);
       GameManager.Instance.UpdateGameState(GameManager.GameState.Playing); // 게임 상태를 로딩으로 변경합니다.
    }

    // '이어하기' 버튼을 위한 함수
    public void OnContinueButton()
    {
        GameManager.Instance.UpdateGameState(GameManager.GameState.Playing); // 게임 상태를 로딩으로 변경합니다.
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

    public void OnQuitGameButton()
    {
        Debug.Log("게임 종료 버튼 클릭됨!");

#if UNITY_EDITOR
        // Unity 에디터에서 실행했을 경우, 에디터의 플레이 모드를 중지시킵니다.
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 빌드된 게임에서 실행했을 경우, 어플리케이션을 종료합니다.
        Application.Quit();
#endif
    }

    // '크레딧' 버튼을 위한 함수
    public void OnCreditButton()
    {
        GameManager.Instance.UIManager.Hide<MainMenu>(); // 현재 메뉴를 숨깁니다.
        GameManager.Instance.UIManager.Show<EndingUI>(true); // 크레딧 UI를 보여달라고 요청합니다.
        Debug.Log("크레딧 버튼 클릭됨!");
    }
    public override void Show(bool show)
    {
        base.Show(show); // 부모 클래스의 Show 함수 호출
        if (show)
        {
            // 투명한 상태에서 시작
            _mainMenuGroup.alpha = 0f;
            gameObject.SetActive(true);

            // 1초 동안 부드럽게 나타나도록 애니메이션 실행
            _mainMenuGroup.DOFade(1f, 1f).SetEase(Ease.OutQuad);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
