using UnityEngine;

public class SlideZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<Player>();
            if (player != null)
            {
                player.MovementController.ActivateSliding(); // 슬라이딩 시작
                Debug.Log("슬라이딩을 시작합니다.");
            }
            Debug.Log("플레이어를 감지했습니다.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<Player>();
            if (player != null)
            {
                player.MovementController.DeactivateSliding(); // 슬라이딩 종료
                Debug.Log("슬라이딩을 종료합니다.");
            }
            Debug.Log("플레이어가 없습니다..");
        }
    }
}
