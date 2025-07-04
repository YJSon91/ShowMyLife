using UnityEngine;
using DG.Tweening;

public enum RotationAxis
{
    X, Y, Z
}

public class RotatingObstacle : BaseObstacle
{
    [Header("회전 설정")]
    [SerializeField] private RotationAxis _rotationAxis = RotationAxis.Y;

    [Tooltip("초당 회전 각도")]
    [SerializeField] private float _rotationSpeed = 90f;

    [Tooltip("시계 방향 회전 여부")]
    [SerializeField] private bool _clockwise = true;

    private Quaternion _lastRotation;
    private float _currentAngle = 0f;

    public float power;

    private void Start()
    {
        _lastRotation = transform.rotation;
        StartRotating();
    }

    private Vector3 GetRotationAxis()
    {
        switch (_rotationAxis)
        {
            case RotationAxis.X: return Vector3.right;
            case RotationAxis.Y: return Vector3.up;
            case RotationAxis.Z: return Vector3.forward;
            default: return Vector3.up;
        }
    }

    private void Update()
    {
        Quaternion currentRotation = transform.rotation;
        Quaternion deltaRotation = currentRotation * Quaternion.Inverse(_lastRotation);

        RotatePlayerIfOnTop(deltaRotation);

        _lastRotation = currentRotation;
    }

    private void StartRotating()
    {
        float direction = _clockwise ? 1f : -1f;
        Vector3 axis = GetRotationAxis() * direction;

        DOTween.To(() => _currentAngle, x =>
        {
            _currentAngle = x % 360f;
            transform.localRotation = Quaternion.AngleAxis(_currentAngle, axis.normalized);
        },
        360f,
        360f / _rotationSpeed)
        .SetEase(Ease.Linear)
        .SetLoops(-1, LoopType.Incremental)
        .SetUpdate(UpdateType.Fixed);
    }

    // 플레이어가 위에 있을 때 회전 이동
    protected void RotatePlayerIfOnTop(Quaternion deltaRotation)
    {
        if (TryGetPlayerOnTop(out Transform player))
        {
            Vector3 dir = player.position - transform.position;
            Vector3 newPos = transform.position + deltaRotation * dir;
            Vector3 delta = newPos - player.position;

            Rigidbody rb = player.GetComponent<Rigidbody>();
            
            if (rb != null)
            {
                // 리지드바디가 있으면 MovePosition 사용
                rb.MovePosition(newPos);
                
                // 플레이어가 회전 플랫폼 위에 있음을 알림 (필요시 구현)
                Player playerComponent = player.GetComponent<Player>();
                if (playerComponent != null)
                {
                    // 필요한 경우 플레이어에게 회전 플랫폼 위에 있음을 알림
                }
            }
            else
            {
                // 리지드바디가 없는 경우 직접 위치 설정
                player.position = newPos;
            }
        }
    }
}
