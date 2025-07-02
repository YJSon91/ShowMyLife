using UnityEngine;

/// <summary>
/// 플레이어의 목표 지점 도착을 감지하여 StageManager에게 보고하는 트리거입니다.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class GoalTrigger : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 충돌한 오브젝트가 'Player' 태그를 가지고 있다면
        if (other.CompareTag("Player"))
        {
            // GameManager를 통해 StageManager에게 레벨 클리어 사실을 '보고'합니다.
            if (GameManager.Instance != null && GameManager.Instance.StageManager != null)
            {
                GameManager.Instance.StageManager.OnPlayerReachedGoal();
            }

            // 한 번만 작동하도록 트리거를 비활성화합니다.
            gameObject.SetActive(false);
        }
    }
}
