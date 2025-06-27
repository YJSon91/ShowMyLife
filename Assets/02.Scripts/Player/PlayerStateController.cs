using System;
using UnityEngine;

/// <summary>
/// 플레이어의 상태를 관리하는 컨트롤러
/// </summary>
public class PlayerStateController : MonoBehaviour
{
    #region 컴포넌트 참조

    [Header("필수 컴포넌트")]
    [Tooltip("플레이어 메인 컴포넌트")]
    [SerializeField] private Player _player;
    
    // 내부 컴포넌트 참조 (초기화 시 할당)
    private PlayerAnimationController _animationController;
    private PlayerMovementController _movementController;
    private InputReader _inputReader;

    #endregion

    #region 상태 변수

    /// <summary>
    /// 현재 플레이어 애니메이션 상태
    /// </summary>
    private PlayerAnimationState _currentState = PlayerAnimationState.Base;

    /// <summary>
    /// 현재 플레이어 걸음걸이 상태
    /// </summary>
    private PlayerGaitState _currentGait = PlayerGaitState.Idle;

    #endregion

    #region 이벤트

    /// <summary>
    /// 상태가 변경될 때 발생하는 이벤트
    /// </summary>
    public event Action<PlayerAnimationState, PlayerAnimationState> OnStateChanged;

    /// <summary>
    /// 걸음걸이가 변경될 때 발생하는 이벤트
    /// </summary>
    public event Action<PlayerGaitState, PlayerGaitState> OnGaitChanged;

    #endregion

    #region 공개 속성

    /// <summary>
    /// 현재 플레이어 애니메이션 상태
    /// </summary>
    public PlayerAnimationState CurrentState => _currentState;

    /// <summary>
    /// 현재 플레이어 걸음걸이 상태
    /// </summary>
    public PlayerGaitState CurrentGait => _currentGait;

    #endregion

    #region Unity 라이프사이클

    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        // 초기 상태 설정
        SwitchState(PlayerAnimationState.Movement);
        
        // 입력 이벤트 구독
        _inputReader.onJumpPerformed += HandleJumpInput;
    }

    private void OnDestroy()
    {
        // 입력 이벤트 구독 해제
        _inputReader.onJumpPerformed -= HandleJumpInput;
    }

    private void Update()
    {
        UpdateCurrentState();
        UpdateGaitState();
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
            _animationController = _player.AnimationController;
            _movementController = _player.MovementController;
            _inputReader = _player.InputReader;
        }
        else
        {
            Debug.LogError("PlayerStateController: Player 컴포넌트를 찾을 수 없습니다!");
        }

        ValidateComponents();
    }

    /// <summary>
    /// 필요한 컴포넌트가 모두 할당되었는지 확인합니다
    /// </summary>
    private void ValidateComponents()
    {
        if (_animationController == null)
            Debug.LogError("PlayerStateController: PlayerAnimationController가 할당되지 않았습니다!");

        if (_movementController == null)
            Debug.LogError("PlayerStateController: PlayerMovementController가 할당되지 않았습니다!");

        if (_inputReader == null)
            Debug.LogError("PlayerStateController: InputReader가 할당되지 않았습니다!");
    }

    #endregion

    #region 상태 관리 메서드

    /// <summary>
    /// 현재 상태를 업데이트합니다
    /// </summary>
    private void UpdateCurrentState()
    {
        switch (_currentState)
        {
            case PlayerAnimationState.Base:
                UpdateBaseState();
                break;
            case PlayerAnimationState.Movement:
                UpdateMovementState();
                break;
            case PlayerAnimationState.Jump:
                UpdateJumpState();
                break;
            case PlayerAnimationState.Fall:
                UpdateFallState();
                break;
            case PlayerAnimationState.Crouch:
                UpdateCrouchState();
                break;
        }
    }

    /// <summary>
    /// 걸음걸이 상태를 업데이트합니다
    /// </summary>
    private void UpdateGaitState()
    {
        // 속도에 따라 걸음걸이 상태 결정
        float speed = _movementController.Speed2D;
        float runThreshold = (_movementController._walkSpeed + _movementController._runSpeed) / 2;
        float sprintThreshold = (_movementController._runSpeed + _movementController._sprintSpeed) / 2;

        PlayerGaitState newGait;
        if (speed < 0.01f)
        {
            newGait = PlayerGaitState.Idle;
        }
        else if (speed < runThreshold)
        {
            newGait = PlayerGaitState.Walk;
        }
        else if (speed < sprintThreshold)
        {
            newGait = PlayerGaitState.Run;
        }
        else
        {
            newGait = PlayerGaitState.Sprint;
        }

        // 상태가 변경된 경우에만 이벤트 발생
        if (newGait != _currentGait)
        {
            PlayerGaitState oldGait = _currentGait;
            _currentGait = newGait;
            OnGaitChanged?.Invoke(oldGait, _currentGait);
        }
    }

    /// <summary>
    /// 현재 상태를 전달된 상태로 전환합니다
    /// </summary>
    /// <param name="newState">전환할 상태</param>
    public void SwitchState(PlayerAnimationState newState)
    {
        if (_currentState == newState)
            return;

        PlayerAnimationState oldState = _currentState;
        
        // 현재 상태 종료
        ExitCurrentState();
        
        // 새 상태 설정
        _currentState = newState;
        
        // 새 상태 진입
        EnterState(newState);
        
        // 상태 변경 이벤트 발생
        OnStateChanged?.Invoke(oldState, newState);
    }

    /// <summary>
    /// 주어진 상태로 진입합니다
    /// </summary>
    /// <param name="stateToEnter">진입할 상태</param>
    private void EnterState(PlayerAnimationState stateToEnter)
    {
        switch (stateToEnter)
        {
            case PlayerAnimationState.Base:
                EnterBaseState();
                break;
            case PlayerAnimationState.Movement:
                EnterMovementState();
                break;
            case PlayerAnimationState.Jump:
                EnterJumpState();
                break;
            case PlayerAnimationState.Fall:
                EnterFallState();
                break;
            case PlayerAnimationState.Crouch:
                EnterCrouchState();
                break;
        }
    }

    /// <summary>
    /// 현재 상태에서 나갑니다
    /// </summary>
    private void ExitCurrentState()
    {
        switch (_currentState)
        {
            case PlayerAnimationState.Base:
                ExitBaseState();
                break;
            case PlayerAnimationState.Movement:
                ExitMovementState();
                break;
            case PlayerAnimationState.Jump:
                ExitJumpState();
                break;
            case PlayerAnimationState.Fall:
                ExitFallState();
                break;
            case PlayerAnimationState.Crouch:
                ExitCrouchState();
                break;
        }
    }

    #endregion

    #region 입력 핸들러

    /// <summary>
    /// 점프 입력을 처리합니다
    /// </summary>
    private void HandleJumpInput()
    {
        switch (_currentState)
        {
            case PlayerAnimationState.Movement:
                SwitchState(PlayerAnimationState.Jump);
                break;
            case PlayerAnimationState.Crouch:
                if (!_movementController._cannotStandUp)
                {
                    _movementController.DeactivateCrouch();
                    SwitchState(PlayerAnimationState.Jump);
                }
                break;
        }
    }

    #endregion

    #region 상태 구현

    #region Base State

    /// <summary>
    /// 기본 상태로 진입합니다
    /// </summary>
    private void EnterBaseState()
    {
        // 기본 상태 진입 로직
    }

    /// <summary>
    /// 기본 상태를 업데이트합니다
    /// </summary>
    private void UpdateBaseState()
    {
        // 기본 상태 업데이트 로직
    }

    /// <summary>
    /// 기본 상태에서 나갑니다
    /// </summary>
    private void ExitBaseState()
    {
        // 기본 상태 종료 로직
    }

    #endregion

    #region Movement State

    /// <summary>
    /// 이동 상태로 진입합니다
    /// </summary>
    private void EnterMovementState()
    {
        // 이동 상태 진입 로직
    }

    /// <summary>
    /// 이동 상태를 업데이트합니다
    /// </summary>
    private void UpdateMovementState()
    {
        // 지면 확인
        _movementController.GroundedCheck();

        // 지면에 있지 않으면 낙하 상태로 전환
        if (!_movementController.IsGrounded)
        {
            SwitchState(PlayerAnimationState.Fall);
            return;
        }

        // 웅크리고 있으면 웅크리기 상태로 전환
        if (_movementController.IsCrouching)
        {
            SwitchState(PlayerAnimationState.Crouch);
            return;
        }

        // 이동 로직 실행
        _movementController.CalculateMoveDirection();
        _movementController.FaceMoveDirection();
        _movementController.Move();
    }

    /// <summary>
    /// 이동 상태에서 나갑니다
    /// </summary>
    private void ExitMovementState()
    {
        // 이동 상태 종료 로직
    }

    #endregion

    #region Jump State

    /// <summary>
    /// 점프 상태로 진입합니다
    /// </summary>
    private void EnterJumpState()
    {
        // 애니메이션 설정
        _animationController.SetJumping(true);

        // 슬라이딩 비활성화
        _movementController.DeactivateSliding();

        // 점프 힘 적용
        _movementController.Jump();
    }

    /// <summary>
    /// 점프 상태를 업데이트합니다
    /// </summary>
    private void UpdateJumpState()
    {
        // 중력 적용
        _movementController.ApplyGravity();

        // 하강 시작하면 낙하 상태로 전환
        if (_movementController._velocity.y <= 0f)
        {
            _animationController.SetJumping(false);
            SwitchState(PlayerAnimationState.Fall);
            return;
        }

        // 지면 확인
        _movementController.GroundedCheck();

        // 이동 로직 실행
        _movementController.CalculateMoveDirection();
        _movementController.FaceMoveDirection();
        _movementController.Move();
    }

    /// <summary>
    /// 점프 상태에서 나갑니다
    /// </summary>
    private void ExitJumpState()
    {
        // 애니메이션 설정
        _animationController.SetJumping(false);
    }

    #endregion

    #region Fall State

    /// <summary>
    /// 낙하 상태로 진입합니다
    /// </summary>
    private void EnterFallState()
    {
        // 낙하 지속 시간 초기화
        _movementController.ResetFallingDuration();
        _movementController._velocity.y = 0f;

        // 웅크리기 비활성화
        _movementController.DeactivateCrouch();
        _movementController.DeactivateSliding();
    }

    /// <summary>
    /// 낙하 상태를 업데이트합니다
    /// </summary>
    private void UpdateFallState()
    {
        // 지면 확인
        _movementController.GroundedCheck();

        // 이동 로직 실행
        _movementController.CalculateMoveDirection();
        _movementController.FaceMoveDirection();
        _movementController.ApplyGravity();
        _movementController.Move();

        // 낙하 지속 시간 업데이트
        _movementController.UpdateFallingDuration();

        // 지면에 착지하면 이동 상태로 전환
        if (_movementController.IsGrounded)
        {
            SwitchState(PlayerAnimationState.Movement);
        }
    }

    /// <summary>
    /// 낙하 상태에서 나갑니다
    /// </summary>
    private void ExitFallState()
    {
        // 낙하 상태 종료 로직
    }

    #endregion

    #region Crouch State

    /// <summary>
    /// 웅크리기 상태로 진입합니다
    /// </summary>
    private void EnterCrouchState()
    {
        // 웅크리기 상태 진입 로직
    }

    /// <summary>
    /// 웅크리기 상태를 업데이트합니다
    /// </summary>
    private void UpdateCrouchState()
    {
        // 지면 확인
        _movementController.GroundedCheck();

        // 지면에 있지 않으면 낙하 상태로 전환
        if (!_movementController.IsGrounded)
        {
            _movementController.DeactivateCrouch();
            SwitchState(PlayerAnimationState.Fall);
            return;
        }

        // 천장 높이 확인
        _movementController.CeilingHeightCheck();

        // 웅크리기가 해제되었으면 이동 상태로 전환
        if (!_movementController.IsCrouching)
        {
            SwitchState(PlayerAnimationState.Movement);
            return;
        }

        // 이동 로직 실행
        _movementController.CalculateMoveDirection();
        _movementController.FaceMoveDirection();
        _movementController.Move();
    }

    /// <summary>
    /// 웅크리기 상태에서 나갑니다
    /// </summary>
    private void ExitCrouchState()
    {
        // 웅크리기 상태 종료 로직
    }

    #endregion

    #endregion
} 