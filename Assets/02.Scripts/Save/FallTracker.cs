using UnityEngine;
using UnityEngine.Events;

public class FallTracker : MonoBehaviour
{
    [Header("낙하 감지 설정")]
    [SerializeField] private float fallYThreshold = -10f;
    [SerializeField] private int fallLimit = 3;
    [SerializeField] private UnityEvent onFallLimitReached;

    private int fallCount = 0;

    private void Update()
    {
        if (transform.position.y < fallYThreshold)
        {
            fallCount++;
            Debug.Log($"[FallTracker] 낙하 감지됨 ({fallCount}회)");

            if (fallCount >= fallLimit)
            {
                onFallLimitReached?.Invoke();
                fallCount = 0;
            }
        }
    }
}
