using UnityEngine;

/// <summary>
/// 플레이어의 낙하를 감지하여 StageManager에게 보고하는 트리거입니다.
/// </summary>
[RequireComponent(typeof(BoxCollider))] // 이 스크립트는 BoxCollider가 필수입니다.
public class FallDetector : MonoBehaviour
{
    private void Awake()
    {
        // 트리거로 작동해야 하므로, BoxCollider의 isTrigger를 코드로 보장합니다.
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 충돌한 오브젝트가 'Player' 태그를 가지고 있다면
        if (other.CompareTag("Player"))
        {
            // GameManager를 통해 StageManager에게 플레이어 낙하 사실을 '보고'합니다.
            if (GameManager.Instance != null && GameManager.Instance.StageManager != null)
            {
                GameManager.Instance.StageManager.OnPlayerFell();
            }
        }
    }

    // 디버깅을 위해 트리거 영역을 씬 뷰에 표시해주는 기능입니다.
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f); // 반투명한 빨간색
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, Vector3.one);
    }
}
