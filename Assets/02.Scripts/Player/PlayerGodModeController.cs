using UnityEngine;

/// <summary>
/// 플레이어의 갓모드 기능을 관리하는 컨트롤러
/// 갓모드 활성화 시 자유롭게 날아다닐 수 있습니다
/// </summary>
public class PlayerGodModeController : MonoBehaviour
{
    #region 컴포넌트 참조
    
    [Header("필수 컴포넌트")]
    [Tooltip("플레이어 메인 컴포넌트")]
    [SerializeField] private Player _player;
    
    // 내부 컴포넌트 참조
    private Rigidbody _rigidbody;
    private CapsuleCollider _capsuleCollider;
    private PlayerMovementController _movementController;
    private InputReader _inputReader;
    private Transform _cameraTransform;
    
    #endregion
    
    #region 갓모드 설정
    
    [Header("갓모드 설정")]
    [Tooltip("갓모드 비행 속도")]
    [SerializeField] private float _flySpeed = 10f;
    [Tooltip("갓모드 빠른 비행 속도 (Shift 누를 때)")]
    [SerializeField] private float _fastFlySpeed = 20f;
    [Tooltip("갓모드 느린 비행 속도 (Ctrl 누를 때)")]
    [SerializeField] private float _slowFlySpeed = 5f;
    [Tooltip("갓모드 이동 가속도")]
    [SerializeField] private float _flyAcceleration = 15f;
    [Tooltip("갓모드 이동 감쇠")]
    [SerializeField] private float _flyDamping = 10f;
    
    #endregion
    
    #region 런타임 속성
    
    private bool _isGodModeActive = false;
    private Vector3 _flyVelocity = Vector3.zero;
    
    // 원래 컴포넌트 상태 저장
    private bool _originalUseGravity;
    private bool _originalIsKinematic;
    private RigidbodyConstraints _originalConstraints;
    
    #endregion
    
    #region 공개 속성
    
    /// <summary>
    /// 갓모드 활성화 여부
    /// </summary>
    public bool IsGodModeActive => _isGodModeActive;
    
    #endregion
    
    #region Unity 생명주기
    
    private void Awake()
    {
        InitializeComponents();
    }
    
    private void Start()
    {
        // 카메라를 다시 찾기 시도 (Player의 Start에서 카메라 초기화가 완료된 후)
        if (_cameraTransform == null && _player != null)
        {
            _cameraTransform = _player.MainCameraTransform;
            if (_cameraTransform != null)
            {
                Debug.Log("PlayerGodModeController: 카메라 Transform을 찾았습니다.");
            }
        }
        
        // 갓모드 토글 입력 이벤트 구독
        SubscribeToInputEvents();
    }
    
    private void Update()
    {
        // 갓모드가 활성화되어 있을 때만 처리
        if (_isGodModeActive)
        {
            UpdateGodModeMovement();
        }
        
        // 키보드 1번으로 갓모드 토글 (임시 - 나중에 Input System으로 대체)
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ToggleGodMode();
        }
    }
    
    private void OnDestroy()
    {
        // 입력 이벤트 구독 해제
        UnsubscribeFromInputEvents();
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
            _movementController = _player.MovementController;
            _inputReader = _player.InputReader;
            _cameraTransform = _player.MainCameraTransform;
        }
        else
        {
            Debug.LogError("PlayerGodModeController: Player 컴포넌트를 찾을 수 없습니다!");
        }
        
        ValidateComponents();
    }
    
    /// <summary>
    /// 필요한 컴포넌트가 모두 할당되었는지 확인합니다
    /// </summary>
    private void ValidateComponents()
    {
        if (_rigidbody == null)
            Debug.LogError("PlayerGodModeController: Rigidbody가 할당되지 않았습니다!");
            
        if (_capsuleCollider == null)
            Debug.LogError("PlayerGodModeController: CapsuleCollider가 할당되지 않았습니다!");
        
        if (_movementController == null)
            Debug.LogError("PlayerGodModeController: PlayerMovementController가 할당되지 않았습니다!");
        
        if (_inputReader == null)
            Debug.LogError("PlayerGodModeController: InputReader가 할당되지 않았습니다!");
    }
    
    /// <summary>
    /// 입력 이벤트에 구독합니다
    /// </summary>
    private void SubscribeToInputEvents()
    {
        // TODO: InputReader에 갓모드 이벤트가 추가되면 여기서 구독
        _inputReader.onGodModeToggled += ToggleGodMode;
    }
    
    /// <summary>
    /// 입력 이벤트 구독을 해제합니다
    /// </summary>
    private void UnsubscribeFromInputEvents()
    {
        // TODO: InputReader에 갓모드 이벤트가 추가되면 여기서 구독 해제
        _inputReader.onGodModeToggled -= ToggleGodMode;
    }
    
    #endregion
    
    #region 갓모드 제어 메서드
    
    /// <summary>
    /// 갓모드를 토글합니다
    /// </summary>
    public void ToggleGodMode()
    {
        if (_isGodModeActive)
        {
            DeactivateGodMode();
        }
        else
        {
            ActivateGodMode();
        }
    }
    
    /// <summary>
    /// 갓모드를 활성화합니다
    /// </summary>
    public void ActivateGodMode()
    {
        if (_isGodModeActive) return;
        
        _isGodModeActive = true;
        
        // 원래 상태 저장
        _originalUseGravity = _rigidbody.useGravity;
        _originalIsKinematic = _rigidbody.isKinematic;
        _originalConstraints = _rigidbody.constraints;
        
        // 리지드바디 설정 변경
        _rigidbody.useGravity = false;
        _rigidbody.isKinematic = true; // 물리 영향을 받지 않도록 설정
        _rigidbody.constraints = RigidbodyConstraints.None; // 모든 제약 해제
        
        // 비행 속도 초기화
        _flyVelocity = Vector3.zero;
        
        Debug.Log("[갓모드] 갓모드가 활성화되었습니다. WASD로 이동, Shift로 빠르게, Ctrl로 느리게 이동할 수 있습니다.");
    }
    
    /// <summary>
    /// 갓모드를 비활성화합니다
    /// </summary>
    public void DeactivateGodMode()
    {
        if (!_isGodModeActive) return;
        
        _isGodModeActive = false;
        
        // 원래 상태 복원
        _rigidbody.useGravity = _originalUseGravity;
        _rigidbody.isKinematic = _originalIsKinematic;
        _rigidbody.constraints = _originalConstraints;
        
        // 속도 초기화
        _rigidbody.velocity = Vector3.zero;
        
        Debug.Log("[갓모드] 갓모드가 비활성화되었습니다.");
    }
    
    /// <summary>
    /// 갓모드 이동을 업데이트합니다
    /// </summary>
    private void UpdateGodModeMovement()
    {
        if (!_isGodModeActive || _cameraTransform == null) return;
        
        // 입력 방향 가져오기
        Vector2 moveInput = _inputReader._moveComposite;
        
        // 카메라 기준 방향 계산
        Vector3 forward = _cameraTransform.forward;
        Vector3 right = _cameraTransform.right;
        
        // Y축은 유지 (비행을 위해)
        
        // 이동 방향 계산
        Vector3 moveDirection = (forward * moveInput.y + right * moveInput.x).normalized;
        
        // 수직 이동 (Space: 위로, Ctrl: 아래로)
        if (Input.GetKey(KeyCode.Space))
        {
            moveDirection += Vector3.up;
        }
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            moveDirection += Vector3.down;
        }
        
        // 속도 결정
        float currentSpeed = _flySpeed;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            currentSpeed = _fastFlySpeed;
        }
        else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            currentSpeed = _slowFlySpeed;
        }
        
        // 목표 속도 계산
        Vector3 targetVelocity = moveDirection * currentSpeed;
        
        // 부드러운 이동을 위한 보간
        _flyVelocity = Vector3.Lerp(_flyVelocity, targetVelocity, _flyAcceleration * Time.deltaTime);
        
        // 위치 업데이트
        transform.position += _flyVelocity * Time.deltaTime;
    }
    
    #endregion
    
    #region 공개 메서드
    
    /// <summary>
    /// 갓모드 활성화 상태를 강제로 설정합니다
    /// </summary>
    /// <param name="active">활성화 여부</param>
    public void SetGodModeActive(bool active)
    {
        if (active)
        {
            ActivateGodMode();
        }
        else
        {
            DeactivateGodMode();
        }
    }
    
    #endregion
} 