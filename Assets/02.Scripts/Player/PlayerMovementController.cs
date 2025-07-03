using UnityEngine;
using System.Collections;

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
    [SerializeField] private float _rotationSmoothing = 10f;
    [Tooltip("카메라 회전 오프셋")]
    [SerializeField] private float _cameraRotationOffset;

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
    [Tooltip("지면 체크 거리")]
    [SerializeField] private float _groundCheckDistance = 0.2f;

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
    
    // 슬립 관련 변수 추가
    private bool _isSlipping;
    private Vector3 _slipDirection;
    private float _slipForce;
    private float _slipGravityMultiplier;
    private float _slipInputReduction;
    
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

    /// <summary>
    /// 플레이어가 슬립 중인지 여부
    /// </summary>
    public bool IsSlipping => _isSlipping;

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
        
        // Rigidbody 설정
        ConfigureRigidbody();
    }
    
    private void ConfigureRigidbody()
    {
        if (_rigidbody != null)
        {
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
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
    
    private void FixedUpdate()
    {
        // 지면 체크
        GroundedCheck();
        
        // 이동 방향 계산
        CalculateMoveDirection();
        
        // 이동 적용
        Move();
        
        // 회전 적용
        FaceMoveDirection();
        
        // 낙하 지속 시간 업데이트
        if (!_isGrounded)
        {
            UpdateFallingDuration();
        }
        else
        {
            ResetFallingDuration();
        }
        
        // 천장 높이 체크 (웅크리기 관련)
        if (_isCrouching)
        {
            CeilingHeightCheck();
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        // 지면과 충돌 시 추가 처리
        if (IsGroundLayer(collision.gameObject.layer))
        {
            _isGrounded = true;
            ResetFallingDuration();
        }
    }
    
    private void OnCollisionExit(Collision collision)
    {
        // 지면과 분리 시 추가 처리
        if (IsGroundLayer(collision.gameObject.layer))
        {
            StartCoroutine(CheckGroundedWithDelay(0.1f));
        }
    }
    
    private IEnumerator CheckGroundedWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        GroundedCheck();
    }
    
    private bool IsGroundLayer(int layer)
    {
        return ((1 << layer) & _groundLayerMask) != 0;
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
        // 리지드바디를 사용한 이동 처리
        Vector3 moveVelocity = new Vector3(_velocity.x, _rigidbody.velocity.y, _velocity.z);
        
        // 슬립 중이거나 지면에 있지 않은 경우 중력 적용
        if (_isSlipping || !_isGrounded)
        {
            // 현재 수직 속도 유지
            moveVelocity.y = _rigidbody.velocity.y;
        }
        
        _rigidbody.velocity = moveVelocity;
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

        // 슬립 중인지 확인하여 이동 방향 결정
        if (_isSlipping)
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
        // 슬립 중인 경우 중력 배수 적용
        float gravityMultiplier = _isSlipping ? 
            _gravityMultiplier * _slipGravityMultiplier : 
            _gravityMultiplier;
            
        // 리지드바디는 자체적으로 중력을 적용하므로 추가 중력만 적용
        if (!_isGrounded)
        {
            Vector3 extraGravity = Physics.gravity * (gravityMultiplier - 1f);
            _rigidbody.AddForce(extraGravity, ForceMode.Acceleration);
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
            _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0f, _rigidbody.velocity.z);
            _rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
            _isGrounded = false;
        }
    }

    #endregion

    #region 상태 확인 메서드

    /// <summary>
    /// 플레이어가 지면에 있는지 확인합니다
    /// </summary>
    public void GroundedCheck()
    {
        // 레이캐스트로 지면 체크
        Vector3 spherePosition = new Vector3(
            transform.position.x,
            transform.position.y - _groundedOffset,
            transform.position.z
        );
        
        bool wasGrounded = _isGrounded;
        _isGrounded = Physics.CheckSphere(spherePosition, _capsuleCollider.radius, _groundLayerMask, QueryTriggerInteraction.Ignore);

        if (_isGrounded && !wasGrounded)
        {
            // 지면에 착지
            ResetFallingDuration();
        }
        else if (!_isGrounded && wasGrounded)
        {
            // 지면에서 떨어짐
            _fallStartTime = Time.time;
        }

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
} 