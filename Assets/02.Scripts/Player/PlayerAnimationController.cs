using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어의 애니메이션을 제어하는 컨트롤러
/// </summary>
public class PlayerAnimationController : MonoBehaviour
{
    #region 컴포넌트 참조

    [Header("외부 컴포넌트")]
    [Tooltip("플레이어 메인 컴포넌트")]
    [SerializeField] private Player _player;
    [Tooltip("플레이어 애니메이션을 제어하는 Animator 컴포넌트")]
    [SerializeField] private Animator _animator;

    // 내부 컴포넌트 참조 (초기화 시 할당)
    private PlayerMovementController _movementController;
    private PlayerStateController _stateController;
    private InputReader _inputReader;
    

    #endregion

    #region 애니메이션 변수 해쉬
    private readonly int _movementInputTappedHash = Animator.StringToHash("MovementInputTapped");
    private readonly int _movementInputPressedHash = Animator.StringToHash("MovementInputPressed");
    private readonly int _movementInputHeldHash = Animator.StringToHash("MovementInputHeld");
    private readonly int _shuffleDirectionXHash = Animator.StringToHash("ShuffleDirectionX");
    private readonly int _shuffleDirectionZHash = Animator.StringToHash("ShuffleDirectionZ");

    private readonly int _moveSpeedHash = Animator.StringToHash("MoveSpeed");
    private readonly int _currentGaitHash = Animator.StringToHash("CurrentGait");

    private readonly int _isJumpingAnimHash = Animator.StringToHash("IsJumping");
    private readonly int _fallingDurationHash = Animator.StringToHash("FallingDuration");

    private readonly int _inclineAngleHash = Animator.StringToHash("InclineAngle");

    private readonly int _strafeDirectionXHash = Animator.StringToHash("StrafeDirectionX");
    private readonly int _strafeDirectionZHash = Animator.StringToHash("StrafeDirectionZ");

    private readonly int _forwardStrafeHash = Animator.StringToHash("ForwardStrafe");
    private readonly int _cameraRotationOffsetHash = Animator.StringToHash("CameraRotationOffset");
    private readonly int _isStrafingHash = Animator.StringToHash("IsStrafing");
    private readonly int _isTurningInPlaceHash = Animator.StringToHash("IsTurningInPlace");

    private readonly int _isCrouchingHash = Animator.StringToHash("IsCrouching");

    private readonly int _isWalkingHash = Animator.StringToHash("IsWalking");
    private readonly int _isStoppedHash = Animator.StringToHash("IsStopped");
    private readonly int _isStartingHash = Animator.StringToHash("IsStarting");

    private readonly int _isGroundedHash = Animator.StringToHash("IsGrounded");

    private readonly int _leanValueHash = Animator.StringToHash("LeanValue");
    private readonly int _headLookXHash = Animator.StringToHash("HeadLookX");
    private readonly int _headLookYHash = Animator.StringToHash("HeadLookY");

    private readonly int _bodyLookXHash = Animator.StringToHash("BodyLookX");
    private readonly int _bodyLookYHash = Animator.StringToHash("BodyLookY");

    private readonly int _movementStartDirectionHash = Animator.StringToHash("MovementStartDirection");

    #endregion

    #region Head Look 설정

    [Header("플레이어 머리 회전")]
    [Tooltip("머리 회전이 활성화되었는지 여부를 나타내는 플래그")]
    [SerializeField]
    private bool _enableHeadTurn = true;
    [Tooltip("머리 회전 지연 시간")]
    [SerializeField]
    private float _headLookDelay;
    [Tooltip("머리 회전을 위한 X축 값")]
    [SerializeField]
    private float _headLookX;
    [Tooltip("머리 회전을 위한 Y축 값")]
    [SerializeField]
    private float _headLookY;
    [Tooltip("X축 머리 회전을 위한 커브")]
    [SerializeField]
    private AnimationCurve _headLookXCurve;

    #endregion

    #region Body Look 설정

    [Header("플레이어 몸체 회전")]
    [Tooltip("몸체 회전이 활성화되었는지 여부를 나타내는 플래그")]
    [SerializeField]
    private bool _enableBodyTurn = true;
    [Tooltip("몸체 회전 지연 시간")]
    [SerializeField]
    private float _bodyLookDelay;
    [Tooltip("몸체 회전을 위한 X축 값")]
    [SerializeField]
    private float _bodyLookX;
    [Tooltip("몸체 회전을 위한 Y축 값")]
    [SerializeField]
    private float _bodyLookY;
    [Tooltip("X축 몸체 회전을 위한 커브")]
    [SerializeField]
    private AnimationCurve _bodyLookXCurve;

    #endregion

    #region Lean 설정

    [Header("플레이어 기울기")]
    [Tooltip("기울기가 활성화되었는지 여부를 나타내는 플래그")]
    [SerializeField]
    private bool _enableLean = true;
    [Tooltip("기울기 지연 시간")]
    [SerializeField]
    private float _leanDelay;
    [Tooltip("기울기 값")]
    [SerializeField]
    private float _leanValue;
    [Tooltip("기울기를 위한 커브")]
    [SerializeField]
    private AnimationCurve _leanCurve;

    #endregion

    #region 애니메이션 상태 변수

    private bool _isStarting;
    private bool _isStopped = true;
    private bool _isTurningInPlace;
    private float _initialLeanValue;
    private float _initialTurnValue;
    private Vector3 _currentRotation;
    private Vector3 _previousRotation;
    private float _rotationRate;
    private float _strafeDirectionX;
    private float _strafeDirectionZ;
    private float _shuffleDirectionX;
    private float _shuffleDirectionZ;
    private float _movementStartDirection;
    private float _movementInputDuration;
    private bool _movementInputDetected;
    private bool _movementInputTapped;
    private bool _movementInputPressed;
    private bool _movementInputHeld;
    private float _cameraRotationOffset;

    #endregion

    #region 이동 입력 설정
    [Header("이동 입력 설정")]
    [Tooltip("버튼 홀드 지속 시간 임계값")]
    [SerializeField] private float _buttonHoldThreshold = 0.15f;
    
    // 애니메이션 보간 상수
    private const float ANIMATION_DAMP_TIME = 5f;
    #endregion

    #region Unity 라이프사이클

    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        SubscribeToEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void Update()
    {
        UpdateAnimationParameters();
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

        // 컴포넌트가 Inspector에서 할당되지 않은 경우 자동으로 찾기
        if (_animator == null)
            _animator = GetComponent<Animator>();

        // Player 컴포넌트에서 필요한 컴포넌트 가져오기
        if (_player != null)
        {
            _movementController = _player.MovementController;
            _stateController = _player.StateController;
            _inputReader = _player.InputReader;
           
        }
        else
        {
            Debug.LogError("PlayerAnimationController: Player 컴포넌트를 찾을 수 없습니다!");
        }

        ValidateComponents();
    }

    /// <summary>
    /// 필요한 컴포넌트가 모두 할당되었는지 확인합니다
    /// </summary>
    private void ValidateComponents()
    {
        if (_player == null)
            Debug.LogError("PlayerAnimationController: Player 컴포넌트가 할당되지 않았습니다!");

        if (_animator == null)
            Debug.LogError("PlayerAnimationController: Animator 컴포넌트가 할당되지 않았습니다!");
    }

    #endregion

    #region 이벤트 핸들러

    /// <summary>
    /// 이벤트에 구독합니다
    /// </summary>
    private void SubscribeToEvents()
    {
        // 리플렉션 대신 직접 이벤트에 구독
        if (_stateController != null)
        {
            _stateController.OnStateChanged += HandleStateChanged;
            _stateController.OnGaitChanged += HandleGaitChanged;
        }
    }

    /// <summary>
    /// 이벤트 구독을 해제합니다
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        // 이벤트 구독 해제
        if (_stateController != null)
        {
            _stateController.OnStateChanged -= HandleStateChanged;
            _stateController.OnGaitChanged -= HandleGaitChanged;
        }
    }

    /// <summary>
    /// 플레이어 상태 변경 이벤트 처리
    /// </summary>
    private void HandleStateChanged(PlayerAnimationState previousState, PlayerAnimationState newState)
    {
        // 상태에 따른 애니메이션 처리
        if (newState == PlayerAnimationState.Jump)
        {
            _animator.SetBool(_isJumpingAnimHash, true);
        }
        else
        {
            _animator.SetBool(_isJumpingAnimHash, false);
        }
    }

    /// <summary>
    /// 플레이어 걸음걸이 변경 이벤트 처리
    /// </summary>
    private void HandleGaitChanged(PlayerGaitState previousGait, PlayerGaitState newGait)
    {
        // 걸음걸이 상태에 따른 애니메이션 파라미터 설정
        int gaitValue = 0;
        
        switch (newGait)
        {
            case PlayerGaitState.Idle:
                gaitValue = 0;
                break;
            case PlayerGaitState.Walk:
                gaitValue = 1;
                break;
            case PlayerGaitState.Run:
                gaitValue = 2;
                break;
            case PlayerGaitState.Sprint:
                gaitValue = 3;
                break;
        }
        
        _animator.SetInteger(_currentGaitHash, gaitValue);
    }

    #endregion

    #region 애니메이션 파라미터 업데이트

    /// <summary>
    /// 애니메이션 파라미터를 업데이트합니다
    /// </summary>
    private void UpdateAnimationParameters()
    {
        UpdateMovementInputState();
        UpdateStrafeDirection();
        CheckIfStarting();
        UpdateLeanAndLook();
        
        // 애니메이션 파라미터 설정
        float speed = 0f;
        float fallingDuration = 0f;
        float inclineAngle = 0f;
        bool isGrounded = true;
        bool isCrouching = false;
        bool isWalking = false;
        bool isStrafing = false;
        
        // MovementController에서 값을 가져옴
        if (_movementController != null)
        {
            speed = _movementController.Speed2D;
            fallingDuration = _movementController.FallingDuration;
            inclineAngle = _movementController.InclineAngle;
            isGrounded = _movementController.IsGrounded;
            isCrouching = _movementController.IsCrouching;
            isWalking = _movementController.IsWalking;
            isStrafing = _movementController.IsStrafing;
        }
        
        _animator.SetFloat(_moveSpeedHash, speed);
        _animator.SetFloat(_fallingDurationHash, fallingDuration);
        _animator.SetFloat(_inclineAngleHash, inclineAngle);
        _animator.SetBool(_isGroundedHash, isGrounded);
        _animator.SetBool(_isCrouchingHash, isCrouching);
        _animator.SetBool(_isWalkingHash, isWalking);
        
        _animator.SetFloat(_leanValueHash, _leanValue);
        _animator.SetFloat(_headLookXHash, _headLookX);
        _animator.SetFloat(_headLookYHash, _headLookY);
        _animator.SetFloat(_bodyLookXHash, _bodyLookX);
        _animator.SetFloat(_bodyLookYHash, _bodyLookY);

        _animator.SetFloat(_isStrafingHash, isStrafing ? 1f : 0f);
        
        _animator.SetFloat(_strafeDirectionXHash, _strafeDirectionX);
        _animator.SetFloat(_strafeDirectionZHash, _strafeDirectionZ);
        
        _animator.SetFloat(_shuffleDirectionXHash, _shuffleDirectionX);
        _animator.SetFloat(_shuffleDirectionZHash, _shuffleDirectionZ);
        
        _animator.SetBool(_movementInputTappedHash, _movementInputTapped);
        _animator.SetBool(_movementInputPressedHash, _movementInputPressed);
        _animator.SetBool(_movementInputHeldHash, _movementInputHeld);
        
        _animator.SetBool(_isStoppedHash, _isStopped);
        _animator.SetBool(_isStartingHash, _isStarting);
        _animator.SetBool(_isTurningInPlaceHash, _isTurningInPlace);
        
        _animator.SetFloat(_movementStartDirectionHash, _movementStartDirection);
    }

    #endregion

    #region 이동 입력 처리

    /// <summary>
    /// 이동 입력 상태를 업데이트합니다
    /// </summary>
    private void UpdateMovementInputState()
    {
        if (_inputReader != null)
        {
            _movementInputDetected = _inputReader._movementInputDetected;
            
            // 이동 입력 지속 시간 업데이트
            if (_movementInputDetected)
            {
                if (_inputReader._movementInputDuration == 0)
                {
                    _movementInputTapped = true;
                    _movementInputPressed = false;
                    _movementInputHeld = false;
                }
                else if (_inputReader._movementInputDuration > 0 && _inputReader._movementInputDuration < _buttonHoldThreshold)
                {
                    _movementInputTapped = false;
                    _movementInputPressed = true;
                    _movementInputHeld = false;
                }
                else
                {
                    _movementInputTapped = false;
                    _movementInputPressed = false;
                    _movementInputHeld = true;
                }
                
                _inputReader._movementInputDuration += Time.deltaTime;
            }
            else
            {
                _inputReader._movementInputDuration = 0f;
                _movementInputTapped = false;
                _movementInputPressed = false;
                _movementInputHeld = false;
            }
        }
        else
        {
            _movementInputDetected = false;
            _movementInputDuration = 0f;
            _movementInputTapped = false;
            _movementInputPressed = false;
            _movementInputHeld = false;
        }
    }

    /// <summary>
    /// 스트레이프 방향을 업데이트합니다
    /// </summary>
    private void UpdateStrafeDirection()
    {
        if (_movementController == null)
            return;
            
        // 캐릭터의 전방 및 오른쪽 벡터 계산
        Vector3 characterForward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
        Vector3 characterRight = new Vector3(transform.right.x, 0f, transform.right.z).normalized;
        Vector3 directionForward = new Vector3(_movementController.MoveDirection.x, 0f, _movementController.MoveDirection.z).normalized;
        
        if (directionForward.magnitude > 0.01f)
        {
            // 원본 코드와 동일하게 Vector3.Dot을 사용하여 방향 계산
            float targetZ = Vector3.Dot(characterForward, directionForward);
            float targetX = Vector3.Dot(characterRight, directionForward);
            
            // 스트레이프 방향 부드럽게 업데이트
            _strafeDirectionZ = Mathf.Lerp(_strafeDirectionZ, targetZ, ANIMATION_DAMP_TIME * Time.deltaTime);
            _strafeDirectionX = Mathf.Lerp(_strafeDirectionX, targetX, ANIMATION_DAMP_TIME * Time.deltaTime);
            _strafeDirectionZ = Mathf.Round(_strafeDirectionZ * 1000f) / 1000f;
            _strafeDirectionX = Mathf.Round(_strafeDirectionX * 1000f) / 1000f;
            
            // 셔플 방향 설정 (제자리에서의 작은 움직임)
            _shuffleDirectionZ = Vector3.Dot(characterForward, directionForward);
            _shuffleDirectionX = Vector3.Dot(characterRight, directionForward);
        }
        else
        {
            // 이동이 없을 때는 기본값 설정
            _strafeDirectionZ = Mathf.Lerp(_strafeDirectionZ, 1f, ANIMATION_DAMP_TIME * Time.deltaTime);
            _strafeDirectionX = Mathf.Lerp(_strafeDirectionX, 0f, ANIMATION_DAMP_TIME * Time.deltaTime);
            _shuffleDirectionZ = 1f;
            _shuffleDirectionX = 0f;
        }
    }

    /// <summary>
    /// 움직임 시작 여부를 확인합니다
    /// </summary>
    private void CheckIfStarting()
    {
        float speed = 0f;
        
        // MovementController에서 속도 값을 가져옴
        if (_movementController != null && _movementController.GetType().GetProperty("Speed2D") != null)
        {
            speed = (float)_movementController.GetType().GetProperty("Speed2D").GetValue(_movementController);
        }
        
        // 정지 상태 업데이트
        _isStopped = speed < 0.1f;
        
        // 시작 상태 업데이트
        if (_isStopped && _movementInputDetected)
        {
            _isStarting = true;
            
            // 시작 방향 계산 (입력 방향에 따라 각도 결정)
            Vector2 inputVector = _inputReader != null ? _inputReader._moveComposite : Vector2.zero;
            _movementStartDirection = Mathf.Atan2(inputVector.x, inputVector.y) * Mathf.Rad2Deg;
            
            // 각도를 0-360 범위로 변환
            if (_movementStartDirection < 0)
                _movementStartDirection += 360f;
        }
        else if (speed > 0.5f)
        {
            _isStarting = false;
        }
    }

    #endregion

    #region 기울기 및 회전 처리

    /// <summary>
    /// 플레이어의 기울기와 시선 방향을 업데이트합니다
    /// </summary>
    private void UpdateLeanAndLook()
    {
        // 회전 속도 계산
        _currentRotation = transform.forward;

        _rotationRate = _currentRotation != _previousRotation
            ? Vector3.SignedAngle(_currentRotation, _previousRotation, Vector3.up) / Time.deltaTime * -1f
            : 0f;
        
        // 기울기 및 시선 방향 업데이트
        UpdateLean();
        UpdateLook();
    }

    /// <summary>
    /// 플레이어의 기울기를 업데이트합니다
    /// </summary>
    private void UpdateLean()
    {
        bool leanActivated = _enableLean;
        float maxLeanRotationRate = 5f;
        float leanSmoothness = 5f;
        
        _initialLeanValue = leanActivated ? _rotationRate : 0f;
        
        _leanValue = CalculateSmoothedValue(
            _leanValue,
            _initialLeanValue,
            maxLeanRotationRate,
            leanSmoothness,
            _leanCurve,
            _leanValue,
            true
        );
    }

    /// <summary>
    /// 플레이어의 시선 방향을 업데이트합니다
    /// </summary>
    private void UpdateLook()
    {
       bool headLookActivated = _enableHeadTurn;
        bool bodyLookActivated = _enableBodyTurn;
        float maxLeanRotationRate = 275f;

        // 머리 회전 처리
        float headTurnSmoothness = 5f;

        if (headLookActivated && _isTurningInPlace)
        {
            _initialTurnValue = _cameraRotationOffset;
            _headLookX = Mathf.Lerp(_headLookX, _initialTurnValue / 200f, headTurnSmoothness * Time.deltaTime);
        }
        else
        {
            _initialTurnValue = headLookActivated ? _rotationRate : 0f;
            _headLookX = CalculateSmoothedValue(
                _headLookX,
                _initialTurnValue,
                maxLeanRotationRate,
                headTurnSmoothness,
                _headLookXCurve,
                _headLookX,
                false
            );
        }

        // 몸체 회전 처리
        float bodyTurnSmoothness = 5f;

        _initialTurnValue = bodyLookActivated ? _rotationRate : 0f;

        _bodyLookX = CalculateSmoothedValue(
            _bodyLookX,
            _initialTurnValue,
            maxLeanRotationRate,
            bodyTurnSmoothness,
            _bodyLookXCurve,
            _bodyLookX,
            false
        );

        // Camera.main을 사용하여 카메라 틸트 계산
        float cameraTilt = _player.MainCameraTransform.eulerAngles.x;
        cameraTilt = (cameraTilt > 180f ? cameraTilt - 360f : cameraTilt) / -180f;
        cameraTilt = Mathf.Clamp(cameraTilt, -0.1f, 1.0f);

        _headLookY = cameraTilt;
        _bodyLookY = cameraTilt;

        _previousRotation = _currentRotation;

    }

    /// <summary>
    /// 부드러운 값을 계산합니다
    /// </summary>
    /// <param name="currentValue">현재 값</param>
    /// <param name="targetValue">목표 값</param>
    /// <param name="maxValue">최대 값</param>
    /// <param name="smoothness">부드러움 정도</param>
    /// <param name="curve">애니메이션 커브</param>
    /// <param name="referenceValue">참조 값</param>
    /// <param name="useCurve">커브 사용 여부</param>
    /// <returns>계산된 부드러운 값</returns>
    private float CalculateSmoothedValue(
        float currentValue,
        float targetValue,
        float maxValue,
        float smoothness,
        AnimationCurve curve,
        float referenceValue,
        bool useCurve)
    {
        float normalizedTarget = Mathf.Clamp(targetValue / maxValue, -1f, 1f);
        
        float curveMultiplier = useCurve ? curve.Evaluate(referenceValue) : 1f;
        
        float smoothedValue = Mathf.Lerp(
            currentValue,
            normalizedTarget * curveMultiplier,
            smoothness * Time.deltaTime
        );
        
        return smoothedValue;
    }

    #endregion

    #region 공개 메서드

    /// <summary>
    /// 점프 애니메이션 상태를 설정합니다
    /// </summary>
    /// <param name="isJumping">점프 중인지 여부</param>
    public void SetJumping(bool isJumping)
    {
        _animator.SetBool(_isJumpingAnimHash, isJumping);
    }

    /// <summary>
    /// 웅크리기 애니메이션 상태를 설정합니다
    /// </summary>
    /// <param name="isCrouching">웅크리고 있는지 여부</param>
    public void SetCrouching(bool isCrouching)
    {
        _animator.SetBool(_isCrouchingHash, isCrouching);
    }

    /// <summary>
    /// 낙하 지속 시간을 설정합니다
    /// </summary>
    /// <param name="duration">낙하 지속 시간</param>
    public void SetFallingDuration(float duration)
    {
        _animator.SetFloat(_fallingDurationHash, duration);
    }

    /// <summary>
    /// 지면에 있는지 여부를 설정합니다
    /// </summary>
    /// <param name="isGrounded">지면에 있는지 여부</param>
    public void SetGrounded(bool isGrounded)
    {
        _animator.SetBool(_isGroundedHash, isGrounded);
    }

    /// <summary>
    /// 이동 속도를 설정합니다
    /// </summary>
    /// <param name="speed">이동 속도</param>
    public void SetMoveSpeed(float speed)
    {
        _animator.SetFloat(_moveSpeedHash, speed);
    }

    #endregion
}
