using UnityEngine;

public class BaseObstacle : MonoBehaviour
{
    public enum SenseMode { Collision, Trigger, Hybrid }
    [Header("플레이어 감지 옵션")]
    [Tooltip("플레이어 감지 방식을 선택하세요 (Collision/Trigger/Hybrid)")]
    [SerializeField] protected SenseMode _senseMode = SenseMode.Collision;
    [Tooltip("플레이어 감지 시 장애물 동작에 반영할지 여부")]
    [SerializeField] protected bool enablePlayerCarry = true;
    
    [Header("트리거 감지 강화")]
    [SerializeField] private bool _useEnhancedTrigger = false;
    [SerializeField] private float _triggerRadius = 2f;
    [SerializeField] private LayerMask _playerLayerMask = -1;

    protected Transform _playerOnPlatform;
    protected Rigidbody _playerRigidbody;

    private void Start()
    {
        if (_useEnhancedTrigger)
        {
            SetupEnhancedTrigger();
        }
    }

    private void Update()
    {
        if (_useEnhancedTrigger && _senseMode != SenseMode.Collision)
        {
            UpdateEnhancedTriggerDetection();
        }
    }

    private void SetupEnhancedTrigger()
    {
        // 기존 콜라이더가 있다면 제거
        var existingCollider = GetComponent<SphereCollider>();
        if (existingCollider != null)
        {
            DestroyImmediate(existingCollider);
        }
        
        // 새로운 스피어 트리거 추가
        var sphereCollider = gameObject.AddComponent<SphereCollider>();
        sphereCollider.radius = _triggerRadius;
        sphereCollider.isTrigger = true;
        sphereCollider.center = Vector3.zero;
    }

    private void UpdateEnhancedTriggerDetection()
    {
        // 이미 감지된 플레이어가 있다면 유효성 재확인
        if (_playerOnPlatform != null)
        {
            if (!IsPlayerStillValid())
            {
                _playerOnPlatform = null;
                _playerRigidbody = null;
            }
        }
        
        // 새로운 플레이어 감지 시도
        if (_playerOnPlatform == null)
        {
            TryDetectPlayerInTrigger();
        }
    }

    private bool IsPlayerStillValid()
    {
        if (_playerOnPlatform == null) return false;
        
        // 플레이어가 여전히 존재하는지 확인
        if (_playerOnPlatform == null || _playerRigidbody == null) return false;
        
        // 플레이어가 너무 멀어졌는지 확인
        float distance = Vector3.Distance(transform.position, _playerOnPlatform.position);
        if (distance > _triggerRadius * 1.5f) return false;
        
        return true;
    }

    private void TryDetectPlayerInTrigger()
    {
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, _triggerRadius, _playerLayerMask);
        
        foreach (var col in nearbyColliders)
        {
            if (IsPlayerObject(col.gameObject))
            {
                _playerOnPlatform = col.transform;
                _playerRigidbody = col.GetComponent<Rigidbody>();
                break;
            }
        }
    }

    // --- Collision 방식 감지 ---
    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (_senseMode != SenseMode.Collision && _senseMode != SenseMode.Hybrid) return;
        if (!enablePlayerCarry) return;
        if (IsPlayerObject(collision.gameObject))
        {
            _playerOnPlatform = collision.transform;
            _playerRigidbody = collision.gameObject.GetComponent<Rigidbody>();
        }
    }

    protected virtual void OnCollisionExit(Collision collision)
    {
        if (_senseMode != SenseMode.Collision && _senseMode != SenseMode.Hybrid) return;
        if (!enablePlayerCarry) return;
        if (IsPlayerObject(collision.gameObject))
        {
            if (_playerOnPlatform == collision.transform)
            {
                _playerOnPlatform = null;
                _playerRigidbody = null;
            }
        }
    }

    // --- Trigger 방식 감지 ---
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (_senseMode != SenseMode.Trigger && _senseMode != SenseMode.Hybrid) return;
        if (!enablePlayerCarry) return;
        if (IsPlayerObject(other.gameObject))
        {
            _playerOnPlatform = other.transform;
            _playerRigidbody = other.GetComponent<Rigidbody>();
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (_senseMode != SenseMode.Trigger && _senseMode != SenseMode.Hybrid) return;
        if (!enablePlayerCarry) return;
        if (IsPlayerObject(other.gameObject))
        {
            if (_playerOnPlatform == other.transform)
            {
                _playerOnPlatform = null;
                _playerRigidbody = null;
            }
        }
    }

    // 플레이어 오브젝트 감별 (태그/구조 모두 대응)
    protected virtual bool IsPlayerObject(GameObject obj)
    {
        // "Player" 태그 + 본체/자식 모두 허용
        if (obj.CompareTag("Player")) return true;
        if (obj.transform.parent != null && obj.transform.parent.CompareTag("Player")) return true;
        return false;
    }

    /// <summary>
    /// 플레이어가 장애물 위에 올라와있는지 판정
    /// </summary>
    protected bool IsPlayerOnPlatform()
    {
        if (!enablePlayerCarry) return false;
        return _playerOnPlatform != null && _playerRigidbody != null;
    }

    protected Transform GetPlayerOnPlatform() => enablePlayerCarry ? _playerOnPlatform : null;
    protected Rigidbody GetPlayerRigidbody() => enablePlayerCarry ? _playerRigidbody : null;

    private void OnDrawGizmosSelected()
    {
        if (_useEnhancedTrigger)
        {
            // 트리거 범위 시각화
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _triggerRadius);
            
            // 감지된 플레이어 위치 표시
            if (_playerOnPlatform != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, _playerOnPlatform.position);
            }
        }
    }
}
