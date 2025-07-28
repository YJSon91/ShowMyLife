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

    [Header("플레이어 이동 설정")]
    [Tooltip("플레이어가 장애물 위에 있는지 감지할 콜라이더")]
    [SerializeField] private BoxCollider _topCollider;

    private bool _isMoving = false;
    private Tween _moveTween;
    private bool _playerIsOnTop = false;
    private Transform _playerTransformOnTop;
    private Rigidbody _playerRigidbodyOnTop;

    private void Awake()
    {
        // 트리거 방식으로 설정
        _senseMode = SenseMode.Trigger;
        
        // 상단 콜라이더가 없으면 자동 생성
        if (_topCollider == null)
        {
            _topCollider = gameObject.AddComponent<BoxCollider>();
            
            // 기존 콜라이더가 있다면 크기 복사
            Collider existingCollider = GetComponent<Collider>();
            if (existingCollider != null && existingCollider is BoxCollider)
            {
                BoxCollider boxCollider = existingCollider as BoxCollider;
                _topCollider.size = new Vector3(boxCollider.size.x, 0.1f, boxCollider.size.z);
                _topCollider.center = new Vector3(boxCollider.center.x, boxCollider.size.y / 2 + 0.05f, boxCollider.center.z);
            }
            else
            {
                // 기본 크기 설정
                _topCollider.size = new Vector3(1f, 0.1f, 1f);
                _topCollider.center = new Vector3(0, 0.55f, 0);
            }
            
            // 트리거로 설정하지 않음 (물리적 충돌 필요)
            _topCollider.isTrigger = false;
        }
    }

    // Start 메서드 오버라이드 - 자동으로 움직이지 않도록 함
    protected override void Start()
    {
        _startPosition = transform.position;
        _lastPosition = transform.position;
        // 부모 클래스의 StartMoving()을 호출하지 않고, 트리거 진입을 기다림
    }

    // 플레이어 이동을 위한 FixedUpdate 구현
    protected override void FixedUpdate()
    {
        // 장애물이 움직이는 경우에만 플레이어 이동 처리
        if (_isMoving)
        {
            Vector3 currentPosition = transform.position;
            Vector3 delta = currentPosition - _lastPosition;

            if (delta != Vector3.zero && _playerIsOnTop && _playerRigidbodyOnTop != null)
            {
                _playerRigidbodyOnTop.MovePosition(_playerRigidbodyOnTop.position + delta * _forceMultiplier);
            }

            _lastPosition = currentPosition;
        }
    }

    // 플레이어가 장애물 위에 올라왔을 때
    private void OnCollisionEnter(Collision collision)
    {
        if (IsPlayerObject(collision.gameObject))
        {
            // 충돌 지점이 장애물 위쪽인지 확인
            foreach (ContactPoint contact in collision.contacts)
            {
                if (Vector3.Dot(contact.normal, Vector3.down) > 0.7f)
                {
                    _playerIsOnTop = true;
                    _playerTransformOnTop = collision.transform;
                    _playerRigidbodyOnTop = collision.rigidbody;
                    break;
                }
            }
        }
    }

    // 플레이어가 장애물에서 내려왔을 때
    private void OnCollisionExit(Collision collision)
    {
        if (IsPlayerObject(collision.gameObject) && _playerTransformOnTop == collision.transform)
        {
            _playerIsOnTop = false;
            _playerTransformOnTop = null;
            _playerRigidbodyOnTop = null;
        }
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