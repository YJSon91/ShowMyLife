 using UnityEngine;

/// <summary>
/// 플레이어가 닿으면 지정된 AppearingObstacle을 비활성화하는 컴포넌트
/// </summary>
public class AppearingObstacleDeactivator : MonoBehaviour
{
    [Tooltip("비활성화할 AppearingObstacle 컴포넌트")]
    [SerializeField] private AppearingObstacle targetObstacle;

    private void OnValidate()
    {
        // 콜라이더가 있는지 확인
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            //Debug.LogError("AppearingObstacleDeactivator에는 Collider 컴포넌트가 필요합니다!");
        }
        else if (!col.isTrigger)
        {
            // 자동으로 트리거로 설정
            col.isTrigger = true;
            //Debug.Log("콜라이더가 자동으로 트리거 모드로 설정되었습니다.");
        }
    }

    private void Start()
    {
        // 타겟 오브젝트 확인
        if (targetObstacle == null)
        {
            //Debug.LogError("비활성화할 AppearingObstacle이 지정되지 않았습니다!");
        }

        // 콜라이더 확인
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            //Debug.LogError("AppearingObstacleDeactivator에 Collider가 없습니다!");
        }
        else if (!col.isTrigger)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 플레이어 태그 확인
        if (other.CompareTag("Player") && targetObstacle != null)
        {
            //Debug.Log("플레이어가 비활성화 트리거에 닿음: 발판 비활성화 시도");
            targetObstacle.ForceDisappear();
        }
    }
} 
