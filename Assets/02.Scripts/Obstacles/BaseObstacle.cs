using UnityEngine;

public class BaseObstacle : MonoBehaviour
{
    [Tooltip("감지 거리")]
    [SerializeField] private float _castDistance = 1.0f;
    [Tooltip("감지 박스 여유치")]
    [SerializeField] private float _margin = 0.05f;

    [Header("곡면 장애물 감지 옵션")]
    [Tooltip("곡면(파이프/공형) 장애물에서 플레이어 감지시 사용하는 감지 박스 크기(플레이어 콜라이더 반지름+0.01~0.02 권장, 자동 지정됨)")]
    [SerializeField] private float _curveSenseSize = 0.21f; // 플레이어 캡슐 반지름이 0.2라면 0.21~0.22
    [Tooltip("곡면 감지시, 표면에서 얼마나 띄워서 감지할지(0.01~0.015 사이 권장)")]
    [SerializeField] private float _curveSenseOffset = 0.012f;
    [Tooltip("플레이어 아래로 Raycast 쏠 때 위로 얼마나 띄울지")]
    [SerializeField] private float _playerRayOriginOffset = 0.2f;
    [Tooltip("플레이어가 이 거리 안에 들어올 때만 감지 연산(최적화)")]
    [SerializeField] private float _activationRange = 10f;

    /// <summary>
    /// 시작 시 플레이어 콜라이더 크기를 자동 감지박스 크기로 지정
    /// </summary>
    private void Awake()
    {
        if (GameManager.Instance?.Player != null)
        {
            CapsuleCollider playerCapsule = GameManager.Instance.Player.GetComponentInChildren<CapsuleCollider>();
            if (playerCapsule != null)
            {
                // 콜라이더 반지름 + 0.01로 자동 지정 (수동으로 오버라이드 가능)
                _curveSenseSize = playerCapsule.radius + 0.01f;
            }
        }
    }

    /// <summary>
    /// 플레이어가 장애물 위에 있는지 감지
    /// 박스형(BoxCollider)일 때는 BoxCast, 곡면일 때는 Raycast+OverlapBox로 감지
    /// </summary>
    /// <param name="player">감지된 플레이어 Transform 반환</param>
    /// <returns>플레이어가 위에 있으면 true, 아니면 false</returns>
    protected bool TryGetPlayerOnTop(out Transform player)
    {
        player = null;

        // GameManager에서 플레이어 참조 가져오기
        if (GameManager.Instance == null || GameManager.Instance.Player == null)
            return false;

        Transform playerTransform = GameManager.Instance.Player.transform;

        // [최적화] 플레이어와 너무 멀면 감지 연산 자체 스킵
        float dist = Vector3.Distance(transform.position, playerTransform.position);
        if (dist > _activationRange)
            return false;

        // 박스콜라이더(박스/평면 장애물) 감지
        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
        {
            // Collider 기준 중심과 크기를 사용, 로컬 업벡터 기준 약간만 내림
            Vector3 center = transform.TransformPoint(box.center) - transform.up * 0.01f;
            Vector3 halfExtents = Vector3.Scale(box.size * 0.5f, transform.lossyScale) + Vector3.one * _margin;
            Quaternion rotation = transform.rotation;
            float castDistance = _castDistance;

            // 감지 방향은 항상 표면 normal(transform.up)
            Vector3 direction = transform.up;

            // BoxCast로 플레이어 감지
            if (Physics.BoxCast(center, halfExtents, direction, out RaycastHit hit, rotation, castDistance))
            {
                // 충돌한 오브젝트가 플레이어인지 확인
                if (hit.collider.transform == playerTransform || hit.collider.transform.IsChildOf(playerTransform))
                {
                    player = playerTransform;
                    return true;
                }
            }
            // Debug.Log("박스형 장애물: 감지 실패");
            return false;
        }
        else // 곡면(파이프/공/메쉬 등)
        {
            // 플레이어 바로 아래로 Raycast (오프셋, Inspector에서 조정 가능)
            Vector3 rayOrigin = playerTransform.position + Vector3.up * _playerRayOriginOffset;
            Vector3 rayDir = Vector3.down;
            float rayDist = 2.0f;
            RaycastHit hitInfo;

            if (Physics.Raycast(rayOrigin, rayDir, out hitInfo, rayDist))
            {
                // 내 콜라이더 표면에 닿았는지 확인
                if (hitInfo.collider == GetComponent<Collider>())
                {
                    Vector3 hitPoint = hitInfo.point;
                    Vector3 hitNormal = hitInfo.normal;

                    // 감지박스 중심: 표면 normal 방향으로 약간만 띄움
                    Vector3 center = hitPoint + hitNormal * _curveSenseOffset;
                    // 감지박스 크기: 플레이어 캡슐 반지름보다 0.01~0.02만 크게! (자동 지정됨)
                    Vector3 halfExtents = Vector3.one * _curveSenseSize;
                    Quaternion rotation = Quaternion.LookRotation(hitNormal);

                    // OverlapBox로 플레이어 감지
                    var hits = Physics.OverlapBox(center, halfExtents, rotation);
                    foreach (var h in hits)
                    {
                        if (h.transform == playerTransform || h.transform.IsChildOf(playerTransform))
                        {
                            player = playerTransform;
                            return true;
                        }
                    }
                }
            }
            // Debug.Log("곡면 장애물: 감지 실패");
        }
        return false;
    }

    // 감지 영역 시각화 (디버깅용)
    private void OnDrawGizmos()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
        {
            Vector3 center = transform.TransformPoint(box.center) - transform.up * 0.01f;
            Vector3 halfExtents = Vector3.Scale(box.size * 0.5f, transform.lossyScale) + Vector3.one * _margin;
            Vector3 direction = transform.up;

            Gizmos.color = Color.green;

            // 박스형 장애물: BoxCast 범위 시각화
            Gizmos.matrix = Matrix4x4.TRS(center + direction * (_castDistance / 2f), transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2f);
        }
        else
        {
            // 곡면 장애물: 플레이어 바로 아래 hitNormal 기준 OverlapBox 시각화
            if (GameManager.Instance == null || GameManager.Instance.Player == null)
                return;

            Transform playerTransform = GameManager.Instance.Player.transform;
            Vector3 rayOrigin = playerTransform.position + Vector3.up * _playerRayOriginOffset;
            Vector3 rayDir = Vector3.down;
            float rayDist = 2.0f;
            RaycastHit hitInfo;

            if (Physics.Raycast(rayOrigin, rayDir, out hitInfo, rayDist))
            {
                if (hitInfo.collider == GetComponent<Collider>())
                {
                    Vector3 hitPoint = hitInfo.point;
                    Vector3 hitNormal = hitInfo.normal;
                    Vector3 center = hitPoint + hitNormal * _curveSenseOffset;
                    Vector3 halfExtents = Vector3.one * _curveSenseSize;
                    Quaternion rotation = Quaternion.LookRotation(hitNormal);

                    Gizmos.color = Color.yellow;
                    Gizmos.matrix = Matrix4x4.TRS(center, rotation, Vector3.one);
                    Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2f);
                }
            }
        }
    }
}
