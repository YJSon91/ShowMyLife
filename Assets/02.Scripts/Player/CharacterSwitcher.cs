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
    
    // 현재 캐릭터 상태를 저장하는 불값
    private bool _isSchoolBoy = false;
    
    // 외부에서 현재 캐릭터 상태를 확인할 수 있는 프로퍼티
    public bool IsSchoolBoy => _isSchoolBoy;
    
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
        _isSchoolBoy = false;
    }
    
    /// <summary>
    /// School Boy 모델로 전환
    /// </summary>
    public void SwitchToSchoolBoy()
    {
        kidModel.SetActive(false);
        schoolBoyModel.SetActive(true);
        animator.avatar = schoolBoyAvatar;
        _isSchoolBoy = true;
    }
} 