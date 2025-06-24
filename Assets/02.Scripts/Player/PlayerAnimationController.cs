using UnityEngine;
using System.Collections;

/// <summary>
/// 플레이어 캐릭터의 애니메이션을 제어하는 컴포넌트
/// 애니메이션 씹힘 현상을 방지하기 위한 개선된 버전
/// </summary>
public class PlayerAnimationController : MonoBehaviour
{
    [Header("애니메이션 설정")]
    [Tooltip("랜덤 Idle 애니메이션 최소 대기 시간")]
    [SerializeField] private float _minIdleTime = 5f;
    [Tooltip("랜덤 Idle 애니메이션 최대 대기 시간")]
    [SerializeField] private float _maxIdleTime = 10f;
    [Tooltip("점프 애니메이션 지연 시간")]
    [SerializeField] private float _jumpDelay = 0.1f;
    [Tooltip("착지 감지 최소 공중 시간")]
    [SerializeField] private float _minAirTimeForLanding = 0.2f;
    [Tooltip("점프 애니메이션 최소 재생 시간")]
    [SerializeField] private float _minJumpAnimTime = 0.3f;
    [Tooltip("점프 애니메이션 최대 재생 시간")]
    [SerializeField] private float _maxJumpAnimTime = 0.8f;
    [Tooltip("애니메이션 트랜지션 시간")]
    [SerializeField] private float _animationTransitionTime = 0.15f;

    // 컴포넌트 참조
    private Animator _animator;
    private PlayerController _playerController;
    private PlayerInput _playerInput;

    // 애니메이션 상태 관리
    private enum AnimState
    {
        Idle,
        Walking,
        Running,
        Jumping,
        Falling,
        Landing
    }

    // 현재 및 이전 애니메이션 상태
    private AnimState _currentAnimState = AnimState.Idle;
    private AnimState _previousAnimState = AnimState.Idle;

    // 애니메이션 관련 변수
    private float _idleAnimTimer;
    private float _airTime;
    private bool _isJumping;
    private bool _isLanding;
    private bool _jumpRequested;
    private bool _transitioningToJump;
    private bool _landingAnimationCompleted;
    private bool _jumpInputDuringLanding; // 착지 중 점프 입력 감지
    private float _jumpAnimTimer; // 점프 애니메이션 타이머
    private bool _jumpAnimationLocked; // 점프 애니메이션 잠금 상태
    private Coroutine _currentJumpCoroutine; // 현재 실행 중인 점프 코루틴
    private Coroutine _currentLandingCoroutine; // 현재 실행 중인 착지 코루틴
    private float _lastJumpTime; // 마지막 점프 시간

    // 애니메이션 파라미터 해시값 (성능 최적화)
    private static readonly int Walking = Animator.StringToHash("Walking");
    private static readonly int Running = Animator.StringToHash("Running");
    private static readonly int RandomIdle = Animator.StringToHash("RandomIdle");
    private static readonly int Jumping = Animator.StringToHash("Jumping");
    private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
    private static readonly int Landing = Animator.StringToHash("Landing");
    private static readonly int Fall = Animator.StringToHash("Fall");

    private void Awake()
    {
        // 컴포넌트 참조 캐싱
        _animator = GetComponent<Animator>();
        _playerController = GetComponent<PlayerController>();
        _playerInput = GetComponent<PlayerInput>();
        
        // 컴포넌트 유효성 검사
        if (_animator == null)
        {
            Debug.LogError("Animator 컴포넌트를 찾을 수 없습니다.");
            enabled = false;
            return;
        }
        
        if (_playerController == null)
        {
            Debug.LogError("PlayerController 컴포넌트를 찾을 수 없습니다.");
            enabled = false;
            return;
        }
        
        if (_playerInput == null)
        {
            Debug.LogError("PlayerInput 컴포넌트를 찾을 수 없습니다.");
            enabled = false;
            return;
        }
        
        // 초기 상태 설정
        _idleAnimTimer = Random.Range(_minIdleTime, _maxIdleTime);
        _airTime = 0f;
        _isJumping = false;
        _isLanding = false;
        _jumpRequested = false;
        _transitioningToJump = false;
        _landingAnimationCompleted = false;
        _jumpInputDuringLanding = false;
        _jumpAnimTimer = 0f;
        _jumpAnimationLocked = false;
        _currentJumpCoroutine = null;
        _currentLandingCoroutine = null;
        _lastJumpTime = -10f; // 초기값은 충분히 이전 시간으로 설정
        
        // 애니메이션 초기화
        InitializeAnimator();
        
        // 플레이어 컨트롤러 이벤트 구독
        _playerController.OnJump += HandleJump;
        _playerController.OnLand += HandleLand;
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (_playerController != null)
        {
            _playerController.OnJump -= HandleJump;
            _playerController.OnLand -= HandleLand;
        }
        
        // 실행 중인 코루틴 정리
        StopAllCoroutines();
    }

    private void InitializeAnimator()
    {
        // 애니메이터 설정 초기화
        _animator.SetBool(IsGrounded, true);
        _animator.SetBool(Walking, false);
        _animator.SetBool(Running, false);
        _animator.SetInteger(RandomIdle, 0);
        _animator.SetBool(Fall, false);
        
        // 트리거 초기화
        ResetAllTriggers();
    }

    /// <summary>
    /// 모든 애니메이션 트리거 리셋
    /// </summary>
    private void ResetAllTriggers()
    {
        _animator.ResetTrigger(Jumping);
        _animator.ResetTrigger(Landing);
    }

    private void Update()
    {
        // 플레이어 상태 가져오기
        bool isMoving = _playerController.IsMoving;
        bool isSprinting = _playerController.IsSprinting;
        bool isGrounded = _playerController.IsGrounded;
        
        // 점프 애니메이션 타이머 업데이트
        if (_jumpAnimTimer > 0)
        {
            _jumpAnimTimer -= Time.deltaTime;
            if (_jumpAnimTimer <= 0)
            {
                _jumpAnimationLocked = false;
                
                // 점프 애니메이션이 끝났고 지면에 있다면 상태 초기화
                if (isGrounded && _isJumping)
                {
                    _isJumping = false;
                    
                    // 애니메이션 상태 업데이트
                    UpdateAnimationBasedOnMovement(isMoving, isSprinting);
                }
            }
        }
        
        // 점프 입력 감지
        if (_playerInput.GetJumpInputRaw())
        {
            if (isGrounded && !_transitioningToJump)
            {
                if (!_isLanding)
                {
                    // 일반적인 점프 처리는 PlayerController에서 처리됨
                }
                else
                {
                    // 착지 중 점프 입력 저장
                    _jumpInputDuringLanding = true;
                }
            }
        }
        
        // 지면 상태 추적
        TrackGroundState(isGrounded);
        
        // 애니메이션 상태 업데이트
        UpdateAnimationState(isMoving, isSprinting, isGrounded);
        
        // Idle 애니메이션 업데이트
        if (_currentAnimState == AnimState.Idle && isGrounded && !_isJumping && !_isLanding)
        {
            UpdateIdleAnimation();
        }
        else
        {
            ResetIdleTimer();
        }
    }

    /// <summary>
    /// 이동 상태에 따른 애니메이션 업데이트
    /// </summary>
    private void UpdateAnimationBasedOnMovement(bool isMoving, bool isSprinting)
    {
        if (isMoving)
        {
            ChangeAnimationState(isSprinting ? AnimState.Running : AnimState.Walking);
        }
        else
        {
            ChangeAnimationState(AnimState.Idle);
        }
    }

    /// <summary>
    /// 지면 상태 추적
    /// </summary>
    private void TrackGroundState(bool isGrounded)
    {
        // 지면 상태 설정
        _animator.SetBool(IsGrounded, isGrounded);
        
        // 공중에 있는 시간 추적
        if (!isGrounded)
        {
            _airTime += Time.deltaTime;
            
            // 점프 중이고 일정 시간이 지났으면 낙하 상태로 전환
            if (_isJumping && _airTime > 0.5f && _currentAnimState != AnimState.Falling)
            {
                ChangeAnimationState(AnimState.Falling);
                _animator.SetBool(Fall, true);
            }
        }
        else
        {
            // 공중 시간 초기화
            _airTime = 0f;
            _animator.SetBool(Fall, false);
        }
    }

    /// <summary>
    /// 애니메이션 상태 업데이트
    /// </summary>
    private void UpdateAnimationState(bool isMoving, bool isSprinting, bool isGrounded)
    {
        // 점프나 착지 중에는 이동 애니메이션 변경하지 않음
        if (_isJumping || _isLanding || _transitioningToJump)
        {
            return;
        }
        
        // 지면에 있을 때만 이동 애니메이션 변경
        if (isGrounded)
        {
            UpdateAnimationBasedOnMovement(isMoving, isSprinting);
        }
    }

    /// <summary>
    /// 애니메이션 상태 변경
    /// </summary>
    private void ChangeAnimationState(AnimState newState)
    {
        // 같은 상태면 변경하지 않음
        if (_currentAnimState == newState) return;
        
        // 이전 상태 저장
        _previousAnimState = _currentAnimState;
        
        // 새로운 상태로 변경
        _currentAnimState = newState;
        
        // 애니메이션 파라미터 설정
        switch (newState)
        {
            case AnimState.Idle:
                _animator.SetBool(Walking, false);
                _animator.SetBool(Running, false);
                break;
                
            case AnimState.Walking:
                _animator.SetBool(Walking, true);
                _animator.SetBool(Running, false);
                break;
                
            case AnimState.Running:
                _animator.SetBool(Walking, false);
                _animator.SetBool(Running, true);
                break;
                
            case AnimState.Jumping:
                // 점프 트리거는 HandleJumpCoroutine에서 처리
                break;
                
            case AnimState.Falling:
                // 낙하 상태는 Fall 파라미터로 처리
                break;
                
            case AnimState.Landing:
                // 착지 트리거는 HandleLandingCoroutine에서 처리
                break;
        }
    }

    /// <summary>
    /// PlayerController의 점프 이벤트 처리
    /// </summary>
    private void HandleJump()
    {
        // 마지막 점프 시간 체크 (너무 빠른 연속 점프 방지)
        if (Time.time - _lastJumpTime < 0.1f)
        {
            return;
        }
        
        _lastJumpTime = Time.time;
        
        // 현재 실행 중인 점프 코루틴이 있으면 중지
        if (_currentJumpCoroutine != null)
        {
            StopCoroutine(_currentJumpCoroutine);
            _currentJumpCoroutine = null;
        }
        
        // 현재 실행 중인 착지 코루틴이 있으면 중지
        if (_currentLandingCoroutine != null)
        {
            StopCoroutine(_currentLandingCoroutine);
            _currentLandingCoroutine = null;
            _isLanding = false;
        }
        
        // 새로운 점프 코루틴 시작
        _currentJumpCoroutine = StartCoroutine(HandleJumpCoroutine());
    }

    /// <summary>
    /// PlayerController의 착지 이벤트 처리
    /// </summary>
    private void HandleLand()
    {
        // 충분한 공중 시간이 있었을 때만 착지 애니메이션 재생
        if (_airTime > _minAirTimeForLanding && !_isLanding)
        {
            // 현재 실행 중인 착지 코루틴이 있으면 중지
            if (_currentLandingCoroutine != null)
            {
                StopCoroutine(_currentLandingCoroutine);
            }
            
            _currentLandingCoroutine = StartCoroutine(HandleLandingCoroutine());
        }
        else
        {
            // 짧은 점프였다면 바로 상태 초기화
            _isJumping = false;
            _jumpAnimationLocked = false;
            
            // 애니메이션 상태 업데이트
            bool isMoving = _playerController.IsMoving;
            bool isSprinting = _playerController.IsSprinting;
            UpdateAnimationBasedOnMovement(isMoving, isSprinting);
        }
    }

    /// <summary>
    /// 점프 처리 (코루틴)
    /// </summary>
    private IEnumerator HandleJumpCoroutine()
    {
        // 모든 트리거 리셋
        ResetAllTriggers();
        
        _transitioningToJump = true;
        _jumpAnimationLocked = true;
        _isJumping = true;
        _jumpRequested = false;
        _jumpAnimTimer = _maxJumpAnimTime;
        
        // 현재 애니메이션이 완전히 끝날 때까지 약간 대기
        yield return new WaitForSeconds(_jumpDelay);
        
        // 점프 상태로 변경
        ChangeAnimationState(AnimState.Jumping);
        
        // 점프 트리거 설정
        _animator.SetTrigger(Jumping);
        
        _transitioningToJump = false;
        
        // 최소 점프 애니메이션 시간 동안 대기
        yield return new WaitForSeconds(_minJumpAnimTime);
        
        // 지면에 있고 다른 점프 요청이 없으면 점프 상태 종료 준비
        if (_playerController.IsGrounded)
        {
            _jumpAnimTimer = _animationTransitionTime; // 전환 시간 설정
        }
        
        _currentJumpCoroutine = null;
    }

    /// <summary>
    /// 착지 처리 (코루틴)
    /// </summary>
    private IEnumerator HandleLandingCoroutine()
    {
        // 이미 착지 중이면 무시
        if (_isLanding) yield break;
        
        // 점프 상태 초기화
        _isJumping = false;
        _isLanding = true;
        _landingAnimationCompleted = false;
        _jumpAnimationLocked = true;
        
        // 모든 트리거 리셋
        ResetAllTriggers();
        
        // 착지 상태로 변경
        ChangeAnimationState(AnimState.Landing);
        
        // 착지 트리거 설정
        _animator.SetTrigger(Landing);
        
        // 착지 애니메이션이 완료될 때까지 대기
        float landingStartTime = Time.time;
        float maxLandingWaitTime = 1.0f; // 최대 1초까지만 대기
        
        while (!_landingAnimationCompleted && Time.time - landingStartTime < maxLandingWaitTime)
        {
            yield return null;
        }
        
        _isLanding = false;
        _jumpAnimationLocked = false;
        
        // 착지 중 점프 입력이 있었다면 즉시 점프 실행
        if (_jumpInputDuringLanding)
        {
            _jumpInputDuringLanding = false;
            
            // 현재 실행 중인 점프 코루틴이 있으면 중지
            if (_currentJumpCoroutine != null)
            {
                StopCoroutine(_currentJumpCoroutine);
            }
            
            _currentJumpCoroutine = StartCoroutine(HandleJumpCoroutine());
        }
        else
        {
            // 착지 후 상태 업데이트
            bool isMoving = _playerController.IsMoving;
            bool isSprinting = _playerController.IsSprinting;
            UpdateAnimationBasedOnMovement(isMoving, isSprinting);
        }
        
        _currentLandingCoroutine = null;
    }

    /// <summary>
    /// Idle 애니메이션 업데이트
    /// </summary>
    private void UpdateIdleAnimation()
    {
        _idleAnimTimer -= Time.deltaTime;
        
        if (_idleAnimTimer <= 0)
        {
            TriggerRandomIdleAnimation();
            ResetIdleTimer();
        }
    }

    /// <summary>
    /// 랜덤 Idle 애니메이션 트리거
    /// </summary>
    private void TriggerRandomIdleAnimation()
    {
        // 현재 이동 중이거나 점프/착지 중이면 실행하지 않음
        if (_currentAnimState != AnimState.Idle || _isJumping || _isLanding) return;
        
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
    /// 애니메이션 이벤트 - 착지 완료 시 호출
    /// </summary>
    public void OnLandingAnimationComplete()
    {
        _landingAnimationCompleted = true;
    }
} 