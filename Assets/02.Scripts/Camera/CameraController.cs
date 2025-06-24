using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 3인칭 카메라 컨트롤러 - 플레이어를 화면 정중앙에 완전히 고정 (Hard Look At)
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("카메라 설정")]
    [Tooltip("카메라가 따라갈 대상")]
    [SerializeField] private Transform _target;
    [Tooltip("카메라 거리")]
    [SerializeField] private float _distance = 5f;
    [Tooltip("카메라 높이")]
    [SerializeField] private float _height = 2f;
    [Tooltip("카메라 회전 속도")]
    [SerializeField] private float _rotationSpeed = 3f;
    [Tooltip("카메라 상하 각도 제한 (최소)")]
    [SerializeField] private float _minVerticalAngle = -30f;
    [Tooltip("카메라 상하 각도 제한 (최대)")]
    [SerializeField] private float _maxVerticalAngle = 60f;
    [Tooltip("카메라 줌 속도")]
    [SerializeField] private float _zoomSpeed = 1f;
    [Tooltip("최소 카메라 거리")]
    [SerializeField] private float _minDistance = 2f;
    [Tooltip("최대 카메라 거리")]
    [SerializeField] private float _maxDistance = 10f;
    [Tooltip("카메라 충돌 레이어")]
    [SerializeField] private LayerMask _collisionLayers;
    [Tooltip("카메라 충돌 오프셋")]
    [SerializeField] private float _collisionOffset = 0.2f;
    [Tooltip("카메라 입력 보간 속도")]
    [SerializeField] private float _inputSmoothSpeed = 10f;
    [Tooltip("카메라 위치 고정 강도 (높을수록 더 정확히 고정)")]
    [SerializeField] private float _positionFixStrength = 1000f;
    [Tooltip("Hard Look At 활성화")]
    [SerializeField] private bool _hardLookAt = true;

    [Header("입력 설정")]
    [Tooltip("마우스 감도")]
    [SerializeField] private float _mouseSensitivity = 3f;
    [Tooltip("마우스 Y축 반전")]
    [SerializeField] private bool _invertYAxis = false;
    [Tooltip("마우스 X축 반전")]
    [SerializeField] private bool _invertXAxis = false;

    // 내부 변수
    private float _currentDistance;
    private float _currentHeight;
    private float _horizontalAngle;
    private float _verticalAngle;
    private Vector3 _targetPosition;
    private Quaternion _targetRotation;
    private float _smoothHorizontalInput;
    private float _smoothVerticalInput;
    private Camera _camera;
    private Vector3 _lastTargetPosition;
    
    // 인풋 시스템 변수
    private Vector2 _lookInput;
    private float _zoomInput;
    private UnityEngine.InputSystem.PlayerInput _playerInputSystem;

    /// <summary>
    /// 카메라가 따라갈 대상 설정
    /// </summary>
    public void SetTarget(Transform target)
    {
        _target = target;
        
        if (_target != null)
        {
            _lastTargetPosition = _target.position;
            transform.position = CalculateCameraPosition(_target.position);
            LookAtTarget();
        }
    }

    private void Awake()
    {
        // 카메라 컴포넌트 캐싱
        _camera = GetComponent<Camera>();
        if (_camera == null)
        {
            _camera = Camera.main;
        }
        
        // 플레이어 인풋 시스템 찾기
        _playerInputSystem = FindObjectOfType<UnityEngine.InputSystem.PlayerInput>();
        
        // 카메라 설정 최적화
        Application.targetFrameRate = 60; // 프레임 레이트 고정
        QualitySettings.vSyncCount = 1;   // 수직 동기화 활성화
    }
    
    private void Start()
    {
        // 초기값 설정
        _currentDistance = _distance;
        _currentHeight = _height;
        
        // 마우스 커서 숨기기 및 고정
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // 초기 위치 설정
        if (_target != null)
        {
            _lastTargetPosition = _target.position;
            _targetPosition = CalculateCameraPosition(_target.position);
            transform.position = _targetPosition;
            LookAtTarget();
            _targetRotation = transform.rotation;
        }
    }

    private void Update()
    {
        // 마우스 입력 처리
        HandleMouseInput();
        
        // 줌 입력 처리
        HandleZoomInput();
    }

    private void LateUpdate()
    {
        if (_target == null)
            return;
        
        // 플레이어 위치 추적과 회전을 분리하여 더 정확하게 처리
        UpdateCameraPosition();
        
        // Hard Look At이 활성화된 경우 매 프레임 타겟을 정확히 바라봄
        if (_hardLookAt)
        {
            LookAtTarget();
        }
        else
        {
            UpdateCameraRotation();
        }
    }

    /// <summary>
    /// Look 입력 처리 (인풋 시스템에서 호출)
    /// </summary>
    public void OnLook(InputAction.CallbackContext context)
    {
        _lookInput = context.ReadValue<Vector2>();
    }
    
    /// <summary>
    /// Zoom 입력 처리 (인풋 시스템에서 호출)
    /// </summary>
    public void OnZoom(InputAction.CallbackContext context)
    {
        _zoomInput = context.ReadValue<float>();
    }

    /// <summary>
    /// 마우스 입력 처리
    /// </summary>
    private void HandleMouseInput()
    {
        // 마우스 입력 받기 (인풋 시스템 사용)
        float mouseX = _lookInput.x * _mouseSensitivity * Time.deltaTime;
        float mouseY = _lookInput.y * _mouseSensitivity * Time.deltaTime;
        
        // 반전 적용
        if (_invertXAxis) mouseX = -mouseX;
        if (_invertYAxis) mouseY = -mouseY;
        
        // 입력값 부드럽게 보간
        _smoothHorizontalInput = Mathf.Lerp(_smoothHorizontalInput, mouseX, Time.deltaTime * _inputSmoothSpeed);
        _smoothVerticalInput = Mathf.Lerp(_smoothVerticalInput, mouseY, Time.deltaTime * _inputSmoothSpeed);
        
        // 카메라 각도 업데이트 (부드러운 입력값 사용)
        _horizontalAngle += _smoothHorizontalInput * _rotationSpeed;
        _verticalAngle -= _smoothVerticalInput * _rotationSpeed;
        
        // 수직 각도 제한
        _verticalAngle = Mathf.Clamp(_verticalAngle, _minVerticalAngle, _maxVerticalAngle);
    }

    /// <summary>
    /// 줌 입력 처리
    /// </summary>
    private void HandleZoomInput()
    {
        // 마우스 휠 입력 (인풋 시스템 사용)
        float scrollWheel = _zoomInput * _zoomSpeed * Time.deltaTime;
        
        // 줌 조절
        _currentDistance -= scrollWheel;
        _currentDistance = Mathf.Clamp(_currentDistance, _minDistance, _maxDistance);
    }

    /// <summary>
    /// 카메라 위치 업데이트 (플레이어 추적)
    /// </summary>
    private void UpdateCameraPosition()
    {
        if (_target == null) return;
        
        // 타겟의 현재 위치를 정확히 사용
        Vector3 currentTargetPosition = _target.position;
        
        // 목표 카메라 위치 계산
        _targetPosition = CalculateCameraPosition(currentTargetPosition);
        
        // 카메라 충돌 처리
        HandleCameraCollision();
        
        // 카메라 위치를 즉시 업데이트 (완전히 고정)
        transform.position = _targetPosition;
        
        // 마지막 타겟 위치 업데이트
        _lastTargetPosition = currentTargetPosition;
    }
    
    /// <summary>
    /// 카메라 회전 업데이트 (부드러운 회전)
    /// </summary>
    private void UpdateCameraRotation()
    {
        if (_target == null) return;
        
        // 목표 회전 계산
        Vector3 lookAtPosition = _target.position + Vector3.up * _height;
        _targetRotation = Quaternion.LookRotation(lookAtPosition - transform.position);
        
        // 카메라 회전 부드럽게 적용
        transform.rotation = Quaternion.Slerp(
            transform.rotation, 
            _targetRotation, 
            Time.deltaTime * _positionFixStrength
        );
    }
    
    /// <summary>
    /// 타겟을 정확히 바라보도록 카메라 회전 (Hard Look At)
    /// </summary>
    private void LookAtTarget()
    {
        if (_target == null) return;
        
        // 타겟의 시선 위치 (머리 높이)
        Vector3 lookAtPosition = _target.position + Vector3.up * _height;
        
        // 카메라가 타겟을 정확히 바라보도록 회전
        transform.LookAt(lookAtPosition);
    }

    /// <summary>
    /// 카메라 위치 계산
    /// </summary>
    private Vector3 CalculateCameraPosition(Vector3 targetPos)
    {
        // 구형 좌표계를 사용하여 카메라 위치 계산
        float horizontalRadians = _horizontalAngle * Mathf.Deg2Rad;
        float verticalRadians = _verticalAngle * Mathf.Deg2Rad;
        
        // 카메라 오프셋 계산
        float xOffset = _currentDistance * Mathf.Sin(horizontalRadians) * Mathf.Cos(verticalRadians);
        float zOffset = _currentDistance * Mathf.Cos(horizontalRadians) * Mathf.Cos(verticalRadians);
        float yOffset = _currentDistance * Mathf.Sin(verticalRadians) + _currentHeight;
        
        // 타겟 기준 카메라 위치 계산
        return targetPos + new Vector3(xOffset, yOffset, zOffset);
    }
    
    /// <summary>
    /// 카메라 위치 계산 (기본 타겟 사용)
    /// </summary>
    private Vector3 CalculateCameraPosition()
    {
        return CalculateCameraPosition(_target.position);
    }

    /// <summary>
    /// 카메라 충돌 처리
    /// </summary>
    private void HandleCameraCollision()
    {
        // 타겟에서 카메라 방향으로 레이캐스트
        Vector3 lookAtPosition = _target.position + Vector3.up * _height;
        Vector3 direction = _targetPosition - lookAtPosition;
        float distance = direction.magnitude;
        Ray ray = new Ray(lookAtPosition, direction.normalized);
        
        // 충돌 감지 (여러 방향으로 추가 레이캐스트)
        if (Physics.SphereCast(ray, 0.2f, out RaycastHit hit, distance, _collisionLayers))
        {
            // 충돌 지점에 카메라 위치 조정 (오프셋 적용)
            _targetPosition = hit.point + hit.normal * _collisionOffset;
        }
    }

    /// <summary>
    /// 카메라가 타겟을 정확히 중앙에 보고 있는지 확인
    /// </summary>
    private bool IsCameraLookingAtTarget()
    {
        if (_target == null || _camera == null) return false;
        
        // 타겟의 스크린 좌표 계산
        Vector3 screenPos = _camera.WorldToScreenPoint(_target.position + Vector3.up * _height);
        
        // 화면 중앙 좌표
        Vector2 screenCenter = new Vector2(_camera.pixelWidth * 0.5f, _camera.pixelHeight * 0.5f);
        
        // 타겟이 화면 중앙에 있는지 확인 (약간의 오차 허용)
        float distance = Vector2.Distance(screenCenter, new Vector2(screenPos.x, screenPos.y));
        
        return distance < 5f; // 5픽셀 이내면 중앙에 있다고 판단 (더 엄격하게 조정)
    }

    /// <summary>
    /// Hard Look At 모드 설정
    /// </summary>
    public void SetHardLookAt(bool enabled)
    {
        _hardLookAt = enabled;
    }

    /// <summary>
    /// OnDrawGizmos - 디버깅용 시각화
    /// </summary>
    private void OnDrawGizmos()
    {
        if (_target == null) return;
        
        // 타겟 위치 표시
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(_target.position + Vector3.up * _height, 0.2f);
        
        // 카메라 시선 방향 표시
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, _target.position + Vector3.up * _height);
    }
} 