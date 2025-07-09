using UnityEngine;
using DG.Tweening;

public class SeesawObstacle : BaseObstacle
{
    [Header("시소 기울기 설정")]
    [Tooltip("최대 기울기 각도(도 단위)")]
    [SerializeField] private float _maxAngle = 25f;
    [Tooltip("복원에 걸리는 시간(초)")]
    [SerializeField] private float _recoverTime = 2.0f;
    [Tooltip("플레이어가 한쪽 끝에 있을 때 기울어지는 속도(초)")]
    [SerializeField] private float _tiltTime = 0.2f;
    [Tooltip("플레이어 감지 범위(시소 길이의 절반)")]
    [SerializeField] private float _detectLength = 2.5f;

    private float _currentAngle = 0f; // 실제 적용된 각도(도)
    private float _targetAngle = 0f;  // 목표 각도

    private Tween _seesawTween;

    private Quaternion _lastRotation;

    private void Start()
    {
        _lastRotation = transform.localRotation;
    }

    private void Update()
    {
        if (IsPlayerOnPlatform())
        {
            Transform playerTransform = GetPlayerOnPlatform();
            if (playerTransform != null)
            {
                Vector3 localPlayerPos = transform.InverseTransformPoint(playerTransform.position);
                if (Mathf.Abs(localPlayerPos.x) <= _detectLength && Mathf.Abs(localPlayerPos.z) < 0.7f)
                {
                    float ratio = Mathf.Clamp(localPlayerPos.x / _detectLength, -1f, 1f);
                    float newTargetAngle = -_maxAngle * ratio;

                    if (!Mathf.Approximately(_targetAngle, newTargetAngle))
                    {
                        _targetAngle = newTargetAngle;
                        TweenToAngle(_targetAngle, _tiltTime);
                    }
                }
            }
            // 플레이어가 올라와 있는 동안은 수동 각도 복원 없음
        }
        else
        {
            // 플레이어가 떠난 뒤 마지막 각도에서 천천히 복구(관성)
            if (!Mathf.Approximately(_targetAngle, 0f))
            {
                _targetAngle = 0f;
                TweenToAngle(_targetAngle, _recoverTime);
            }
        }
    }

    private void TweenToAngle(float toAngle, float time)
    {
        if (_seesawTween != null && _seesawTween.IsActive()) _seesawTween.Kill();

        // DOTween 트윈
        _seesawTween = DOTween.To(() => _currentAngle, x =>
        {
            // 이전 회전과 현재 회전의 차이만큼 플레이어도 같이 회전 이동!
            Quaternion newRotation = Quaternion.Euler(0, 0, x);
            Quaternion deltaRot = newRotation * Quaternion.Inverse(_lastRotation);

            if (IsPlayerOnPlatform())
            {
                Transform player = GetPlayerOnPlatform();
                Rigidbody rb = GetPlayerRigidbody();
                if (player != null && rb != null)
                {
                    Vector3 dir = player.position - transform.position;
                    Vector3 newPos = transform.position + deltaRot * dir;
                    rb.MovePosition(newPos);
                }
            }

            _currentAngle = x;
            transform.localRotation = newRotation;
            _lastRotation = newRotation;
        },
        toAngle,
        time).SetEase(Ease.OutCubic);
    }
}
