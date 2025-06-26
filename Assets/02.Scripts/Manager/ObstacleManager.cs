using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    private static ObstacleManager _instance;
    public static ObstacleManager Instance => _instance;

    private BaseObstacle[] _obstacles;

    private void Awake()
    {
        // GameManager에 자신을 등록
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterObstacleManager(this);
        }
        else
        {
            Debug.LogError("[ObstacleManager] GameManager가 씬에 존재하지 않습니다!");
        }

        _obstacles = FindObjectsOfType<BaseObstacle>();
        Debug.Log($"[ObstacleManager] {_obstacles.Length}개 장애물 등록됨");
    }

    public void ResetAllObstacles()
    {
        foreach (var obstacle in _obstacles)
        {
            obstacle.gameObject.SetActive(true);
        }
    }

    // 실행 전에 자동 생성
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateManagerIfMissing()
    {
        if (_instance == null)
        {
            GameObject obj = new GameObject("ObstacleManager");
            obj.AddComponent<ObstacleManager>();
        }
    }
}
