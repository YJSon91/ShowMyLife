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
    private CharacterController _controller;
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
    private bool _originalControllerEnabled;
    private float _originalGravity;
    
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
        // 갓모드 토글 입력 이벤트 구독
        SubscribeToInputEvents();
    }
    
    private void Update()
    {
        // 갓모드가 활성화되어 있을 때만 처리
        if (_isGodModeActive)
        {
            HandleGodModeMovement();
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
    /// 필수 컴포넌트들을 초기화합니다
    /// </summary>
    private void InitializeComponents()
    {
        // Player 컴포넌트가 할당되지 않았다면 자동으로 찾기
        if (_player == null)
            _player = GetComponent<Player>();
        
        // Player 컴포넌트로부터 다른 컴포넌트들 가져오기
        if (_player != null)
        {
            _controller = _player.CharacterController;
            _movementController = _player.MovementController;
            _inputReader = _player.InputReader;
            _cameraTransform = _player.MainCameraTransform;
        }
        
        ValidateComponents();
    }
    
    /// <summary>
    /// 모든 필수 컴포넌트가 존재하는지 확인합니다
    /// </summary>
    private void ValidateComponents()
    {
        if (_player == null)
            Debug.LogError("PlayerGodModeController: Player 컴포넌트가 할당되지 않았습니다!");
        
        if (_controller == null)
            Debug.LogError("PlayerGodModeController: CharacterController를 찾을 수 없습니다!");
        
        if (_movementController == null)
            Debug.LogError("PlayerGodModeController: PlayerMovementController를 찾을 수 없습니다!");
        
        if (_cameraTransform == null)
            Debug.LogError("PlayerGodModeController: Camera Transform을 찾을 수 없습니다!");
    }
    
    #endregion
    
    #region 입력 이벤트 처리
    
    /// <summary>
    /// 입력 이벤트에 구독합니다
    /// </summary>
    private void SubscribeToInputEvents()
    {
        // TODO: InputReader에 갓모드 이벤트가 추가되면 여기서 구독
        // _inputReader.onGodModeToggled += ToggleGodMode;
    }
    
    /// <summary>
    /// 입력 이벤트 구독을 해제합니다
    /// </summary>
    private void UnsubscribeFromInputEvents()
    {
        // TODO: InputReader에 갓모드 이벤트가 추가되면 여기서 구독 해제
        // _inputReader.onGodModeToggled -= ToggleGodMode;
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
        _originalControllerEnabled = _controller.enabled;
        
        // CharacterController의 중력 비활성화
        _controller.enabled = false;
        
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
        _controller.enabled = _originalControllerEnabled;
        
        // 비행 속도 초기화
        _flyVelocity = Vector3.zero;
        
        Debug.Log("[갓모드] 갓모드가 비활성화되었습니다.");
    }
    
    #endregion
    
    #region 갓모드 이동 처리
    
    /// <summary>
    /// 갓모드에서의 이동을 처리합니다
    /// </summary>
    private void HandleGodModeMovement()
    {
        // 입력 값 가져오기
        Vector2 moveInput = Vector2.zero;
        
        // InputReader가 있다면 사용, 없다면 직접 입력 처리
        if (_inputReader != null)
        {
            moveInput = _inputReader._moveComposite;
        }
        else
        {
            // 직접 입력 처리 (fallback)
            moveInput.x = Input.GetAxis("Horizontal");
            moveInput.y = Input.GetAxis("Vertical");
        }
        
        // 상하 이동 입력 (스페이스바: 위로, Q: 아래로)
        float verticalInput = 0f;
        if (Input.GetKey(KeyCode.Space))
            verticalInput = 1f;
        else if (Input.GetKey(KeyCode.Q))
            verticalInput = -1f;
        
        // 속도 계산
        float currentFlySpeed = CalculateCurrentFlySpeed();
        
        // 카메라 기준 이동 방향 계산
        Vector3 targetVelocity = CalculateTargetVelocity(moveInput, verticalInput, currentFlySpeed);
        
        // 부드러운 가속/감속 적용
        _flyVelocity = Vector3.Lerp(_flyVelocity, targetVelocity, _flyAcceleration * Time.deltaTime);
        
        // 실제 이동 적용
        transform.position += _flyVelocity * Time.deltaTime;
    }
    
    /// <summary>
    /// 현재 비행 속도를 계산합니다 (Shift/Ctrl 입력 고려)
    /// </summary>
    private float CalculateCurrentFlySpeed()
    {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            return _fastFlySpeed; // 빠른 속도
        }
        else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            return _slowFlySpeed; // 느린 속도
        }
        else
        {
            return _flySpeed; // 기본 속도
        }
    }
    
    /// <summary>
    /// 카메라 기준으로 목표 속도를 계산합니다
    /// </summary>
    private Vector3 CalculateTargetVelocity(Vector2 moveInput, float verticalInput, float speed)
    {
        Vector3 targetVelocity = Vector3.zero;
        
        if (_cameraTransform != null)
        {
            // 카메라의 전방/우측 벡터 계산 (Y축 회전만 고려)
            Vector3 cameraForward = _cameraTransform.forward;
            cameraForward.y = 0f;
            cameraForward.Normalize();
            
            Vector3 cameraRight = _cameraTransform.right;
            cameraRight.y = 0f;
            cameraRight.Normalize();
            
            // 수평 이동
            targetVelocity = (cameraForward * moveInput.y + cameraRight * moveInput.x) * speed;
            
            // 수직 이동
            targetVelocity.y = verticalInput * speed;
        }
        else
        {
            // 카메라가 없을 경우 월드 좌표계 사용
            targetVelocity = new Vector3(moveInput.x, verticalInput, moveInput.y) * speed;
        }
        
        return targetVelocity;
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