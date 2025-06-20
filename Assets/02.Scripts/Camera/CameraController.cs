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
            transform.position = CalculateCameraPosition();
            transform.LookAt(_target.position + Vector3.up * _height);
        }
    }

    private void LateUpdate()
    {
        if (_target == null)
            return;
            
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
        
        // 카메라 각도 업데이트
        _horizontalAngle += mouseX * _rotationSpeed;
        _verticalAngle -= mouseY * _rotationSpeed;
        
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
        // 목표 카메라 위치 계산
        _targetPosition = CalculateCameraPosition();
        
        // 카메라 충돌 처리
        HandleCameraCollision();
        
        // 카메라 위치 부드럽게 이동
        transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * 10f);
        
        // 카메라가 타겟을 바라보도록 회전
        transform.LookAt(_target.position + Vector3.up * _height);
    }

    /// <summary>
    /// 카메라 위치 계산
    /// </summary>
    private Vector3 CalculateCameraPosition()
    {
        // 구형 좌표계를 사용하여 카메라 위치 계산
        float horizontalRadians = _horizontalAngle * Mathf.Deg2Rad;
        float verticalRadians = _verticalAngle * Mathf.Deg2Rad;
        
        // 카메라 오프셋 계산
        float xOffset = _currentDistance * Mathf.Sin(horizontalRadians) * Mathf.Cos(verticalRadians);
        float zOffset = _currentDistance * Mathf.Cos(horizontalRadians) * Mathf.Cos(verticalRadians);
        float yOffset = _currentDistance * Mathf.Sin(verticalRadians) + _currentHeight;
        
        // 타겟 기준 카메라 위치 계산
        return _target.position + new Vector3(xOffset, yOffset, zOffset);
    }

    /// <summary>
    /// 카메라 충돌 처리
    /// </summary>
    private void HandleCameraCollision()
    {
        // 타겟에서 카메라 방향으로 레이캐스트
        Vector3 direction = _targetPosition - (_target.position + Vector3.up * _height);
        float distance = direction.magnitude;
        Ray ray = new Ray(_target.position + Vector3.up * _height, direction.normalized);
        
        // 충돌 감지
        if (Physics.Raycast(ray, out RaycastHit hit, distance, _collisionLayers))
        {
            // 충돌 지점에 카메라 위치 조정 (오프셋 적용)
            _targetPosition = hit.point + hit.normal * _collisionOffset;
        }
    }
} 