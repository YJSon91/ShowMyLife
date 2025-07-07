using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;


    public class InputReader : MonoBehaviour, Controls.IPlayerActions
    {
        public Vector2 _mouseDelta;
        public Vector2 _moveComposite;

        public float _movementInputDuration;
        public bool _movementInputDetected;

        private Controls _controls;

        public Action onAimActivated;
        public Action onAimDeactivated;

        public Action onCrouchActivated;
        public Action onCrouchDeactivated;

        public Action onJumpPerformed;

        public Action onLockOnToggled;

        public Action onSprintActivated;
        public Action onSprintDeactivated;

        public Action onWalkToggled;

        public Action onGodModeToggled;

        public event System.Action OnPausePerformed;

        public Vector2 LookInput { get; private set; }

        /// <inheritdoc cref="OnEnable" />
        private void OnEnable()
        {
            if (_controls == null)
            {
                _controls = new Controls();
                _controls.Player.SetCallbacks(this);
            }

            _controls.Player.Enable();
        }

        /// <inheritdoc cref="OnDisable" />
        public void OnDisable()
        {
            _controls.Player.Disable();
        }
        
        /// <summary>
        /// 외부에서 호출 가능한 입력 활성화 메서드
        /// </summary>
        public void EnableInput()
        {
            if (_controls == null)
            {
                _controls = new Controls();
                _controls.Player.SetCallbacks(this);
            }
            _controls.Player.Enable();
        }
        
        /// <summary>
        /// 외부에서 호출 가능한 입력 비활성화 메서드
        /// </summary>
        public void DisableInput()
        {
            if (_controls != null)
                _controls.Player.Disable();
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

            onGodModeToggled?.Invoke();
        }
    }

