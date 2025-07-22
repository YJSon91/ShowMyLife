using UnityEngine;

/// <summary>
/// 플레이어가 착지했을 때 시작 지점 기능을 제공하는 장애물
/// </summary>
public class StartingAreaObstacle : BaseObstacle
{
    [Header("시작 지점 설정")]
    [Tooltip("플레이어 레이어 마스크")]
    [SerializeField] private LayerMask _playerLayerMask;
    
    [Tooltip("일반 착지에도 시작 지점 UI를 표시할지 여부")]
    [SerializeField] private bool _activateOnNormalLanding = true;
    
    [Tooltip("하드 착지에만 시작 지점 UI를 표시할지 여부")]
    [SerializeField] private bool _activateOnHardLanding = true;
    
    [Tooltip("이 지점의 이름 (저장 시 사용)")]
    [SerializeField] private string _areaName = "시작 지점";
    
    private PlayerAnimationEventHandler _playerAnimEventHandler;
    
    private void OnTriggerEnter(Collider other)
    {
        // 플레이어 레이어 확인
        if (((1 << other.gameObject.layer) & _playerLayerMask.value) != 0)
        {
            // 플레이어의 애니메이션 이벤트 핸들러 컴포넌트 가져오기
            Player player = other.GetComponentInParent<Player>();
            if (player != null && player.AnimationEventHandler != null)
            {
                _playerAnimEventHandler = player.AnimationEventHandler;
                
                // 이벤트 구독
                if (_activateOnNormalLanding)
                {
                    _playerAnimEventHandler.OnLandingAnimationEvent += HandleLandingEvent;
                }
                
                if (_activateOnHardLanding)
                {
                    _playerAnimEventHandler.OnLandingHardAnimationEvent += HandleHardLandingEvent;
                }
                
                Debug.Log($"플레이어가 시작 지점({_areaName})에 들어왔습니다.");
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        // 플레이어 레이어 확인
        if (((1 << other.gameObject.layer) & _playerLayerMask.value) != 0 && _playerAnimEventHandler != null)
        {
            // 이벤트 구독 해제
            if (_activateOnNormalLanding)
            {
                _playerAnimEventHandler.OnLandingAnimationEvent -= HandleLandingEvent;
            }
            
            if (_activateOnHardLanding)
            {
                _playerAnimEventHandler.OnLandingHardAnimationEvent -= HandleHardLandingEvent;
            }
            
            _playerAnimEventHandler = null;
            Debug.Log($"플레이어가 시작 지점({_areaName})에서 나갔습니다.");
        }
    }
    
    private void HandleLandingEvent(PlayerAnimationEventHandler eventHandler)
    {
        // 일반 착지 시 시작 지점 UI 표시
        ShowStartingAreaUI();
    }
    
    private void HandleHardLandingEvent(PlayerAnimationEventHandler eventHandler)
    {
        // 하드 착지 시 시작 지점 UI 표시
        ShowStartingAreaUI();
    }
    
    private void ShowStartingAreaUI()
    {
        // GameManager를 통해 UI 매니저에 접근하여 시작 지점 UI 표시
        if (GameManager.Instance != null && GameManager.Instance.UIManager != null)
        {
            Debug.Log($"시작 지점 UI 표시: {_areaName}");
            //GameManager.Instance.UIManager.ShowStartingAreaUI(_areaName, transform.position);
        }
        else
        {
            Debug.LogWarning("GameManager 또는 UIManager를 찾을 수 없습니다.");
        }
    }
    
    private void OnDestroy()
    {
        // 이벤트 구독 해제 확인
        if (_playerAnimEventHandler != null)
        {
            if (_activateOnNormalLanding)
            {
                _playerAnimEventHandler.OnLandingAnimationEvent -= HandleLandingEvent;
            }
            
            if (_activateOnHardLanding)
            {
                _playerAnimEventHandler.OnLandingHardAnimationEvent -= HandleHardLandingEvent;
            }
        }
    }
} 