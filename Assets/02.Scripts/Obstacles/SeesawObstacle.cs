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
        Transform playerTransform;
        bool playerOnSeesaw = TryGetPlayerOnTop(out playerTransform);

        if (playerOnSeesaw && playerTransform != null)
        {
            // 시소의 local 좌표계에서 플레이어 위치 계산
            Vector3 localPlayerPos = transform.InverseTransformPoint(playerTransform.position);

            // 시소의 길이(X축 기준, z축이면 localPlayerPos.z)에서 플레이어의 상대 위치로 비율 구함
            if (Mathf.Abs(localPlayerPos.x) <= _detectLength && Mathf.Abs(localPlayerPos.z) < 0.7f)
            {
                // 플레이어가 왼쪽(-)에 있으면 음각, 오른쪽(+)이면 양각
                float ratio = Mathf.Clamp(localPlayerPos.x / _detectLength, -1f, 1f);
                _targetAngle = -_maxAngle * ratio;
            }
            else
            {
                // 시소 범위 바깥으로 나가면 천천히 복원
                _targetAngle = 0f;
            }
        }
        else
        {
            // 시소에 플레이어 없으면 천천히 복원
            _targetAngle = 0f;
        }

        // 기울기 속도 적용(가속/감속), 부드럽게 회전
        float speed = (_targetAngle == 0f) ? _recoverSpeed : _tiltSpeed;
        _currentAngle = Mathf.MoveTowards(_currentAngle, _targetAngle, speed * Time.deltaTime);

        // 실제 회전 적용 (Z축을 시소 방향으로)
        transform.localRotation = Quaternion.Euler(0, 0, _currentAngle);
    }
}
