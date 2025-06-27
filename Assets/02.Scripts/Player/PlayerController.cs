using System.Collections;
using System.Collections.Generic;
using UnityEngine;


    public class PlayerController : MonoBehaviour
    {
        [Header("외부 컴포넌트")]
    [SerializeField] private PlayerCamera _cameraController;
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private Animator _animator;
    [SerializeField] private CharacterController _controller;
    
    [Header("내부 컴포넌트")]
    private PlayerMovement _playerMovement;
    private PlayerAnimationController _playerAnimController;
    
    // 플레이어 상태 변수
    private bool _isWalking;
    private bool _isSprinting;
        private bool _isCrouching;
        private bool _isGrounded = true;
        private bool _isSliding;
    
    private void Awake()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        _playerAnimController = GetComponent<PlayerAnimationController>();
    }
    
        private void Start()
        {
        // InputReader 이벤트 구독
            _inputReader.onWalkToggled += ToggleWalk;
            _inputReader.onSprintActivated += ActivateSprint;
            _inputReader.onSprintDeactivated += DeactivateSprint;
            _inputReader.onCrouchActivated += ActivateCrouch;
            _inputReader.onCrouchDeactivated += DeactivateCrouch;
        _inputReader.onJumpPerformed += PerformJump;
        
        // 컴포넌트 초기화
        _playerMovement.Initialize(_controller, _cameraController);
        _playerAnimController.Initialize(_animator, _cameraController);
    }
    
    private void Update()
    {
        // 지면 상태 업데이트
        _isGrounded = _playerMovement.CheckGrounded();
        
        // 이동 처리
        _playerMovement.ProcessMovement(_inputReader._moveComposite, _isWalking, _isSprinting, _isCrouching);
        
        // 셔플 방향 값 가져오기
        float shuffleDirectionX = _playerMovement.GetShuffleDirectionX();
        float shuffleDirectionZ = _playerMovement.GetShuffleDirectionZ();
        
        // 애니메이션 업데이트 (셔플 방향 값 전달)
        _playerAnimController.UpdateShuffleDirection(shuffleDirectionX, shuffleDirectionZ);
        _playerAnimController.UpdateAnimation(_playerMovement.GetVelocity(), _playerMovement.GetMoveDirection(), 
            _isGrounded, _isWalking, _isSprinting, _isCrouching, _playerMovement.GetInclineAngle());
    }
    
    // 입력 이벤트 처리 메서드
        private void ToggleWalk()
        {
        _isWalking = !_isWalking && _isGrounded && !_isSprinting;
    }
    
        private void ActivateSprint()
        {
            if (!_isCrouching)
            {
            _isWalking = false;
                _isSprinting = true;
            _playerMovement.SetStrafing(false);
            }
        }

        private void DeactivateSprint()
        {
            _isSprinting = false;
        _playerMovement.SetStrafing(true);
    }
    
        private void ActivateCrouch()
        {
            if (_isGrounded)
            {
            _playerMovement.SetCrouchingSize(true);
                DeactivateSprint();
                _isCrouching = true;
            }
        }

        private void DeactivateCrouch()
    {
        if (!_playerMovement.CannotStandUp() && !_isSliding)
        {
            _playerMovement.SetCrouchingSize(false);
                _isCrouching = false;
            }
        }

    private void PerformJump()
    {
        if (_isGrounded)
        {
            _playerMovement.Jump();
            _playerAnimController.SetJumping(true);
        }
    }
    
    public void ActivateSliding()
    {
        _isSliding = true;
    }
    
        public void DeactivateSliding()
        {
            _isSliding = false;
        }

    private void OnDestroy()
    {
        // InputReader 이벤트 구독 해제
        _inputReader.onWalkToggled -= ToggleWalk;
        _inputReader.onSprintActivated -= ActivateSprint;
        _inputReader.onSprintDeactivated -= DeactivateSprint;
        _inputReader.onCrouchActivated -= ActivateCrouch;
        _inputReader.onCrouchDeactivated -= DeactivateCrouch;
        _inputReader.onJumpPerformed -= PerformJump;
    }
    }
