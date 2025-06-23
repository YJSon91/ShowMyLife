using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어 오브젝트에 InputSystem PlayerInput 컴포넌트를 설정하는 에디터 스크립트
/// </summary>
[RequireComponent(typeof(PlayerInput), typeof(UnityEngine.InputSystem.PlayerInput))]
public class PlayerInputSetup : MonoBehaviour
{
    private void Awake()
    {
        var playerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>();
        
        // 액션 이벤트들을 PlayerInput 클래스의 메서드에 연결
        if (playerInput != null)
        {
            // 이미 연결되어 있지 않다면 행동 이벤트 연결
            if (!playerInput.notificationBehavior.HasFlag(PlayerNotifications.InvokeUnityEvents))
            {
                playerInput.notificationBehavior = PlayerNotifications.InvokeUnityEvents;
            }
            
            // 액션 에셋이 설정되어 있지 않다면 설정
            if (playerInput.actions == null)
            {
                // 프로젝트에서 Player_Input.inputactions 파일 찾기
                var inputActions = Resources.Load<InputActionAsset>("Player_Input");
                if (inputActions != null)
                {
                    playerInput.actions = inputActions;
                }
                else
                {
                    Debug.LogWarning("Player_Input.inputactions 파일을 Resources 폴더에서 찾을 수 없습니다.");
                }
            }
            
            // 기본 액션 맵 활성화
            playerInput.defaultActionMap = "Player";
        }
    }
} 