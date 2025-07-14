using UnityEngine;

/// <summary>
/// 플레이어 애니메이션 이벤트를 처리하는 클래스
/// 애니메이션 클립에서 이벤트로 호출되는 메서드들을 관리합니다.
/// </summary>
public class PlayerAnimationEventHandler : MonoBehaviour
{
    private SoundManager _soundManager;
    private bool _isJumping = false;

    private void Start()
    {
        // GameManager를 통해 SoundManager 참조 가져오기
        if (GameManager.Instance != null)
        {
            _soundManager = GameManager.Instance.SoundManager;
            
        }
        
        if (_soundManager == null)
        {
            Debug.LogWarning("[PlayerAnimationEventHandler] SoundManager를 찾을 수 없습니다!");
        }
    }

    /// <summary>
    /// 점프 애니메이션 시작 시 호출될 이벤트 메서드
    /// 애니메이션 클립에서 점프 시작 프레임에 이벤트로 등록하여 사용
    /// </summary>
    public void OnJumpAnimationEvent()
    {
        
        // 중복 재생 방지를 위한 상태 체크
        if (!_isJumping)
        {
            _isJumping = true;
            if (_soundManager != null)
            {
                _soundManager.PlaySFX(SfxType.Jump);
                
            }
        }
    }
    
    /// <summary>
    /// 점프 애니메이션 종료 시 호출될 이벤트 메서드
    /// 애니메이션 클립에서 점프 종료 프레임에 이벤트로 등록하여 사용
    /// </summary>
    public void OnJumpAnimationEndEvent()
    {
        GameManager.Instance.Player._stateController.PublicSwitchState();
        // 점프 상태 초기화
        _isJumping = false;
        
    }

    /// <summary>
    /// 착지 애니메이션에서 호출될 이벤트 메서드
    /// </summary>
    public void OnLandAnimationEvent()
    {
        if (_soundManager != null)
        {
            _soundManager.PlaySFX(SfxType.Land);
           
        }
    }

    /// <summary>
    /// 발소리 애니메이션에서 호출될 이벤트 메서드
    /// </summary>
    public void OnFootstepAnimationEvent()
    {
        if (_soundManager != null)
        {
            // 걷기/달리기 상태에 따라 다른 사운드 재생 가능
            // 현재는 걷기 사운드만 재생
            _soundManager.PlaySFX(SfxType.Walk);
        }
    }  
    
    
    /// <summary>
    /// 발소리 애니메이션에서 호출될 이벤트 메서드
    /// </summary>
    public void OnFootstepSprintAnimationEvent()
    {
        if (_soundManager != null)
        {
            // 걷기/달리기 상태에 따라 다른 사운드 재생 가능
            // 현재는 걷기 사운드만 재생
            _soundManager.PlaySFX(SfxType.Run);
        }
    }
} 