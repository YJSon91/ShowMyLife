using UnityEngine;

public class BaseObstacle : MonoBehaviour
{
    // 플레이어가 위에 있을 때 위치 이동
    protected void MovePlayerIfOnTop(Vector3 delta)
    {
        if (TryGetPlayerOnTop(out Transform player))
        {
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
                rb.MovePosition(rb.position + delta);
            else
                player.position += delta;
        }
    }

    // 플레이어가 위에 있을 때 회전 이동
    protected void RotatePlayerIfOnTop(Quaternion deltaRotation)
    {
        if (TryGetPlayerOnTop(out Transform player))
        {
            Vector3 dir = player.position - transform.position;
            Vector3 newPos = transform.position + deltaRotation * dir;

            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
                rb.MovePosition(newPos);
            else
                player.position = newPos;
        }
    }

    // BoxCast를 이용해 장애물 위에 있는 플레이어 감지
    protected bool TryGetPlayerOnTop(out Transform player)
    {
        player = null;

        Vector3 direction = Vector3.up;
        float castDistance = 1.0f;

        Vector3 center = transform.position + Vector3.down * 0.1f;
        Vector3 halfExtents = transform.localScale / 2f + new Vector3(0.05f, 0.05f, 0.05f);

        if (Physics.BoxCast(center, halfExtents, direction, out RaycastHit hit, transform.rotation, castDistance))
        {
            if (hit.collider.CompareTag("Player"))
            {
                player = hit.collider.transform;
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
        Debug.Log($"Cast from: {center}, size: {halfExtents}, dir: {direction}, dist: {castDistance}");
    }
}
