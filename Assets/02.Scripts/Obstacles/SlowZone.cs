using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BaseObstacle;

public class SlowZone : BaseObstacle
{
    [Header("슬로우존 옵션")]
    [Range(0.1f, 1f)] public float moveSpeedMultiplier = 0.7f;
    [Tooltip("존 이탈 후 서서히 복구 시간(초, 0이면 즉시 복구)")]
    public float restoreDuration = 2.0f;

    private PlayerMovementController _playerController;
    private float _originalRunSpeed;
    private float _originalSprintSpeed;
    private float _restoreTimer = 0f;
    private bool _isSlowing = false;

    // Trigger 모드만 사용 권장 (Inspector에서 강제도 가능)
    private void Reset()
    {
        _senseMode = SenseMode.Trigger;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        if (_senseMode != SenseMode.Trigger) return;
        if (!enablePlayerCarry) return;

        if (IsPlayerObject(other.gameObject))
        {
            _playerController = other.GetComponent<PlayerMovementController>();
            if (_playerController != null && !_isSlowing)
            {
                _originalRunSpeed = _playerController._runSpeed;
                _originalSprintSpeed = _playerController._sprintSpeed;

                // 곱셈 방식으로 감속 적용
                _playerController._runSpeed *= moveSpeedMultiplier;
                _playerController._sprintSpeed *= moveSpeedMultiplier;

                _isSlowing = true;
                _restoreTimer = 0f;
            }
        }
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);

        if (_senseMode != SenseMode.Trigger) return;
        if (!enablePlayerCarry) return;

        if (IsPlayerObject(other.gameObject) && _playerController != null)
        {
            _isSlowing = false;
            _restoreTimer = 0f;

            if (restoreDuration <= 0f)
            {
                // 즉시 복구
                _playerController._runSpeed = _originalRunSpeed;
                _playerController._sprintSpeed = _originalSprintSpeed;
                _playerController = null;
            }
            // 아니면 Update에서 점진 복구
        }
    }

    private void Update()
    {
        if (_playerController == null || _isSlowing) return;
        if (restoreDuration <= 0f) return;

        _restoreTimer += Time.deltaTime;
        float t = Mathf.Clamp01(_restoreTimer / restoreDuration);

        _playerController._runSpeed = Mathf.Lerp(_playerController._runSpeed, _originalRunSpeed, t);
        _playerController._sprintSpeed = Mathf.Lerp(_playerController._sprintSpeed, _originalSprintSpeed, t);

        // 거의 다 복구되면 완전히 원래 값으로 맞추고 끝냄
        if (t >= 1f ||
            Mathf.Abs(_playerController._runSpeed - _originalRunSpeed) < 0.01f)
        {
            _playerController._runSpeed = _originalRunSpeed;
            _playerController._sprintSpeed = _originalSprintSpeed;
            _playerController = null;
        }
    }
}
