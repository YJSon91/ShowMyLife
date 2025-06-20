using UnityEngine;

/// <summary>
/// 플레이어 입력을 처리하는 클래스
/// </summary>
public class PlayerInput : MonoBehaviour
{
    [Header("키 설정")]
    [Tooltip("앞으로 이동 키")]
    [SerializeField] private KeyCode _forwardKey = KeyCode.W;
    [Tooltip("뒤로 이동 키")]
    [SerializeField] private KeyCode _backwardKey = KeyCode.S;
    [Tooltip("왼쪽으로 이동 키")]
    [SerializeField] private KeyCode _leftKey = KeyCode.A;
    [Tooltip("오른쪽으로 이동 키")]
    [SerializeField] private KeyCode _rightKey = KeyCode.D;
    [Tooltip("점프 키")]
    [SerializeField] private KeyCode _jumpKey = KeyCode.Space;
    [Tooltip("달리기 키")]
    [SerializeField] private KeyCode _sprintKey = KeyCode.LeftShift;

    [Header("입력 설정")]
    [Tooltip("입력 감도")]
    [SerializeField] private float _inputSensitivity = 1f;
    [Tooltip("입력 감쇠율 (0-1)")]
    [SerializeField] private float _inputDamping = 0.1f;

    // 내부 변수
    private float _horizontalInput;
    private float _verticalInput;
    private float _currentHorizontal;
    private float _currentVertical;

    /// <summary>
    /// 수평 입력값 반환 (-1 ~ 1)
    /// </summary>
    public float GetHorizontalInput()
    {
        return _currentHorizontal;
    }

    /// <summary>
    /// 수직 입력값 반환 (-1 ~ 1)
    /// </summary>
    public float GetVerticalInput()
    {
        return _currentVertical;
    }

    /// <summary>
    /// 점프 입력 여부 반환
    /// </summary>
    public bool GetJumpInput()
    {
        return Input.GetKeyDown(_jumpKey);
    }

    /// <summary>
    /// 달리기 입력 여부 반환
    /// </summary>
    public bool GetSprintInput()
    {
        return Input.GetKey(_sprintKey);
    }

    private void Update()
    {
        // WASD 키 입력 처리
        ProcessKeyInput();
        
        // 입력값 부드럽게 처리
        SmoothInput();
    }

    /// <summary>
    /// 키보드 입력 처리
    /// </summary>
    private void ProcessKeyInput()
    {
        // 수평 입력 (A, D)
        _horizontalInput = 0;
        if (Input.GetKey(_rightKey))
            _horizontalInput += 1;
        if (Input.GetKey(_leftKey))
            _horizontalInput -= 1;

        // 수직 입력 (W, S)
        _verticalInput = 0;
        if (Input.GetKey(_forwardKey))
            _verticalInput += 1;
        if (Input.GetKey(_backwardKey))
            _verticalInput -= 1;
    }

    /// <summary>
    /// 입력값 부드럽게 처리
    /// </summary>
    private void SmoothInput()
    {
        // 부드러운 입력 처리 (선형 보간)
        _currentHorizontal = Mathf.Lerp(_currentHorizontal, _horizontalInput * _inputSensitivity, 1f - _inputDamping);
        _currentVertical = Mathf.Lerp(_currentVertical, _verticalInput * _inputSensitivity, 1f - _inputDamping);
        
        // 미세한 입력값 제거
        if (Mathf.Abs(_currentHorizontal) < 0.01f)
            _currentHorizontal = 0;
        if (Mathf.Abs(_currentVertical) < 0.01f)
            _currentVertical = 0;
    }
} 