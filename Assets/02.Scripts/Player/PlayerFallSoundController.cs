using UnityEngine;

/// <summary>
/// 플레이어의 낙하 시간에 따른 사운드를 관리하는 컨트롤러
/// </summary>
public class PlayerFallSoundController : MonoBehaviour
{
    [Header("낙하 사운드 설정")]
    [Tooltip("짧은 낙하 시간 기준 (1.5~2초)")]
    [SerializeField] private float _shortFallTime = 1.5f;
    
    [Tooltip("중간 낙하 시간 기준 (2~2.5초)")]
    [SerializeField] private float _mediumFallTime = 2.0f;
    
    [Tooltip("긴 낙하 시간 기준 (2.5초 이상)")]
    [SerializeField] private float _longFallTime = 2.5f;
    
    // 컴포넌트 참조
    private Player _player;
    private PlayerMovementController _movementController;
    private PlayerStateController _stateController;
    private SoundManager _soundManager;
    
    // 낙하 관련 변수
    private bool _isFalling = false;
    private float _fallStartTime = 0f;
    private float _fallDuration = 0f;
    
    private void Awake()
    {
        // 컴포넌트 초기화
        _player = GetComponentInParent<Player>();
    }
    
    private void Start()
    {
        // Player 컴포넌트 참조가 Awake에서 설정되지 않았을 경우 다시 시도
        if (_player == null)
        {
            _player = GetComponentInParent<Player>();
        }
        
        if (_player != null)
        {
            _movementController = _player.MovementController;
            _stateController = _player.StateController;
            
            // GameManager를 통해 SoundManager 참조 가져오기
            if (GameManager.Instance != null)
            {
                _soundManager = GameManager.Instance.SoundManager;
            }
            
            // 상태 변경 이벤트 구독
            _stateController.OnStateChanged += HandleStateChanged;
        }
        else
        {
            // Player 컴포넌트를 찾을 수 없습니다
        }
    }
    
    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (_stateController != null)
        {
            _stateController.OnStateChanged -= HandleStateChanged;
        }
    }
    
    /// <summary>
    /// 플레이어 상태 변경 처리
    /// </summary>
    private void HandleStateChanged(PlayerAnimationState oldState, PlayerAnimationState newState)
    {
        // 낙하 상태 시작
        if (newState == PlayerAnimationState.Fall)
        {
            StartFalling();
        }
        // 낙하 상태 종료
        else if (oldState == PlayerAnimationState.Fall)
        {
            StopFalling();
        }
    }
    
    /// <summary>
    /// 낙하 시작 처리
    /// </summary>
    private void StartFalling()
    {
        _isFalling = true;
        _fallStartTime = Time.time;
    }
    
    /// <summary>
    /// 낙하 종료 및 사운드 재생
    /// </summary>
    private void StopFalling()
    {
        if (!_isFalling) return;
        
        _isFalling = false;
        _fallDuration = Time.time - _fallStartTime;
        
        // 낙하 시간에 따라 다른 사운드 재생
        PlayFallSound();
    }
    
    /// <summary>
    /// 낙하 시간에 따른 사운드 재생
    /// </summary>
    private void PlayFallSound()
    {
        // 너무 짧은 낙하는 무시
        if (_fallDuration < _shortFallTime) return;
        
        if (_soundManager != null)
        {
            // 낙하 시간에 따라 적절한 사운드 타입 선택
            SfxType fallType;
            
            if (_fallDuration >= _longFallTime)
            {
                fallType = SfxType.FallLong;
            }
            else if (_fallDuration >= _mediumFallTime)
            {
                fallType = SfxType.FallMedium;
            }
            else
            {
                fallType = SfxType.FallShort;
            }
            
            // 선택된 타입의 사운드 재생
            _soundManager.PlaySFX(fallType);
        }
    }
}