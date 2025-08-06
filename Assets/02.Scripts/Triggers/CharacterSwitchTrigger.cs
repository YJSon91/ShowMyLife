using UnityEngine;

public class CharacterSwitchTrigger : MonoBehaviour
{
    [Header("전환 설정")]
    [SerializeField] private bool switchToSchoolBoy = true; // 키드 → 스쿨보이 전환
    [SerializeField] private bool switchToKid = false; // 스쿨보이 → 키드 전환
    
    [Header("한 번만 작동")]
    [SerializeField] private bool triggerOnce = true;
    private bool hasTriggered = false;
    
    private void OnTriggerEnter(Collider other)
    {
        // 이미 작동했고 한 번만 작동하는 설정이면 무시
        if (hasTriggered && triggerOnce)
            return;
            
        // 플레이어인지 확인
        if (other.CompareTag("Player"))
        {
            CharacterSwitcher switcher = other.GetComponent<CharacterSwitcher>();
            if (switcher != null)
            {
                // 설정에 따라 캐릭터 전환
                if (switchToSchoolBoy)
                {
                    switcher.SwitchToSchoolBoy();
                }
                else if (switchToKid)
                {
                    switcher.SwitchToKid();
                }
                
                // 트리거 작동 표시
                hasTriggered = true;
            }
        }
    }
} 