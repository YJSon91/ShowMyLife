using UnityEngine;
using DG.Tweening;

public class PendulumObstacle : BaseObstacle
{
    public enum RotationAxis { X, Y, Z }

    [Header("회전 설정")]
    [Tooltip("회전할 축 (X/Y/Z 중 선택)")]
    [SerializeField] private RotationAxis _rotationAxis = RotationAxis.Z;
    [Tooltip("양쪽 최대 각도 (예: 70도 → -70도~70도)")]
    [SerializeField] private float swingAngle = 70f;
    [Tooltip("한 쪽 끝에서 반대쪽까지 왕복하는 데 걸리는 시간")]
    [SerializeField] private float swingDuration = 1.5f;

    private void Start()
    {
        StartSwing();
    }

    private void StartSwing()
    {
        Vector3 targetAngle = Vector3.zero;

        switch (_rotationAxis)
        {
            case RotationAxis.X:
                targetAngle = new Vector3(swingAngle, 0, 0);
                break;
            case RotationAxis.Y:
                targetAngle = new Vector3(0, swingAngle, 0);
                break;
            case RotationAxis.Z:
                targetAngle = new Vector3(0, 0, swingAngle);
                break;
        }

        // 왕복 진자운동 (끝에서 느려졌다가 다시 빨라짐)
        transform.DORotate(targetAngle, swingDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }
}
