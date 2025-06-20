using UnityEngine;

/// <summary>
/// 3인칭 카메라 컨트롤러
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
    [Tooltip("카메라 위치 보간 속도")]
    [SerializeField] private float _positionSmoothSpeed = 15f;
    [Tooltip("카메라 회전 보간 속도")]
    [SerializeField] private float _rotationSmoothSpeed = 15f;
    [Tooltip("카메라 입력 보간 속도")]
    [SerializeField] private float _inputSmoothSpeed = 10f;

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
    private Vector3 _smoothPosition;
    private Vector3 _targetPosition;
    private Quaternion _targetRotation;
    private float _smoothHorizontalInput;
    private float _smoothVerticalInput;
    private Vector3 _previousTargetPosition;
    private Vector3 _targetVelocity;
    private Camera _camera;

    /// <summary>
    /// 카메라가 따라갈 대상 설정
    /// </summary>
    public void SetTarget(Transform target)
    {
        _target = target;
        
        if (_target != null)
        {
            transform.position = CalculateCameraPosition();
            transform.LookAt(_target.position + Vector3.up * _height);
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
            _targetPosition = CalculateCameraPosition();
            transform.position = _targetPosition;
            transform.LookAt(_target.position + Vector3.up * _height);
            _targetRotation = transform.rotation;
            _previousTargetPosition = _target.position;
        }
    }

    private void LateUpdate()
    {
        if (_target == null)
            return;
        
        // 타임스케일이 0일 때도 카메라가 작동하도록 함
        float deltaTime = Time.unscaledDeltaTime;
        
        // 마우스 입력 처리
        HandleMouseInput();
        
        // 줌 입력 처리
        HandleZoomInput();
        
        // 카메라 위치 및 회전 업데이트
        UpdateCameraTransform();
    }

    /// <summary>
    /// 마우스 입력 처리
    /// </summary>
    private void HandleMouseInput()
    {
        // 마우스 입력 받기
        float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity;
        
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
        // 마우스 휠 입력
        float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
        
        // 줌 조절
        _currentDistance -= scrollWheel * _zoomSpeed;
        _currentDistance = Mathf.Clamp(_currentDistance, _minDistance, _maxDistance);
    }

    /// <summary>
    /// 카메라 위치 및 회전 업데이트
    /// </summary>
    private void UpdateCameraTransform()
    {
        if (_target == null) return;
        
        // 타겟 속도 계산 (예측 이동에 사용)
        Vector3 targetDelta = _target.position - _previousTargetPosition;
        _targetVelocity = targetDelta / Time.deltaTime;
        _previousTargetPosition = _target.position;
        
        // 목표 카메라 위치 계산 (약간의 예측 이동 포함)
        Vector3 predictedTargetPosition = _target.position + _targetVelocity * 0.025f;
        _targetPosition = CalculateCameraPosition(predictedTargetPosition);
        
        // 카메라 충돌 처리
        HandleCameraCollision();
        
        // 목표 회전 계산
        Vector3 lookAtPosition = _target.position + Vector3.up * _height;
        _targetRotation = Quaternion.LookRotation(lookAtPosition - _targetPosition);
        
        // 카메라 위치 부드럽게 이동 (SmoothDamp 사용)
        Vector3 velocity = Vector3.zero;
        transform.position = Vector3.SmoothDamp(
            transform.position, 
            _targetPosition, 
            ref velocity, 
            1f / _positionSmoothSpeed, 
            Mathf.Infinity, 
            Time.deltaTime
        );
        
        // 카메라 회전 부드럽게 적용
        transform.rotation = Quaternion.Slerp(
            transform.rotation, 
            _targetRotation, 
            Time.deltaTime * _rotationSmoothSpeed
        );
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
} 