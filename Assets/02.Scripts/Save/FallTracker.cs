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
            GameManager.Instance.StageManager.OnPlayerFell();
    }
}
