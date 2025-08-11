 using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameLoadState
{
    // static 변수는 게임이 꺼지기 전까지 값이 유지됩니다.
    public static bool ShouldLoadGame = false;
}
public class MainMenu : UiBase
{
    private CanvasGroup _mainMenuGroup;
    [SerializeField] private GameObject continueButton;

    public override void Init()
    {
        GameManager.Instance.UIManager.Add<MainMenu>(this);
        _mainMenuGroup = GetComponent<CanvasGroup>();
    }

    // '새 게임' 버튼을 위한 함수
    public void OnNewGameButton()
    {
        // "로드해야 한다" 깃발을 내린 상태로 설정합니다.
        GameLoadState.ShouldLoadGame = false;
        // -------------

        GameManager.Instance.UpdateGameState(GameManager.GameState.Playing);
    }

    // '이어하기' 버튼을 위한 함수
    public void OnContinueButton()
    {
        // --- 수정 ---
        // "로드해야 한다" 깃발을 올린 상태로 설정합니다.
        GameLoadState.ShouldLoadGame = true;
        // -------------

        GameManager.Instance.UpdateGameState(GameManager.GameState.Playing);
    }

    // '설정' 버튼을 위한 함수
    public void OnSettingsButton()
    {
        if (GameManager.Instance != null && GameManager.Instance.UIManager != null)
        {
            GameManager.Instance.UIManager.Hide<MainMenu>();
            GameManager.Instance.UIManager.Show<SettingsMenu>(true);
        }
        else
        {
            Debug.LogError("UIManager가 GameManager에 등록되지 않았습니다!");
        }
    }

    public void OnQuitGameButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // '크레딧' 버튼을 위한 함수
    public void OnCreditButton()
    {
        GameManager.Instance.UIManager.Hide<MainMenu>();
        GameManager.Instance.UIManager.Show<EndingUI>(true);
    }

    public override void Show(bool show)
    {
        base.Show(show);
        if (show)
        {
            // --- 추가 ---
            // 메뉴가 보일 때, 저장 파일 유무를 확인하고 '이어하기' 버튼의 활성화 상태를 결정합니다.
            continueButton.SetActive(SaveLoader.Exists());
            // -------------

            _mainMenuGroup.alpha = 0f;
            gameObject.SetActive(true);

            _mainMenuGroup.DOFade(1f, 1f).SetEase(Ease.OutQuad);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
