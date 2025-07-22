using UnityEngine;

public class CharacterSwitcher : MonoBehaviour
{
    [Header("모델 참조")]
    [SerializeField] private GameObject kidModel;
    [SerializeField] private GameObject schoolBoyModel;
    
    [Header("아바타 참조")]
    [SerializeField] private Avatar kidAvatar;
    [SerializeField] private Avatar schoolBoyAvatar;
    
    private Animator animator;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        
        // 초기 상태 설정 (Kid 모델로 시작)
        SwitchToKid();
    }
    
    /// <summary>
    /// Kid 모델로 전환
    /// </summary>
    public void SwitchToKid()
    {
        kidModel.SetActive(true);
        schoolBoyModel.SetActive(false);
        animator.avatar = kidAvatar;
        
        // 디버그 로그
        Debug.Log("캐릭터가 Kid로 전환되었습니다.");
    }
    
    /// <summary>
    /// School Boy 모델로 전환
    /// </summary>
    public void SwitchToSchoolBoy()
    {
        kidModel.SetActive(false);
        schoolBoyModel.SetActive(true);
        animator.avatar = schoolBoyAvatar;
        
        // 디버그 로그
        Debug.Log("캐릭터가 School Boy로 전환되었습니다.");
    }
} 