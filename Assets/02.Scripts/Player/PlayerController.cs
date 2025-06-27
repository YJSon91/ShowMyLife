using System.Collections.Generic;
using UnityEngine;


    public class PlayerController : MonoBehaviour
    {
        private enum AnimationState
        {
            Base,
            Movement,
            Jump,
            Fall,
            Crouch
        }
        private enum GaitState
        {
            Idle,
            Walk,
            Run,
            Sprint
        }

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

        #region 플레이어 설정 변수

        #region Scripts/Objects

        [Header("외부 컴포넌트")]
        [Tooltip("카메라 동작을 제어하는 스크립트")]
        [SerializeField]
        private PlayerCamera _cameraController;
        [Tooltip("InputReader는 플레이어 입력을 처리합니다")]
        [SerializeField]
        private InputReader _inputReader;
        [Tooltip("플레이어 애니메이션을 제어하는 Animator 컴포넌트")]
        [SerializeField]
        private Animator _animator;
        [Tooltip("플레이어 이동을 제어하는 Character Controller 컴포넌트")]
        [SerializeField]
        private CharacterController _controller;

        #endregion

        #region Movement 설정

        [Header("플레이어 이동")]
        [Header("기본 설정")]
        [Tooltip("캐릭터가 항상 카메라 방향을 바라보도록 할지 여부")]
        [SerializeField]
        private bool _alwaysStrafe = true;
        [Tooltip("걷기 상태나 반누름 시 플레이어의 가장 느린 이동 속도")]
        [SerializeField]
        private float _walkSpeed = 1.4f;
        [Tooltip("플레이어의 기본 이동 속도")]
        [SerializeField]
        private float _runSpeed = 2.5f;
        [Tooltip("플레이어의 최고 이동 속도")]
        [SerializeField]
        private float _sprintSpeed = 7f;
        [Tooltip("속도 변경을 위한 감쇠 계수")]
        [SerializeField]
        private float _speedChangeDamping = 10f;
        [Tooltip("회전 부드러움 계수")]
        [SerializeField]
        private float _rotationSmoothing = 10f;
        [Tooltip("카메라 회전 오프셋")]
        [SerializeField]
        private float _cameraRotationOffset;

        #endregion

        #region Shuffle 설정

        [Header("셔플")]
        [Tooltip("버튼 홀드 지속 시간 임계값")]
        [SerializeField]
        private float _buttonHoldThreshold = 0.15f;
        [Tooltip("X축 셔플 방향")]
        [SerializeField]
        private float _shuffleDirectionX;
        [Tooltip("Z축 셔플 방향")]
        [SerializeField]
        private float _shuffleDirectionZ;

        #endregion

        #region Capsule 설정

        [Header("캡슐 값")]
        [Tooltip("플레이어 캡슐의 서있는 높이")]
        [SerializeField]
        private float _capsuleStandingHeight = 1.8f;
        [Tooltip("플레이어 캡슐의 서있는 중심점")]
        [SerializeField]
        private float _capsuleStandingCentre = 0.93f;
        [Tooltip("플레이어 캡슐의 웅크린 높이")]
        [SerializeField]
        private float _capsuleCrouchingHeight = 1.2f;
        [Tooltip("플레이어 캡슐의 웅크린 중심점")]
        [SerializeField]
        private float _capsuleCrouchingCentre = 0.6f;

        #endregion

        #region Strafing 설정

        [Header("플레이어 스트레이핑")]
        [Tooltip("전방 스트레이핑 각도의 최소 임계값")]
        [SerializeField]
        private float _forwardStrafeMinThreshold = -55.0f;
        [Tooltip("전방 스트레이핑 각도의 최대 임계값")]
        [SerializeField]
        private float _forwardStrafeMaxThreshold = 125.0f;
        [Tooltip("현재 전방 스트레이핑 값")]
        [SerializeField]
        private float _forwardStrafe = 1f;

        #endregion

        #region Grounded 설정 

        [Header("지면 각도")]
        [Tooltip("지면 각도 확인을 위한 후방 레이 위치")]
        [SerializeField]
        private Transform _rearRayPos;
        [Tooltip("지면 각도 확인을 위한 전방 레이 위치")]
        [SerializeField]
        private Transform _frontRayPos;
        [Tooltip("지면 확인을 위한 레이어 마스크")]
        [SerializeField]
        private LayerMask _groundLayerMask;
        [Tooltip("현재 경사 각도")]
        [SerializeField]
        private float _inclineAngle;
        [Tooltip("거친 지면에 유용함")]
        [SerializeField]
        private float _groundedOffset = -0.14f;

        #endregion

        #region In-Air 설정

        [Header("플레이어 공중")]
        [Tooltip("플레이어가 점프할 때 적용되는 힘")]
        [SerializeField]
        private float _jumpForce = 10f;
        [Tooltip("공중에 있을 때의 중력 배수")]
        [SerializeField]
        private float _gravityMultiplier = 2f;
        [Tooltip("낙하 지속 시간")]
        [SerializeField]
        private float _fallingDuration;

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
        [Tooltip("기울기의 현재 값")]
        [SerializeField]
        private float _leanValue;
        [Tooltip("기울기를 위한 커브")]
        [SerializeField]
        private AnimationCurve _leanCurve;
        [Tooltip("머리 기울기 시선 지연 시간")]
        [SerializeField]
        private float _leansHeadLooksDelay;
        [Tooltip("애니메이션 클립이 종료되었는지 여부를 나타내는 플래그")]
        [SerializeField]
        private bool _animationClipEnd;

        #endregion

        #endregion

        #region Runtime Properties

        private readonly List<GameObject> _currentTargetCandidates = new List<GameObject>();
        private AnimationState _currentState = AnimationState.Base;
        private bool _cannotStandUp;
        private bool _crouchKeyPressed;
        private bool _isAiming;
        private bool _isCrouching;
        private bool _isGrounded = true;
        private bool _isLockedOn;
        private bool _isSliding;
        private bool _isSprinting;
        private bool _isStarting;
        private bool _isStopped = true;
        private bool _isStrafing;
        private bool _isTurningInPlace;
        private bool _isWalking;
        private bool _movementInputHeld;
        private bool _movementInputPressed;
        private bool _movementInputTapped;
        private float _currentMaxSpeed;
        private float _movementStartDirection;
        private float _movementStartTimer;
        private float _newDirectionDifferenceAngle;
        private float _speed2D;
        private float _strafeAngle;
        private float _strafeDirectionX;
        private float _strafeDirectionZ;
        private GaitState _currentGait;
        private Vector3 _currentRotation = new Vector3(0f, 0f, 0f);
        private Vector3 _moveDirection;
        private Vector3 _previousRotation;
        private Vector3 _velocity;

        #endregion

        #region Base State 변수

        private const float _ANIMATION_DAMP_TIME = 5f;
        private const float _STRAFE_DIRECTION_DAMP_TIME = 20f;
        private float _targetMaxSpeed;
        private float _fallStartTime;
        private float _rotationRate;
        private float _initialLeanValue;
        private float _initialTurnValue;
        private Vector3 _cameraForward;
        private Vector3 _targetVelocity;

        #endregion

        #region Animation Controller

        #region Start

        /// <inheritdoc cref="Start" />
        private void Start()
        {

            _inputReader.onWalkToggled += ToggleWalk;
            _inputReader.onSprintActivated += ActivateSprint;
            _inputReader.onSprintDeactivated += DeactivateSprint;
            _inputReader.onCrouchActivated += ActivateCrouch;
            _inputReader.onCrouchDeactivated += DeactivateCrouch;

            _isStrafing = _alwaysStrafe;

            SwitchState(AnimationState.Movement);
        }

        #endregion


        #region Walking State

        /// <summary>
        ///     걷기 상태를 토글합니다.
        /// </summary>
        private void ToggleWalk()
        {
            EnableWalk(!_isWalking);
        }

        /// <summary>
        ///     걷기 상태를 전달된 상태로 설정합니다.
        /// </summary>
        /// <param name="enable">설정할 상태.</param>
        private void EnableWalk(bool enable)
        {
            _isWalking = enable && _isGrounded && !_isSprinting;
        }

        #endregion

        #region Sprinting State

        /// <summary>
        ///     달리기 동작을 활성화합니다.
        /// </summary>
        private void ActivateSprint()
        {
            if (!_isCrouching)
            {
                EnableWalk(false);
                _isSprinting = true;
                _isStrafing = false;
            }
        }

        /// <summary>
        ///     달리기 동작을 비활성화합니다.
        /// </summary>
        private void DeactivateSprint()
        {
            _isSprinting = false;

            if (_alwaysStrafe || _isAiming || _isLockedOn)
            {
                _isStrafing = true;
            }
        }

        #endregion

        #region Crouching State

        /// <summary>
        ///     웅크리기 동작을 활성화합니다.
        /// </summary>
        private void ActivateCrouch()
        {
            _crouchKeyPressed = true;

            if (_isGrounded)
            {
                CapsuleCrouchingSize(true);
                DeactivateSprint();
                _isCrouching = true;
            }
        }

        /// <summary>
        ///     웅크리기 동작을 비활성화합니다.
        /// </summary>
        private void DeactivateCrouch()
        {
            _crouchKeyPressed = false;

            if (!_cannotStandUp && !_isSliding)
            {
                CapsuleCrouchingSize(false);
                _isCrouching = false;
            }
        }

        /// <summary>
        ///     슬라이딩 동작을 활성화합니다.
        /// </summary>
        public void ActivateSliding()
        {
            _isSliding = true;
        }

        /// <summary>
        ///     슬라이딩 동작을 비활성화합니다.
        /// </summary>
        public void DeactivateSliding()
        {
            _isSliding = false;
        }

        /// <summary>
        ///     전달된 부울 값에 따라 플레이어의 캡슐 크기를 조정합니다.
        /// </summary>
        /// <param name="crouching">플레이어가 웅크리고 있는지 여부.</param>
        private void CapsuleCrouchingSize(bool crouching)
        {
            if (crouching)
            {
                _controller.center = new Vector3(0f, _capsuleCrouchingCentre, 0f);
                _controller.height = _capsuleCrouchingHeight;
            }
            else
            {
                _controller.center = new Vector3(0f, _capsuleStandingCentre, 0f);
                _controller.height = _capsuleStandingHeight;
            }
        }

        #endregion

        #endregion

        #region Shared State

        #region State Change

        /// <summary>
        ///     현재 상태를 전달된 상태로 전환합니다.
        /// </summary>
        /// <param name="newState">전환할 상태.</param>
        private void SwitchState(AnimationState newState)
        {
            ExitCurrentState();
            EnterState(newState);
        }

        /// <summary>
        ///     주어진 상태로 진입합니다.
        /// </summary>
        /// <param name="stateToEnter">진입할 상태.</param>
        private void EnterState(AnimationState stateToEnter)
        {
            _currentState = stateToEnter;
            switch (_currentState)
            {
                case AnimationState.Base:
                    EnterBaseState();
                    break;
                case AnimationState.Movement:
                    EnterMovementState();
                    break;
                case AnimationState.Jump:
                    EnterJumpState();
                    break;
                case AnimationState.Fall:
                    EnterFallState();
                    break;
                case AnimationState.Crouch:
                    EnterCrouchState();
                    break;
            }
        }

        /// <summary>
        ///     현재 상태에서 나갑니다.
        /// </summary>
        private void ExitCurrentState()
        {
            switch (_currentState)
            {
                case AnimationState.Movement:
                    ExitMovementState();
                    break;
                case AnimationState.Jump:
                    ExitJumpState();
                    break;
                case AnimationState.Crouch:
                    ExitCrouchState();
                    break;
            }
        }

        #endregion

        #region Updates

        /// <inheritdoc cref="Update" />
        private void Update()
        {
            switch (_currentState)
            {
                case AnimationState.Movement:
                    UpdateMovementState();
                    break;
                case AnimationState.Jump:
                    UpdateJumpState();
                    break;
                case AnimationState.Fall:
                    UpdateFallState();
                    break;
                case AnimationState.Crouch:
                    UpdateCrouchState();
                    break;
            }
        }

        /// <summary>
        ///     애니메이터를 최신 값으로 업데이트합니다.
        /// </summary>
        private void UpdateAnimatorController()
        {
            _animator.SetFloat(_leanValueHash, _leanValue);
            _animator.SetFloat(_headLookXHash, _headLookX);
            _animator.SetFloat(_headLookYHash, _headLookY);
            _animator.SetFloat(_bodyLookXHash, _bodyLookX);
            _animator.SetFloat(_bodyLookYHash, _bodyLookY);

            _animator.SetFloat(_isStrafingHash, _isStrafing ? 1.0f : 0.0f);

            _animator.SetFloat(_inclineAngleHash, _inclineAngle);

            _animator.SetFloat(_moveSpeedHash, _speed2D);
            _animator.SetInteger(_currentGaitHash, (int) _currentGait);

            _animator.SetFloat(_strafeDirectionXHash, _strafeDirectionX);
            _animator.SetFloat(_strafeDirectionZHash, _strafeDirectionZ);
            _animator.SetFloat(_forwardStrafeHash, _forwardStrafe);
            _animator.SetFloat(_cameraRotationOffsetHash, _cameraRotationOffset);

            _animator.SetBool(_movementInputHeldHash, _movementInputHeld);
            _animator.SetBool(_movementInputPressedHash, _movementInputPressed);
            _animator.SetBool(_movementInputTappedHash, _movementInputTapped);
            _animator.SetFloat(_shuffleDirectionXHash, _shuffleDirectionX);
            _animator.SetFloat(_shuffleDirectionZHash, _shuffleDirectionZ);

            _animator.SetBool(_isTurningInPlaceHash, _isTurningInPlace);
            _animator.SetBool(_isCrouchingHash, _isCrouching);

            _animator.SetFloat(_fallingDurationHash, _fallingDuration);
            _animator.SetBool(_isGroundedHash, _isGrounded);

            _animator.SetBool(_isWalkingHash, _isWalking);
            _animator.SetBool(_isStoppedHash, _isStopped);

            _animator.SetFloat(_movementStartDirectionHash, _movementStartDirection);
        }

        #endregion

        #endregion

        #region Base State

        #region Setup

        /// <summary>
        ///     기본 상태로 진입할 때 필요한 작업을 수행합니다.
        /// </summary>
        private void EnterBaseState()
        {
            _previousRotation = transform.forward;
        }

        /// <summary>
        ///     입력 유형을 계산하고 필요한 내부 상태를 설정합니다.
        /// </summary>
        private void CalculateInput()
        {
            if (_inputReader._movementInputDetected)
            {
                if (_inputReader._movementInputDuration == 0)
                {
                    _movementInputTapped = true;
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
                _inputReader._movementInputDuration = 0;
                _movementInputTapped = false;
                _movementInputPressed = false;
                _movementInputHeld = false;
            }

            _moveDirection = (_cameraController.GetCameraForwardZeroedYNormalised() * _inputReader._moveComposite.y)
                + (_cameraController.GetCameraRightZeroedYNormalised() * _inputReader._moveComposite.x);
        }

        #endregion

        #region Movement

        /// <summary>
        ///     플레이어의 이동을 수행합니다
        /// </summary>
        private void Move()
        {
            _controller.Move(_velocity * Time.deltaTime);
        }

        /// <summary>
        ///     플레이어에게 중력을 적용합니다.
        /// </summary>
        private void ApplyGravity()
        {
            if (_velocity.y > Physics.gravity.y)
            {
                _velocity.y += Physics.gravity.y * _gravityMultiplier * Time.deltaTime;
            }
        }

        /// <summary>
        ///     플레이어의 이동 방향을 계산하고 관련 플래그를 설정합니다.
        /// </summary>
        private void CalculateMoveDirection()
        {
            CalculateInput();

            if (!_isGrounded)
            {
                _targetMaxSpeed = _currentMaxSpeed;
            }
            else if (_isCrouching)
            {
                _targetMaxSpeed = _walkSpeed;
            }
            else if (_isSprinting)
            {
                _targetMaxSpeed = _sprintSpeed;
            }
            else if (_isWalking)
            {
                _targetMaxSpeed = _walkSpeed;
            }
            else
            {
                _targetMaxSpeed = _runSpeed;
            }

            _currentMaxSpeed = Mathf.Lerp(_currentMaxSpeed, _targetMaxSpeed, _ANIMATION_DAMP_TIME * Time.deltaTime);

            _targetVelocity.x = _moveDirection.x * _currentMaxSpeed;
            _targetVelocity.z = _moveDirection.z * _currentMaxSpeed;

            _velocity.z = Mathf.Lerp(_velocity.z, _targetVelocity.z, _speedChangeDamping * Time.deltaTime);
            _velocity.x = Mathf.Lerp(_velocity.x, _targetVelocity.x, _speedChangeDamping * Time.deltaTime);

            _speed2D = new Vector3(_velocity.x, 0f, _velocity.z).magnitude;
            _speed2D = Mathf.Round(_speed2D * 1000f) / 1000f;

            Vector3 playerForwardVector = transform.forward;

            _newDirectionDifferenceAngle = playerForwardVector != _moveDirection
                ? Vector3.SignedAngle(playerForwardVector, _moveDirection, Vector3.up)
                : 0f;

            CalculateGait();
        }

        ///     <pre>
        ///         캐릭터의 걸음걸이를 계산
        ///         현재 이동 걸음걸이(걷기, 달리기, 질주)를 계산
        ///         (점프, 착지 등에서 어떤 애니메이션을 사용할지 결정할 때 사용)
        ///         정지 = 0, 걷기 = 1, 달리기 = 2, 질주 = 3
        ///     </pre>
        /// </summary>
        private void CalculateGait()
        {
            float runThreshold = (_walkSpeed + _runSpeed) / 2;
            float sprintThreshold = (_runSpeed + _sprintSpeed) / 2;

            if (_speed2D < 0.01)
            {
                _currentGait = GaitState.Idle;
            }
            else if (_speed2D < runThreshold)
            {
                _currentGait = GaitState.Walk;
            }
            else if (_speed2D < sprintThreshold)
            {
                _currentGait = GaitState.Run;
            }
            else
            {
                _currentGait = GaitState.Sprint;
            }
        }

        /// <summary>
        ///     캐릭터의 이동 방향을 기준으로 바라보는 방향을 계산
        /// </summary>
        private void FaceMoveDirection()
        {
            Vector3 characterForward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
            Vector3 characterRight = new Vector3(transform.right.x, 0f, transform.right.z).normalized;
            Vector3 directionForward = new Vector3(_moveDirection.x, 0f, _moveDirection.z).normalized;

            _cameraForward = _cameraController.GetCameraForwardZeroedYNormalised();
            Quaternion strafingTargetRotation = Quaternion.LookRotation(_cameraForward);

            _strafeAngle = characterForward != directionForward ? Vector3.SignedAngle(characterForward, directionForward, Vector3.up) : 0f;

            _isTurningInPlace = false;

            if (_isStrafing)
            {
                if (_moveDirection.magnitude > 0.01)
                {
                    if (_cameraForward != Vector3.zero)
                    {
                        // 셔플 방향 값 - 이것들은 스트레이프 값과 별개로, 보간(lerp)을 하지않음, 즉시 회전
                        // 입력을 잃은 후에도 값이 0으로 돌아가지 않도록 값을 고정
                        // (그래야 블렌드 트리가 애니메이션 클립의 끝까지 작동)
                        _shuffleDirectionZ = Vector3.Dot(characterForward, directionForward);
                        _shuffleDirectionX = Vector3.Dot(characterRight, directionForward);

                        UpdateStrafeDirection(
                            Vector3.Dot(characterForward, directionForward),
                            Vector3.Dot(characterRight, directionForward)
                        );
                        _cameraRotationOffset = Mathf.Lerp(_cameraRotationOffset, 0f, _rotationSmoothing * Time.deltaTime);

                        float targetValue = _strafeAngle > _forwardStrafeMinThreshold && _strafeAngle < _forwardStrafeMaxThreshold ? 1f : 0f;

                        if (Mathf.Abs(_forwardStrafe - targetValue) <= 0.001f)
                        {
                            _forwardStrafe = targetValue;
                        }
                        else
                        {
                            float t = Mathf.Clamp01(_STRAFE_DIRECTION_DAMP_TIME * Time.deltaTime);
                            _forwardStrafe = Mathf.SmoothStep(_forwardStrafe, targetValue, t);
                        }
                    }

                    transform.rotation = Quaternion.Slerp(transform.rotation, strafingTargetRotation, _rotationSmoothing * Time.deltaTime);
                }
                else
                {
                    UpdateStrafeDirection(1f, 0f);

                    float t = 20 * Time.deltaTime;
                    float newOffset = 0f;

                    if (characterForward != _cameraForward)
                    {
                        newOffset = Vector3.SignedAngle(characterForward, _cameraForward, Vector3.up);
                    }

                    _cameraRotationOffset = Mathf.Lerp(_cameraRotationOffset, newOffset, t);

                    if (Mathf.Abs(_cameraRotationOffset) > 10)
                    {
                        _isTurningInPlace = true;
                    }
                }
            }
            else
            {
                UpdateStrafeDirection(1f, 0f);
                _cameraRotationOffset = Mathf.Lerp(_cameraRotationOffset, 0f, _rotationSmoothing * Time.deltaTime);

                _shuffleDirectionZ = 1;
                _shuffleDirectionX = 0;

                Vector3 faceDirection = new Vector3(_velocity.x, 0f, _velocity.z);

                if (faceDirection == Vector3.zero)
                {
                    return;
                }

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(faceDirection),
                    _rotationSmoothing * Time.deltaTime
                );
            }
        }

        /// <summary>
        ///     플레이어가 움직임을 멈췄는지 확인
        /// </summary>
        private void CheckIfStopped()
        {
            _isStopped = _moveDirection.magnitude == 0 && _speed2D < .5;
        }

        /// <summary>
        ///     플레이어가 움직이기 시작했는지 확인
        /// </summary>
        private void CheckIfStarting()
        {
            _movementStartTimer = VariableOverrideDelayTimer(_movementStartTimer);

            bool isStartingCheck = false;

            if (_movementStartTimer <= 0.0f)
            {
                if (_moveDirection.magnitude > 0.01 && _speed2D < 1 && !_isStrafing)
                {
                    isStartingCheck = true;
                }

                if (isStartingCheck)
                {
                    if (!_isStarting)
                    {
                        _movementStartDirection = _newDirectionDifferenceAngle;
                        _animator.SetFloat(_movementStartDirectionHash, _movementStartDirection);
                    }

                    float delayTime = 0.2f;
                    _leanDelay = delayTime;
                    _headLookDelay = delayTime;
                    _bodyLookDelay = delayTime;

                    _movementStartTimer = delayTime;
                }
            }
            else
            {
                isStartingCheck = true;
            }

            _isStarting = isStartingCheck;
            _animator.SetBool(_isStartingHash, _isStarting);
        }

        /// <summary>
        ///     제공된 값으로 스트레이프 방향 변수를 업데이트합니다.
        /// </summary>
        /// <param name="TargetZ">Z축에 설정할 값.</param>
        /// <param name="TargetX">X축에 설정할 값.</param>
        private void UpdateStrafeDirection(float TargetZ, float TargetX)
        {
            _strafeDirectionZ = Mathf.Lerp(_strafeDirectionZ, TargetZ, _ANIMATION_DAMP_TIME * Time.deltaTime);
            _strafeDirectionX = Mathf.Lerp(_strafeDirectionX, TargetX, _ANIMATION_DAMP_TIME * Time.deltaTime);
            _strafeDirectionZ = Mathf.Round(_strafeDirectionZ * 1000f) / 1000f;
            _strafeDirectionX = Mathf.Round(_strafeDirectionX * 1000f) / 1000f;
        }

        #endregion

        #region Ground Checks

        /// <summary>
        ///     캐릭터가 지면에 있는지 확인
        /// </summary>
        private void GroundedCheck()
        {
            Vector3 spherePosition = new Vector3(
                _controller.transform.position.x,
                _controller.transform.position.y - _groundedOffset,
                _controller.transform.position.z
            );
            _isGrounded = Physics.CheckSphere(spherePosition, _controller.radius, _groundLayerMask, QueryTriggerInteraction.Ignore);

            if (_isGrounded)
            {
                GroundInclineCheck();
            }
        }

        /// <summary>
        ///     지면 경사를 확인하고 필요한 변수를 설정
        /// </summary>
        private void GroundInclineCheck()
        {
            float rayDistance = Mathf.Infinity;
            _rearRayPos.rotation = Quaternion.Euler(transform.rotation.x, 0, 0);
            _frontRayPos.rotation = Quaternion.Euler(transform.rotation.x, 0, 0);

            Physics.Raycast(_rearRayPos.position, _rearRayPos.TransformDirection(-Vector3.up), out RaycastHit rearHit, rayDistance, _groundLayerMask);
            Physics.Raycast(
                _frontRayPos.position,
                _frontRayPos.TransformDirection(-Vector3.up),
                out RaycastHit frontHit,
                rayDistance,
                _groundLayerMask
            );

            Vector3 hitDifference = frontHit.point - rearHit.point;
            float xPlaneLength = new Vector2(hitDifference.x, hitDifference.z).magnitude;

            _inclineAngle = Mathf.Lerp(_inclineAngle, Mathf.Atan2(hitDifference.y, xPlaneLength) * Mathf.Rad2Deg, 20f * Time.deltaTime);
        }

        /// <summary>
        ///     웅크리고 있을 때 일어설 수 있는 충분한 공간이 있는지 플레이어 위의 천장 높이를 확인
        /// </summary>
        private void CeilingHeightCheck()
        {
            float rayDistance = Mathf.Infinity;
            float minimumStandingHeight = _capsuleStandingHeight - _frontRayPos.localPosition.y;

            Vector3 midpoint = new Vector3(transform.position.x, transform.position.y + _frontRayPos.localPosition.y, transform.position.z);
            if (Physics.Raycast(midpoint, transform.TransformDirection(Vector3.up), out RaycastHit ceilingHit, rayDistance, _groundLayerMask))
            {
                _cannotStandUp = ceilingHit.distance < minimumStandingHeight;
            }
            else
            {
                _cannotStandUp = false;
            }
        }

        #endregion

        #region Falling

        /// <summary>
        ///     낙하 지속 시간 변수를 초기화
        /// </summary>
        private void ResetFallingDuration()
        {
            _fallStartTime = Time.time;
            _fallingDuration = 0f;
        }

        /// <summary>
        ///     낙하 지속 시간 변수를 업데이트
        /// </summary>
        private void UpdateFallingDuration()
        {
            _fallingDuration = Time.time - _fallStartTime;
        }

        #endregion

        #region Checks

        /// <summary>
        ///     몸체 회전이 활성화될 수 있는지 확인하고 필요에 따라 활성화하거나 비활성화
        private void CheckEnableTurns()
        {
            _headLookDelay = VariableOverrideDelayTimer(_headLookDelay);
            _enableHeadTurn = _headLookDelay == 0.0f && !_isStarting;
            _bodyLookDelay = VariableOverrideDelayTimer(_bodyLookDelay);
            _enableBodyTurn = _bodyLookDelay == 0.0f && !(_isStarting || _isTurningInPlace);
        }

        /// <summary>
        ///     기울기가 활성화될 수 있는지 확인한 다음 필요에 따라 활성화하거나 비활성화
        /// </summary>
        private void CheckEnableLean()
        {
            _leanDelay = VariableOverrideDelayTimer(_leanDelay);
            _enableLean = _leanDelay == 0.0f && !(_isStarting || _isTurningInPlace);
        }

        #endregion

        #region Lean and Offsets

        /// <summary>
        ///     전달된 매개변수에 따라 필요한 회전 추가 값을 계산
        /// </summary>
        /// <param name="leansActivated">기울기가 활성화되었는지 여부.</param>
        /// <param name="headLookActivated">머리 회전이 활성화되었는지 여부.</param>
        /// <param name="bodyLookActivated">몸체 회전이 활성화되었는지 여부.</param>
        private void CalculateRotationalAdditives(bool leansActivated, bool headLookActivated, bool bodyLookActivated)
        {
            if (headLookActivated || leansActivated || bodyLookActivated)
            {
                _currentRotation = transform.forward;

                _rotationRate = _currentRotation != _previousRotation
                    ? Vector3.SignedAngle(_currentRotation, _previousRotation, Vector3.up) / Time.deltaTime * -1f
                    : 0f;
            }

            _initialLeanValue = leansActivated ? _rotationRate : 0f;

            float leanSmoothness = 5;
            float maxLeanRotationRate = 275.0f;

            float referenceValue = _speed2D / _sprintSpeed;
            _leanValue = CalculateSmoothedValue(
                _leanValue,
                _initialLeanValue,
                maxLeanRotationRate,
                leanSmoothness,
                _leanCurve,
                referenceValue,
                true
            );

            float headTurnSmoothness = 5f;

            if (headLookActivated && _isTurningInPlace)
            {
                _initialTurnValue = _cameraRotationOffset;
                _headLookX = Mathf.Lerp(_headLookX, _initialTurnValue / 200, 5f * Time.deltaTime);
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

            float cameraTilt = _cameraController.GetCameraTiltX();
            cameraTilt = (cameraTilt > 180f ? cameraTilt - 360f : cameraTilt) / -180;
            cameraTilt = Mathf.Clamp(cameraTilt, -0.1f, 1.0f);
            _headLookY = cameraTilt;
            _bodyLookY = cameraTilt;

            _previousRotation = _currentRotation;
        }

        /// <summary>
        ///     주어진 매개변수에서 주어진 변수와 대상 변수 사이의 부드러운 값을 계산
        /// </summary>
        /// <param name="mainVariable">부드럽게 할 변수.</param>
        /// <param name="newValue">목표 새 값.</param>
        /// <param name="maxRateChange">최대 변화율.</param>
        /// <param name="smoothness">부드러움 정도.</param>
        /// <param name="referenceCurve">참조 커브.</param>
        /// <param name="referenceValue">참조 값.</param>
        /// <param name="isMultiplier">값이 승수인지 여부.</param>
        /// <returns>부드럽게 처리된 값.</returns>
        private float CalculateSmoothedValue(
            float mainVariable,
            float newValue,
            float maxRateChange,
            float smoothness,
            AnimationCurve referenceCurve,
            float referenceValue,
            bool isMultiplier
        )
        {
            float changeVariable = newValue / maxRateChange;

            changeVariable = Mathf.Clamp(changeVariable, -1.0f, 1.0f);

            if (isMultiplier)
            {
                float multiplier = referenceCurve.Evaluate(referenceValue);
                changeVariable *= multiplier;
            }
            else
            {
                changeVariable = referenceCurve.Evaluate(changeVariable);
            }

            if (!changeVariable.Equals(mainVariable))
            {
                changeVariable = Mathf.Lerp(mainVariable, changeVariable, smoothness * Time.deltaTime);
            }

            return changeVariable;
        }

        /// <summary>
        ///     애니메이션 전환 문제를 방지하기 위한 제한된 오버라이드 지연을 제공
        /// </summary>
        /// <param name="timeVariable">사용할 시간 변수.</param>
        /// <returns>제한된 오버라이드 지연.</returns>
        private float VariableOverrideDelayTimer(float timeVariable)
        {
            if (timeVariable > 0.0f)
            {
                timeVariable -= Time.deltaTime;
                timeVariable = Mathf.Clamp(timeVariable, 0.0f, 1.0f);
            }
            else
            {
                timeVariable = 0.0f;
            }

            return timeVariable;
        }

        #endregion


        #region Movement State

        /// <summary>
        ///     진입 시 이동 상태를 설정
        /// </summary>
        private void EnterMovementState()
        {
            _inputReader.onJumpPerformed += MovementToJumpState;
        }

        /// <summary>
        ///     이동 상태를 업데이트
        /// </summary>
        private void UpdateMovementState()
        {
            GroundedCheck();

            if (!_isGrounded)
            {
                SwitchState(AnimationState.Fall);
            }

            if (_isCrouching)
            {
                SwitchState(AnimationState.Crouch);
            }

            CheckEnableTurns();
            CheckEnableLean();
            CalculateRotationalAdditives(_enableLean, _enableHeadTurn, _enableBodyTurn);

            CalculateMoveDirection();
            CheckIfStarting();
            CheckIfStopped();
            FaceMoveDirection();
            Move();
            UpdateAnimatorController();
        }

        /// <summary>
        ///     이동 상태에서 나갈 때 필요한 작업을 수행
        private void ExitMovementState()
        {
            _inputReader.onJumpPerformed -= MovementToJumpState;
        }

        /// <summary>
        ///     이동 상태에서 점프 상태로 이동
        /// </summary>
        private void MovementToJumpState()
        {
            SwitchState(AnimationState.Jump);
        }

        #endregion

        #region Jump State

        /// <summary>
        ///     진입 시 점프 상태를 설정
        /// </summary>
        private void EnterJumpState()
        {
            _animator.SetBool(_isJumpingAnimHash, true);

            _isSliding = false;

            _velocity = new Vector3(_velocity.x, _jumpForce, _velocity.z);
        }

        /// <summary>
        ///     점프 상태를 업데이트
        /// </summary>
        private void UpdateJumpState()
        {
            ApplyGravity();

            if (_velocity.y <= 0f)
            {
                _animator.SetBool(_isJumpingAnimHash, false);
                SwitchState(AnimationState.Fall);
            }

            GroundedCheck();

            CalculateRotationalAdditives(false, _enableHeadTurn, _enableBodyTurn);
            CalculateMoveDirection();
            FaceMoveDirection();
            Move();
            UpdateAnimatorController();
        }

        /// <summary>
        ///     점프 상태에서 나갈 때 필요한 작업을 수행
        /// </summary>
        private void ExitJumpState()
        {
            _animator.SetBool(_isJumpingAnimHash, false);
        }

        #endregion

        #region Fall State
        /// <summary>
        ///     진입 시 낙하 상태를 설정
        /// </summary>
        private void EnterFallState()
        {
            ResetFallingDuration();
            _velocity.y = 0f;

            DeactivateCrouch();
            _isSliding = false;
        }

        /// <summary>
        ///     낙하 상태를 업데이트
        /// </summary>
        private void UpdateFallState()
        {
            GroundedCheck();

            CalculateRotationalAdditives(false, _enableHeadTurn, _enableBodyTurn);

            CalculateMoveDirection();
            FaceMoveDirection();

            ApplyGravity();
            Move();
            UpdateAnimatorController();

            if (_controller.isGrounded)
            {
                SwitchState(AnimationState.Movement);
            }

            UpdateFallingDuration();
        }

        #endregion

        #region Crouch State

        /// <summary>
        ///     웅크리기 상태 진입 시 설정
        private void EnterCrouchState()
        {
            _inputReader.onJumpPerformed += CrouchToJumpState;
        }

        /// <summary>
        ///     웅크리기 상태를 업데이트
        /// </summary>
        private void UpdateCrouchState()
        {

            GroundedCheck();
            if (!_isGrounded)
            {
                DeactivateCrouch();
                CapsuleCrouchingSize(false);
                SwitchState(AnimationState.Fall);
            }

            CeilingHeightCheck();

            if (!_crouchKeyPressed && !_cannotStandUp)
            {
                DeactivateCrouch();
                SwitchToMovementState();
            }

            if (!_isCrouching)
            {
                CapsuleCrouchingSize(false);
                SwitchToMovementState();
            }

            CheckEnableTurns();
            CheckEnableLean();

            CalculateRotationalAdditives(false, _enableHeadTurn, false);

            CalculateMoveDirection();
            CheckIfStarting();
            CheckIfStopped();

            FaceMoveDirection();
            Move();
            UpdateAnimatorController();
        }

        /// <summary>
        ///     웅크리기 상태에서 나갈 때 필요한 작업을 수행
        /// </summary>
        private void ExitCrouchState()
        {
            _inputReader.onJumpPerformed -= CrouchToJumpState;
        }

        /// <summary>
        ///     웅크리기 상태에서 점프 상태로 이동
        /// </summary>
        private void CrouchToJumpState()
        {
            if (!_cannotStandUp)
            {
                DeactivateCrouch();
                SwitchState(AnimationState.Jump);
            }
        }

        /// <summary>
        ///     웅크리기 상태에서 이동 상태로 전환
        /// </summary>
        private void SwitchToMovementState()
        {
            DeactivateCrouch();
            SwitchState(AnimationState.Movement);
        }
        #endregion
        #endregion
    }
