using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 플레이어의 이동 및 물리 동작을 처리하는 컨트롤러 (리지드바디 기반)
/// </summary>
public class PlayerMovementController : MonoBehaviour
{
    #region 컴포넌트 참조

    [Header("필수 컴포넌트")]
    [Tooltip("플레이어 메인 컴포넌트")]
    [SerializeField] private Player _player;

    // 내부 컴포넌트 참조 (초기화 시 할당)
    private Rigidbody _rigidbody;
    private CapsuleCollider _capsuleCollider;
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
    [SerializeField] private float _rotationSmoothing = 15f; // 더 부드러운 회전을 위해 값 증가
    [Tooltip("카메라 회전 오프셋")]
    [SerializeField] private float _cameraRotationOffset;
    [Tooltip("이동 방식 - Force 또는 Velocity")]
    [SerializeField] private MovementType _movementType = MovementType.Velocity;

    // 이동 방식 열거형
    public enum MovementType
    {
        Force,  // AddForce 사용
        Velocity // velocity 직접 설정
    }

    #endregion

    #region 캡슐 콜라이더 설정

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
    [Tooltip("플레이어 점프 시 적용되는 초기 속도")]
    [SerializeField] private float _jumpForce = 12f;
    [Tooltip("공중에 있을 때의 중력 배수")]
    [SerializeField] private float _gravityMultiplier = 2f;
    [Tooltip("지면 체크를 위한 레이캐스트 거리")]
    [SerializeField] private float _groundCheckDistance = 0.25f;
    [Tooltip("점프 쿨타임 (초)")]
    [SerializeField] private float _jumpCooldown = 1.0f;
    [Tooltip("코요테 타임 길이 (초)")]
    [SerializeField] private float _coyoteTimeThreshold = 0.25f;


    // 점프 쿨타임 관련 변수
    private float _jumpCooldownTimer = 0f;
    private bool _isJumpOnCooldown = false;

    // 코요테 타임 관련 변수
    private float _coyoteTimeCounter;
    private bool _canCoyoteJump;


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

    // 박스캐스트 관련 설정 추가
    [Header("박스캐스트 지면 확인")]
    [Tooltip("박스캐스트 너비 (x축)")]
    [SerializeField] private float _boxCastWidth = 0.6f;
    [Tooltip("박스캐스트 깊이 (z축)")]
    [SerializeField] private float _boxCastDepth = 0.6f;

    // 경사면 제한 설정 추가
    [Header("경사면 제한")]
    [Tooltip("플레이어가 올라갈 수 있는 최대 경사 각도")]
    [SerializeField] private float _slopeLimit = 45f;
    [Tooltip("경사면 제한 기능 활성화 여부")]
    [SerializeField] private bool _slopeLimiting = true;
    [Tooltip("플레이어가 미끄러지기 시작하는 경사 각도")]
    [SerializeField] private float _slipAngle = 35f;
    [Tooltip("경사면에서 미끄러지는 속도")]
    [SerializeField] private float _slipSpeed = 2.5f;
    [Tooltip("미끄러질 때 플레이어 입력 영향 감소 (0-1)")]
    [SerializeField] private float _slipInputReduction = 0.8f;

    [Header("레이어별 경사면 제한")]
    [Tooltip("Obstacle 레이어가 아닌 오브젝트에 적용할 최대 경사 각도")]
    [SerializeField] private float _nonObstacleSlopeLimit = 5f;
    [Tooltip("Obstacle 레이어 마스크")]
    [SerializeField] private LayerMask _obstacleLayerMask;

    private Vector3 _initialPosition; // 경사면 제한을 위한 초기 위치

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

    // 슬립 관련 변수
    private bool _isSlipping;
    private Vector3 _slipDirection;
    private float _slipForce;
    private float _slipGravityMultiplier;

    // --- 슬로우존 관리 ---
    private float _baseRunSpeed;
    private float _baseSprintSpeed;
    private int _slowZoneCount = 0;
    private float _currentSlowMultiplier = 1f;
    // --- restoreDuration 복구 관련 ---
    private bool _isRestoringSpeed = false;
    private float _restoreTimer = 0f;
    private float _restoreDuration = 0f;
    
    // --- 빙판 관련 변수 ---
    [Header("빙판 설정")]
    [Tooltip("빙판에서의 감속 비율 (낮을수록 오래 미끄러짐, 0.01-0.2 권장)")]
    [SerializeField] private float _iceSlowdownRate = 0.05f;
    [Tooltip("빙판에서의 가속 비율 (낮을수록 천천히 가속)")]
    [SerializeField] private float _iceAccelerationRate = 0.5f;
    [Tooltip("빙판 레이어 마스크")]
    [SerializeField] private LayerMask _iceLayerMask;

    private bool _isOnIce;
    private Vector3 _iceVelocity;
    private Vector3 _lastInputDirection;
    private float _currentIceSpeed;

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

    // 리지드바디 이동 관련 변수
    private Vector3 _desiredVelocity;
    private Vector3 _groundNormal = Vector3.up;
    private RaycastHit _groundHit;

    // 미끄럼틀 함정 관련 변수 추가
    private bool _isObstacleSliding;
    private Vector3 _obstacleSlideDirection;
    private float _obstacleSlideForce;
    private float _obstacleSlideGravityMultiplier;
    private float _obstacleSlideInputReduction;

    

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

    /// <summary>
    /// 플레이어가 슬립 중인지 여부
    /// </summary>
    public bool IsSlipping => _isSlipping;

    /// <summary>
    /// 플레이어가 미끄럼틀 함정에서 슬라이딩 중인지 여부
    /// </summary>
    public bool IsObstacleSliding => _isObstacleSliding;
    
    /// <summary>
    /// 플레이어가 빙판 위에 있는지 여부
    /// </summary>
    public bool IsOnIce => _isOnIce;

    public Rigidbody Rigidbody => _rigidbody;

    /// <summary>
    /// 점프 쿨타임이 활성화되어 있는지 확인합니다
    /// </summary>
    public bool IsJumpOnCooldown => _isJumpOnCooldown;
    
    /// <summary>
    /// 현재 점프 쿨타임 타이머 값을 반환합니다 (0~1 사이의 값, 1이 쿨타임 시작, 0이 쿨타임 종료)
    /// </summary>
    public float JumpCooldownNormalized => _isJumpOnCooldown ? _jumpCooldownTimer / _jumpCooldown : 0f;

    #endregion

    #region Unity 라이프사이클

    private void Awake()
    {
        InitializeComponents();
        _baseRunSpeed = _runSpeed;
        _baseSprintSpeed = _sprintSpeed;
        
        // 빙판 속성 초기화
        _currentIceSlowdownRate = _iceSlowdownRate;
        _currentIceAccelerationRate = _iceAccelerationRate;
        _currentIceSpeedMultiplier = 1.0f;
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
        _inputReader.onJumpPerformed += OnJumpInput;
    }



    private void Update()
    {
        // 입력 처리 및 방향 계산은 Update에서 처리 (더 부드러운 응답성)
        CalculateMoveDirection();

        // 지면에 있을 때만 회전 적용 (시각적 부드러움을 위해)
        if (_isGrounded)
        {
            FaceMoveDirection();
        }
        
        // 점프 쿨타임 업데이트
        UpdateJumpCooldown();
        
        // 코요테 타임 업데이트
        if (!_isGrounded && _canCoyoteJump)
        {
            _coyoteTimeCounter -= Time.deltaTime;
            if (_coyoteTimeCounter <= 0)
            {
                _canCoyoteJump = false;
            }
        }
        

        
        // 슬로우 복구 처리
        if (_isRestoringSpeed)
        {
            if (_restoreDuration > 0f)
            {
                _restoreTimer += Time.deltaTime;
                float t = Mathf.Clamp01(_restoreTimer / _restoreDuration);
                _runSpeed = Mathf.Lerp(_runSpeed, _baseRunSpeed, t);
                _sprintSpeed = Mathf.Lerp(_sprintSpeed, _baseSprintSpeed, t);

                if (t >= 1f || Mathf.Abs(_runSpeed - _baseRunSpeed) < 0.01f)
                {
                    _runSpeed = _baseRunSpeed;
                    _sprintSpeed = _baseSprintSpeed;
                    _isRestoringSpeed = false;
                    _currentSlowMultiplier = 1f;
                }
            }
            else
            {
                _runSpeed = _baseRunSpeed;
                _sprintSpeed = _baseSprintSpeed;
                _isRestoringSpeed = false;
                _currentSlowMultiplier = 1f;
            }
        }
    }

    private void FixedUpdate()
    {
        //현재 위치 저장 (경사면 제한을 위해)
        _initialPosition = transform.position;

        // 물리 기반 처리는 FixedUpdate에서 유지
        GroundedCheck();
        
        // 빙판 확인 추가
        CheckIceSurface();

        //경사면 제한 적용
        if (_isGrounded && _slopeLimiting)
        {
            SlopeLimit();
        }
        if (jumpRequest) {
            Vector3 velocity = _rigidbody.velocity;
            velocity.y = _jumpForce;
            _rigidbody.velocity = velocity;
            _isGrounded = false;
            jumpRequest = false;
        }

        Move();
        ApplyGravity();
    }

    private bool jumpRequest= false;


    private void OnDestroy()
    {
        // 입력 이벤트 구독 해제
        _inputReader.onWalkToggled -= ToggleWalk;
        _inputReader.onSprintActivated -= ActivateSprint;
        _inputReader.onSprintDeactivated -= DeactivateSprint;
        _inputReader.onCrouchActivated -= ActivateCrouch;
        _inputReader.onCrouchDeactivated -= DeactivateCrouch;
        _inputReader.onJumpPerformed -= OnJumpInput;
    }

    // 충돌 감지 이벤트
    private void OnCollisionStay(Collision collision)
    {
        // 지면 충돌 확인을 위한 추가 체크
        foreach (ContactPoint contact in collision.contacts)
        {
            // 접촉점의 법선이 위쪽을 향하면 지면으로 간주
            if (Vector3.Dot(contact.normal, Vector3.up) > 0.5f)
            {
                _groundNormal = contact.normal;
                _isGrounded = true;
                return;
            }
        }
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
            _rigidbody = _player.Rigidbody;
            _capsuleCollider = _player.CapsuleCollider;
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
        if (_rigidbody == null)
            Debug.LogError("PlayerMovementController: Rigidbody가 할당되지 않았습니다!");

        if (_capsuleCollider == null)
            Debug.LogError("PlayerMovementController: CapsuleCollider가 할당되지 않았습니다!");

        if (_inputReader == null)
            Debug.LogError("PlayerMovementController: InputReader가 할당되지 않았습니다!");

        if (_rearRayPos == null || _frontRayPos == null)
            Debug.LogError("PlayerMovementController: 지면 확인을 위한 레이 위치가 할당되지 않았습니다!");
    }

    #endregion

    #region 이동 메서드

    /// <summary>
    /// 벡터에 NaN이 포함되어 있는지 확인합니다
    /// </summary>
    private bool ContainsNaN(Vector3 vector)
    {
        return float.IsNaN(vector.x) || float.IsNaN(vector.y) || float.IsNaN(vector.z);
    }
    
    /// <summary>
    /// 플레이어의 이동을 처리합니다
    /// </summary>
    public void Move()
    {
        // NaN 체크 및 처리
        if (ContainsNaN(_targetVelocity))
        {
            Debug.LogWarning("목표 속도에 NaN 값 감지됨! 초기화합니다.");
            _targetVelocity = Vector3.zero;
        }
        
        // 이동 방식에 따라 다른 물리 이동 적용
        if (_movementType == MovementType.Force)
        {
            // 기존 AddForce 대신 velocity 직접 수정 방식으로 변경
            Vector3 moveForce = new Vector3(_targetVelocity.x, 0, _targetVelocity.z) - new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
            moveForce = Vector3.ClampMagnitude(moveForce * _speedChangeDamping, _currentMaxSpeed * 2);

            // Time.deltaTime을 곱해 프레임 독립적인 이동 구현
            Vector3 velocityChange = moveForce * Time.deltaTime;
            Vector3 newVelocity = _rigidbody.velocity + velocityChange;

            // y축 속도는 그대로 유지
            newVelocity.y = _rigidbody.velocity.y;
            
            // NaN 체크
            if (ContainsNaN(newVelocity))
            {
                Debug.LogError("리지드바디에 NaN 속도가 할당되려고 합니다! 현재 속도를 유지합니다.");
                return;
            }
            
            _rigidbody.velocity = newVelocity;
        }
        else
        {
            // 속도 직접 설정 (부드러운 반응을 위해 약간의 보간 추가)
            Vector3 currentVel = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
            Vector3 targetVel = new Vector3(_targetVelocity.x, 0, _targetVelocity.z);
            Vector3 newHorizontalVel = Vector3.Lerp(currentVel, targetVel, _speedChangeDamping * Time.fixedDeltaTime);

            // y축 속도는 그대로 유지
            Vector3 newVelocity = new Vector3(newHorizontalVel.x, _rigidbody.velocity.y, newHorizontalVel.z);
            
            // NaN 체크
            if (ContainsNaN(newVelocity))
            {
                Debug.LogError("리지드바디에 NaN 속도가 할당되려고 합니다! 현재 속도를 유지합니다.");
                return;
            }
            
            _rigidbody.velocity = newVelocity;
        }

        // 현재 속도 저장 (애니메이션 등에서 사용)
        _velocity = _rigidbody.velocity;
        _speed2D = new Vector3(_velocity.x, 0f, _velocity.z).magnitude;
        _speed2D = Mathf.Round(_speed2D * 1000f) / 1000f;
    }

    /// <summary>
    /// 이동 방향과 속도를 완전히 초기화합니다.
    /// 이벤트 시작/종료 시 호출하여 플레이어가 멈추도록 합니다.
    /// </summary>
    public void ResetMovement()
    {
        // 이동 방향 초기화
        _moveDirection = Vector3.zero;
        _targetVelocity = Vector3.zero;
        
        // 리지드바디 속도 초기화 (y축 속도는 유지)
        Vector3 currentVelocity = _rigidbody.velocity;
        _rigidbody.velocity = new Vector3(0f, currentVelocity.y, 0f);
        
        // 현재 속도 저장 변수도 초기화
        _velocity = _rigidbody.velocity;
        _speed2D = 0f;
    }

    /// <summary>
    /// 플레이어의 이동 방향을 계산합니다
    /// </summary>
    public void CalculateMoveDirection()
    {
        // Camera.main을 사용하여 카메라 방향 벡터 얻기
        Vector3 cameraForward = _player.MainCameraTransform.forward;
        Vector3 cameraRight = _player.MainCameraTransform.right;

        // Y축 값을 0으로 설정하고 정규화
        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        // 이동 방향 계산
        Vector3 playerInputDirection = (cameraForward * _inputReader._moveComposite.y)
                   + (cameraRight * _inputReader._moveComposite.x);
                   
        // 입력이 있으면 마지막 입력 방향 저장
        if (playerInputDirection.magnitude > 0.1f)
        {
            _lastInputDirection = playerInputDirection.normalized;
        }
        
        // 빙판 처리 (최우선 순위)
        if (_isOnIce)
        {
            HandleIceMovement(playerInputDirection);
            return;
        }

        // 미끄럼틀 함정 슬라이드가 우선순위가 가장 높음
        if (_isObstacleSliding)
        {
            // 미끄럼틀에서는 강제 방향으로 이동하고 플레이어 입력을 크게 제한
            _moveDirection = Vector3.Lerp(_obstacleSlideDirection, playerInputDirection, 1f - _obstacleSlideInputReduction);
            _targetMaxSpeed = _obstacleSlideForce;
        }
        // 다음으로 자연 경사면 슬립 확인
        else if (_isSlipping)
        {
            // 슬립 중에는 강제 방향으로 이동하고 플레이어 입력을 크게 제한
            _moveDirection = Vector3.Lerp(_slipDirection, playerInputDirection, 1f - _slipInputReduction);
            _targetMaxSpeed = _slipForce;
        }
        else
        {
            _moveDirection = playerInputDirection;

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
        }

        const float ANIMATION_DAMP_TIME = 10f; // 더 빠른 반응성을 위해 값 증가
        _currentMaxSpeed = Mathf.Lerp(_currentMaxSpeed, _targetMaxSpeed, ANIMATION_DAMP_TIME * Time.deltaTime);

        _targetVelocity.x = _moveDirection.x * _currentMaxSpeed;
        _targetVelocity.z = _moveDirection.z * _currentMaxSpeed;

        // 경사면에서 미끄러짐 방지를 위한 지면 법선 기반 속도 조정
        if (_isGrounded && _groundNormal != Vector3.up)
        {
            _targetVelocity = Vector3.ProjectOnPlane(_targetVelocity, _groundNormal);
        }

        Vector3 playerForwardVector = transform.forward;

        _newDirectionDifferenceAngle = playerForwardVector != _moveDirection
            ? Vector3.SignedAngle(playerForwardVector, _moveDirection, Vector3.up)
            : 0f;
    }
    
    /// <summary>
    /// 벡터를 안전하게 정규화합니다. 길이가 너무 작으면 기본 방향을 반환합니다.
    /// </summary>
    private Vector3 SafeNormalize(Vector3 vector, Vector3 defaultDirection)
    {
        // 벡터가 유효한지 확인 (NaN 체크)
        if (float.IsNaN(vector.x) || float.IsNaN(vector.y) || float.IsNaN(vector.z))
        {
            Debug.LogWarning("NaN 벡터 감지됨! 기본 방향으로 대체합니다.");
            return defaultDirection;
        }
        
        // 벡터 길이 확인
        float magnitude = vector.magnitude;
        if (magnitude > 0.001f)
        {
            return vector / magnitude; // 직접 나누기로 정규화
        }
        else
        {
            return defaultDirection;
        }
    }
    
    /// <summary>
    /// 빙판 위에서의 이동을 처리합니다
    /// </summary>
    private void HandleIceMovement(Vector3 inputDirection)
    {
        // NaN 값 확인 및 수정
        if (float.IsNaN(_iceVelocity.x) || float.IsNaN(_iceVelocity.y) || float.IsNaN(_iceVelocity.z))
        {
            Debug.LogWarning("빙판 속도에 NaN 값 감지됨! 초기화합니다.");
            _iceVelocity = Vector3.zero;
            _currentIceSpeed = 0f;
        }
        
        // 입력이 있을 때 (방향 전환 시도)
        if (inputDirection.magnitude > 0.1f)
        {
            // 현재 빙판 속도 방향과 입력 방향을 혼합 (서서히 방향 전환)
            Vector3 currentDir;
            Vector3 targetDir;
            
            if (_iceVelocity.magnitude > 0.1f)
            {
                currentDir = SafeNormalize(_iceVelocity, inputDirection.normalized);
            }
            else
            {
                currentDir = inputDirection.normalized;
            }
            
            targetDir = inputDirection.normalized;
            
            // 입력 방향으로 서서히 전환 (낮은 가속률)
            Vector3 newDir = Vector3.Lerp(currentDir, targetDir, _currentIceAccelerationRate * Time.deltaTime);
            
            // 추가 안전 검사
            if (newDir.magnitude < 0.001f)
            {
                newDir = targetDir;
            }
            
            // 현재 속도에 약간의 가속/감속 적용
            float targetSpeed = _isSprinting ? _sprintSpeed : (_isWalking ? _walkSpeed : _runSpeed);
            targetSpeed *= _currentIceSpeedMultiplier; // 속도 배율 적용
            
            _currentIceSpeed = Mathf.Lerp(_currentIceSpeed, targetSpeed, _currentIceAccelerationRate * 0.5f * Time.deltaTime);
            
            // 새 속도 계산
            _iceVelocity = newDir * _currentIceSpeed;
        }
        // 입력이 없을 때 (관성으로 미끄러짐)
        else
        {
            // 현재 방향 유지하면서 서서히 감속
            _currentIceSpeed = Mathf.Lerp(_currentIceSpeed, 0, _currentIceSlowdownRate * Time.deltaTime);
            
            // 속도가 매우 작아지면 완전히 정지
            if (_currentIceSpeed < 0.1f)
            {
                _currentIceSpeed = 0f;
                _iceVelocity = Vector3.zero;
            }
            else if (_iceVelocity.magnitude > 0.001f)
            {
                Vector3 iceDir = SafeNormalize(_iceVelocity, transform.forward);
                _iceVelocity = iceDir * _currentIceSpeed;
            }
            else
            {
                _iceVelocity = Vector3.zero;
            }
        }
        
        // 이동 방향과 목표 속도 설정
        if (_iceVelocity.magnitude > 0.001f)
        {
            _moveDirection = SafeNormalize(_iceVelocity, transform.forward);
        }
        else
        {
            _moveDirection = transform.forward;
        }
        
        _targetVelocity = _iceVelocity;
        
        // 현재 최대 속도 업데이트 (애니메이션 등에 사용)
        _currentMaxSpeed = _currentIceSpeed;
    }

    /// <summary>
    /// 플레이어에게 중력을 적용합니다
    /// </summary>
    public void ApplyGravity()
    {
        // 중력 배수 결정
        float gravityMultiplier = _gravityMultiplier;

        // 미끄럼틀 함정이 우선
        if (_isObstacleSliding)
        {
            gravityMultiplier = _obstacleSlideGravityMultiplier;
        }
        // 그 다음 자연 경사면 슬립
        else if (_isSlipping)
        {
            gravityMultiplier = _slipGravityMultiplier;
        }

        // 리지드바디 속도에 중력 직접 적용 (Time.deltaTime 사용)
        Vector3 gravityVelocity = Physics.gravity * gravityMultiplier * Time.deltaTime;
        Vector3 currentVelocity = _rigidbody.velocity;
        currentVelocity.y += gravityVelocity.y;
        _rigidbody.velocity = currentVelocity;

        // 지면에 있지 않을 때만 낙하 지속 시간 업데이트
        if (!_isGrounded)
        {
            UpdateFallingDuration();
        }
        // 지면에 있을 때는 일정 시간이 지난 후에만 낙하 지속 시간 초기화
        // 이렇게 하면 착지 애니메이션이 재생될 시간을 확보할 수 있음
        else if (_fallingDuration > 0f)
        {
            // 낙하 지속 시간을 바로 초기화하지 않고 애니메이터가 값을 사용할 수 있도록 유지
            // 다음 프레임에서 PlayerStateController가 상태를 변경할 때 ResetFallingDuration()이 호출됨
        }
    }

    /// <summary>
    /// 플레이어가 이동 방향을 바라보도록 합니다
    /// </summary>
    public void FaceMoveDirection()
    {
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
        // 쿨타임 중이면 점프할 수 없음
        if (_isJumpOnCooldown)
        {
            if (_isJumpOnCooldown)
            {
                Debug.Log($"점프 쿨타임 중: {_jumpCooldownTimer:F1}초 남음");
            }
            return;
        }

        // 지면에 있거나 코요테 타임 중이면 점프 가능
        if (_isGrounded || _canCoyoteJump)
        {
            _isGrounded = false;
            _canCoyoteJump = false;
            jumpRequest = true;
            
            // 쿨타임 시작
            _isJumpOnCooldown = true;
            _jumpCooldownTimer = _jumpCooldown;
            Debug.Log($"점프 실행! 쿨타임 {_jumpCooldown}초 시작");
        }
    }

    public void ExternalJump(Vector3 velocity)
    {
        // 기존 y속도를 새로운 값으로 대체 (XZ도 원하는 경우엔 같이 적용)
        Vector3 v = _rigidbody.velocity;
        v.y = velocity.y;
        // 만약 XZ도 통째로 덮고 싶으면 아래처럼:
        // v.x = velocity.x;
        // v.z = velocity.z;
        _rigidbody.velocity = v;

        _isGrounded = false;
        jumpRequest = false;
        // 필요하면 점프 애니메이션 등도 여기서 처리
    }

    /// <summary>
    /// 경사면 제한 기능을 적용합니다. 플레이어가 너무 가파른 경사면을 올라가지 못하게 합니다.
    /// </summary>
    private bool SlopeLimit()
    {
        // 지면 법선 벡터와 위쪽 벡터 사이의 각도 계산
        float slopeAngle = Vector3.Angle(_groundNormal, Vector3.up);

        // 현재 충돌한 오브젝트의 레이어 확인
        int hitLayer = _groundHit.collider.gameObject.layer;

        // 레이어에 따라 다른 경사각 제한 적용
        float currentSlopeLimit = ((1 << hitLayer) & _obstacleLayerMask.value) != 0 ? _slopeLimit : _nonObstacleSlopeLimit;

        // 경사각이 제한 각도보다 크면 경사면 제한 적용
        if (slopeAngle > currentSlopeLimit)
        {
            // 이동 방향 계산
            Vector3 absoluteMoveDirection = Vector3.ProjectOnPlane(_groundNormal, transform.position - _initialPosition);

            // 경사면 아래쪽을 가리키는 벡터 계산
            Vector3 crossVector = Vector3.Cross(_groundNormal, Vector3.down);
            Vector3 downSlopeDirection = Vector3.Cross(crossVector, _groundNormal);

            // 플레이어가 경사면을 올라가려고 하는지 확인
            float angle = Vector3.Angle(absoluteMoveDirection, downSlopeDirection);

            // 플레이어가 경사면을 내려가려고 하면 제한하지 않음
            if (angle <= 90.0f)
            {
                return false;
            }

            // 경사면 위나 아래에 플레이어를 배치할 위치 계산
            Vector3 resolvedPosition = ProjectPointOnLine(_initialPosition, crossVector, transform.position);
            Vector3 direction = Vector3.ProjectOnPlane(_groundNormal, resolvedPosition - transform.position);

            // 해결된 위치로 가는 경로가 다른 콜라이더에 의해 막혀 있는지 확인
            if (Physics.CapsuleCast(
                GetBottomCapsulePoint(),
                GetTopCapsulePoint(),
                _capsuleCollider.radius,
                direction.normalized,
                out RaycastHit hit,
                direction.magnitude,
                _groundLayerMask,
                QueryTriggerInteraction.Ignore))
            {
                // 충돌 지점까지만 이동
                transform.position += downSlopeDirection.normalized * hit.distance;
            }
            else
            {
                // 계산된 위치로 이동
                transform.position += direction;
            }

            // 경사면 제한이 적용되었음을 반환
            return true;
        }

        // 경사면 제한이 적용되지 않았음을 반환
        return false;
    }

    /// <summary>
    /// 캡슐 콜라이더의 하단 지점을 반환합니다.
    /// </summary>
    private Vector3 GetBottomCapsulePoint()
    {
        return transform.position +
               transform.up * (_capsuleCollider.center.y - _capsuleCollider.height / 2 + _capsuleCollider.radius);
    }

    /// <summary>
    /// 캡슐 콜라이더의 상단 지점을 반환합니다.
    /// </summary>
    private Vector3 GetTopCapsulePoint()
    {
        return transform.position +
               transform.up * (_capsuleCollider.center.y + _capsuleCollider.height / 2 - _capsuleCollider.radius);
    }

    /// <summary>
    /// 점을 선에 투영합니다. (Math3d.ProjectPointOnLine 대체)
    /// </summary>
    private Vector3 ProjectPointOnLine(Vector3 linePoint, Vector3 lineVec, Vector3 point)
    {
        Vector3 linePointToPoint = point - linePoint;
        float t = Vector3.Dot(linePointToPoint, lineVec) / Vector3.Dot(lineVec, lineVec);
        return linePoint + lineVec * t;
    }

    #endregion

    #region 상태 확인 메서드
    
    /// <summary>
    /// 플레이어가 빙판 위에 있는지 확인합니다
    /// </summary>
    private void CheckIceSurface()
    {
        // 박스캐스트로 빙판 확인 (지면 확인과 유사한 방식)
        Vector3 boxCenter = _capsuleCollider.bounds.center;
        Vector3 boxHalfExtents = new Vector3(_boxCastWidth / 2f, 0.05f, _boxCastDepth / 2f);
        Quaternion orientation = transform.rotation;
        float distance = _capsuleCollider.height / 2 + _groundCheckDistance;

        // 빙판 확인
        bool wasOnIce = _isOnIce;
        bool isOnIceNow = Physics.BoxCast(
            boxCenter,
            boxHalfExtents,
            Vector3.down,
            out RaycastHit iceHit,
            orientation,
            distance,
            _iceLayerMask,
            QueryTriggerInteraction.Ignore
        );
        
        // 빙판에 처음 진입했을 때
        if (!wasOnIce && isOnIceNow)
        {
            // 빙판 오브젝트에서 속성 가져오기
            GameObject iceObject = iceHit.collider?.gameObject;
            
            EnterIceSurface(iceObject);
            _isOnIce = true;
        }
        // 빙판에서 나갔을 때
        else if (wasOnIce && !isOnIceNow)
        {
            ExitIceSurface();
            _isOnIce = false;
        }
        // 한 빙판에서 다른 빙판으로 이동했을 때
        else if (isOnIceNow && iceHit.collider != null)
        {
            // 빙판 속성 업데이트
            GameObject iceObject = iceHit.collider.gameObject;
            if (iceObject != null)
            {
                UpdateIceProperties(iceObject);
            }
        }
    }

    // 현재 사용 중인 빙판 속성
    private float _currentIceSlowdownRate;
    private float _currentIceAccelerationRate;
    private float _currentIceSpeedMultiplier;
    
    /// <summary>
    /// 빙판에 진입했을 때 호출됩니다
    /// </summary>
    private void EnterIceSurface(GameObject iceObject = null)
    {
        // 현재 속도와 목표 속도 중 더 큰 값을 사용
        Vector3 currentVelocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
        Vector3 targetVelocity = new Vector3(_targetVelocity.x, 0, _targetVelocity.z);
        
        float currentSpeed = currentVelocity.magnitude;
        float targetSpeed = targetVelocity.magnitude;
        
        // 현재 속도와 목표 속도 중 더 큰 값을 사용
        float bestSpeed = Mathf.Max(currentSpeed, targetSpeed);
        
        // _speed2D를 사용하여 최근의 실제 이동 속도를 반영
        bestSpeed = Mathf.Max(bestSpeed, _speed2D);
        
        // 상태에 따라 최소 속도 보장 (방법 2: 스프린트 상태 확인)
        float minimumSpeed = 0f;
        if (_isSprinting)
        {
            minimumSpeed = _sprintSpeed * 0.8f; // 스프린트 속도의 80%
        }
        else if (_isWalking)
        {
            minimumSpeed = _walkSpeed * 0.7f; // 걷기 속도의 70%
        }
        else
        {
            minimumSpeed = _runSpeed * 0.7f; // 달리기 속도의 70%
        }
        
        // 최종 속도 결정 (실제 속도와 최소 보장 속도 중 큰 값)
        _currentIceSpeed = Mathf.Max(bestSpeed, minimumSpeed);
        
        // 방향 설정 (속도가 충분히 있을 때만 현재 방향 사용)
        if (currentSpeed > 0.5f)
        {
            _iceVelocity = currentVelocity.normalized * _currentIceSpeed;
        }
        else if (targetSpeed > 0.5f)
        {
            _iceVelocity = targetVelocity.normalized * _currentIceSpeed;
        }
        else if (_lastInputDirection.magnitude > 0.1f)
        {
            // 입력 방향이 있으면 그 방향 사용
            _iceVelocity = _lastInputDirection * _currentIceSpeed;
        }
        else
        {
            // 모든 속도가 낮으면 플레이어가 바라보는 방향 사용
            _iceVelocity = transform.forward * _currentIceSpeed;
        }
        
        // 빙판 속성 설정
        SetIceProperties(iceObject);
        
        Debug.Log($"빙판 진입: 초기 속도 = {_currentIceSpeed}, 감속 비율 = {_currentIceSlowdownRate}, 방향 = {_iceVelocity.normalized}");
    }

    /// <summary>
    /// 빙판 속성을 설정합니다
    /// </summary>
    private void SetIceProperties(GameObject iceObject)
    {
        // 기본값 설정
        _currentIceSlowdownRate = _iceSlowdownRate;
        _currentIceAccelerationRate = _iceAccelerationRate;
        _currentIceSpeedMultiplier = 1.0f;
        
        if (iceObject != null)
        {
            // IceZoneController 컴포넌트가 있는지 확인
            MonoBehaviour[] components = iceObject.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour component in components)
            {
                string typeName = component.GetType().Name;
                if (typeName == "IceZoneController")
                {
                    // 리플렉션을 통해 속성 가져오기
                    try
                    {
                        var slowdownField = component.GetType().GetField("slowdownRate");
                        var accelerationField = component.GetType().GetField("accelerationRate");
                        var speedMultiplierField = component.GetType().GetField("speedMultiplier");
                        
                        if (slowdownField != null)
                            _currentIceSlowdownRate = (float)slowdownField.GetValue(component);
                        
                        if (accelerationField != null)
                            _currentIceAccelerationRate = (float)accelerationField.GetValue(component);
                            
                        if (speedMultiplierField != null)
                            _currentIceSpeedMultiplier = (float)speedMultiplierField.GetValue(component);
                            
                        break;
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"빙판 속성을 가져오는 중 오류 발생: {e.Message}");
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// 빙판 속성을 업데이트합니다
    /// </summary>
    private void UpdateIceProperties(GameObject iceObject)
    {
        if (iceObject != null)
        {
            SetIceProperties(iceObject);
        }
    }

    /// <summary>
    /// 빙판에서 나갔을 때 호출됩니다
    /// </summary>
    private void ExitIceSurface()
    {
        _iceVelocity = Vector3.zero;
        _currentIceSpeed = 0f;
        _currentIceSlowdownRate = _iceSlowdownRate;
        _currentIceAccelerationRate = _iceAccelerationRate;
        _currentIceSpeedMultiplier = 1.0f;
        
        Debug.Log("빙판 이탈");
    }

    /// <summary>
    /// 박스캐스트를 사용하여 지면 확인을 수행합니다
    /// </summary>
    public void GroundedCheck()
    {
        bool wasGrounded = _isGrounded;
        
        // 박스캐스트로 지면 확인
        Vector3 boxCenter = _capsuleCollider.bounds.center;
        Vector3 boxHalfExtents = new Vector3(_boxCastWidth / 2f, 0.05f, _boxCastDepth / 2f);
        Quaternion orientation = transform.rotation;
        float distance = _capsuleCollider.height / 2 + _groundCheckDistance;

        // 지면 확인 (바로 아래)
        bool groundHit = Physics.BoxCast(
            boxCenter,
            boxHalfExtents,
            Vector3.down,
            out _groundHit,
            orientation,
            distance,
            _groundLayerMask,
            QueryTriggerInteraction.Ignore
        );

        if (groundHit)
        {
            _groundNormal = _groundHit.normal;
            _isGrounded = true;
            _canCoyoteJump = true;
            _coyoteTimeCounter = _coyoteTimeThreshold;

            // 지면에 있을 때 경사 확인
            GroundInclineCheck();

            // 미끄러운 경사면 확인
            CheckSlipperySlope();
        }
        else
        {
            // 지면에서 막 떨어졌을 때 코요테 타임 시작
            if (wasGrounded)
            {
                _canCoyoteJump = true;
                _coyoteTimeCounter = _coyoteTimeThreshold;
            }
            
            // 일정 시간 후에 지면 상태 해제 (약간의 지연으로 계단 등에서 자연스러운 이동)
            _isGrounded = false;

            //지면에 없으면 미끄러짐 상태 해제
            if(_isSlipping)
            {
                DeactivateSlipping();
            }
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

     /// <summary>
    /// 미끄러운 경사면을 확인하고 필요시 미끄러짐 상태를 활성화합니다
    /// </summary>
    private void CheckSlipperySlope()
    {
        // 지면 법선 벡터와 위쪽 벡터 사이의 각도 계산
        float slopeAngle = Vector3.Angle(_groundNormal, Vector3.up);

        // 현재 충돌한 오브젝트의 레이어 확인
        int hitLayer = _groundHit.collider.gameObject.layer;

        // 레이어에 따라 다른 미끄러짐 각도 적용
        float currentSlipAngle = ((1 << hitLayer) & _obstacleLayerMask.value) != 0 ? _slipAngle : _nonObstacleSlopeLimit;

        // 경사각이 미끄러짐 각도보다 크면 미끄러짐 활성화
        if (slopeAngle > currentSlipAngle)
        {
            // 경사면 아래쪽을 가리키는 벡터 계산
            Vector3 crossVector = Vector3.Cross(_groundNormal, Vector3.down);
            Vector3 downSlopeDirection = Vector3.Cross(crossVector, _groundNormal);

            // 미끄러짐 상태가 아니면 활성화
            if (!_isSlipping)
            {
                ActivateSlipping(downSlopeDirection, _slipSpeed, 1f);
            }
            // 이미 미끄러짐 상태라면 방향만 업데이트
            else
            {
                _slipDirection = downSlopeDirection.normalized;
            }
        }
        // 경사각이 미끄러짐 각도보다 작으면 미끄러짐 비활성화
        else if (_isSlipping)
        {
            DeactivateSlipping();
        }
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
    /// 슬립 상태를 활성화합니다
    /// </summary>
    /// <param name="direction">슬립 방향</param>
    /// <param name="force">슬립 힘</param>
    /// <param name="gravityMultiplier">중력 배수</param>
    /// <param name="inputReduction">입력 감소 정도 (0~1)</param>
    public void ActivateSlipping(Vector3 direction, float force, float gravityMultiplier, float inputReduction = 0.8f)
    {
        _isSlipping = true;
        _slipDirection = direction.normalized;
        _slipForce = force;
        _slipGravityMultiplier = gravityMultiplier;
        _slipInputReduction = Mathf.Clamp01(inputReduction);

        Debug.Log($"슬립 활성화: 방향={_slipDirection}, 힘={_slipForce}, 중력배수={_slipGravityMultiplier}");
    }

    /// <summary>
    /// 슬립 상태를 비활성화합니다
    /// </summary>
    public void DeactivateSlipping()
    {
        _isSlipping = false;
        _slipDirection = Vector3.zero;
        _slipForce = 0f;
        _slipGravityMultiplier = 0f;
        _slipInputReduction = 0f;

        Debug.Log("슬립 비활성화");
    }

    /// <summary>
    /// 슬립 속도를 업데이트합니다 (DOTween에서 호출)
    /// </summary>
    /// <param name="newSpeed">새로운 슬립 속도</param>
    public void UpdateSlideSpeed(float newSpeed)
    {
        if (_isSlipping)
        {
            _slipForce = newSpeed;
            Debug.Log($"슬립 속도 업데이트: {_slipForce}");
        }
    }

    /// <summary>
    /// 플레이어의 캡슐 크기를 조정합니다
    /// </summary>
    /// <param name="crouching">플레이어가 웅크리고 있는지 여부</param>
    private void CapsuleCrouchingSize(bool crouching)
    {
        if (crouching)
        {
            _capsuleCollider.height = _capsuleCrouchingHeight;
            _capsuleCollider.center = new Vector3(0f, _capsuleCrouchingCentre, 0f);
        }
        else
        {
            _capsuleCollider.height = _capsuleStandingHeight;
            _capsuleCollider.center = new Vector3(0f, _capsuleStandingCentre, 0f);
        }
    }

    #endregion
/// <summary>
    /// 미끄럼틀 함정에서의 슬라이드 상태를 활성화합니다
    /// </summary>
    /// <param name="direction">슬라이드 방향</param>
    /// <param name="force">슬라이드 힘</param>
    /// <param name="gravityMultiplier">중력 배수</param>
    /// <param name="inputReduction">입력 감소 정도 (0~1)</param>
    public void ActivateObstacleSlide(Vector3 direction, float force, float gravityMultiplier, float inputReduction)
    {
        _isObstacleSliding = true;
        _obstacleSlideDirection = direction.normalized;
        _obstacleSlideForce = force;
        _obstacleSlideGravityMultiplier = gravityMultiplier;
        _obstacleSlideInputReduction = Mathf.Clamp01(inputReduction);

        // 자연 경사면 슬립은 비활성화 (미끄럼틀이 우선)
        if (_isSlipping)
        {
            _isSlipping = false;
        }

        Debug.Log($"미끄럼틀 슬라이드 활성화: 방향={_obstacleSlideDirection}, 힘={_obstacleSlideForce}, 중력배수={_obstacleSlideGravityMultiplier}, 입력감소={_obstacleSlideInputReduction}");
    }

    /// <summary>
    /// 미끄럼틀 함정에서의 슬라이드 상태를 비활성화합니다
    /// </summary>
    public void DeactivateObstacleSlide()
    {
        _isObstacleSliding = false;
        _obstacleSlideDirection = Vector3.zero;
        _obstacleSlideForce = 0f;
        _obstacleSlideGravityMultiplier = 0f;
        _obstacleSlideInputReduction = 0f;

        Debug.Log("미끄럼틀 슬라이드 비활성화");
    }

    /// <summary>
    /// 미끄럼틀 슬라이드 속도를 업데이트합니다 (DOTween에서 호출)
    /// </summary>
    /// <param name="newSpeed">새로운 슬라이드 속도</param>
    public void UpdateObstacleSlideSpeed(float newSpeed)
    {
        if (_isObstacleSliding)
        {
            _obstacleSlideForce = newSpeed;
            Debug.Log($"미끄럼틀 슬라이드 속도 업데이트: {_obstacleSlideForce}");
        }
    }
    
    /// <summary>
    /// 슬로우존 진입 시 이동 속도 감소
    /// </summary>
    /// <param name="multiplier">속도 감소 배율 (0.1~1.0)</param>
    /// <param name="restoreDuration">복구 시간(초)</param>
    public void EnterSlowZone(float multiplier, float restoreDuration)
    {
        _slowZoneCount++;
        // 첫 진입이거나 더 강한 슬로우일 때만 적용
        if (_slowZoneCount == 1 || multiplier < _currentSlowMultiplier)
        {
            _currentSlowMultiplier = multiplier;
            _runSpeed = _baseRunSpeed * _currentSlowMultiplier;
            _sprintSpeed = _baseSprintSpeed * _currentSlowMultiplier;
            _isRestoringSpeed = false;
        }
        _restoreDuration = restoreDuration;
    }

    /// <summary>
    /// 슬로우존 이탈 시 속도 복구 시작
    /// </summary>
    public void ExitSlowZone()
    {
        _slowZoneCount = Mathf.Max(0, _slowZoneCount - 1);
        if (_slowZoneCount == 0)
        {
            _isRestoringSpeed = true;
            _restoreTimer = 0f;
        }
    }


    // 디버그 시각화를 위한 메서드
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || _capsuleCollider == null) return;

        // 박스캐스트 시각화
        Vector3 boxCenter = _capsuleCollider.bounds.center;
        Vector3 boxHalfExtents = new Vector3(_boxCastWidth / 2f, 0.05f, _boxCastDepth / 2f);
        Vector3 endPosition = boxCenter + Vector3.down * (_capsuleCollider.height / 2 + _groundCheckDistance);

        // 기본 지면 확인 박스
        Gizmos.color = _isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireCube(boxCenter, boxHalfExtents * 2);
        Gizmos.DrawWireCube(endPosition, boxHalfExtents * 2);
        Gizmos.DrawLine(boxCenter, endPosition);
    }

    /// <summary>
    /// 점프 쿨타임을 업데이트합니다
    /// </summary>
    private void UpdateJumpCooldown()
    {
        if (_isJumpOnCooldown)
        {
            _jumpCooldownTimer -= Time.deltaTime;
            
            if (_jumpCooldownTimer <= 0f)
            {
                _isJumpOnCooldown = false;
                _jumpCooldownTimer = 0f;
            }
        }
    }

    /// <summary>
    /// 점프 입력을 받았을 때 호출되는 함수
    /// </summary>
    private void OnJumpInput()
    {
        // 지면에 있거나 코요테 타임 중이고 쿨다운이 아니면 점프 실행
        if ((_isGrounded || _canCoyoteJump) && !_isJumpOnCooldown)
        {
            Jump();
        }
    }
}
