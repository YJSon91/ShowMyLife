using System;
using UnityEngine;

/// <summary>
/// 게임의 전체 상태와 다른 모든 매니저들을 총괄하는 최상위 싱글톤 클래스입니다.
/// </summary>
public class GameManager : MonoBehaviour
{
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
    public PlayerController Player { get; private set; }
    public CameraController Cam { get; private set; }


    // --- Unity 생명주기 메서드 ---
    private void Awake()
    {
        // 싱글톤 및 DontDestroyOnLoad 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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
    public void RegisterPlayer(PlayerController player) => Player = player;
    public void RegisterCamera(CameraController camera) => Cam = camera;


    // --- 핵심 로직 메서드 ---

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
