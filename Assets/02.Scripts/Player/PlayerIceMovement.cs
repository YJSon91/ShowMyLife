using UnityEngine;

/// <summary>
/// 빙판 효과를 플레이어에게 적용하는 컴포넌트
/// 플레이어가 빙판에 들어갔을 때 진행 방향으로 미끄러지는 효과 구현
/// </summary>
public class PlayerIceMovement : MonoBehaviour
{
    // 빙판 효과 설정
    private float _frictionFactor;
    private float _controlReduceFactor;
    private float _minSpeedThreshold;
    
    // 미끄러짐 상태 추적
    private Vector3 _slideDirection;
    private float _slideSpeed;
    private bool _isOnIce;
    
    // 플레이어 컴포넌트 참조
    private Player _player;
    private PlayerMovementController _playerMovement;
    private Rigidbody _rigidbody;
    
    // 원래 이동 설정 저장용
    private bool _originalIsSlipping;
    private Vector3 _originalSlipDirection;
    private float _originalSlipForce;
    private float _originalSlipGravityMultiplier;
    
    // 빙판 효과 시작
    public void StartIceEffect(Player player, float friction, float controlReduce, float speedThreshold)
    {
        _player = player;
        _playerMovement = player.MovementController;
        _rigidbody = _playerMovement.Rigidbody;
        
        _frictionFactor = friction;
        _controlReduceFactor = controlReduce;
        _minSpeedThreshold = speedThreshold;
        
        // 현재 이동 상태 저장
        _originalIsSlipping = _playerMovement.IsSlipping;
        
        // 플레이어의 현재 이동 방향과 속도 가져오기
        Vector3 playerVelocity = _rigidbody.velocity;
        
        // 수평 방향의 속도만 고려
        Vector3 horizontalVelocity = new Vector3(playerVelocity.x, 0, playerVelocity.z);
        _slideSpeed = horizontalVelocity.magnitude;
        
        // 속도가 너무 작으면 전방 방향 사용
        if (_slideSpeed < 0.5f)
        {
            _slideDirection = _player.transform.forward;
            _slideSpeed = 2f; // 최소 초기 속도 설정
        }
        else
        {
            _slideDirection = horizontalVelocity.normalized;
        }
        
        // 빙판 효과 활성화
        _isOnIce = true;
        
        // 경사면 미끄러짐 효과 사용 (PlayerMovementController의 기존 기능 활용)
        _playerMovement.ActivateSlipping(_slideDirection, _slideSpeed, 0.1f, _controlReduceFactor);
    }
    
    // 빙판 효과 종료
    public void StopIceEffect()
    {
        if (!_isOnIce) return;
        
        _isOnIce = false;
        
        // 경사면 미끄러짐 효과 비활성화
        _playerMovement.DeactivateSlipping();
        
        // 컴포넌트 제거
        Destroy(this);
    }
    
    private void Update()
    {
        if (!_isOnIce) return;
        
        // 속도가 임계값 이하면 미끄러짐 중단
        if (_slideSpeed < _minSpeedThreshold)
        {
            StopIceEffect();
            return;
        }
        
        // 미끄러짐 속도 감소
        _slideSpeed *= _frictionFactor;
        
        // 플레이어 입력 방향 가져오기
        Vector3 inputDirection = GetPlayerInputDirection();
        
        // 미끄러짐 방향 업데이트 (플레이어 입력 약간 반영)
        if (inputDirection.magnitude > 0.1f)
        {
            _slideDirection = Vector3.Lerp(_slideDirection, inputDirection, 0.02f);
        }
        
        // 경사면 미끄러짐 효과 업데이트
        _playerMovement.ActivateSlipping(_slideDirection, _slideSpeed, 0.1f, _controlReduceFactor);
    }
    
    // 플레이어 입력 방향 계산
    private Vector3 GetPlayerInputDirection()
    {
        // 카메라 기준 입력 방향 계산
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;
        
        // Y축 값을 0으로 설정하고 정규화
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();
        
        // 입력 값 가져오기
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        // 이동 방향 계산
        return (cameraForward * vertical + cameraRight * horizontal).normalized;
    }
    
    private void OnDestroy()
    {
        // 컴포넌트가 제거될 때 빙판 효과 종료
        if (_isOnIce && _playerMovement != null)
        {
            _playerMovement.DeactivateSlipping();
        }
    }
}