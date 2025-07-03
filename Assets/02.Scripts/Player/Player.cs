using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어의 모든 컴포넌트를 관리하는 중앙 클래스
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(PlayerAnimationController))]
[RequireComponent(typeof(PlayerMovementController))]
[RequireComponent(typeof(PlayerStateController))]
[RequireComponent(typeof(InputReader))]
[RequireComponent(typeof(PlayerGodModeController))]
public class Player : MonoBehaviour
{
    #region 컴포넌트 참조

    [Header("필수 컴포넌트")]
    [Tooltip("플레이어 애니메이션을 제어하는 컴포넌트")]
    [SerializeField] private PlayerAnimationController _animationController;
    [Tooltip("플레이어 이동을 제어하는 Rigidbody 컴포넌트")]
    [SerializeField] private Rigidbody _rigidbody;
    [Tooltip("플레이어 충돌을 제어하는 CapsuleCollider 컴포넌트")]
    [SerializeField] private CapsuleCollider _capsuleCollider;
    //메인카메라 트랜스폼 캐싱
    private Transform _mainCameraTransform;
    
    [Tooltip("InputReader는 플레이어 입력을 처리합니다")]
    [SerializeField] private InputReader _inputReader;

    #endregion

    #region 상태 관리 컴포넌트
    [Header("상태 관리 컴포넌트")]
    [Tooltip("플레이어 이동을 제어하는 컴포넌트")]
    [SerializeField] private PlayerMovementController _movementController;
    [Tooltip("플레이어 상태를 관리하는 컴포넌트")]
    [SerializeField] private PlayerStateController _stateController;
    [Tooltip("플레이어 갓모드를 제어하는 컴포넌트")]
    [SerializeField] private PlayerGodModeController _godModeController;
    #endregion

    #region 속성

    public PlayerAnimationController AnimationController => _animationController;
    public Rigidbody Rigidbody => _rigidbody;
    public CapsuleCollider CapsuleCollider => _capsuleCollider;
    public Transform MainCameraTransform => _mainCameraTransform;
    public InputReader InputReader => _inputReader;
    public PlayerMovementController MovementController => _movementController;
    public PlayerStateController StateController => _stateController;
    public PlayerGodModeController GodModeController => _godModeController;

    #endregion

    #region Unity 생명주기

    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        // Start에서 카메라 초기화 (씬이 완전히 로드된 후)
        InitializeCamera();
        
        // GameManager에 플레이어 등록
        if (GameManager.Instance != null)
            GameManager.Instance.RegisterPlayer(this);
    }

    /// <summary>
    /// 카메라 Transform을 초기화합니다
    /// </summary>
    private void InitializeCamera()
    {
        // 여러 방법으로 카메라를 찾아보기
        Camera mainCamera = Camera.main;
        
        if (mainCamera == null)
        {
            // MainCamera 태그로 찾기
            GameObject cameraObject = GameObject.FindWithTag("MainCamera");
            if (cameraObject != null)
                mainCamera = cameraObject.GetComponent<Camera>();
        }
        
        if (mainCamera == null)
        {
            // Camera 컴포넌트로 찾기
            mainCamera = FindObjectOfType<Camera>();
        }
        
        if (mainCamera != null)
        {
            _mainCameraTransform = mainCamera.transform;
            Debug.Log($"Player: 메인 카메라를 찾았습니다: {mainCamera.name}");
        }
        else
        {
            Debug.LogWarning("Player: 메인 카메라를 찾을 수 없습니다. 갓모드에서 월드 좌표계를 사용합니다.");
        }
    }
    
    #endregion

    #region 초기화 메서드

    /// <summary>
    /// 모든 필수 컴포넌트를 초기화합니다
    /// </summary>
    private void InitializeComponents()
    {
        // 컴포넌트가 Inspector에서 할당되지 않은 경우 자동으로 찾기
        if (_rigidbody == null)
            _rigidbody = GetComponent<Rigidbody>();
            
        if (_capsuleCollider == null)
            _capsuleCollider = GetComponent<CapsuleCollider>();

        if (_animationController == null)
            _animationController = GetComponent<PlayerAnimationController>();

        if (_inputReader == null)
            _inputReader = GetComponent<InputReader>();

        if (_movementController == null)
            _movementController = GetComponent<PlayerMovementController>();

        if (_stateController == null)
            _stateController = GetComponent<PlayerStateController>();

        if (_godModeController == null)
            _godModeController = GetComponent<PlayerGodModeController>();

        // 카메라 컨트롤러는 다른 게임 오브젝트에 있을 수 있으므로 자동으로 찾지 않음

        ValidateComponents();
    }

    /// <summary>
    /// 모든 필수 컴포넌트가 존재하는지 확인합니다
    /// </summary>
    private void ValidateComponents()
    {
        if (_rigidbody == null)
            Debug.LogError("Player: Rigidbody가 할당되지 않았습니다!");
            
        if (_capsuleCollider == null)
            Debug.LogError("Player: CapsuleCollider가 할당되지 않았습니다!");

        if (_animationController == null)
            Debug.LogError("Player: PlayerAnimationController가 할당되지 않았습니다!");

        if (_inputReader == null)
            Debug.LogError("Player: InputReader가 할당되지 않았습니다!");       

        if (_movementController == null)
            Debug.LogError("Player: PlayerMovementController가 할당되지 않았습니다!");

        if (_stateController == null)
            Debug.LogError("Player: PlayerStateController가 할당되지 않았습니다!");
    }

    #endregion

    #region 공개 메서드

    /// <summary>
    /// 플레이어 위치를 설정합니다
    /// </summary>
    /// <param name="position">새 위치</param>
    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    /// <summary>
    /// 플레이어 회전을 설정합니다
    /// </summary>
    /// <param name="rotation">새 회전</param>
    public void SetRotation(Quaternion rotation)
    {
        transform.rotation = rotation;
    }

    #endregion
} 