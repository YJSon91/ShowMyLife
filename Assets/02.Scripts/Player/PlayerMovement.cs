using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float _walkSpeed = 1.4f;
    [SerializeField] private float _runSpeed = 2.5f;
    [SerializeField] private float _sprintSpeed = 7f;
    [SerializeField] private float _speedChangeDamping = 10f;
    [SerializeField] private float _rotationSmoothing = 10f;
    [SerializeField] private float _cameraRotationOffset;
    [SerializeField] private bool _alwaysStrafe = true;
    
    [Header("점프 및 중력")]
    [SerializeField] private float _jumpForce = 10f;
    [SerializeField] private float _gravityMultiplier = 2f;
    [SerializeField] private float _fallingDuration;
    
    [Header("지면 확인")]
    [SerializeField] private Transform _rearRayPos;
    [SerializeField] private Transform _frontRayPos;
    [SerializeField] private LayerMask _groundLayerMask;
    [SerializeField] private float _groundedOffset = -0.14f;
    [SerializeField] private float _inclineAngle;
    
    [Header("캡슐 설정")]
    [SerializeField] private float _capsuleStandingHeight = 1.8f;
    [SerializeField] private float _capsuleStandingCentre = 0.93f;
    [SerializeField] private float _capsuleCrouchingHeight = 1.2f;
    [SerializeField] private float _capsuleCrouchingCentre = 0.6f;
    
    [Header("스트레이핑 설정")]
    [SerializeField] private float _forwardStrafeMinThreshold = -55.0f;
    [SerializeField] private float _forwardStrafeMaxThreshold = 125.0f;
    [SerializeField] private float _forwardStrafe = 1f;
    [SerializeField] private float _strafeDirectionX;
    [SerializeField] private float _strafeDirectionZ;
    [SerializeField] private float _shuffleDirectionX;
    [SerializeField] private float _shuffleDirectionZ;
    //[SerializeField] private float _buttonHoldThreshold = 0.15f;
    
    // 내부 변수
    private CharacterController _controller;
    private PlayerCamera _cameraController;
    private Vector3 _velocity;
    private Vector3 _moveDirection;
    private Vector3 _targetVelocity;
    private float _currentMaxSpeed;
    private float _targetMaxSpeed;
    private float _speed2D;
    private float _strafeAngle;
    private bool _isGrounded = true;
    private bool _cannotStandUp;
    private bool _isStrafing;
    private float _fallStartTime;
    private float _newDirectionDifferenceAngle;
    
    private const float _ANIMATION_DAMP_TIME = 5f;
    private const float _STRAFE_DIRECTION_DAMP_TIME = 20f;
    
    public void Initialize(CharacterController controller, PlayerCamera cameraController)
    {
        _controller = controller;
        _cameraController = cameraController;
        _isStrafing = _alwaysStrafe;
    }
    
    public Vector3 GetVelocity() => _velocity;
    public Vector3 GetMoveDirection() => _moveDirection;
    public float GetInclineAngle() => _inclineAngle;
    public bool CannotStandUp() => _cannotStandUp;
    
    public bool CheckGrounded()
    {
        GroundedCheck();
        return _isGrounded;
    }
    
    public void ProcessMovement(Vector2 moveInput, bool isWalking, bool isSprinting, bool isCrouching)
    {
        // 이동 방향 계산
        CalculateMoveDirection(moveInput, isWalking, isSprinting, isCrouching);
        
        // 회전 처리
        FaceMoveDirection();
        
        // 중력 적용
        if (!_isGrounded)
        {
            ApplyGravity();
        }
        
        // 이동 실행
        Move();
        
        // 추가 체크
        if (_isGrounded)
        {
            GroundInclineCheck();
        }
        
        if (isCrouching)
        {
            CeilingHeightCheck();
        }
    }
    
    public void Jump()
    {
        _velocity = new Vector3(_velocity.x, _jumpForce, _velocity.z);
        _isGrounded = false;
        ResetFallingDuration();
    }
    
    public void SetCrouchingSize(bool crouching)
    {
        if (crouching)
        {
            _controller.center = new Vector3(0f, _capsuleCrouchingCentre, 0f);
            _controller.height = _capsuleCrouchingHeight;
        }
        else
        {
            _controller.center = new Vector3(0f, _capsuleStandingCentre, 0f);
            _controller.height = _capsuleStandingHeight;
        }
    }
    
    private void CalculateMoveDirection(Vector2 moveInput, bool isWalking, bool isSprinting, bool isCrouching)
    {
        _moveDirection = (_cameraController.GetCameraForwardZeroedYNormalised() * moveInput.y)
            + (_cameraController.GetCameraRightZeroedYNormalised() * moveInput.x);
        
        if (!_isGrounded)
        {
            _targetMaxSpeed = _currentMaxSpeed;
        }
        else if (isCrouching)
        {
            _targetMaxSpeed = _walkSpeed;
        }
        else if (isSprinting)
        {
            _targetMaxSpeed = _sprintSpeed;
        }
        else if (isWalking)
        {
            _targetMaxSpeed = _walkSpeed;
        }
        else
        {
            _targetMaxSpeed = _runSpeed;
        }
        
        _currentMaxSpeed = Mathf.Lerp(_currentMaxSpeed, _targetMaxSpeed, _ANIMATION_DAMP_TIME * Time.deltaTime);
        
        _targetVelocity.x = _moveDirection.x * _currentMaxSpeed;
        _targetVelocity.z = _moveDirection.z * _currentMaxSpeed;
        
        _velocity.z = Mathf.Lerp(_velocity.z, _targetVelocity.z, _speedChangeDamping * Time.deltaTime);
        _velocity.x = Mathf.Lerp(_velocity.x, _targetVelocity.x, _speedChangeDamping * Time.deltaTime);
        
        _speed2D = new Vector3(_velocity.x, 0f, _velocity.z).magnitude;
        _speed2D = Mathf.Round(_speed2D * 1000f) / 1000f;
        
        Vector3 playerForwardVector = transform.forward;
        
        _newDirectionDifferenceAngle = playerForwardVector != _moveDirection
            ? Vector3.SignedAngle(playerForwardVector, _moveDirection, Vector3.up)
            : 0f;
    }
    
    private void FaceMoveDirection()
    {
        Vector3 characterForward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
        Vector3 characterRight = new Vector3(transform.right.x, 0f, transform.right.z).normalized;
        Vector3 directionForward = new Vector3(_moveDirection.x, 0f, _moveDirection.z).normalized;
        
        Vector3 cameraForward = _cameraController.GetCameraForwardZeroedYNormalised();
        Quaternion strafingTargetRotation = Quaternion.LookRotation(cameraForward);
        
        _strafeAngle = characterForward != directionForward ? Vector3.SignedAngle(characterForward, directionForward, Vector3.up) : 0f;
        
        if (_isStrafing)
        {
            if (_moveDirection.magnitude > 0.01)
            {
                if (cameraForward != Vector3.zero)
                {
                    _shuffleDirectionZ = Vector3.Dot(characterForward, directionForward);
                    _shuffleDirectionX = Vector3.Dot(characterRight, directionForward);
                    
                    UpdateStrafeDirection(
                        Vector3.Dot(characterForward, directionForward),
                        Vector3.Dot(characterRight, directionForward)
                    );
                    _cameraRotationOffset = Mathf.Lerp(_cameraRotationOffset, 0f, _rotationSmoothing * Time.deltaTime);
                    
                    float targetValue = _strafeAngle > _forwardStrafeMinThreshold && _strafeAngle < _forwardStrafeMaxThreshold ? 1f : 0f;
                    
                    if (Mathf.Abs(_forwardStrafe - targetValue) <= 0.001f)
                    {
                        _forwardStrafe = targetValue;
                    }
                    else
                    {
                        float t = Mathf.Clamp01(_STRAFE_DIRECTION_DAMP_TIME * Time.deltaTime);
                        _forwardStrafe = Mathf.SmoothStep(_forwardStrafe, targetValue, t);
                    }
                }
                
                transform.rotation = Quaternion.Slerp(transform.rotation, strafingTargetRotation, _rotationSmoothing * Time.deltaTime);
            }
            else
            {
                UpdateStrafeDirection(1f, 0f);
                
                float t = 20 * Time.deltaTime;
                float newOffset = 0f;
                
                if (characterForward != cameraForward)
                {
                    newOffset = Vector3.SignedAngle(characterForward, cameraForward, Vector3.up);
                }
                
                _cameraRotationOffset = Mathf.Lerp(_cameraRotationOffset, newOffset, t);
            }
        }
        else
        {
            UpdateStrafeDirection(1f, 0f);
            _cameraRotationOffset = Mathf.Lerp(_cameraRotationOffset, 0f, _rotationSmoothing * Time.deltaTime);
            
            _shuffleDirectionZ = 1;
            _shuffleDirectionX = 0;
            
            Vector3 faceDirection = new Vector3(_velocity.x, 0f, _velocity.z);
            
            if (faceDirection == Vector3.zero)
            {
                return;
            }
            
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(faceDirection),
                _rotationSmoothing * Time.deltaTime
            );
        }
    }
    
    private void UpdateStrafeDirection(float TargetZ, float TargetX)
    {
        _strafeDirectionZ = Mathf.Lerp(_strafeDirectionZ, TargetZ, _ANIMATION_DAMP_TIME * Time.deltaTime);
        _strafeDirectionX = Mathf.Lerp(_strafeDirectionX, TargetX, _ANIMATION_DAMP_TIME * Time.deltaTime);
        _strafeDirectionZ = Mathf.Round(_strafeDirectionZ * 1000f) / 1000f;
        _strafeDirectionX = Mathf.Round(_strafeDirectionX * 1000f) / 1000f;
    }
    
    private void Move()
    {
        _controller.Move(_velocity * Time.deltaTime);
    }
    
    private void ApplyGravity()
    {
        if (_velocity.y > Physics.gravity.y)
        {
            _velocity.y += Physics.gravity.y * _gravityMultiplier * Time.deltaTime;
        }
        
        UpdateFallingDuration();
    }
    
    private void GroundedCheck()
    {
        Vector3 spherePosition = new Vector3(
            _controller.transform.position.x,
            _controller.transform.position.y - _groundedOffset,
            _controller.transform.position.z
        );
        _isGrounded = Physics.CheckSphere(spherePosition, _controller.radius, _groundLayerMask, QueryTriggerInteraction.Ignore);
    }
    
    private void GroundInclineCheck()
    {
        float rayDistance = Mathf.Infinity;
        _rearRayPos.rotation = Quaternion.Euler(transform.rotation.x, 0, 0);
        _frontRayPos.rotation = Quaternion.Euler(transform.rotation.x, 0, 0);
        
        Physics.Raycast(_rearRayPos.position, _rearRayPos.TransformDirection(-Vector3.up), out RaycastHit rearHit, rayDistance, _groundLayerMask);
        Physics.Raycast(
            _frontRayPos.position,
            _frontRayPos.TransformDirection(-Vector3.up),
            out RaycastHit frontHit,
            rayDistance,
            _groundLayerMask
        );
        
        Vector3 hitDifference = frontHit.point - rearHit.point;
        float xPlaneLength = new Vector2(hitDifference.x, hitDifference.z).magnitude;
        
        _inclineAngle = Mathf.Lerp(_inclineAngle, Mathf.Atan2(hitDifference.y, xPlaneLength) * Mathf.Rad2Deg, 20f * Time.deltaTime);
    }
    
    private void CeilingHeightCheck()
    {
        float rayDistance = Mathf.Infinity;
        float minimumStandingHeight = _capsuleStandingHeight - _frontRayPos.localPosition.y;
        
        Vector3 midpoint = new Vector3(transform.position.x, transform.position.y + _frontRayPos.localPosition.y, transform.position.z);
        if (Physics.Raycast(midpoint, transform.TransformDirection(Vector3.up), out RaycastHit ceilingHit, rayDistance, _groundLayerMask))
        {
            _cannotStandUp = ceilingHit.distance < minimumStandingHeight;
        }
        else
        {
            _cannotStandUp = false;
        }
    }
    
    private void ResetFallingDuration()
    {
        _fallStartTime = Time.time;
        _fallingDuration = 0f;
    }
    
    private void UpdateFallingDuration()
    {
        _fallingDuration = Time.time - _fallStartTime;
    }
    
    public float GetFallingDuration()
    {
        return _fallingDuration;
    }
    
    public void SetStrafing(bool strafe)
    {
        _isStrafing = strafe;
    }
}