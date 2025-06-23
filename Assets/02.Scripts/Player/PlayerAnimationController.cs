using UnityEngine;

/// <summary>
/// 플레이어 캐릭터의 애니메이션을 제어하는 컴포넌트
/// </summary>
public class PlayerAnimationController : MonoBehaviour
{
    [Header("애니메이션 설정")]
    [Tooltip("랜덤 Idle 애니메이션 최소 대기 시간")]
    [SerializeField] private float _minIdleTime = 5f;
    [Tooltip("랜덤 Idle 애니메이션 최대 대기 시간")]
    [SerializeField] private float _maxIdleTime = 10f;
    [Tooltip("디버그 로그 활성화")]
    [SerializeField] private bool _debugMode = true;
    [Tooltip("착지 감지 최소 공중 시간")]
    [SerializeField] private float _minAirTimeForLanding = 0.0f;

    // 컴포넌트 참조
    private Animator _animator;
    private PlayerController _playerController;
    private PlayerInput _playerInput;

    // 애니메이션 관련 변수
    private float _idleAnimTimer;
    private bool _isMoving;
    private bool _wasMoving; // 이전 프레임의 이동 상태
    private bool _jumpInputDetected; // 점프 입력 감지 플래그
    private bool _isJumping; // 점프 중인지 여부
    private bool _wasGrounded; // 이전 프레임의 지면 상태
    private bool _landingTriggered; // 착지 트리거 발동 여부
    private float _airTime; // 공중에 있는 시간
    private float _jumpCooldown; // 점프 쿨다운 타이머
    private float _landingCooldown; // 착지 쿨다운 타이머
    
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
    
    private AnimState _currentAnimState = AnimState.Idle;
   
    
    // 애니메이션 파라미터 해시값
    private static readonly int Walking = Animator.StringToHash("Walking");
    private static readonly int Running = Animator.StringToHash("Running");
    private static readonly int RandomIdle = Animator.StringToHash("RandomIdle");
    private static readonly int Jumping = Animator.StringToHash("Jumping");
    private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
    private static readonly int Landing = Animator.StringToHash("Landing");

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
        
        // 랜덤 Idle 타이머 초기화
        ResetIdleTimer();
        
        // 초기 상태 설정
        _wasMoving = false;
        _isMoving = false;
        _jumpInputDetected = false;
        _isJumping = false;
        _wasGrounded = true;
        _landingTriggered = false;
        _airTime = 0f;
        _jumpCooldown = 0f;
        _landingCooldown = 0f;
    }

    private void Update()
    {
        // 타이머 업데이트
        UpdateTimers();
        
        // 이전 프레임의 이동 상태 저장
        _wasMoving = _isMoving;
        
        // 이전 프레임의 지면 상태 저장
        _wasGrounded = _playerController.IsGrounded;
        
        // 이동 상태 확인
        CheckMovementState();
        
        // 점프 입력 감지 (직접 감지)
        if (Input.GetKeyDown(KeyCode.Space) && _playerController.IsGrounded && !_isJumping && _jumpCooldown <= 0)
        {
            _jumpInputDetected = true;
            
        }
        
        // 애니메이션 업데이트
        UpdateAnimations();
    }
    
    /// <summary>
    /// 타이머 업데이트
    /// </summary>
    private void UpdateTimers()
    {
        // 쿨다운 타이머 업데이트
        if (_jumpCooldown > 0)
        {
            _jumpCooldown -= Time.deltaTime;
        }
        
        if (_landingCooldown > 0)
        {
            _landingCooldown -= Time.deltaTime;
        }
    }
    
    /// <summary>
    /// 이동 상태 확인
    /// </summary>
    private void CheckMovementState()
    {
        // 입력 값을 통해 이동 상태 확인
        float horizontalInput = _playerInput.GetHorizontalInput();
        float verticalInput = _playerInput.GetVerticalInput();
        
        // 입력이 있으면 이동 중으로 판단
        _isMoving = Mathf.Abs(horizontalInput) > 0.01f || Mathf.Abs(verticalInput) > 0.01f;
        
        // 이동 시작 감지 (이전에 멈춰있다가 이동 시작)
        if (_isMoving && !_wasMoving)
        {
            // 이동 시작 시 RandomIdle 값을 0으로 초기화
            _animator.SetInteger(RandomIdle, 0);
        }
        
        // 지면 상태 확인
        bool isGrounded = _playerController.IsGrounded;
        
        // 공중에 있는 시간 추적
        if (!isGrounded)
        {
            _airTime += Time.deltaTime;
            
            // 현재 애니메이션 상태 확인
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            
            // 점프 애니메이션이 끝나가고 아직 공중에 있으면 낙하 상태로 변경
            if (_currentAnimState == AnimState.Jumping && stateInfo.normalizedTime > 0.8f)
            {
                _currentAnimState = AnimState.Falling;
                
            }
        }
        else
        {
            // 지면에 닿았을 때 처리
            HandleGroundedState();
        }
    }
    
    /// <summary>
    /// 지면에 닿았을 때 상태 처리
    /// </summary>
    private void HandleGroundedState()
    {
        // 이전에 공중에 있었고 현재 지면에 있는 경우 (착지)
        if (!_wasGrounded)
        {
            
            
            // 공중에서 지면으로 착지한 경우 무조건 착지 애니메이션 재생
            if (_isJumping || _currentAnimState == AnimState.Jumping || _currentAnimState == AnimState.Falling)
            {
                TriggerLandingAnimation();
            }
            
            // 공중 시간 초기화
            _airTime = 0f;
        }
        else if (_isJumping && _landingCooldown <= 0)
        {
            // 이미 지면에 있었지만 여전히 점프 상태인 경우 (버그 상황)
            // 강제로 착지 상태로 전환
            
            TriggerLandingAnimation();
        }
    }
    
    /// <summary>
    /// 착지 애니메이션 트리거
    /// </summary>
    private void TriggerLandingAnimation()
    {
        // 착지 애니메이션으로 상태 변경
        _currentAnimState = AnimState.Landing;
        
        // 모든 다른 트리거 초기화
        _animator.ResetTrigger(Jumping);
        
        // 착지 애니메이션 트리거
        _animator.SetTrigger(Landing);
        _landingTriggered = true;
        
       
        
        // 점프 상태 종료
        _isJumping = false;
        
        // 점프 쿨다운 설정 (연속 점프 방지)
        _jumpCooldown = 0.1f;
        
        // 착지 쿨다운 설정 (연속 착지 방지)
        _landingCooldown = 0.5f;
    }
    
    /// <summary>
    /// 애니메이션 상태 업데이트
    /// </summary>
    private void UpdateAnimations()
    {
        if (_animator == null) return;
        
        // 달리기 상태 확인
        bool isSprinting = _playerInput.GetSprintInput();
        
        // 지면 상태 확인
        bool isGrounded = _playerController.IsGrounded;
        
        // 지면 상태 설정
        _animator.SetBool(IsGrounded, isGrounded);
        
        // 현재 애니메이터 상태 확인 (디버그용)
        if (_debugMode)
        {
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            
            if (stateInfo.IsName("player_jump1"))
            {
                
                
                // 점프 애니메이션 중에 지면에 닿았다면 강제로 착지 애니메이션 재생
                if (isGrounded && stateInfo.normalizedTime > 0.5f && !_landingTriggered && _landingCooldown <= 0)
                {
                    
                    TriggerLandingAnimation();
                }
            }
            else if (stateInfo.IsName("Player_landing"))
            {
                
            }
        }
        
        // 점프 애니메이션 트리거 - 지면에 있을 때만 점프 가능
        if (_jumpInputDetected && isGrounded && _jumpCooldown <= 0)
        {
            // 현재 상태를 점프로 변경
            _currentAnimState = AnimState.Jumping;
            
            // 모든 다른 트리거 초기화
            _animator.ResetTrigger(Landing);
            
            // 점프 트리거 설정
            _animator.SetTrigger(Jumping);
            _jumpInputDetected = false; // 점프 입력 플래그 초기화
            _isJumping = true; // 점프 상태 활성화
            _landingTriggered = false; // 착지 트리거 초기화
            
            
        }
        
        // 이동 애니메이션 설정 - 점프나 착지 중이 아닐 때만
        if (_currentAnimState != AnimState.Jumping && _currentAnimState != AnimState.Falling && _currentAnimState != AnimState.Landing)
        {
            if (_isMoving)
            {
                _currentAnimState = isSprinting ? AnimState.Running : AnimState.Walking;
                _animator.SetBool(Walking, !isSprinting);
                _animator.SetBool(Running, isSprinting);
            }
            else
            {
                _currentAnimState = AnimState.Idle;
                _animator.SetBool(Walking, false);
                _animator.SetBool(Running, false);
            }
            
            // 랜덤 Idle 애니메이션 처리 - 점프 중이 아닐 때만
            if (!_isMoving && isGrounded && !_isJumping)
            {
                UpdateIdleAnimation();
            }
            else
            {
                // 이동 중이면 타이머 리셋
                ResetIdleTimer();
            }
        }
        
        // 강제 상태 검사 - 점프 상태인데 지면에 있는 경우 상태 수정
        // 이 부분은 애니메이션 버그를 방지하기 위한 안전장치
        if (_isJumping && isGrounded && _landingCooldown <= 0)
        {
            // 0.5초 이상 지면에 있으면 강제로 점프 상태 해제
            if (_airTime <= 0f)
            {
                
                _isJumping = false;
                _landingTriggered = false;
                
                // 현재 애니메이션이 점프나 낙하 상태라면 착지 애니메이션 재생
                AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.IsName("player_jump1") || stateInfo.IsName("player_fall"))
                {
                    TriggerLandingAnimation();
                }
            }
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
        // 이동 중이면 랜덤 애니메이션 재생하지 않음
        if (_isMoving) return;
        
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
        
        _isJumping = false;
        _landingTriggered = false;
        
        // 착지 애니메이션이 끝난 후 상태 업데이트
        if (_isMoving)
        {
            _currentAnimState = _playerInput.GetSprintInput() ? AnimState.Running : AnimState.Walking;
        }
        else
        {
            _currentAnimState = AnimState.Idle;
        }
    }
} 