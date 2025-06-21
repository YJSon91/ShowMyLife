using UnityEngine;
using System.Collections;

/// <summary>
/// 플레이어 캐릭터의 이동을 담당하는 컨트롤러
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("이동 설정")]
    [Tooltip("이동 속도")]
    [SerializeField] private float _moveSpeed = 5f;
    [Tooltip("달리기 속도 배율")]
    [SerializeField] private float _sprintMultiplier = 2f;
    [Tooltip("회전 속도")]
    [SerializeField] private float _rotationSpeed = 10f;
    [Tooltip("점프 힘")]
    [SerializeField] private float _jumpForce = 8f;  // 점프력 증가
    [Tooltip("중력 크기")]
    [SerializeField] private float _gravityMagnitude = 9.8f;
    [Tooltip("이동 가속도")]
    [SerializeField] private float _acceleration = 10f;
    [Tooltip("이동 감속도")]
    [SerializeField] private float _deceleration = 8f;
    [Tooltip("공중 이동 제어 계수 (0-1)")]
    [SerializeField] private float _airControlFactor = 0.3f;

    [Header("지면 감지")]
    [Tooltip("지면 체크 거리")]
    [SerializeField] private float _groundCheckDistance = 0.2f;
    [Tooltip("지면 레이어")]
    [SerializeField] private LayerMask _groundLayer;
    [Tooltip("경사 제한 각도")]
    [SerializeField] private float _slopeLimit = 45f;
    [Tooltip("스킨 너비 (충돌 오프셋)")]
    [SerializeField] private float _skinWidth = 0.08f;

    [Header("충돌 설정")]
    [Tooltip("충돌 감지 레이어")]
    [SerializeField] private LayerMask _collisionLayer;
    [Tooltip("최대 충돌 감지 수")]
    [SerializeField] private int _maxCollisionCount = 5;
    [Tooltip("벽 충돌 반동 계수")]
    [SerializeField] private float _wallBounceModifier = 0.1f;

    [Header("애니메이션 설정")]
    [Tooltip("랜덤 Idle 애니메이션 최소 대기 시간")]
    [SerializeField] private float _minIdleTime = 5f;
    [Tooltip("랜덤 Idle 애니메이션 최대 대기 시간")]
    [SerializeField] private float _maxIdleTime = 10f;

    // 내부 변수
    private Vector3 _moveDirection;
    private Vector3 _velocity;
    private Vector3 _externalForce;
    private bool _isGrounded;
    private bool _isSprinting;
    private bool _wasGrounded;
    private float _verticalVelocity;
    private Rigidbody _rigidbody;
    private PlayerInput _playerInput;
    private Transform _cameraTransform;
    private Vector3 _groundNormal;
    private RaycastHit _groundRaycastHit;
    private RaycastHit[] _raycastHits;
    private Collider[] _colliders;
    private int _colliderCount;
    private float _height;
    private float _radius;
    private Vector3 _center;
    private bool _isGroundCheckDisabled;
    private float _groundCheckDisabledTimer;
    
    // 애니메이션 관련 변수
    private Animator _animator;
    private float _idleAnimTimer;
    private bool _isMoving;
    private static readonly int Walking = Animator.StringToHash("Walking");
    private static readonly int Running = Animator.StringToHash("Running");
    private static readonly int RandomIdle = Animator.StringToHash("RandomIdle");

    // 프로퍼티
    public bool IsGrounded => _isGrounded;
    public Vector3 Velocity => _velocity;
    public bool IsSprinting => _isSprinting;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _playerInput = GetComponent<PlayerInput>();
        _animator = GetComponent<Animator>();
        _cameraTransform = Camera.main.transform;
        _raycastHits = new RaycastHit[_maxCollisionCount];
        
        // Rigidbody 설정
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotation; // 회전은 직접 제어하기 위해 고정
        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _rigidbody.useGravity = false; // 중력을 직접 처리
        _rigidbody.isKinematic = false; // 물리 작용을 위해 키네마틱 해제
        
        // 콜라이더 정보 캐싱
        _colliders = GetComponentsInChildren<Collider>();
        _colliderCount = _colliders.Length;
        
        // 캐릭터 크기 계산
        CalculateCharacterDimensions();
        
        // 랜덤 Idle 타이머 초기화
        ResetIdleTimer();
    }

    private void Update()
    {
        // 입력 처리
        HandleInput();
        
        // 애니메이션 업데이트
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        // 지면 체크 타이머 업데이트
        UpdateGroundCheckTimer();
        
        // 지면 체크
        CheckGround();
        
        // 이동 처리
        HandleMovement();
        
        // 중력 및 점프 처리
        HandleGravityAndJump();
        
        // 충돌 감지 및 처리
        HandleCollisions();
        
        // 최종 이동 적용
        ApplyMovement();
    }
    
    /// <summary>
    /// 지면 체크 비활성화 타이머 업데이트
    /// </summary>
    private void UpdateGroundCheckTimer()
    {
        if (_isGroundCheckDisabled)
        {
            _groundCheckDisabledTimer -= Time.fixedDeltaTime;
            
            if (_groundCheckDisabledTimer <= 0)
            {
                _isGroundCheckDisabled = false;
            }
        }
    }

    /// <summary>
    /// 캐릭터 크기 계산
    /// </summary>
    private void CalculateCharacterDimensions()
    {
        if (_colliderCount == 0) return;
        
        // 캐릭터의 높이, 반지름, 중심점 계산
        Bounds bounds = new Bounds(transform.position, Vector3.zero);
        foreach (Collider col in _colliders)
        {
            bounds.Encapsulate(col.bounds);
        }
        
        _height = bounds.size.y;
        _radius = Mathf.Max(bounds.extents.x, bounds.extents.z);
        _center = bounds.center - transform.position;
    }

    /// <summary>
    /// 입력 처리
    /// </summary>
    private void HandleInput()
    {
        // WASD 입력 받기
        float horizontalInput = _playerInput.GetHorizontalInput();
        float verticalInput = _playerInput.GetVerticalInput();
        
        // 입력값 처리
        
        // 입력이 있는 경우에만 처리
        if (Mathf.Abs(horizontalInput) > 0.01f || Mathf.Abs(verticalInput) > 0.01f)
        {
            // 카메라 기준으로 이동 방향 계산
            Vector3 forward = _cameraTransform ? _cameraTransform.forward : transform.forward;
            Vector3 right = _cameraTransform ? _cameraTransform.right : transform.right;
            
            // y축 영향 제거 (수평 이동만 처리)
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();
            
            // 최종 이동 방향 계산
            _moveDirection = (forward * verticalInput + right * horizontalInput).normalized;
            _isMoving = true;
        }
        else
        {
            _moveDirection = Vector3.zero;
            _isMoving = false;
        }
        
        // 달리기 입력 처리
        _isSprinting = _playerInput.GetSprintInput();
        
        // 최종 이동 방향 설정 완료
    }
    
    /// <summary>
    /// 애니메이션 상태 업데이트
    /// </summary>
    private void UpdateAnimations()
    {
        if (_animator == null) return;
        
        // 이동 애니메이션 설정
        _animator.SetBool(Walking, _isMoving && !_isSprinting);
        _animator.SetBool(Running, _isMoving && _isSprinting);
        
        // 랜덤 Idle 애니메이션 처리
        if (!_isMoving)
        {
            UpdateIdleAnimation();
        }
        else
        {
            // 이동 중이면 타이머 리셋
            ResetIdleTimer();
        }
    }
    
    /// <summary>
    /// Idle 애니메이션 업데이트
    /// </summary>
    private void UpdateIdleAnimation()
    {
        _idleAnimTimer -= Time.deltaTime;
        
        if (_idleAnimTimer <= 0)
        {
            // 랜덤 Idle 애니메이션 재생
            TriggerRandomIdleAnimation();
            
            // 타이머 리셋
            ResetIdleTimer();
        }
    }
    
    /// <summary>
    /// 랜덤 Idle 애니메이션 트리거
    /// </summary>
    private void TriggerRandomIdleAnimation()
    {
        // 랜덤 값 생성 (1: idle2, 2: idle3)
        int randomIdle = Random.Range(1, 3);
        
        // 애니메이터 파라미터 설정
        _animator.SetInteger(RandomIdle, randomIdle);
    }
    
    /// <summary>
    /// Idle 타이머 리셋
    /// </summary>
    private void ResetIdleTimer()
    {
        _idleAnimTimer = Random.Range(_minIdleTime, _maxIdleTime);
    }

    /// <summary>
    /// 지면 체크
    /// </summary>
    private void CheckGround()
    {
        _wasGrounded = _isGrounded;
        
        // 지면 체크가 비활성화된 경우 스킵
        if (_isGroundCheckDisabled)
        {
            return;
        }
        
        // 캐릭터의 발 위치 계산 (중심에서 아래로 이동)
        Vector3 footPosition = transform.position + _center - new Vector3(0, _height * 0.48f, 0);
        
        // 발 위치에서 여러 방향으로 레이캐스트 (더 정확한 지면 감지)
        bool hitGround = false;
        float rayDistance = _groundCheckDistance;
        
        // 중앙 레이
        hitGround |= Physics.Raycast(footPosition, Vector3.down, out _groundRaycastHit, rayDistance, _groundLayer);
        
        // 중앙 레이캐스트 완료
        
        // 추가 레이캐스트 (발 주변 여러 지점에서 체크)
        if (!hitGround)
        {
            Vector3[] checkPoints = new Vector3[]
            {
                footPosition + new Vector3(_radius * 0.5f, 0, 0),
                footPosition + new Vector3(-_radius * 0.5f, 0, 0),
                footPosition + new Vector3(0, 0, _radius * 0.5f),
                footPosition + new Vector3(0, 0, -_radius * 0.5f)
            };
            
            foreach (Vector3 point in checkPoints)
            {
                if (Physics.Raycast(point, Vector3.down, out _groundRaycastHit, rayDistance, _groundLayer))
                {
                    hitGround = true;
                    // 지면 감지 성공
                    break;
                }
                else
                {
                    // 지면 감지 실패
                }
            }
        }
        
        _isGrounded = hitGround;
        
        // 지면 노말 저장
        if (_isGrounded)
        {
            _groundNormal = _groundRaycastHit.normal;
            
            // 경사 제한 체크
            float slopeAngle = Vector3.Angle(Vector3.up, _groundNormal);
            if (slopeAngle > _slopeLimit)
            {
                _isGrounded = false;
            }
        }
        else
        {
            _groundNormal = Vector3.up;
        }
        
        // 지면에 착지한 경우
        if (!_wasGrounded && _isGrounded)
        {
            // 착지 이벤트 발생
            // 착지 효과 등을 여기에 추가
        }
    }

    /// <summary>
    /// 이동 처리
    /// </summary>
    private void HandleMovement()
    {
        // 현재 속도 계산 (y축 제외)
        Vector3 currentVelocity = new Vector3(_velocity.x, 0, _velocity.z);
        
        // 목표 속도 계산
        Vector3 targetVelocity = Vector3.zero;
        if (_moveDirection.magnitude > 0.1f)
        {
            // 이동 속도 계산 (달리기 적용)
            float currentSpeed = _moveSpeed;
            if (_isSprinting)
            {
                currentSpeed *= _sprintMultiplier;
            }
            
            targetVelocity = _moveDirection * currentSpeed;
            
            // 목표 속도 설정 완료
        }
        
        // 가속/감속 계수 (공중에서는 제어 감소)
        float accelerationFactor = _isGrounded ? 1.0f : _airControlFactor;
        float acceleration = _moveDirection.magnitude > 0.1f ? _acceleration : _deceleration;
        
        // 속도 보간 - 더 빠른 응답성을 위해 보간 계수 증가
        currentVelocity = Vector3.Lerp(
            currentVelocity, 
            targetVelocity, 
            acceleration * accelerationFactor * Time.fixedDeltaTime * 2f // 응답성 향상
        );
        
        // 경사면에서의 속도 조정
        if (_isGrounded && Vector3.Angle(Vector3.up, _groundNormal) > 0)
        {
            // 경사면에 투영된 속도 계산
            currentVelocity = Vector3.ProjectOnPlane(currentVelocity, _groundNormal);
        }
        
        // 최종 속도 업데이트 (y축 유지)
        _velocity.x = currentVelocity.x;
        _velocity.z = currentVelocity.z;
        
        // 이동 방향으로 회전
        if (_moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.fixedDeltaTime);
        }
    }

    /// <summary>
    /// 중력 및 점프 처리
    /// </summary>
    private void HandleGravityAndJump()
    {
        // 지면에 있을 때
        if (_isGrounded)
        {
            // 지면에 있을 때는 약간의 중력만 적용
            _velocity.y = -0.5f;
            
            // 점프 입력 처리
            if (_playerInput.GetJumpInput())
            {
                _velocity.y = _jumpForce;
                _isGrounded = false;
                // 점프 시 지면 체크를 잠시 비활성화
                DisableGroundCheck(0.2f);
            }
        }
        else
        {
            // 공중에 있을 때는 중력 적용 (중력 감소)
            _velocity.y -= _gravityMagnitude * 0.8f * Time.fixedDeltaTime;
            
            // 최대 낙하 속도 제한
            if (_velocity.y < -20f)
            {
                _velocity.y = -20f;
            }
        }
        
        // 중력 적용 완료
    }
    
    /// <summary>
    /// 지면 체크를 일정 시간 동안 비활성화
    /// </summary>
    /// <param name="duration">비활성화 지속 시간(초)</param>
    private void DisableGroundCheck(float duration)
    {
        _isGroundCheckDisabled = true;
        _groundCheckDisabledTimer = duration;
    }

    /// <summary>
    /// 충돌 감지 및 처리
    /// </summary>
    private void HandleCollisions()
    {
        // 수평 방향 충돌 감지
        Vector3 horizontalMovement = new Vector3(_velocity.x, 0, _velocity.z) * Time.fixedDeltaTime;
        if (horizontalMovement.magnitude > 0)
        {
            // 이동 방향으로 레이캐스트
            Vector3 rayOrigin = transform.position + _center;
            float rayDistance = _radius + horizontalMovement.magnitude + _skinWidth;
            
            // 충돌 감지
            int hitCount = Physics.RaycastNonAlloc(rayOrigin, horizontalMovement.normalized, _raycastHits, rayDistance, _collisionLayer);
            if (hitCount > 0)
            {
                // 가장 가까운 충돌 찾기
                float closestDistance = float.MaxValue;
                int closestIndex = -1;
                
                for (int i = 0; i < hitCount; i++)
                {
                    // 자기 자신의 콜라이더는 무시
                    bool isSelf = false;
                    for (int j = 0; j < _colliderCount; j++)
                    {
                        if (_raycastHits[i].collider == _colliders[j])
                        {
                            isSelf = true;
                            break;
                        }
                    }
                    
                    if (!isSelf && _raycastHits[i].distance < closestDistance)
                    {
                        closestDistance = _raycastHits[i].distance;
                        closestIndex = i;
                    }
                }
                
                // 충돌 처리
                if (closestIndex >= 0)
                {
                    // 충돌 노말을 기준으로 속도 조정
                    Vector3 normal = _raycastHits[closestIndex].normal;
                    Vector3 reflection = Vector3.Reflect(horizontalMovement.normalized, normal);
                    
                    // 벽 반동 적용
                    _velocity.x += reflection.x * _wallBounceModifier;
                    _velocity.z += reflection.z * _wallBounceModifier;
                    
                    // 벽을 따라 미끄러지도록 속도 조정
                    Vector3 deflection = Vector3.ProjectOnPlane(horizontalMovement, normal).normalized;
                    _velocity.x = deflection.x * _velocity.magnitude * (1 - _wallBounceModifier);
                    _velocity.z = deflection.z * _velocity.magnitude * (1 - _wallBounceModifier);
                }
            }
        }
        
        // 수직 방향 충돌 감지 (천장)
        if (_velocity.y > 0)
        {
            Vector3 rayOrigin = transform.position + _center;
            // 레이캐스트 거리를 약간 줄여서 더 정확한 충돌 감지
            float rayDistance = (_height * 0.45f) + (_velocity.y * Time.fixedDeltaTime);
            
            // 천장 레이캐스트 실행
            
            if (Physics.Raycast(rayOrigin, Vector3.up, out RaycastHit hit, rayDistance, _collisionLayer))
            {
                // 자기 자신의 콜라이더는 무시
                bool isSelf = false;
                for (int j = 0; j < _colliderCount; j++)
                {
                    if (hit.collider == _colliders[j])
                    {
                        isSelf = true;
                        break;
                    }
                }
                
                if (!isSelf)
                {
                    // 천장 충돌 감지
                    // 천장에 부딪히면 수직 속도를 약간의 반동으로 설정
                    _velocity.y = -0.5f;
                }
            }
        }
    }

    /// <summary>
    /// 최종 이동 적용
    /// </summary>
    private void ApplyMovement()
    {
        // 외부 힘 적용
        _velocity += _externalForce * Time.fixedDeltaTime;
        
        // 외부 힘 감쇠
        _externalForce *= Mathf.Pow(0.01f, Time.fixedDeltaTime);
        if (_externalForce.magnitude < 0.1f)
        {
            _externalForce = Vector3.zero;
        }
        
        // 최종 속도 적용
        
        // Rigidbody를 통한 이동 (속도 직접 설정)
        _rigidbody.velocity = _velocity;
    }

    /// <summary>
    /// 외부 힘 추가
    /// </summary>
    public void AddForce(Vector3 force)
    {
        _externalForce += force;
    }

    /// <summary>
    /// 상대적 외부 힘 추가
    /// </summary>
    public void AddRelativeForce(Vector3 force)
    {
        _externalForce += transform.TransformDirection(force);
    }
} 