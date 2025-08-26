 using UnityEngine;

/// <summary>
/// 플레이어가 트리거에 닿으면 게임오브젝트를 비활성화하는 컴포넌트
/// </summary>
public class PlayerDeactivator : MonoBehaviour
{
    [Header("설정")]
    [Tooltip("비활성화할 게임오브젝트 (비워두면 자신을 비활성화)")]
    [SerializeField] private GameObject targetToDeactivate;
    
    [Tooltip("비활성화 후 다시 활성화할지 여부")]
    [SerializeField] private bool reactivateAfterDelay = false;
    
    [Tooltip("재활성화까지의 대기 시간 (초)")]
    [SerializeField] private float reactivateDelay = 3f;
    
    [Tooltip("트리거가 한 번만 작동할지 여부")]
    [SerializeField] private bool oneTimeUse = true;
    
    private bool hasTriggered = false;
    
    private void Start()
    {
        // 타겟이 설정되지 않았다면 자신을 타겟으로 설정
        if (targetToDeactivate == null)
        {
            targetToDeactivate = gameObject;
        }
        
        // Collider가 트리거인지 확인
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning($"{gameObject.name}: Collider가 트리거로 설정되어 있지 않습니다. 트리거로 설정해주세요.");
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // 이미 트리거된 경우 무시
        if (oneTimeUse && hasTriggered) return;
        
        // 플레이어 태그 확인
        if (!other.CompareTag("Player")) return;
        
        hasTriggered = true;
        
        // 타겟 게임오브젝트 비활성화
        if (targetToDeactivate != null)
        {
            targetToDeactivate.SetActive(false);
            Debug.Log($"{gameObject.name}: {targetToDeactivate.name} 비활성화됨");
        }
        
        // 재활성화가 설정된 경우 지연 후 재활성화
        if (reactivateAfterDelay)
        {
            Invoke(nameof(ReactivateTarget), reactivateDelay);
        }
    }
    
    /// <summary>
    /// 타겟 게임오브젝트를 재활성화
    /// </summary>
    private void ReactivateTarget()
    {
        if (targetToDeactivate != null)
        {
            targetToDeactivate.SetActive(true);
            Debug.Log($"{gameObject.name}: {targetToDeactivate.name} 재활성화됨");
        }
    }
    
    /// <summary>
    /// 수동으로 타겟을 비활성화
    /// </summary>
    public void DeactivateTarget()
    {
        if (targetToDeactivate != null)
        {
            targetToDeactivate.SetActive(false);
        }
    }
    
    /// <summary>
    /// 수동으로 타겟을 활성화
    /// </summary>
    public void ActivateTarget()
    {
        if (targetToDeactivate != null)
        {
            targetToDeactivate.SetActive(true);
        }
    }
    
    /// <summary>
    /// 트리거 상태 초기화 (재사용 가능하게 만들기)
    /// </summary>
    public void ResetTrigger()
    {
        hasTriggered = false;
    }
    
    // 범위 표시 (에디터에서만)
    private void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col is BoxCollider box && box.isTrigger)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.color = new Color(1f, 0f, 0f, 1f);
            Gizmos.DrawWireCube(box.center, box.size);
        }
        else if (col is SphereCollider sphere && sphere.isTrigger)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawSphere(sphere.center, sphere.radius);
            Gizmos.color = new Color(1f, 0f, 0f, 1f);
            Gizmos.DrawWireSphere(sphere.center, sphere.radius);
        }
    }
}
