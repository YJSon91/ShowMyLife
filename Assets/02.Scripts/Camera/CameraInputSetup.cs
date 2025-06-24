using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 카메라 컨트롤러와 인풋 시스템을 연결하는 스크립트
/// </summary>
[RequireComponent(typeof(CameraController))]
public class CameraInputSetup : MonoBehaviour
{
    private CameraController _cameraController;
    private UnityEngine.InputSystem.PlayerInput _playerInput;

    private void Awake()
    {
        _cameraController = GetComponent<CameraController>();
        _playerInput = FindObjectOfType<UnityEngine.InputSystem.PlayerInput>();

        if (_playerInput != null)
        {
            // 이벤트 기반 입력 설정
            SetupInputEvents();
        }
        else
        {
            Debug.LogWarning("PlayerInput 컴포넌트를 찾을 수 없습니다. 카메라 컨트롤이 작동하지 않을 수 있습니다.");
        }
    }

    /// <summary>
    /// 인풋 시스템 이벤트 설정
    /// </summary>
    private void SetupInputEvents()
    {
        // 이미 이벤트 기반 입력이 설정되어 있는지 확인
        if (!_playerInput.notificationBehavior.HasFlag(PlayerNotifications.InvokeUnityEvents))
        {
            _playerInput.notificationBehavior = PlayerNotifications.InvokeUnityEvents;
        }

        // 액션 맵이 활성화되어 있는지 확인
        if (!_playerInput.actions.enabled)
        {
            _playerInput.actions.Enable();
        }
    }

    /// <summary>
    /// Look 액션 이벤트 처리
    /// </summary>
    public void OnLook(InputAction.CallbackContext context)
    {
        if (_cameraController != null)
        {
            _cameraController.OnLook(context);
        }
    }

    /// <summary>
    /// Zoom 액션 이벤트 처리
    /// </summary>
    public void OnZoom(InputAction.CallbackContext context)
    {
        if (_cameraController != null)
        {
            _cameraController.OnZoom(context);
        }
    }
} 