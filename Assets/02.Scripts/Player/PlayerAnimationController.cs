using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
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
    
    [Header("머리 회전 설정")]
    [SerializeField] private bool _enableHeadTurn = true;
    [SerializeField] private float _headLookDelay;
    [SerializeField] private float _headLookX;
    [SerializeField] private float _headLookY;
    [SerializeField] private AnimationCurve _headLookXCurve;
    
    [Header("몸체 회전 설정")]
    [SerializeField] private bool _enableBodyTurn = true;
    [SerializeField] private float _bodyLookDelay;
    [SerializeField] private float _bodyLookX;
    [SerializeField] private float _bodyLookY;
    [SerializeField] private AnimationCurve _bodyLookXCurve;
    
    [Header("기울기 설정")]
    [SerializeField] private bool _enableLean = true;
    [SerializeField] private float _leanDelay;
    [SerializeField] private float _leanValue;
    [SerializeField] private AnimationCurve _leanCurve;
    [SerializeField] private float _leansHeadLooksDelay;
    
    // 내부 변수
    private Animator _animator;
    private PlayerCamera _cameraController;
    private AnimationState _currentState = AnimationState.Base;
    private GaitState _currentGait;
    
    private Vector3 _currentRotation = new Vector3(0f, 0f, 0f);
    private Vector3 _previousRotation;
    private float _rotationRate;
    private float _initialLeanValue;
    private float _initialTurnValue;
    private bool _isJumping;
    private bool _isTurningInPlace;
    private bool _isStopped = true;
    private bool _isStarting;
    private float _movementStartDirection;
    private float _movementStartTimer;
    private float _fallStartTime;
    
    // 애니메이션 변수 해쉬
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
    
    public void Initialize(Animator animator, PlayerCamera cameraController)
    {
        _animator = animator;
        _cameraController = cameraController;
        _previousRotation = transform.forward;
        
        SwitchState(AnimationState.Movement);
    }
    
    public void UpdateAnimation(Vector3 velocity, Vector3 moveDirection, bool isGrounded, bool isWalking, bool isSprinting, bool isCrouching, float inclineAngle)
    {
        switch (_currentState)
        {
            case AnimationState.Movement:
                UpdateMovementState(velocity, moveDirection, isGrounded, isWalking, isSprinting, isCrouching, inclineAngle);
                break;
            case AnimationState.Jump:
                UpdateJumpState(velocity, moveDirection);
                break;
            case AnimationState.Fall:
                UpdateFallState(velocity, moveDirection, isGrounded);
                break;
            case AnimationState.Crouch:
                UpdateCrouchState(velocity, moveDirection, isGrounded, isWalking);
                break;
        }
    }
    
    public void SetJumping(bool jumping)
    {
        if (jumping && _currentState != AnimationState.Jump)
        {
            SwitchState(AnimationState.Jump);
        }
        else if (!jumping && _currentState == AnimationState.Jump)
        {
            SwitchState(AnimationState.Fall);
        }
        
        _isJumping = jumping;
    }
    
    private void UpdateMovementState(Vector3 velocity, Vector3 moveDirection, bool isGrounded, bool isWalking, bool isSprinting, bool isCrouching, float inclineAngle)
    {
        if (!isGrounded)
        {
            SwitchState(AnimationState.Fall);
            return;
        }
        
        if (isCrouching)
        {
            SwitchState(AnimationState.Crouch);
            return;
        }
        
        float speed2D = new Vector3(velocity.x, 0f, velocity.z).magnitude;
        
        CheckEnableTurns();
        CheckEnableLean();
        CalculateRotationalAdditives(_enableLean, _enableHeadTurn, _enableBodyTurn);
        
        CalculateGait(speed2D, isWalking, isSprinting);
        CheckIfStarting(moveDirection, speed2D);
        CheckIfStopped(moveDirection, speed2D);
        
        UpdateAnimatorController(speed2D, isGrounded, isWalking, isSprinting, isCrouching, inclineAngle, 0f);
    }
    
    private void UpdateJumpState(Vector3 velocity, Vector3 moveDirection)
    {
        if (velocity.y <= 0f)
        {
            _animator.SetBool(_isJumpingAnimHash, false);
            SwitchState(AnimationState.Fall);
            return;
        }
        
        CalculateRotationalAdditives(false, _enableHeadTurn, _enableBodyTurn);
        
        float speed2D = new Vector3(velocity.x, 0f, velocity.z).magnitude;
        UpdateAnimatorController(speed2D, false, false, false, false, 0f, 0f);
    }
    
    private void UpdateFallState(Vector3 velocity, Vector3 moveDirection, bool isGrounded)
    {
        if (isGrounded)
        {
            SwitchState(AnimationState.Movement);
            return;
        }
        
        CalculateRotationalAdditives(false, _enableHeadTurn, _enableBodyTurn);
        
        float speed2D = new Vector3(velocity.x, 0f, velocity.z).magnitude;
        float fallingDuration = Time.time - _fallStartTime;
        
        UpdateAnimatorController(speed2D, false, false, false, false, 0f, fallingDuration);
    }
    
    private void UpdateCrouchState(Vector3 velocity, Vector3 moveDirection, bool isGrounded, bool isWalking)
    {
        if (!isGrounded)
        {
            SwitchState(AnimationState.Fall);
            return;
        }
        
        CheckEnableTurns();
        CheckEnableLean();
        
        CalculateRotationalAdditives(false, _enableHeadTurn, false);
        
        float speed2D = new Vector3(velocity.x, 0f, velocity.z).magnitude;
        
        CalculateGait(speed2D, isWalking, false);
        CheckIfStarting(moveDirection, speed2D);
        CheckIfStopped(moveDirection, speed2D);
        
        UpdateAnimatorController(speed2D, isGrounded, isWalking, false, true, 0f, 0f);
    }
    
    private void SwitchState(AnimationState newState)
    {
        ExitCurrentState();
        EnterState(newState);
    }
    
    private void EnterState(AnimationState stateToEnter)
    {
        _currentState = stateToEnter;
        switch (_currentState)
        {
            case AnimationState.Jump:
                EnterJumpState();
                break;
            case AnimationState.Fall:
                EnterFallState();
                break;
        }
    }
    
    private void ExitCurrentState()
    {
        switch (_currentState)
        {
            case AnimationState.Jump:
                ExitJumpState();
                break;
        }
    }
    
    private void EnterJumpState()
    {
        _animator.SetBool(_isJumpingAnimHash, true);
    }
    
    private void ExitJumpState()
    {
        _animator.SetBool(_isJumpingAnimHash, false);
    }
    
    private void EnterFallState()
    {
        _fallStartTime = Time.time;
    }
    
    private void CalculateGait(float speed, bool isWalking, bool isSprinting)
    {
        if (speed < 0.01)
        {
            _currentGait = GaitState.Idle;
        }
        else if (isWalking)
        {
            _currentGait = GaitState.Walk;
        }
        else if (isSprinting)
        {
            _currentGait = GaitState.Sprint;
        }
        else
        {
            _currentGait = GaitState.Run;
        }
    }
    
    private void CheckIfStopped(Vector3 moveDirection, float speed)
    {
        _isStopped = moveDirection.magnitude == 0 && speed < .5;
    }
    
    private void CheckIfStarting(Vector3 moveDirection, float speed)
    {
        _movementStartTimer = VariableOverrideDelayTimer(_movementStartTimer);
        
        bool isStartingCheck = false;
        
        if (_movementStartTimer <= 0.0f)
        {
            if (moveDirection.magnitude > 0.01 && speed < 1)
            {
                isStartingCheck = true;
            }
            
            if (isStartingCheck)
            {
                if (!_isStarting)
                {
                    _movementStartDirection = Vector3.SignedAngle(transform.forward, moveDirection, Vector3.up);
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
    
    private void CheckEnableTurns()
    {
        _headLookDelay = VariableOverrideDelayTimer(_headLookDelay);
        _enableHeadTurn = _headLookDelay == 0.0f && !_isStarting;
        _bodyLookDelay = VariableOverrideDelayTimer(_bodyLookDelay);
        _enableBodyTurn = _bodyLookDelay == 0.0f && !(_isStarting || _isTurningInPlace);
    }
    
    private void CheckEnableLean()
    {
        _leanDelay = VariableOverrideDelayTimer(_leanDelay);
        _enableLean = _leanDelay == 0.0f && !(_isStarting || _isTurningInPlace);
    }
    
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
        
        float referenceValue = _currentGait == GaitState.Sprint ? 1f : 0.5f;
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
            _initialTurnValue = 0f; // 카메라 회전 오프셋 필요
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
    
    private void UpdateAnimatorController(float speed, bool isGrounded, bool isWalking, bool isSprinting, bool isCrouching, float inclineAngle, float fallingDuration)
    {
        _animator.SetFloat(_leanValueHash, _leanValue);
        _animator.SetFloat(_headLookXHash, _headLookX);
        _animator.SetFloat(_headLookYHash, _headLookY);
        _animator.SetFloat(_bodyLookXHash, _bodyLookX);
        _animator.SetFloat(_bodyLookYHash, _bodyLookY);
        
        _animator.SetFloat(_isStrafingHash, 1.0f); // 항상 스트레이핑 활성화
        
        _animator.SetFloat(_inclineAngleHash, inclineAngle);
        
        _animator.SetFloat(_moveSpeedHash, speed);
        _animator.SetInteger(_currentGaitHash, (int) _currentGait);
        
        _animator.SetFloat(_strafeDirectionXHash, 0f); // PlayerMovement에서 관리
        _animator.SetFloat(_strafeDirectionZHash, 1f); // PlayerMovement에서 관리
        _animator.SetFloat(_forwardStrafeHash, 1f); // PlayerMovement에서 관리
        _animator.SetFloat(_cameraRotationOffsetHash, 0f); // PlayerMovement에서 관리
        
        _animator.SetBool(_movementInputHeldHash, false); // InputReader에서 관리
        _animator.SetBool(_movementInputPressedHash, false); // InputReader에서 관리
        _animator.SetBool(_movementInputTappedHash, false); // InputReader에서 관리
        _animator.SetFloat(_shuffleDirectionXHash, 0f); // PlayerMovement에서 관리
        _animator.SetFloat(_shuffleDirectionZHash, 1f); // PlayerMovement에서 관리
        
        _animator.SetBool(_isTurningInPlaceHash, _isTurningInPlace);
        _animator.SetBool(_isCrouchingHash, isCrouching);
        
        _animator.SetFloat(_fallingDurationHash, fallingDuration);
        _animator.SetBool(_isGroundedHash, isGrounded);
        
        _animator.SetBool(_isWalkingHash, isWalking);
        _animator.SetBool(_isStoppedHash, _isStopped);
    }
}