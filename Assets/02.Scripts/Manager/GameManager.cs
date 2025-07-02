using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

/// <summary>
/// 게임의 전체 상태와 다른 모든 매니저들을 총괄하는 최상위 싱글톤 클래스입니다.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("핵심 에셋")]
    [Tooltip("프로젝트의 Input Action 에셋을 연결해주세요.")]
    [SerializeField] private InputActionAsset _inputActions;
    /// <summary>
    /// 게임 전체에서 공유될 컨트롤러 인스턴스입니다.
    /// </summary>
    public Controls PlayerControls { get; private set; }
    // --- 상태 정의 ---
    /// <summary>
    /// 게임의 현재 상태를 나타내는 열거형입니다.
    /// </summary>
    public enum GameState { Start, MainMenu, Playing, Paused, LevelClear }


    // --- 이벤트 ---
    /// <summary>
    /// 게임 상태가 변경될 때 방송되는 C# 이벤트입니다.
    /// </summary>
    public static event Action<GameState> OnGameStateChanged;


    // --- 프로퍼티 ---
    /// <summary>
    /// GameManager의 싱글톤 인스턴스입니다.
    /// </summary>
    public static GameManager Instance { get; private set; }

    /// <summary>
    /// 현재 게임의 상태입니다.
    /// </summary>
    public GameState CurrentState { get; private set; }

    // --- 하위 매니저 참조 ---
    public UIManager UIManager { get; private set; }
    public StageManager StageManager { get; private set; }
    public SoundManager SoundManager { get; private set; }
    public ObstacleManager ObstacleManager { get; private set; }
    public Player Player { get; private set; }
    public CameraManager CameraManager { get; private set; }



    // --- Unity 생명주기 메서드 ---
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 1. 컨트롤러 인스턴스를 생성합니다.
            PlayerControls = new Controls();

            // 2. 저장된 키 바인딩을 불러옵니다.
            LoadAllKeybindings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 게임 시작 시 초기 상태는 '메인 메뉴'입니다.
        UpdateGameState(GameState.Start);
    }


    // --- 하위 매니저 등록 메서드 ---
    public void RegisterUIManager(UIManager manager) => UIManager = manager;
    public void RegisterStageManager(StageManager manager) => StageManager = manager;
    public void RegisterSoundManager(SoundManager manager) => SoundManager = manager;
    public void RegisterObstacleManager(ObstacleManager manager) => ObstacleManager = manager;
    public void RegisterCameraManager(CameraManager manager) => CameraManager = manager;
    public void RegisterPlayer(Player newPlayer)
    {
        // 1. 만약 이전에 등록된 플레이어가 있었다면, 그 플레이어의 이벤트 구독을 먼저 해제합니다.
        if (Player != null)
        {
            InputReader oldInputReader = Player.GetComponent<InputReader>();
            if (oldInputReader != null)
            {
                oldInputReader.OnPausePerformed -= TogglePauseState;
            }
        }

        // 2. 새로운 플레이어를 현재 플레이어로 등록합니다.
        Player = newPlayer;

        // 3. 이제 새로운 플레이어의 이벤트에 안전하게 구독합니다.
        InputReader newInputReader = newPlayer.GetComponent<InputReader>();
        if (newInputReader != null)
        {
            newInputReader.OnPausePerformed += TogglePauseState;
        }
    }

    // --- 핵심 로직 메서드 ---
    /// <summary>
    private void OnEnable()
    {
        // Player 액션 맵을 활성화합니다.
        PlayerControls.Player.Enable();
        SceneManager.sceneLoaded += CheckEventSystem;
    }
    private void OnDisable()
    {
        // 내가 진짜 인스턴스가 아니면(복제품이면) 즉시 빠져나갑니다.
        if (Instance != this) return;
        // 진짜 인스턴스일 경우에만 아래 코드를 실행합니다.
       PlayerControls?.Player.Disable();
        SceneManager.sceneLoaded -= CheckEventSystem;
    }

    // OnDestroy도 동일한 안전장치를 추가해주는 것이 좋습니다.
    private void OnDestroy()
    {
        if (Instance != this) return;

        if (Player != null)
        {
            InputReader inputReader = Player.GetComponent<InputReader>();
            if (inputReader != null)
            {
                inputReader.OnPausePerformed -= TogglePauseState;
            }
        }
    }
    private void CheckEventSystem(Scene scene, LoadSceneMode mode)
    {
        // 현재 씬에 EventSystem 타입의 오브젝트가 있는지 확인합니다.
        if (FindObjectOfType<EventSystem>() == null)
        {
            // EventSystem이 없다면, 새로 생성합니다.
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>(); // 키보드/마우스 입력을 위해 필수

            Debug.LogWarning($"[GameManager] 씬 '{scene.name}'에 EventSystem이 없어 자동으로 생성했습니다.");
        }
    }
    private void LoadAllKeybindings()
    {
        if (_inputActions == null) return;
        string rebinds = PlayerPrefs.GetString("AllKeyRebinds", string.Empty);
        if (string.IsNullOrEmpty(rebinds)) return;

        // _inputActions 대신, 우리가 생성한 인스턴스에 오버라이드를 적용합니다.
        PlayerControls.LoadBindingOverridesFromJson(rebinds);
        Debug.Log("[GameManager] 저장된 모든 키 설정을 불러왔습니다.");
    }
    /// <summary>
    /// 게임의 상태를 변경하고, 이 사실을 모든 구독자에게 알립니다.
    /// </summary>
    /// <param name="newState">변경할 새로운 게임 상태</param>
    public void UpdateGameState(GameState newState)
    {
        if (CurrentState == newState) return;

        CurrentState = newState;

        // --- 이 부분이 핵심 수정 내용입니다 ---
        // 새로운 게임 상태에 따라 적절한 액션 맵을 활성화/비활성화하고 커서 상태를 제어합니다.
        switch (newState)
        {
            case GameState.Playing:
                // 플레이 중일 때는 플레이어 조작만 가능해야 합니다.
                PlayerControls?.UI.Disable();
                PlayerControls?.Player.Enable();
                Cursor.lockState = CursorLockMode.Locked; // 커서 잠금
                Cursor.visible = false;
                break;

            case GameState.Paused:
            case GameState.MainMenu:
            case GameState.LevelClear:
                // 메뉴, 일시정지, 클리어 상태에서는 UI 조작만 가능해야 합니다.
                PlayerControls?.Player.Disable();
                PlayerControls?.UI.Enable();
                Cursor.lockState = CursorLockMode.None; // 커서 잠금 해제
                Cursor.visible = true;
                break;
        }
        // --- 수정 끝 ---

        // 씬 로딩 로직
        if (newState == GameState.Playing)
        {
            // 현재 씬이 IntroScene일 때만 게임 씬을 로드하도록 조건을 추가하면 더 안전합니다.
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "IntroScene")
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("TestMapScene_JSC");
            }
        }

        OnGameStateChanged?.Invoke(newState);
        Debug.Log($"[GameManager] Game State Changed to: {newState}");
    }
    /// <summary>
    /// 게임의 일시정지 상태를 토글합니다.
    /// </summary>
    private void TogglePauseState()
    {
        // 현재 게임 상태가 '플레이 중'일 때만 일시정지할 수 있습니다.
        if (CurrentState == GameState.Playing)
        {
            UpdateGameState(GameState.Paused);
        }
        // 현재 게임 상태가 '일시정지 중'일 때만 게임을 재개할 수 있습니다.
        else if (CurrentState == GameState.Paused)
        {
            UpdateGameState(GameState.Playing);
        }
    }
    /// <summary>
    /// 플레이어의 리스폰 절차를 시작하도록 요청합니다.
    /// </summary>
    public void RequestPlayerRespawn()
    {
        // StageManager와 Player가 모두 등록되었는지 확인
        if (StageManager != null && Player != null)
        {
            // 1. StageManager에게 리스폰 위치를 물어봅니다.
            Vector3 respawnPoint = StageManager.GetCurrentRespawnPoint();
            // 2. PlayerController에게 해당 위치로 리스폰하라고 명령합니다.
            //Player.Respawn(respawnPoint);
            Debug.Log($"[GameManager] Player 리스폰 요청 완료: {respawnPoint}");
        }
        else
        {
            Debug.LogError("[GameManager] StageManager 또는 Player가 등록되지 않아 리스폰할 수 없습니다.");
        }
    }
}
