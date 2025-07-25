using UnityEngine;
using DG.Tweening;

/// <summary>
/// 플레이어가 트리거에 들어오면 움직이기 시작하는 장애물
/// </summary>
public class TriggerActivatedMovingObstacle : MovingObstacle
{
    [Header("트리거 설정")]
    [Tooltip("플레이어 감지 후 이동 시작 전 지연 시간")]
    [SerializeField] private float _delayBeforeMoving = 0.5f;
    
    [Tooltip("플레이어가 트리거를 벗어난 후에도 계속 움직일지 여부")]
    [SerializeField] private bool _continueMovingAfterTriggerExit = true;

    private bool _isMoving = false;
    private Tween _moveTween;

    private void Awake()
    {
        // 트리거 방식으로 설정
        _senseMode = SenseMode.Trigger;
    }

    // Start 메서드 오버라이드 - 자동으로 움직이지 않도록 함
    protected override void Start()
    {
        _startPosition = transform.position;
        _lastPosition = transform.position;
        // 부모 클래스의 StartMoving()을 호출하지 않고, 트리거 진입을 기다림
    }

    // 트리거 진입 시 움직임 시작
    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        
        if (IsPlayerObject(other.gameObject) && !_isMoving)
        {
            StartMovingWithDelay();
        }
    }
    
    // 트리거 이탈 시 움직임 정지 (설정에 따라)
    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
        
        if (IsPlayerObject(other.gameObject) && !_continueMovingAfterTriggerExit)
        {
            StopMoving();
        }
    }

    private void StartMovingWithDelay()
    {
        // 이미 움직이고 있다면 무시
        if (_isMoving) return;
        
        // 지연 시간 후 움직임 시작
        DOTween.Sequence()
            .AppendInterval(_delayBeforeMoving)
            .AppendCallback(StartCustomMoving)
            .SetUpdate(UpdateType.Normal);
    }

    // 부모 클래스의 StartMoving 메서드와 구분하기 위해 이름 변경
    private void StartCustomMoving()
    {
        _isMoving = true;
        
        // 왕복 경로 정의 (시작점 -> 목표점 -> 시작점)
        Vector3[] path = new Vector3[]
        {
            _startPosition + _moveTo,  // 목표 위치
            _startPosition             // 시작 위치로 돌아옴
        };

        // 경로를 따라 이동
        _moveTween = transform.DOPath(path, _totalPathTime, PathType.Linear)
            .SetEase(Ease.Linear)  // 일정한 속도로 이동
            .SetLoops(-1, LoopType.Restart)
            .SetUpdate(UpdateType.Fixed);
    }
    
    private void StopMoving()
    {
        if (!_isMoving) return;
        
        // 현재 위치에서 정지
        if (_moveTween != null)
        {
            _moveTween.Kill();
            _moveTween = null;
        }
        
        _isMoving = false;
    }
    
    // OnDestroy 메서드 오버라이드
    protected override void OnDestroy()
    {
        if (_moveTween != null)
        {
            _moveTween.Kill();
        }
        
        base.OnDestroy();
    }
} 