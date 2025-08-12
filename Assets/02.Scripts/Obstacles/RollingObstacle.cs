using UnityEngine;
using DG.Tweening;

/// <summary>
/// 회전 축을 정의하는 열거형
/// </summary>
public enum RollingObstacleAxis
{
    X, Y, Z
}

/// <summary>
/// 로테이션이 계속 돌고 플레이어가 위에 올라가면 자연스럽게 회전하여 아래로 떨어뜨리는 장애물
/// </summary>
public class RollingObstacle : MonoBehaviour
{
    [Header("회전 설정")]
    [SerializeField] private RollingObstacleAxis _rotationAxis = RollingObstacleAxis.Y;
    [Tooltip("초당 회전 각도")]
    [SerializeField] private float _rotationSpeed = 90f;
    [Tooltip("시계 방향 회전 여부")]
    [SerializeField] private bool _clockwise = true;
    
    [Header("플레이어 감지")]
    [SerializeField] private string _playerTag = "Player";   // Player 태그
    [SerializeField] private float _detectionRadius = 1.5f;  // 감지 반지름
    [SerializeField] private Transform _detectionCenter;     // 감지 중심점
    
    [Header("회전 동작")]
    [SerializeField] private float _rotationDuration = 2f;   // 한 바퀴 회전 시간
    [SerializeField] private bool _continuousRotation = true; // 지속 회전 여부

    private Quaternion _lastRotation;
    private float _currentAngle = 0f;
    private bool _isPlayerOnPlatform = false;
    private Transform _playerTransform;
    private Rigidbody _playerRigidbody;
    private Vector3 _playerInitialRelativePos;
    private bool _isRotating = false;

    private void Start()
    {
        // 감지 중심점이 설정되지 않았으면 현재 위치 사용
        if (_detectionCenter == null)
            _detectionCenter = transform;
            
        _lastRotation = transform.rotation;
        StartRotating();
    }

    private void FixedUpdate()
    {
        // 플레이어 감지
        CheckPlayerOnPlatform();
        
        // 플레이어가 위에 있을 때 회전에 따른 이동 처리
        if (_isPlayerOnPlatform && _playerTransform != null && _playerRigidbody != null)
        {
            MovePlayerWithRotation();
        }
        
        _lastRotation = transform.rotation;
    }

    /// <summary>
    /// 플레이어가 플랫폼 위에 있는지 확인
    /// </summary>
    private void CheckPlayerOnPlatform()
    {
        // 플레이어가 위에 있는지 정확하게 확인
        bool wasPlayerOnPlatform = _isPlayerOnPlatform;
        _isPlayerOnPlatform = false;
        
        // 플레이어 감지 (높이 고려)
        if (IsPlayerActuallyOnTop())
        {
            _isPlayerOnPlatform = true;
            
            // 플레이어가 새로 올라왔을 때
            if (!wasPlayerOnPlatform)
            {
                OnPlayerEnterPlatform();
            }
        }
        
        // 플레이어가 떠났을 때
        if (wasPlayerOnPlatform && !_isPlayerOnPlatform)
        {
            OnPlayerExitPlatform();
        }
    }

    /// <summary>
    /// 플레이어가 실제로 플랫폼 위에 있는지 확인
    /// </summary>
    private bool IsPlayerActuallyOnTop()
    {
        // 플랫폼 주변의 모든 콜라이더 감지
        Collider[] colliders = Physics.OverlapSphere(transform.position, _detectionRadius);
        
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag(_playerTag))
            {
                // 플레이어의 하단이 플랫폼 위에 있는지 확인
                float playerBottom = collider.bounds.min.y;
                float platformTop = transform.position.y + transform.localScale.y / 2f;
                
                // 플레이어가 플랫폼 위에 있고, 충분히 가까이 있는지 확인
                if (playerBottom >= platformTop - 0.2f)
                {
                    // 플레이어의 중심이 플랫폼 범위 내에 있는지 확인
                    Vector3 playerCenter = collider.bounds.center;
                    Vector3 platformCenter = transform.position;
                    float horizontalDistance = Vector3.Distance(
                        new Vector3(playerCenter.x, 0, playerCenter.z),
                        new Vector3(platformCenter.x, 0, platformCenter.z)
                    );
                    
                    if (horizontalDistance <= _detectionRadius * 0.8f)
                    {
                        return true;
                    }
                }
            }
        }
        
        return false;
    }

    /// <summary>
    /// 플레이어가 플랫폼에 올라왔을 때 호출
    /// </summary>
    private void OnPlayerEnterPlatform()
    {
        // 플랫폼 주변의 모든 콜라이더 감지
        Collider[] colliders = Physics.OverlapSphere(transform.position, _detectionRadius);
        
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag(_playerTag))
            {
                _playerTransform = collider.transform;
                _playerRigidbody = collider.GetComponent<Rigidbody>();
                
                if (_playerRigidbody != null)
                {
                    // 플레이어의 초기 상대 위치 저장
                    _playerInitialRelativePos = _playerTransform.position - transform.position;
                    
                    // 플레이어를 플랫폼의 자식으로 설정하여 자연스러운 이동
                    _playerTransform.SetParent(transform);
                    
                    // 디버그 로그
                    Debug.Log($"플레이어가 RollingObstacle에 올라왔습니다! 플레이어: {_playerTransform.name}");
                }
                break;
            }
        }
    }

    /// <summary>
    /// 플레이어가 플랫폼에서 떠났을 때 호출
    /// </summary>
    private void OnPlayerExitPlatform()
    {
        if (_playerTransform != null)
        {
            // 플레이어를 월드 좌표계로 복원
            _playerTransform.SetParent(null);
            
            // 플레이어의 현재 속도 유지
            if (_playerRigidbody != null)
            {
                // 회전 방향으로 약간의 속도 추가하여 자연스럽게 떨어지도록
                Vector3 rotationDirection = GetRotationDirection();
                _playerRigidbody.velocity += rotationDirection * _rotationSpeed * 0.1f;
            }
        }
        
        _playerTransform = null;
        _playerRigidbody = null;
        _playerInitialRelativePos = Vector3.zero;
    }

    /// <summary>
    /// 회전에 따른 플레이어 이동 처리
    /// </summary>
    private void MovePlayerWithRotation()
    {
        if (_playerTransform == null || _playerRigidbody == null) return;
        
        // 회전 변화량 계산
        Quaternion deltaRotation = transform.rotation * Quaternion.Inverse(_lastRotation);
        
        // 플레이어의 상대적 위치 계산
        Vector3 relativePos = _playerTransform.position - transform.position;
        
        // 회전에 따른 새로운 위치 계산
        Vector3 newPosition = transform.position + deltaRotation * relativePos;
        
        // 플레이어를 새 위치로 이동
        _playerTransform.position = newPosition;
    }

    /// <summary>
    /// 회전 방향 벡터 반환
    /// </summary>
    private Vector3 GetRotationDirection()
    {
        Vector3 axis = GetRotationAxis();
        return _clockwise ? axis : -axis;
    }

    /// <summary>
    /// 회전 축 벡터 반환
    /// </summary>
    private Vector3 GetRotationAxis()
    {
        switch (_rotationAxis)
        {
            case RollingObstacleAxis.X: return Vector3.right;
            case RollingObstacleAxis.Y: return Vector3.up;
            case RollingObstacleAxis.Z: return Vector3.forward;
            default: return Vector3.up;
        }
    }

    /// <summary>
    /// 회전 시작
    /// </summary>
    private void StartRotating()
    {
        if (_continuousRotation)
        {
            // 지속적인 회전
            StartContinuousRotation();
        }
        else
        {
            // 한 번만 회전
            StartSingleRotation();
        }
    }

    /// <summary>
    /// 지속적인 회전 시작
    /// </summary>
    private void StartContinuousRotation()
    {
        _isRotating = true;
        float direction = _clockwise ? 1f : -1f;
        Vector3 axis = GetRotationAxis() * direction;

        DOTween.To(() => _currentAngle, x =>
        {
            _currentAngle = x % 360f;
            transform.localRotation = Quaternion.AngleAxis(_currentAngle, axis.normalized);
        },
        360f,
        360f / _rotationSpeed)
        .SetEase(Ease.Linear)
        .SetLoops(-1, LoopType.Incremental)
        .SetUpdate(UpdateType.Fixed);
    }

    /// <summary>
    /// 단일 회전 시작
    /// </summary>
    private void StartSingleRotation()
    {
        _isRotating = true;
        float direction = _clockwise ? 1f : -1f;
        Vector3 axis = GetRotationAxis() * direction;

        DOTween.To(() => _currentAngle, x =>
        {
            _currentAngle = x;
            transform.localRotation = Quaternion.AngleAxis(_currentAngle, axis.normalized);
        },
        360f,
        _rotationDuration)
        .SetEase(Ease.InOutQuad)
        .SetUpdate(UpdateType.Fixed)
        .OnComplete(() => {
            _isRotating = false;
            // 회전이 끝나면 플레이어를 떨어뜨림
            if (_isPlayerOnPlatform)
            {
                OnPlayerExitPlatform();
            }
        });
    }

    /// <summary>
    /// 회전 일시정지/재개
    /// </summary>
    public void ToggleRotation()
    {
        if (_isRotating)
        {
            DOTween.Pause(transform);
            _isRotating = false;
        }
        else
        {
            DOTween.Play(transform);
            _isRotating = true;
        }
    }

    /// <summary>
    /// 회전 속도 변경
    /// </summary>
    public void SetRotationSpeed(float newSpeed)
    {
        _rotationSpeed = newSpeed;
        
        // 현재 회전을 중단하고 새로운 속도로 재시작
        DOTween.Kill(transform);
        StartRotating();
    }

    // 디버그 시각화
    private void OnDrawGizmosSelected()
    {
        if (_detectionCenter == null) return;
        
        // 감지 영역 시각화
        Gizmos.color = _isPlayerOnPlatform ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectionRadius);
        
        // 플랫폼 상단 표시
        Gizmos.color = Color.cyan;
        Vector3 platformTop = transform.position + Vector3.up * (transform.localScale.y / 2f);
        Gizmos.DrawWireCube(platformTop, new Vector3(transform.localScale.x, 0.1f, transform.localScale.z));
        
        // 회전 축 시각화
        Gizmos.color = Color.red;
        Vector3 axis = GetRotationAxis();
        Vector3 direction = _clockwise ? axis : -axis;
        Gizmos.DrawRay(transform.position, direction * 2f);
        
        // 플레이어가 감지되었을 때 추가 정보 표시
        if (_isPlayerOnPlatform && _playerTransform != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, _playerTransform.position);
            Gizmos.DrawWireSphere(_playerTransform.position, 0.3f);
        }
    }
}
