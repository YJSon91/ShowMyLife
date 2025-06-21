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

    // 컴포넌트 참조
    private Animator _animator;
    private PlayerController _playerController;
    private PlayerInput _playerInput;

    // 애니메이션 관련 변수
    private float _idleAnimTimer;
    private bool _isMoving;
    private bool _wasMoving; // 이전 프레임의 이동 상태
    
    // 애니메이션 파라미터 해시값
    private static readonly int Walking = Animator.StringToHash("Walking");
    private static readonly int Running = Animator.StringToHash("Running");
    private static readonly int RandomIdle = Animator.StringToHash("RandomIdle");

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
    }

    private void Update()
    {
        // 이전 프레임의 이동 상태 저장
        _wasMoving = _isMoving;
        
        // 이동 상태 확인
        CheckMovementState();
        
        // 애니메이션 업데이트
        UpdateAnimations();
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
            Debug.Log("이동 시작: RandomIdle 초기화");
        }
    }
    
    /// <summary>
    /// 애니메이션 상태 업데이트
    /// </summary>
    private void UpdateAnimations()
    {
        if (_animator == null) return;
        
        // 달리기 상태 확인
        bool isSprinting = _playerInput.GetSprintInput();
        
        // 이동 애니메이션 설정
        _animator.SetBool(Walking, _isMoving && !isSprinting);
        _animator.SetBool(Running, _isMoving && isSprinting);
        
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
        // 이동 중이면 랜덤 애니메이션 재생하지 않음
        if (_isMoving) return;
        
        // 랜덤 값 생성 (1: idle2, 2: idle3)
        int randomIdle = Random.Range(1, 3);
        
        // 애니메이터 파라미터 설정
        _animator.SetInteger(RandomIdle, randomIdle);
        
        // 디버그 로그
        Debug.Log($"랜덤 Idle 애니메이션 재생: {randomIdle}");
    }
    
    /// <summary>
    /// Idle 타이머 리셋
    /// </summary>
    private void ResetIdleTimer()
    {
        _idleAnimTimer = Random.Range(_minIdleTime, _maxIdleTime);
    }
} 