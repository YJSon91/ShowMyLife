using UnityEngine;

public class FallTracker : MonoBehaviour
{
    [Header("낙하 감지 설정")]
    [SerializeField] private float fallYThreshold = -10f;
    [SerializeField] private int fallLimit = 3;

    private int fallCount = 0;

    private void Update()
    {
        if (transform.position.y < fallYThreshold)
        {
            fallCount++;
            Debug.Log($"[FallTracker] 낙하 감지됨 ({fallCount}회)");

            if (fallCount >= fallLimit)
            {
                TriggerRespawn();
                fallCount = 0;
            }
        }
    }

    private void TriggerRespawn()
    {
        if (GameManager.Instance?.StageManager != null)
        {
            GameManager.Instance.StageManager.OnPlayerFell();
        }
        else
        {
            Debug.LogWarning("[FallTracker] StageManager가 연결되지 않아 리스폰 요청 실패");
        }
    }
}
