using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    private BaseObstacle[] _obstacles;

    private void Start()
    {
        // GameManager에 자신을 등록
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterObstacleManager(this);
        }
        else
        {
            Debug.LogError("[ObstacleManager] ObstacleManager 씬에 존재하지 않습니다!");
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

}
