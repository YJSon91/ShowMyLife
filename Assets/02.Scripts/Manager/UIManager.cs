using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

/// <summary>
/// 인게임(In-Game)에서 사용되는 모든 UI 요소(일시정지, 대사, 설정 등)를 총괄하는 매니저입니다.
/// GameManager에 의해 관리됩니다.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private GameObject _settingsPanel;
    [SerializeField] private GameObject _dialoguePanel;
    [SerializeField] private GameObject _levelClearPanel;

    [Header("UI Texts")]
    [SerializeField] private TextMeshProUGUI _dialogueText;

    [Header("Dialogue Settings")]
    [Tooltip("대사가 화면에 표시되는 시간(초)입니다.")]
    [SerializeField] private float _dialogueDisplayTime = 3f;

    // --- Unity 생명주기 메서드 ---

    private void Awake()
    {
        // GameManager가 존재할 경우, 자신을 UIManager로 등록합니다.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterUIManager(this);
        }
        else
        {
            Debug.LogError("[UIManager] GameManager가 씬에 존재하지 않습니다!");
        }
    }

    private void OnEnable()
    {
        // GameManager의 상태 변경 이벤트를 구독합니다.
        GameManager.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        // 오브젝트가 파괴될 때, 이벤트 구독을 반드시 해제합니다.
        GameManager.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void Update()
    {
        // 일시정지 키 (Esc) 처리
        HandlePauseInput();
    }

    // --- 이벤트 핸들러 ---

    /// <summary>
    /// GameManager로부터 게임 상태 변경 신호를 받았을 때 호출되는 함수입니다.
    /// </summary>
    private void HandleGameStateChanged(GameManager.GameState newState)
    {
        // 새로운 상태에 따라 UI를 업데이트합니다.
        ShowPausePanel(newState == GameManager.GameState.Paused);

        if (newState == GameManager.GameState.LevelClear)
        {
            ShowLevelClearPanel();
        }
    }

    // --- 공개 메서드 (API) ---

    /// <summary>
    /// 대사 출력 UI를 활성화하고 메시지를 표시합니다.
    /// </summary>
    public void ShowDialogue(string message)
    {
        StopAllCoroutines();
        StartCoroutine(ShowDialogueCoroutine(message));
    }

    /// <summary>
    /// 일시정지 패널을 켜거나 끕니다.
    /// </summary>
    public void ShowPausePanel(bool show)
    {
        if (_pausePanel != null)
        {
            _pausePanel.SetActive(show);
        }
    }

    /// <summary>
    /// 설정 패널을 켜거나 끕니다.
    /// </summary>
    public void ShowSettingsPanel(bool show)
    {
        if (_settingsPanel != null)
        {
            _settingsPanel.SetActive(show);
        }
    }

    /// <summary>
    /// 레벨 클리어 패널을 보여줍니다.
    /// </summary>
    public void ShowLevelClearPanel()
    {
        if (_levelClearPanel != null)
        {
            _levelClearPanel.SetActive(true);
        }
    }


    // --- UI 버튼 이벤트에 연결될 함수들 ---

    public void OnResumeButton()
    {
        // 게임 상태 변경을 GameManager에 '요청'합니다.
        GameManager.Instance.UpdateGameState(GameManager.GameState.Playing);
    }

    public void OnRestartButton()
    {
        // 재시작은 게임 상태와 무관하게 바로 씬을 로드합니다.
        // 시간을 다시 흐르게 하는 것을 잊지 마세요.
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnOpenSettingsButton()
    {
        ShowSettingsPanel(true);
    }

    public void OnQuitGameButton()
    {
        Application.Quit();
        Debug.Log("게임 종료 버튼 클릭됨!");
    }

    // --- 내부 로직 ---

    private void HandlePauseInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 현재 게임 상태가 '플레이 중'일 때만 일시정지할 수 있습니다.
            if (GameManager.Instance.CurrentState == GameManager.GameState.Playing)
            {
                GameManager.Instance.UpdateGameState(GameManager.GameState.Paused);
            }
            // 현재 게임 상태가 '일시정지 중'일 때만 게임을 재개할 수 있습니다.
            else if (GameManager.Instance.CurrentState == GameManager.GameState.Paused)
            {
                GameManager.Instance.UpdateGameState(GameManager.GameState.Playing);
            }
        }
    }

    private IEnumerator ShowDialogueCoroutine(string message)
    {
        _dialoguePanel.SetActive(true);
        _dialogueText.text = message;
        yield return new WaitForSecondsRealtime(_dialogueDisplayTime); // Time.timeScale 영향 안받음
        _dialoguePanel.SetActive(false);
    }
}
