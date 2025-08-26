 using UnityEngine;
using DG.Tweening;

public class SeesawObstacle : BaseObstacle
{
    [Header("시소 기울기 설정")]
    [Tooltip("최대 기울기 각도(도 단위)")]
    [SerializeField] private float _maxAngle = 25f;
    [Tooltip("회복 속도(초당 각도 변화)")]
    [SerializeField] private float _recoverSpeed = 35f;
    [Tooltip("플레이어가 한쪽 끝에 있을 때 기울어지는 속도")]
    [SerializeField] private float _tiltSpeed = 90f;
    [Tooltip("플레이어 감지 범위(시소 길이의 절반)")]
    [SerializeField] private float _detectLength = 2.5f;

    private float _currentAngle = 0f; // 현재 회전각(도)
    private float _targetAngle = 0f;  // 목표 각도

    private void Update()
    {
        UpdateSeesaw();
    }

    private void UpdateSeesaw()
    {
        // OnCollision 방식으로 감지
        if (IsPlayerOnPlatform())
        {
            Transform playerTransform = GetPlayerOnPlatform();
            Vector3 localPlayerPos = transform.InverseTransformPoint(playerTransform.position);

            if (Mathf.Abs(localPlayerPos.x) <= _detectLength && Mathf.Abs(localPlayerPos.z) < 0.7f)
            {
                float ratio = Mathf.Clamp(localPlayerPos.x / _detectLength, -1f, 1f);
                _targetAngle = -_maxAngle * ratio;
            }
            else
            {
                _targetAngle = 0f;
            }
        }
        else
        {
            _targetAngle = 0f;
        }

        float speed = (_targetAngle == 0f) ? _recoverSpeed : _tiltSpeed;
        _currentAngle = Mathf.MoveTowards(_currentAngle, _targetAngle, speed * Time.deltaTime);

        transform.localRotation = Quaternion.Euler(0, 0, _currentAngle);
    }
}
