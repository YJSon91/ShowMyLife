using UnityEngine;

/// <summary>
/// 플레이어가 트리거에 닿으면 게임오브젝트를 비활성화하는 간단한 컴포넌트
/// </summary>
public class SimplePlayerDeactivator : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // 플레이어 태그 확인
        if (other.CompareTag("Player"))
        {
            // 자신을 비활성화
            gameObject.SetActive(false);
        }
    }
}
