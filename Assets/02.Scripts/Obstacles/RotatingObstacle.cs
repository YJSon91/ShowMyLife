using UnityEngine;
using DG.Tweening;

public class RotatingObstacle : BaseObstacle
{
    [Header("회전 설정")]
    [Tooltip("회전할 축 선택")]
    [SerializeField] private Vector3 _rotationAxis = Vector3.up;

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

    private void FixedUpdate()
    {
        Quaternion currentRotation = transform.rotation;
        Quaternion deltaRotation = currentRotation * Quaternion.Inverse(_lastRotation);

        RotatePlayerIfOnTop(deltaRotation);

        _lastRotation = currentRotation;
    }

    private void StartRotating()
    {
        float direction = _clockwise ? 1f : -1f;

        DOTween.To(() => _currentAngle, x =>
        {
            _currentAngle = x % 360f; // 각도 누적 오버플로우 방지
            transform.localRotation = Quaternion.AngleAxis(_currentAngle, _rotationAxis.normalized);
        },
        360f,
        360f / (_rotationSpeed * Mathf.Abs(direction)))
        .SetEase(Ease.Linear)
        .SetLoops(-1, LoopType.Incremental)
        .SetUpdate(UpdateType.Fixed);
    }
}
