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
            CharacterController cc = player.GetComponentInChildren<CharacterController>();

            if (rb != null)
                rb.MovePosition(newPos);
            else if (cc != null)
                cc.Move(delta);
            else
                player.position = newPos;
        }
    }
}
