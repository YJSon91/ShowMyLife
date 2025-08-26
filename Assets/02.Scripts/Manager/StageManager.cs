 using UnityEngine;

/// <summary>
/// 현재 스테이지의 규칙(시작, 클리어, 실패)을 정의하고,
/// 플레이어의 상태에 따라 게임의 흐름을 GameManager에게 보고하는 매니저입니다.
/// </summary>
public class StageManager : MonoBehaviour
{
    [Header("스테이지 설정")]
    [Tooltip("플레이어가 이 씬에서 처음 시작하거나, 낙하 후 리스폰될 위치입니다.")]
    [SerializeField] private Transform _respawnPoint;

    // --- Unity 생명주기 메서드 ---

    private void Start()
    {
        // GameManager에 자신을 'StageManager'로 등록합니다.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterStageManager(this);
        }
        else
        {
          //  Debug.LogError("[StageManager] GameManager가 씬에 존재하지 않습니다! 등록에 실패했습니다.");
        }
    }


    // --- Public API 메서드 ---

    /// <summary>
    /// FallDetector가 플레이어의 낙하를 감지했을 때 호출할 함수입니다.
    /// </summary>
    public void OnPlayerFell()
    {
      //  Debug.Log("[StageManager] 플레이어 낙하 감지! GameManager에 리스폰을 요청합니다.");
        // GameManager에게 플레이어 리스폰 절차를 시작하도록 '요청'합니다.
        GameManager.Instance.RequestPlayerRespawn();
    }

    /// <summary>
    /// GoalTrigger가 플레이어의 도착을 감지했을 때 호출할 함수입니다.
    /// </summary>
    public void OnPlayerReachedGoal()
    {
      //  Debug.Log("[StageManager] 플레이어 목표 지점 도착! GameManager에 레벨 클리어를 요청합니다.");
        // GameManager에게 게임 상태를 'LevelClear'로 변경하도록 '요청'합니다.
        GameManager.Instance.UpdateGameState(GameManager.GameState.LevelClear);
      //  Debug.Log("[StageManager] 레벨 클리어 상태로 변경하도록 요청했습니다.");
    }

    /// <summary>
    /// GameManager가 리스폰 위치를 물어볼 때 사용할 함수입니다.
    /// </summary>
    /// <returns>현재 지정된 리스폰 위치의 Vector3 값</returns>
    public Vector3 GetCurrentRespawnPoint()
    {
        if (_respawnPoint == null)
        {
         //   Debug.LogWarning("[StageManager] 리스폰 위치가 지정되지 않았습니다! (0, 0, 0) 위치를 반환합니다.");
            return Vector3.zero;
        }
        return _respawnPoint.position;
    }
}
