using System;
using UnityEngine;
using UnityEngine.InputSystem;

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
    public enum GameState { MainMenu, Playing, Paused, LevelClear }


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
    public LevelManager LevelManager { get; private set; }
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
        UpdateGameState(GameState.MainMenu);
    }


    // --- 하위 매니저 등록 메서드 ---
    public void RegisterUIManager(UIManager manager) => UIManager = manager;
    public void RegisterLevelManager(LevelManager manager) => LevelManager = manager;
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
    }
    private void OnDisable()
    {
        // Player 액션 맵을 비활성화합니다.
        PlayerControls.Player.Disable();
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
        if (CurrentState == newState) return; // 같은 상태로의 변경은 무시

        CurrentState = newState;
        OnGameStateChanged?.Invoke(newState); // 상태 변경을 전체에 '방송'
        Debug.Log($"[GameManager] Game State Changed to: {newState}");
    }
    private void OnDestroy()
    {
        // 만약 플레이어와 InputReader가 존재한다면, 구독을 해제합니다.
        if (Player != null)
        {
            InputReader inputReader = Player.GetComponent<InputReader>();
            if (inputReader != null)
            {
                inputReader.OnPausePerformed -= TogglePauseState;
            }
        }
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
    //public void RequestPlayerRespawn()
    //{
    //    if (LevelManager != null && Player != null)
    //    {
    //        Vector3 respawnPoint = LevelManager.GetCurrentRespawnPoint();
    //        Player.Respawn(respawnPoint);
    //        Debug.Log($"[GameManager] Player Respawn Requested at {respawnPoint}");
    //    }
    //    else
    //    {
    //        Debug.LogError("[GameManager] LevelManager 또는 Player가 등록되지 않아 리스폰할 수 없습니다.");
    //    }
    //}
}
