using UnityEngine;
using DG.Tweening; // DOTween 네임스페이스 추가

/// <summary>
/// 미끄럼틀 장애물 트리거 영역 관리
/// 플레이어가 진입하면 강제로 슬립 상태로 만드는 컴포넌트
/// </summary>
public class SlideZone : MonoBehaviour
{
    #region 슬립 설정

    [Header("슬립 설정")]
    [Tooltip("미끄럼틀 진행 방향을 나타내는 Transform (없으면 현재 오브젝트의 forward 사용)")]
    [SerializeField] private Transform _slideDirection;
    
    [Tooltip("미끄러지는 최대 속도")]
    [SerializeField] private float _slideForce = 8f;
    
    [Tooltip("슬립 중 중력 배수")]
    [SerializeField] private float _slipGravityMultiplier = 2f;
    
    [Tooltip("슬립 중 플레이어 입력 무시 정도 (0: 완전 제어 가능, 1: 완전 제어 불가)")]
    [SerializeField] private float _inputReduction = 0.8f;

    [Tooltip("초기 슬라이딩 속도")]
    [SerializeField] private float _initialSlideSpeed = 2f;

    [Tooltip("최대 속도까지 가속하는 시간 (초)")]
    [SerializeField] private float _accelerationDuration = 1.5f;

    [Tooltip("가속 곡선 (비어있으면 기본 InQuad 사용)")]
    [SerializeField] private AnimationCurve _accelerationCurve;

    [Header("디버그")]
    [Tooltip("슬립 방향을 Scene 뷰에서 시각적으로 표시")]
    [SerializeField] private bool _showDirectionGizmo = true;
    [SerializeField] private float _gizmoLength = 2f;

    // 현재 활성화된 트윈 저장용
    private Tween _currentAccelerationTween;
    
    // 플레이어 참조 저장
    private Player _currentPlayer;
    // 원래 Sprint 이벤트 핸들러 저장
    private System.Action _originalSprintActivated;
    // Sprint 비활성화 상태 추적
    private bool _isSprintDisabled = false;

    #endregion

    #region Unity 라이프사이클

    private void Start()
    {
        //ValidateSettings();
        
        // 가속 곡선이 비어있으면 기본 곡선 생성
        if (_accelerationCurve.keys.Length == 0)
        {
            _accelerationCurve = new AnimationCurve(
                new Keyframe(0, 0, 0, 1),
                new Keyframe(1, 1, 1, 0)
            );
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 플레이어 트리거 콜라이더 감지
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponentInParent<Player>();
            if (player != null)
            {
                _currentPlayer = player;
                Vector3 slideDir = GetSlideDirection();
                
                // 달리기 입력 비활성화
                DisableSprintInput(player);
                //Debug.Log("슬라이드존: 달리기 입력 비활성화됨");
                
                // 먼저 초기 슬라이딩 상태 활성화
                player.MovementController.ActivateObstacleSlide(slideDir, _initialSlideSpeed, _slipGravityMultiplier, _inputReduction);
                //Debug.Log($"플레이어 슬립 시작 - 방향: {slideDir}, 초기 속도: {_initialSlideSpeed}");
                
                // 기존 트윈이 있으면 중단
                if (_currentAccelerationTween != null && _currentAccelerationTween.IsActive())
                {
                    _currentAccelerationTween.Kill();
                }
                
                // DOTween을 사용하여 초기 속도에서 최대 속도로 부드럽게 가속
                _currentAccelerationTween = DOVirtual.Float(_initialSlideSpeed, _slideForce, _accelerationDuration, (speed) => {
                    // 현재 속도 업데이트
                    player.MovementController.UpdateObstacleSlideSpeed(speed);
                }).SetEase(_accelerationCurve);
                
                //Debug.Log($"가속 시작: {_initialSlideSpeed} → {_slideForce}, 시간: {_accelerationDuration}초");
            }
            else
            {
                //Debug.LogWarning("SlideZone: Player 컴포넌트를 찾을 수 없습니다.");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponentInParent<Player>();
            if (player != null)
            {
                // 가속 트윈 중단
                if (_currentAccelerationTween != null && _currentAccelerationTween.IsActive())
                {
                    _currentAccelerationTween.Kill();
                    _currentAccelerationTween = null;
                }
                
                player.MovementController.DeactivateObstacleSlide();
                //Debug.Log("플레이어 슬립 종료");
                
                // 달리기 입력 다시 활성화
                EnableSprintInput(player);
                
                _currentPlayer = null;
            }
        }
    }

    private void OnDestroy()
    {
        // 안전하게 트윈 정리
        if (_currentAccelerationTween != null && _currentAccelerationTween.IsActive())
        {
            _currentAccelerationTween.Kill();
            _currentAccelerationTween = null;
        }
        
        // 플레이어가 아직 존재하면 달리기 입력 복원
        if (_currentPlayer != null && _isSprintDisabled)
        {
            EnableSprintInput(_currentPlayer);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (_showDirectionGizmo)
        {
            DrawDirectionGizmo();
        }
    }

    #endregion

    #region 헬퍼 메서드

    /// <summary>
    /// 슬립 방향을 가져옵니다
    /// </summary>
    /// <returns>정규화된 슬립 방향</returns>
    private Vector3 GetSlideDirection()
    {
        if (_slideDirection != null)
        {
            Vector3 direction = _slideDirection.forward;
            direction.y = 0f; // Y축 제거하여 수평 방향만 사용
            return direction.normalized;
        }
        else
        {
            Vector3 direction = transform.forward;
            direction.y = 0f;
            return direction.normalized;
        }
    }

    /// <summary>
    /// 설정값들을 검증합니다
    /// </summary>
    //private void ValidateSettings()
    //{
    //    if (_slideForce <= 0f)
    //    {
    //        Debug.LogWarning($"SlideZone ({gameObject.name}): _slideForce가 0 이하입니다. 기본값 8f로 설정합니다.");
    //        _slideForce = 8f;
    //    }

    //    if (_initialSlideSpeed < 0f)
    //    {
    //        Debug.LogWarning($"SlideZone ({gameObject.name}): _initialSlideSpeed가 0 미만입니다. 0으로 설정합니다.");
    //        _initialSlideSpeed = 0f;
    //    }
        
    //    if (_initialSlideSpeed > _slideForce)
    //    {
    //        Debug.LogWarning($"SlideZone ({gameObject.name}): _initialSlideSpeed가 _slideForce보다 큽니다. _slideForce와 동일하게 설정합니다.");
    //        _initialSlideSpeed = _slideForce;
    //    }
        
    //    if (_accelerationDuration <= 0f)
    //    {
    //        Debug.LogWarning($"SlideZone ({gameObject.name}): _accelerationDuration이 0 이하입니다. 기본값 1.5f로 설정합니다.");
    //        _accelerationDuration = 1.5f;
    //    }

    //    if (_slipGravityMultiplier <= 0f)
    //    {
    //        Debug.LogWarning($"SlideZone ({gameObject.name}): _slipGravityMultiplier가 0 이하입니다. 기본값 2f로 설정합니다.");
    //        _slipGravityMultiplier = 2f;
    //    }

    //    _inputReduction = Mathf.Clamp01(_inputReduction);

    //    if (_slideDirection == null)
    //    {
    //        Debug.Log($"SlideZone ({gameObject.name}): _slideDirection이 설정되지 않았습니다. 현재 오브젝트의 forward 방향을 사용합니다.");
    //    }
    //}

    /// <summary>
    /// Scene 뷰에서 슬립 방향을 시각적으로 표시합니다
    /// </summary>
    private void DrawDirectionGizmo()
    {
        Vector3 startPos = transform.position;
        Vector3 direction = GetSlideDirection();
        Vector3 endPos = startPos + direction * _gizmoLength;

        // 슬립 방향 화살표
        Gizmos.color = Color.red;
        Gizmos.DrawLine(startPos, endPos);
        
        // 화살표 머리 그리기
        Vector3 arrowHead1 = endPos - direction * 0.3f + Vector3.Cross(direction, Vector3.up) * 0.2f;
        Vector3 arrowHead2 = endPos - direction * 0.3f - Vector3.Cross(direction, Vector3.up) * 0.2f;
        
        Gizmos.DrawLine(endPos, arrowHead1);
        Gizmos.DrawLine(endPos, arrowHead2);

        // 방향 텍스트 (에디터에서만)
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(endPos + Vector3.up * 0.5f, 
            $"슬립 방향\n초기 속도: {_initialSlideSpeed}\n최대 속도: {_slideForce}\n가속 시간: {_accelerationDuration}초\n중력: x{_slipGravityMultiplier}");
        #endif
    }
    
    /// <summary>
    /// 플레이어의 달리기 입력을 비활성화합니다
    /// </summary>
    private void DisableSprintInput(Player player)
    {
        if (player != null && player.InputReader != null && !_isSprintDisabled)
        {
            // 현재 달리기 상태 비활성화
            player.InputReader.onSprintDeactivated?.Invoke();
            
            // 원래 이벤트 핸들러 저장
            _originalSprintActivated = player.InputReader.onSprintActivated;
            
            // 달리기 활성화 이벤트를 빈 델리게이트로 대체
            player.InputReader.onSprintActivated = () => { 
                // 아무 작업도 수행하지 않음 (쉬프트키 입력 무시)
                //Debug.Log("슬라이드존: 달리기 입력 무시됨");
            };
            
            _isSprintDisabled = true;
            //Debug.Log("슬라이드존: 달리기 입력 비활성화됨");
        }
    }
    
    /// <summary>
    /// 플레이어의 달리기 입력을 다시 활성화합니다
    /// </summary>
    private void EnableSprintInput(Player player)
    {
        if (player != null && player.InputReader != null && _isSprintDisabled)
        {
            // 원래 이벤트 핸들러 복원
            player.InputReader.onSprintActivated = _originalSprintActivated;
            
            _isSprintDisabled = false;
            //Debug.Log("슬라이드존: 달리기 입력 복원됨");
        }
    }

    #endregion
}
