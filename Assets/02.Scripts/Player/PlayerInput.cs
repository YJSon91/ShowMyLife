using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어 입력을 처리하는 클래스 (새로운 Input System 사용)
/// </summary>
[RequireComponent(typeof(PlayerInput))]
public class PlayerInput : MonoBehaviour
{
    [Header("입력 설정")]
    [Tooltip("입력 감도")]
    [SerializeField] private float _inputSensitivity = 1f;
    [Tooltip("입력 감쇠율 (0-1)")]
    [SerializeField] private float _inputDamping = 0.1f;

    // 내부 변수
    private Vector2 _moveInput;
    private Vector2 _currentMoveInput;
    private bool _jumpInput;
    private bool _sprintInput;
    private UnityEngine.InputSystem.PlayerInput _playerInputSystem;

    private void Awake()
    {
        _playerInputSystem = GetComponent<UnityEngine.InputSystem.PlayerInput>();
    }

    /// <summary>
    /// 수평 입력값 반환 (-1 ~ 1)
    /// </summary>
    public float GetHorizontalInput()
    {
        return _currentMoveInput.x;
    }

    /// <summary>
    /// 수직 입력값 반환 (-1 ~ 1)
    /// </summary>
    public float GetVerticalInput()
    {
        return _currentMoveInput.y;
    }

    /// <summary>
    /// 점프 입력 여부 반환
    /// </summary>
    public bool GetJumpInput()
    {
        bool jump = _jumpInput;
        _jumpInput = false; // 한 번 사용하면 초기화
        return jump;
    }

    /// <summary>
    /// 달리기 입력 여부 반환
    /// </summary>
    public bool GetSprintInput()
    {
        return _sprintInput;
    }

    private void Update()
    {
        // 입력값 부드럽게 처리
        SmoothInput();
    }

    /// <summary>
    /// 이동 입력 처리 (InputSystem에서 호출)
    /// </summary>
    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    /// <summary>
    /// 점프 입력 처리 (InputSystem에서 호출)
    /// </summary>
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _jumpInput = true;
        }
    }

    /// <summary>
    /// 달리기 입력 처리 (InputSystem에서 호출)
    /// </summary>
    public void OnSprint(InputAction.CallbackContext context)
    {
        _sprintInput = context.ReadValueAsButton();
    }

    /// <summary>
    /// 입력값 부드럽게 처리
    /// </summary>
    private void SmoothInput()
    {
        // 부드러운 입력 처리 (선형 보간)
        _currentMoveInput.x = Mathf.Lerp(_currentMoveInput.x, _moveInput.x * _inputSensitivity, 1f - _inputDamping);
        _currentMoveInput.y = Mathf.Lerp(_currentMoveInput.y, _moveInput.y * _inputSensitivity, 1f - _inputDamping);
        
        // 미세한 입력값 제거
        if (Mathf.Abs(_currentMoveInput.x) < 0.01f)
            _currentMoveInput.x = 0;
        if (Mathf.Abs(_currentMoveInput.y) < 0.01f)
            _currentMoveInput.y = 0;
    }
} 