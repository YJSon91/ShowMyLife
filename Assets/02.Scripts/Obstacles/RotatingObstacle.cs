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

            // 리지드바디 우선 사용
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // 리지드바디 이동
                rb.MovePosition(newPos);
                return;
            }

            // 리지드바디가 없는 경우 CharacterController 사용 (이전 코드와의 호환성)
            CharacterController cc = player.GetComponentInChildren<CharacterController>();
            PlayerMovementController playerMovement = player.GetComponentInChildren<PlayerMovementController>();
            
            if (cc != null)
            {
                cc.Move(delta);
                if (playerMovement != null)
                    playerMovement.ApplyGravity();
            }
            else
            {
                // 둘 다 없는 경우 직접 위치 설정
                player.position = newPos;
            }
        }
    }
}
