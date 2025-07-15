using UnityEngine;
using DG.Tweening;

public class MovingObstacle : BaseObstacle
{
    [Header("이동 설정")]
    [Tooltip("이동할 거리")]
    [SerializeField] private Vector3 _moveTo = Vector3.zero;

    [Tooltip("한 번 이동하는 데 걸리는 시간")]
    [SerializeField] private float _moveTime = 1f;
    
    [Tooltip("각 위치에 도달 후 대기 시간")]
    [SerializeField] private float _waitTime = 0.5f;
    
    [Tooltip("대기 시간 사용 여부 (false면 대기 없이 즉시 이동)")]
    [SerializeField] private bool _useWaitTime = true;

    [Tooltip("플레이어 이동 시 적용할 힘 배율")]
    [SerializeField] private float _forceMultiplier = 1.0f;

    private Vector3 _lastPosition;
    private Vector3 _startPosition;
    private Sequence _moveSequence;

    private void Start()
    {
        _startPosition = transform.position;
        _lastPosition = transform.position;
        StartMoving();
    }

    private void FixedUpdate()
    {
        Vector3 currentPosition = transform.position;
        Vector3 delta = currentPosition - _lastPosition;

        if (delta != Vector3.zero)
        {
            MovePlayerIfOnTop(delta);
        }

        _lastPosition = currentPosition;
    }

    private void StartMoving()
    {
        _moveSequence = DOTween.Sequence();
        
        // 지정된 방향으로 이동
        Vector3 targetPosition = _startPosition + _moveTo;
        _moveSequence.Append(transform.DOMove(targetPosition, _moveTime)
            .SetEase(Ease.InOutQuad)
            .SetUpdate(UpdateType.Fixed));
        
        // 도달 후 대기 (옵션)
        if (_useWaitTime && _waitTime > 0)
        {
            _moveSequence.AppendInterval(_waitTime);
        }
        
        // 시작 위치로 복귀
        _moveSequence.Append(transform.DOMove(_startPosition, _moveTime)
            .SetEase(Ease.InOutQuad)
            .SetUpdate(UpdateType.Fixed));
            
        // 시작 위치에서 대기 (옵션)
        if (_useWaitTime && _waitTime > 0)
        {
            _moveSequence.AppendInterval(_waitTime);
        }
        
        // 무한 반복
        _moveSequence.SetLoops(-1, LoopType.Restart);
    }

    // OnCollision 방식으로 변경!
    protected void MovePlayerIfOnTop(Vector3 delta)
    {
        if (IsPlayerOnPlatform())
        {
            Rigidbody rb = GetPlayerRigidbody();
            if (rb != null)
            {
                rb.MovePosition(rb.position + delta * _forceMultiplier);
            }
        }
    }
    
    private void OnDestroy()
    {
        // 시퀀스 정리
        if (_moveSequence != null && _moveSequence.IsActive())
        {
            _moveSequence.Kill();
        }
    }
}
