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
        // 1. 플레이어 태그를 확인합니다.
        if (other.CompareTag("Player"))
        {
            // 2. 플레이어의 Rigidbody 컴포넌트를 가져옵니다.
            Rigidbody playerRb = other.GetComponent<Rigidbody>();

            // 3. 플레이어가 아래로 떨어지는 중일 때만 (Y축 속도가 음수일 때) 동작합니다.
            if (playerRb != null && playerRb.velocity.y < 0)
            {
                // GameManager를 통해 StageManager에게 낙하 사실을 보고합니다.
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
