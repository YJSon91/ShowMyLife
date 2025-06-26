using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    private static ObstacleManager _instance;
    public static ObstacleManager Instance => _instance;

    private BaseObstacle[] _obstacles;

    private void Awake()
    {
        // 중복 생성 방지
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject); // 씬 전환에도 유지

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
