 using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;


    public class InputReader : MonoBehaviour, Controls.IPlayerActions, Controls.ISharedActions
{
        public Vector2 _mouseDelta;
        public Vector2 _moveComposite;

        public float _movementInputDuration;
        public bool _movementInputDetected;

        // GameManager의 PlayerControls를 사용하도록 변경
        private Controls _controls => GameManager.Instance?.PlayerControls;

        public Action onAimActivated;
        public Action onAimDeactivated;

        public Action onCrouchActivated;
        public Action onCrouchDeactivated;

        public Action onJumpPerformed;

        public Action onLockOnToggled;

        public Action onSprintActivated;
        public Action onSprintDeactivated;

        public Action onWalkToggled;

        public event System.Action OnPausePerformed;
        
        // 갓모드 토글 이벤트
        public event System.Action OnGodModeToggled;

        public Vector2 LookInput { get; private set; }

        /// <inheritdoc cref="OnEnable" />
        private void OnEnable()
        {
            //Debug.Log("[InputReader] OnEnable 호출");
            // GameManager의 PlayerControls가 준비될 때까지 대기
            if (_controls == null)
            {
               // Debug.LogWarning("[InputReader] GameManager의 PlayerControls가 아직 준비되지 않았습니다. 잠시 후 다시 시도합니다.");
                return;
            }

            _controls.Player.SetCallbacks(this);
            _controls.Player.Enable();
            _controls.Shared.SetCallbacks(this);

        //  Debug.Log("[InputReader] GameManager의 PlayerControls를 사용하여 입력 시스템을 초기화했습니다.");
    }

        /// <inheritdoc cref="OnDisable" />
        private void OnDisable()
        {
            if (_controls != null)
            {
                _controls.Player.SetCallbacks(null);
                _controls.Player.Disable();
            }
        }

        private void Start()
        {
            // Start에서 다시 한 번 시도 (GameManager가 초기화된 후)
            if (_controls != null && !_controls.Player.enabled)
            {
                _controls.Player.SetCallbacks(this);
                _controls.Player.Enable();
              //  Debug.Log("[InputReader] Start에서 GameManager의 PlayerControls를 성공적으로 연결했습니다.");
            }
        }

        /// <summary>
        ///     OnLook 콜백이 호출될 때 수행할 동작을 정의합니다.
        /// </summary>
        /// <param name="context">콜백의 컨텍스트입니다.</param>
        public void OnLook(InputAction.CallbackContext context)
        {
            _mouseDelta = context.ReadValue<Vector2>();
            LookInput = _mouseDelta;
        }

         /// <summary>
        ///     OnLookInput 콜백이 호출될 때 수행할 동작을 정의합니다.
        /// </summary>
        /// <param name="context">콜백의 컨텍스트입니다.</param>
        public void OnLookInput(InputAction.CallbackContext context)
        {
            LookInput = context.ReadValue<Vector2>();
        }

        /// <summary>
        ///     OnMove 콜백이 호출될 때 수행할 동작을 정의합니다.
        /// </summary>
        /// <param name="context">콜백의 컨텍스트입니다.</param>
        public void OnMove(InputAction.CallbackContext context)
        {
            _moveComposite = context.ReadValue<Vector2>();
            _movementInputDetected = _moveComposite.magnitude > 0;
        }

        /// <summary>
        ///     OnJump 콜백이 호출될 때 수행할 동작을 정의합니다.
        /// </summary>
        /// <param name="context">콜백의 컨텍스트입니다.</param>
        public void OnJump(InputAction.CallbackContext context)
        {
            if (!context.performed)
            {
                return;
            }

            onJumpPerformed?.Invoke();
        }

        /// <summary>
        ///     OnToggleWalk 콜백이 호출될 때 수행할 동작을 정의합니다.
        /// </summary>
        /// <param name="context">콜백의 컨텍스트입니다.</param>
        public void OnToggleWalk(InputAction.CallbackContext context)
        {
            if (!context.performed)
            {
                return;
            }

            onWalkToggled?.Invoke();
        }

        /// <summary>
        ///     OnSprint 콜백이 호출될 때 수행할 동작을 정의합니다.
        /// </summary>
        /// <param name="context">콜백의 컨텍스트입니다.</param>
        public void OnSprint(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                onSprintActivated?.Invoke();
            }
            else if (context.canceled)
            {
                onSprintDeactivated?.Invoke();
            }
        }

        /// <summary>
        ///     OnCrouch 콜백이 호출될 때 수행할 동작을 정의합니다.
        /// </summary>
        /// <param name="context">콜백의 컨텍스트입니다.</param>
        public void OnCrouch(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                onCrouchActivated?.Invoke();
            }
            else if (context.canceled)
            {
                onCrouchDeactivated?.Invoke();
            }
        }

        /// <summary>
        ///     OnAim 콜백이 호출될 때 수행할 동작을 정의합니다.
        /// </summary>
        /// <param name="context">콜백의 컨텍스트입니다.</param>
        public void OnAim(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                onAimActivated?.Invoke();
            }

            if (context.canceled)
            {
                onAimDeactivated?.Invoke();
            }
        }

        /// <summary>
        ///     OnLockOn 콜백이 호출될 때 수행할 동작을 정의합니다.
        /// </summary>
        /// <param name="context">콜백의 컨텍스트입니다.</param>
        public void OnLockOn(InputAction.CallbackContext context)
        {
            if (!context.performed)
            {
                return;
            }

            onLockOnToggled?.Invoke();
            onSprintDeactivated?.Invoke();
        }

        /// <summary>
        ///     OnPause 콜백이 호출될 때 수행할 동작을 정의합니다.
        /// </summary>
        /// <param name="context">콜백의 컨텍스트입니다.</param>
        public void OnPause(InputAction.CallbackContext context)
        {
            if (!context.performed)
            {
                return;
            }

            OnPausePerformed?.Invoke();
        }

        /// <summary>
        ///     OnGodMode 콜백이 호출될 때 수행할 동작을 정의합니다.
        /// </summary>
        /// <param name="context">콜백의 컨텍스트입니다.</param>
        public void OnGodMode(InputAction.CallbackContext context)
        {
            if (!context.performed)
            {
                return;
            }

            OnGodModeToggled?.Invoke();
        }

        /// <summary>
        /// 외부에서 호출 가능한 입력 활성화 메서드
        /// GameManager의 PlayerControls를 사용합니다.
        /// </summary>
        public void EnableInput()
        {
            if (_controls != null)
            {
                _controls.Player.SetCallbacks(this);
                _controls.Player.Enable();
              //  Debug.Log("[InputReader] EnableInput: PlayerControls 활성화됨");
            }
            else
            {
                //Debug.LogWarning("[InputReader] EnableInput: GameManager의 PlayerControls가 아직 준비되지 않았습니다.");
            }
        }
        
        /// <summary>
        /// 외부에서 호출 가능한 입력 비활성화 메서드
        /// GameManager의 PlayerControls를 사용합니다.
        /// </summary>
        public void DisableInput()
        {
            if (_controls != null)
            {
                _controls.Player.SetCallbacks(null);
                _controls.Player.Disable();
              //  Debug.Log("[InputReader] DisableInput: PlayerControls 비활성화됨");
                
                // 입력 비활성화 시 이동 관련 변수 초기화
                _moveComposite = Vector2.zero;
                _movementInputDetected = false;
                _movementInputDuration = 0f;
            }
        }
    }

