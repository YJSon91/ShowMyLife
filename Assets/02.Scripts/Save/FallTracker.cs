using UnityEngine;
using UnityEngine.Events;

public class FallTracker : MonoBehaviour
{
    [SerializeField] private float fallYThreshold = -10f;
    [SerializeField] private int fallLimit = 3;
    [SerializeField] private string groundTag = "Ground";
    [SerializeField] private UnityEvent onFallLimitReached;

    private Vector3 lastGroundedPosition;
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

            transform.position = lastGroundedPosition;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag(groundTag))
        {
            lastGroundedPosition = transform.position;
        }
    }
}
