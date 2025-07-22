using UnityEngine;

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
    [SerializeField] private float _groundCheckDistance = 0.2f;

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
    [SerializeField] private float _boxCastWidth = 0.4f;
    [Tooltip("박스캐스트 깊이 (z축)")]
    [SerializeField] private float _boxCastDepth = 0.4f;

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

    public Rigidbody Rigidbody => _rigidbody;

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



    private void Update()
    {
        // 입력 처리 및 방향 계산은 Update에서 처리 (더 부드러운 응답성)
        CalculateMoveDirection();

        // 지면에 있을 때만 회전 적용 (시각적 부드러움을 위해)
        if (_isGrounded)
        {
            FaceMoveDirection();
        }
    }

    private void FixedUpdate()
    {
        //현재 위치 저장 (경사면 제한을 위해)
        _initialPosition = transform.position;

        // 물리 기반 처리는 FixedUpdate에서 유지
        GroundedCheck();

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
    /// 플레이어의 이동을 처리합니다
    /// </summary>
    public void Move()
    {
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
        if (_isGrounded)
        {
            // 속도를 직접 설정하여 프레임 레이트에 독립적인 일관된 점프 구현
            // Vector3 velocity = _rigidbody.velocity;
            // velocity.y = _jumpForce;
            // _rigidbody.velocity = velocity;

             _isGrounded = false;
            jumpRequest = true;
        }
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
    /// 박스캐스트를 사용하여 지면 확인을 수행합니다
    /// </summary>
    public void GroundedCheck()
    {
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

            // 지면에 있을 때 경사 확인
            GroundInclineCheck();

            // 미끄러운 경사면 확인
            CheckSlipperySlope();
        }
        else
        {
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
}
