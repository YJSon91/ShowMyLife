using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어의 갓모드(자유 비행) 기능을 제어하는 컴포넌트
/// </summary>
public class PlayerGodModeController : MonoBehaviour
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

    #region 갓모드 설정

    [Header("갓모드 설정")]
    [Tooltip("갓모드에서의 이동 속도")]
    [SerializeField] private float _godModeSpeed = 10f;
    [Tooltip("갓모드에서의 상승/하강 속도")]
    [SerializeField] private float _godModeVerticalSpeed = 5f;
    [Tooltip("갓모드에서의 회전 속도")]
    [SerializeField] private float _godModeRotationSpeed = 100f;

    #endregion

    #region 런타임 속성

    private bool _isGodModeActive = false;
    private Vector3 _originalGravity;
    private RigidbodyConstraints _originalConstraints;
    private bool _originalKinematicState;
    private bool _originalUseGravity;
    private Vector3 _moveDirection;

    #endregion

    #region Unity 라이프사이클

    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        // 입력 이벤트 구독
        if (_inputReader != null)
        {
            _inputReader.OnGodModeToggled += ToggleGodMode;
        }
    }

    private void Update()
    {
        // 갓모드가 활성화된 경우에만 업데이트
        if (_isGodModeActive)
        {
            CalculateMoveDirection();
        }
    }

    private void FixedUpdate()
    {
        // 갓모드가 활성화된 경우에만 물리 업데이트
        if (_isGodModeActive)
        {
            MoveInGodMode();
        }
    }

    private void OnDestroy()
    {
        // 입력 이벤트 구독 해제
        if (_inputReader != null)
        {
            _inputReader.OnGodModeToggled -= ToggleGodMode;
        }
    }

    #endregion

    #region 초기화 메서드

    /// <summary>
    /// 모든 필수 컴포넌트를 초기화합니다
    /// </summary>
    private void InitializeComponents()
    {
        // Player 컴포넌트가 할당되지 않은 경우 자동으로 찾기
        if (_player == null)
            _player = GetComponent<Player>();

        if (_player != null)
        {
            _rigidbody = _player.Rigidbody;
            _capsuleCollider = _player.CapsuleCollider;
            _inputReader = _player.InputReader;
        }

        ValidateComponents();
        
        // 원래 물리 설정 저장
        if (_rigidbody != null)
        {
            _originalGravity = Physics.gravity;
            _originalConstraints = _rigidbody.constraints;
            _originalKinematicState = _rigidbody.isKinematic;
            _originalUseGravity = _rigidbody.useGravity;
        }
    }

    /// <summary>
    /// 모든 필수 컴포넌트가 존재하는지 확인합니다
    /// </summary>
    private void ValidateComponents()
    {
        if (_player == null)
            Debug.LogError("PlayerGodModeController: Player가 할당되지 않았습니다!");
            
        if (_rigidbody == null)
            Debug.LogError("PlayerGodModeController: Rigidbody를 찾을 수 없습니다!");
            
        if (_capsuleCollider == null)
            Debug.LogError("PlayerGodModeController: CapsuleCollider를 찾을 수 없습니다!");

        if (_inputReader == null)
            Debug.LogError("PlayerGodModeController: InputReader를 찾을 수 없습니다!");
    }

    #endregion

    #region 갓모드 메서드

    /// <summary>
    /// 갓모드를 토글합니다
    /// </summary>
    public void ToggleGodMode()
    {
        _isGodModeActive = !_isGodModeActive;
        
        if (_isGodModeActive)
        {
            EnableGodMode();
        }
        else
        {
            DisableGodMode();
        }
        
        Debug.Log($"갓모드 {(_isGodModeActive ? "활성화" : "비활성화")}");
    }

    /// <summary>
    /// 갓모드를 활성화합니다
    /// </summary>
    private void EnableGodMode()
    {
        if (_rigidbody != null)
        {
            // 물리 설정 변경
            _rigidbody.useGravity = false;
            _rigidbody.isKinematic = false;
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        }
        
        // 콜라이더 비활성화 (선택적)
        if (_capsuleCollider != null)
        {
            _capsuleCollider.isTrigger = true;
        }
    }

    /// <summary>
    /// 갓모드를 비활성화합니다
    /// </summary>
    private void DisableGodMode()
    {
        if (_rigidbody != null)
        {
            // 원래 물리 설정으로 복원
            _rigidbody.useGravity = _originalUseGravity;
            _rigidbody.isKinematic = _originalKinematicState;
            _rigidbody.constraints = _originalConstraints;
            
            // 회전 초기화 - 플레이어를 항상 위쪽을 향하도록 설정
            transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
            
            // 회전 모멘텀 제거
            _rigidbody.angularVelocity = Vector3.zero;
            
            // 아래쪽 방향으로 초기 속도 부여
            Vector3 initialFallVelocity = new Vector3(0f, -10f, 0f); // 아래쪽 방향으로 10 유닛/초의 초기 속도
            _rigidbody.velocity = initialFallVelocity;
        }
        
        // 콜라이더 재활성화 (선택적)
        if (_capsuleCollider != null)
        {
            _capsuleCollider.isTrigger = false;
        }
        
        // 지면 확인 강제 실행
        if (_player != null && _player.MovementController != null)
        {
            _player.MovementController.GroundedCheck();
        }
    }

    /// <summary>
    /// 갓모드에서의 이동 방향을 계산합니다
    /// </summary>
    private void CalculateMoveDirection()
    {
        if (_player == null || _player.MainCameraTransform == null)
            return;

        // 카메라 기준 방향 벡터 계산
        Vector3 forward = _player.MainCameraTransform.forward;
        Vector3 right = _player.MainCameraTransform.right;
        
        // 수평 이동은 카메라 방향 기준
        forward.y = 0;
        right.y = 0;
        
        // 정규화
        if (forward.magnitude > 0.01f)
            forward.Normalize();
        if (right.magnitude > 0.01f)
            right.Normalize();

        // 입력에 따른 이동 방향 계산
        _moveDirection = Vector3.zero;
        
        // 전후좌우 이동 (WASD)
        if (_inputReader._moveComposite.y > 0)
            _moveDirection += forward;
        if (_inputReader._moveComposite.y < 0)
            _moveDirection -= forward;
        if (_inputReader._moveComposite.x > 0)
            _moveDirection += right;
        if (_inputReader._moveComposite.x < 0)
            _moveDirection -= right;
            
        // 상하 이동 (스페이스와 컨트롤)
        if (Input.GetKey(KeyCode.Space))
            _moveDirection += Vector3.up;
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            _moveDirection += Vector3.down;
    }

    /// <summary>
    /// 갓모드에서 이동합니다
    /// </summary>
    private void MoveInGodMode()
    {
        if (_rigidbody == null)
            return;
            
        // 이동 속도 적용
        float currentSpeed = _godModeSpeed;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            currentSpeed *= 2; // 쉬프트 키로 속도 증가
            
        // 리지드바디 속도 직접 설정
        _rigidbody.velocity = _moveDirection.normalized * currentSpeed;
        
        // 마우스 입력에 따른 회전 (선택적)
        if (_inputReader.LookInput.magnitude > 0 && _player.MainCameraTransform != null)
        {
            // 카메라 방향으로 플레이어 회전
            Quaternion targetRotation = Quaternion.LookRotation(_player.MainCameraTransform.forward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _godModeRotationSpeed);
        }
    }

    #endregion
} 