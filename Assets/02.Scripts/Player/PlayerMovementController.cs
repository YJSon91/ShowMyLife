using UnityEngine;

/// <summary>
/// 플레이어의 이동 및 물리 동작을 처리하는 컨트롤러
/// </summary>
public class PlayerMovementController : MonoBehaviour
{
    #region 컴포넌트 참조

    [Header("필수 컴포넌트")]
    [Tooltip("플레이어 메인 컴포넌트")]
    [SerializeField] private Player _player;
    
    // 내부 컴포넌트 참조 (초기화 시 할당)
    private CharacterController _controller;
    
    private InputReader _inputReader;

    #endregion

    #region 이동 설정

    [Header("플레이어 이동")]
    [Header("기본 설정")]
    [Tooltip("캐릭터가 항상 카메라 방향을 바라보도록 할지 여부")]
    [SerializeField] private bool _alwaysStrafe = true;
    [Tooltip("걷기 상태나 반누름 시 플레이어의 가장 느린 이동 속도")]
    [SerializeField] public float _walkSpeed = 1.4f;
    [Tooltip("플레이어의 기본 이동 속도")]
    [SerializeField] public float _runSpeed = 2.5f;
    [Tooltip("플레이어의 최고 이동 속도")]
    [SerializeField] public float _sprintSpeed = 7f;
    [Tooltip("속도 변경을 위한 감쇠 계수")]
    [SerializeField] private float _speedChangeDamping = 10f;
    [Tooltip("회전 부드러움 계수")]
    [SerializeField] private float _rotationSmoothing = 10f;
    [Tooltip("카메라 회전 오프셋")]
    [SerializeField] private float _cameraRotationOffset;

    #endregion

    #region 케릭터 컨트롤러 캡슐 설정

    [Header("캡슐 값")]
    [Tooltip("플레이어 캡슐의 서있는 높이")]
    [SerializeField] private float _capsuleStandingHeight = 1.8f;
    [Tooltip("플레이어 캡슐의 서있는 중심점")]
    [SerializeField] private float _capsuleStandingCentre = 0.93f;
    [Tooltip("플레이어 캡슐의 웅크린 높이")]
    [SerializeField] private float _capsuleCrouchingHeight = 1.2f;
    [Tooltip("플레이어 캡슐의 웅크린 중심점")]
    [SerializeField] private float _capsuleCrouchingCentre = 0.6f;

    #endregion

    #region 공중 설정

    [Header("플레이어 공중")]
    [Tooltip("플레이어가 점프할 때 적용되는 힘")]
    [SerializeField] private float _jumpForce = 10f;
    [Tooltip("공중에 있을 때의 중력 배수")]
    [SerializeField] private float _gravityMultiplier = 2f;

    #endregion

    #region 지면 확인 설정

    [Header("지면 각도")]
    [Tooltip("지면 각도 확인을 위한 후방 레이 위치")]
    [SerializeField] private Transform _rearRayPos;
    [Tooltip("지면 각도 확인을 위한 전방 레이 위치")]
    [SerializeField] private Transform _frontRayPos;
    [Tooltip("지면 확인을 위한 레이어 마스크")]
    [SerializeField] private LayerMask _groundLayerMask;
    [Tooltip("현재 경사 각도")]
    [SerializeField] private float _inclineAngle;
    [Tooltip("거친 지면에 유용함")]
    [SerializeField] private float _groundedOffset = -0.14f;

    #endregion

    #region 런타임 속성

    private bool _isGrounded = true;
    private bool _isCrouching;
    private bool _isWalking;
    private bool _isSprinting;
    private bool _isStrafing;
    private bool _isAiming;
    private bool _isLockedOn;
    public bool _cannotStandUp;
    private bool _isSliding;
    public Vector3 _velocity;
    private Vector3 _moveDirection;
    private float _speed2D;
    private float _currentMaxSpeed;
    private float _targetMaxSpeed;
    private float _fallingDuration;
    private float _fallStartTime;
    private float _strafeAngle;
    private float _newDirectionDifferenceAngle;
    private Vector3 _targetVelocity;
    private Vector3 _cameraForward;

    #endregion

    #region 공개 속성

    /// <summary>
    /// 플레이어의 2D 속도
    /// </summary>
    public float Speed2D => _speed2D;

    /// <summary>
    /// 플레이어의 낙하 지속 시간
    /// </summary>
    public float FallingDuration => _fallingDuration;

    /// <summary>
    /// 플레이어의 지면 경사 각도
    /// </summary>
    public float InclineAngle => _inclineAngle;

    /// <summary>
    /// 플레이어가 지면에 있는지 여부
    /// </summary>
    public bool IsGrounded => _isGrounded;

    /// <summary>
    /// 플레이어가 웅크리고 있는지 여부
    /// </summary>
    public bool IsCrouching => _isCrouching;

    /// <summary>
    /// 플레이어가 걷고 있는지 여부
    /// </summary>
    public bool IsWalking => _isWalking;

    /// <summary>
    /// 플레이어가 스트레이핑 중인지 여부
    /// </summary>
    public bool IsStrafing => _isStrafing;
    
    /// <summary>
    /// 플레이어의 이동 방향 벡터
    /// </summary>
    public Vector3 MoveDirection => _moveDirection;

    /// <summary>
    /// 플레이어의 카메라 회전 오프셋
    /// </summary>
    public float CameraRotationOffset => _cameraRotationOffset;

    /// <summary>
    /// 플레이어의 새 방향과의 각도 차이
    /// </summary>
    public float NewDirectionDifferenceAngle => _newDirectionDifferenceAngle;

    #endregion

    #region Unity 라이프사이클

    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        _isStrafing = _alwaysStrafe;
        
        // 입력 이벤트 구독
        _inputReader.onWalkToggled += ToggleWalk;
        _inputReader.onSprintActivated += ActivateSprint;
        _inputReader.onSprintDeactivated += DeactivateSprint;
        _inputReader.onCrouchActivated += ActivateCrouch;
        _inputReader.onCrouchDeactivated += DeactivateCrouch;
    }

    private void OnDestroy()
    {
        // 입력 이벤트 구독 해제
        _inputReader.onWalkToggled -= ToggleWalk;
        _inputReader.onSprintActivated -= ActivateSprint;
        _inputReader.onSprintDeactivated -= DeactivateSprint;
        _inputReader.onCrouchActivated -= ActivateCrouch;
        _inputReader.onCrouchDeactivated -= DeactivateCrouch;
    }

    #endregion

    #region 초기화 메서드

    /// <summary>
    /// 필요한 컴포넌트를 초기화합니다
    /// </summary>
    private void InitializeComponents()
    {
        // Player 컴포넌트가 할당되지 않은 경우 부모에서 찾기
        if (_player == null)
            _player = GetComponentInParent<Player>();

        // Player 컴포넌트에서 필요한 컴포넌트 가져오기
        if (_player != null)
        {
            _controller = _player.CharacterController;
            
            _inputReader = _player.InputReader;
        }
        else
        {
            Debug.LogError("PlayerMovementController: Player 컴포넌트를 찾을 수 없습니다!");
        }

        ValidateComponents();
    }

    /// <summary>
    /// 필요한 컴포넌트가 모두 할당되었는지 확인합니다
    /// </summary>
    private void ValidateComponents()
    {
        if (_controller == null)
            Debug.LogError("PlayerMovementController: CharacterController가 할당되지 않았습니다!");
        

        if (_inputReader == null)
            Debug.LogError("PlayerMovementController: InputReader가 할당되지 않았습니다!");

        if (_rearRayPos == null || _frontRayPos == null)
            Debug.LogError("PlayerMovementController: 지면 확인을 위한 레이 위치가 할당되지 않았습니다!");
    }

    #endregion

    #region 이동 메서드

    /// <summary>
    /// 플레이어의 이동을 처리합니다
    /// </summary>
    public void Move()
    {
        _controller.Move(_velocity * Time.deltaTime);
    }

    /// <summary>
    /// 플레이어의 이동 방향을 계산합니다
    /// </summary>
    public void CalculateMoveDirection()
    {
        // _moveDirection = (_cameraController.GetCameraForwardZeroedYNormalised() * _inputReader._moveComposite.y)
        //     + (_cameraController.GetCameraRightZeroedYNormalised() * _inputReader._moveComposite.x);
        // Camera.main을 사용하여 카메라 방향 벡터 얻기
        Vector3 cameraForward = _player.MainCameraTransform.forward;
        Vector3 cameraRight = _player.MainCameraTransform.right;

        // Y축 값을 0으로 설정하고 정규화
        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        // 이동 방향 계산
        _moveDirection = (cameraForward * _inputReader._moveComposite.y)
                       + (cameraRight * _inputReader._moveComposite.x);

        if (!_isGrounded)
        {
            _targetMaxSpeed = _currentMaxSpeed;
        }
        else if (_isCrouching)
        {
            _targetMaxSpeed = _walkSpeed;
        }
        else if (_isSprinting)
        {
            _targetMaxSpeed = _sprintSpeed;
        }
        else if (_isWalking)
        {
            _targetMaxSpeed = _walkSpeed;
        }
        else
        {
            _targetMaxSpeed = _runSpeed;
        }

        const float ANIMATION_DAMP_TIME = 5f;
        _currentMaxSpeed = Mathf.Lerp(_currentMaxSpeed, _targetMaxSpeed, ANIMATION_DAMP_TIME * Time.deltaTime);

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

    /// <summary>
    /// 플레이어에게 중력을 적용합니다
    /// </summary>
    public void ApplyGravity()
    {
        if (_velocity.y > Physics.gravity.y)
        {
            _velocity.y += Physics.gravity.y * _gravityMultiplier * Time.deltaTime;
        }
    }

    /// <summary>
    /// 플레이어가 이동 방향을 바라보도록 합니다
    /// </summary>
    public void FaceMoveDirection()
    {
        // _cameraForward = _cameraController.GetCameraForwardZeroedYNormalised();
        // Camera.main을 사용하여 카메라 전방 벡터 얻기
        Vector3 characterForward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
        Vector3 characterRight = new Vector3(transform.right.x, 0f, transform.right.z).normalized;
        Vector3 directionForward = new Vector3(_moveDirection.x, 0f, _moveDirection.z).normalized;

        Vector3 rawCameraForward = _player.MainCameraTransform.forward;
        _cameraForward = new Vector3(rawCameraForward.x, 0f, rawCameraForward.z).normalized;

        Quaternion strafingTargetRotation = Quaternion.LookRotation(_cameraForward);

        _strafeAngle = characterForward != directionForward
            ? Vector3.SignedAngle(characterForward, directionForward, Vector3.up)
            : 0f;

        if (_isStrafing)
        {
            if (_moveDirection.magnitude > 0.01)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, strafingTargetRotation, _rotationSmoothing * Time.deltaTime);
            }
        }
        else
        {
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

    

    /// <summary>
    /// 플레이어에게 점프 힘을 적용합니다
    /// </summary>
    public void Jump()
    {
        _velocity = new Vector3(_velocity.x, _jumpForce, _velocity.z);
    }

    #endregion

    #region 상태 확인 메서드

    /// <summary>
    /// 플레이어가 지면에 있는지 확인합니다
    /// </summary>
    public void GroundedCheck()
    {
        Vector3 spherePosition = new Vector3(
            _controller.transform.position.x,
            _controller.transform.position.y - _groundedOffset,
            _controller.transform.position.z
        );
        _isGrounded = Physics.CheckSphere(spherePosition, _controller.radius, _groundLayerMask, QueryTriggerInteraction.Ignore);

        if (_isGrounded)
        {
            GroundInclineCheck();
        }
    }

    /// <summary>
    /// 지면 경사를 확인합니다
    /// </summary>
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

    /// <summary>
    /// 웅크리고 있을 때 일어설 수 있는 충분한 공간이 있는지 확인합니다
    /// </summary>
    public void CeilingHeightCheck()
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

    /// <summary>
    /// 낙하 지속 시간을 초기화합니다
    /// </summary>
    public void ResetFallingDuration()
    {
        _fallStartTime = Time.time;
        _fallingDuration = 0f;
    }

    /// <summary>
    /// 낙하 지속 시간을 업데이트합니다
    /// </summary>
    public void UpdateFallingDuration()
    {
        _fallingDuration = Time.time - _fallStartTime;
    }

    #endregion

    #region 상태 변경 메서드

    /// <summary>
    /// 걷기 상태를 토글합니다
    /// </summary>
    private void ToggleWalk()
    {
        EnableWalk(!_isWalking);
    }

    /// <summary>
    /// 걷기 상태를 설정합니다
    /// </summary>
    /// <param name="enable">설정할 상태</param>
    private void EnableWalk(bool enable)
    {
        _isWalking = enable && _isGrounded && !_isSprinting;
    }

    /// <summary>
    /// 달리기 동작을 활성화합니다
    /// </summary>
    private void ActivateSprint()
    {
        if (!_isCrouching)
        {
            EnableWalk(false);
            _isSprinting = true;
            _isStrafing = false;
        }
    }

    /// <summary>
    /// 달리기 동작을 비활성화합니다
    /// </summary>
    private void DeactivateSprint()
    {
        _isSprinting = false;

        if (_alwaysStrafe || _isAiming || _isLockedOn)
        {
            _isStrafing = true;
        }
    }

    /// <summary>
    /// 웅크리기 동작을 활성화합니다
    /// </summary>
    private void ActivateCrouch()
    {
        if (_isGrounded)
        {
            CapsuleCrouchingSize(true);
            DeactivateSprint();
            _isCrouching = true;
        }
    }

    /// <summary>
    /// 웅크리기 동작을 비활성화합니다
    /// </summary>
    public void DeactivateCrouch()
    {
        if (!_cannotStandUp && !_isSliding)
        {
            CapsuleCrouchingSize(false);
            _isCrouching = false;
        }
    }

    /// <summary>
    /// 슬라이딩 동작을 활성화합니다
    /// </summary>
    public void ActivateSliding()
    {
        _isSliding = true;
    }

    /// <summary>
    /// 슬라이딩을 비활성화합니다
    /// </summary>
    public void DeactivateSliding()
    {
        _isSliding = false;
    }

    /// <summary>
    /// 플레이어의 캡슐 크기를 조정합니다
    /// </summary>
    /// <param name="crouching">플레이어가 웅크리고 있는지 여부</param>
    private void CapsuleCrouchingSize(bool crouching)
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

    #endregion
} 