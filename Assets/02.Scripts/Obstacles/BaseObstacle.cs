using UnityEngine;

public class BaseObstacle : MonoBehaviour
{
    // BoxCast를 이용해 장애물 위에 있는 플레이어 감지
    protected bool TryGetPlayerOnTop(out Transform player)
    {
        player = null;

        // GameManager에서 플레이어 참조 가져오기
        if (GameManager.Instance == null || GameManager.Instance.Player == null)
        {
            return false;
        }

        Transform playerTransform = GameManager.Instance.Player.transform;
        Vector3 direction = Vector3.up;
        float castDistance = 1.0f;

        Vector3 center = transform.position + Vector3.down * 0.1f;
        Vector3 halfExtents = transform.localScale / 2f + new Vector3(0.05f, 0.05f, 0.05f);

        if (Physics.BoxCast(center, halfExtents, direction, out RaycastHit hit, transform.rotation, castDistance))
        {
            // 충돌한 오브젝트가 플레이어인지 확인
            if (hit.collider.transform == playerTransform || hit.collider.transform.IsChildOf(playerTransform))
            {
                player = playerTransform;
                return true;
            }
        }

        return false;
    }

    // 감지 영역 시각화 (디버깅용)
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Vector3 direction = Vector3.up;
        float castDistance = 1.0f;

        Vector3 center = transform.position + Vector3.down * 0.1f;
        Vector3 halfExtents = transform.localScale / 2f + new Vector3(0.05f, 0.05f, 0.05f);

        Gizmos.matrix = Matrix4x4.TRS(center, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.up * castDistance / 2f, halfExtents);

        Debug.DrawRay(center, direction * castDistance, Color.red, 0.5f);
    }
}
