using UnityEngine;

/// <summary>
/// 플레이어가 하드 착지했을 때 대화 UI를 표시하는 장애물
/// </summary>
public class DialogueObstacle : BaseObstacle
{
    [Header("하드 착지 대화 설정")]
    [Tooltip("플레이어 태그")]
    [SerializeField] private string _playerTag = "Player";
    
    [Tooltip("하드 착지 시 대화 UI를 표시할지 여부")]
    [SerializeField] private bool _activateOnHardLanding = true;
    
    private PlayerAnimationEventHandler _playerAnimEventHandler;
    
    protected override void OnTriggerEnter(Collider other)
    {
        // 플레이어 태그 확인
        if (other.CompareTag(_playerTag))
        {
            // 플레이어의 애니메이션 이벤트 핸들러 컴포넌트 가져오기
            Player player = other.GetComponentInParent<Player>();
            if (player != null && player.AnimationEventHandler != null)
            {
                _playerAnimEventHandler = player.AnimationEventHandler;
                
                // 하드 착지 이벤트만 구독
                if (_activateOnHardLanding)
                {
                    _playerAnimEventHandler.OnLandingHardAnimationEvent += HandleHardLandingEvent;
                }
                
                Debug.Log("플레이어가 대화 발판에 들어왔습니다.");
            }
        }
    }
    
    protected override void OnTriggerExit(Collider other)
    {
        // 플레이어 태그 확인
        if (other.CompareTag(_playerTag) && _playerAnimEventHandler != null)
        {
            // 하드 착지 이벤트 구독 해제
            if (_activateOnHardLanding)
            {
                _playerAnimEventHandler.OnLandingHardAnimationEvent -= HandleHardLandingEvent;
            }
            
            _playerAnimEventHandler = null;
            Debug.Log("플레이어가 대화 발판에서 나갔습니다.");
        }
    }
    
    private void HandleHardLandingEvent(PlayerAnimationEventHandler eventHandler)
    {
        // 하드 착지 시 대화 UI 표시
        GameManager.Instance.DialogueManager.ShowRandomDialogueByType(DialogueTriggerType.Fall_High);
    }
    
    
    
    private void OnDestroy()
    {
        // 이벤트 구독 해제 확인
        if (_playerAnimEventHandler != null && _activateOnHardLanding)
        {
            _playerAnimEventHandler.OnLandingHardAnimationEvent -= HandleHardLandingEvent;
        }
    }
} 