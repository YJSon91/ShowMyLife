using UnityEngine;
using DG.Tweening;

public class CrossMovingObstacle : BaseObstacle
{
    [Header("이동 설정")]
    [Tooltip("수평 이동 거리")]
    [SerializeField] private float _horizontalDistance = 3f;
    
    [Tooltip("수직 이동 거리 (Y축 방향)")]
    [SerializeField] private float _verticalDistance = 3f;
    
    [Tooltip("한 방향으로 이동하는 데 걸리는 시간")]
    [SerializeField] private float _moveTime = 1f;
    
    [Tooltip("각 위치에 도달 후 대기 시간")]
    [SerializeField] private float _waitTime = 0.5f;
    
    [Tooltip("플레이어 이동 시 적용할 힘 배율")]
    [SerializeField] private float _forceMultiplier = 1.0f;

    private Vector3 _lastPosition;
    private Vector3 _startPosition;
    private Sequence _moveSequence;

    private void Start()
    {
        _startPosition = transform.position;
        _lastPosition = transform.position;
        StartCrossMoving();
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

    private void StartCrossMoving()
    {
        _moveSequence = DOTween.Sequence();
        
        // 1. 오른쪽으로 이동
        _moveSequence.Append(transform.DOMove(_startPosition + Vector3.right * _horizontalDistance, _moveTime)
            .SetEase(Ease.InOutQuad)
            .SetUpdate(UpdateType.Fixed));
        // 끝 지점에서 대기
        _moveSequence.AppendInterval(_waitTime);
        
        // 2. 시작 위치로 복귀
        _moveSequence.Append(transform.DOMove(_startPosition, _moveTime)
            .SetEase(Ease.InOutQuad)
            .SetUpdate(UpdateType.Fixed));
        // 중앙에서 대기
        _moveSequence.AppendInterval(_waitTime);
        
        // 3. 위쪽으로 이동 (Y축 방향)
        _moveSequence.Append(transform.DOMove(_startPosition + Vector3.up * _verticalDistance, _moveTime)
            .SetEase(Ease.InOutQuad)
            .SetUpdate(UpdateType.Fixed));
        // 끝 지점에서 대기
        _moveSequence.AppendInterval(_waitTime);
        
        // 4. 시작 위치로 복귀
        _moveSequence.Append(transform.DOMove(_startPosition, _moveTime)
            .SetEase(Ease.InOutQuad)
            .SetUpdate(UpdateType.Fixed));
        // 중앙에서 대기
        _moveSequence.AppendInterval(_waitTime);
        
        // 5. 왼쪽으로 이동
        _moveSequence.Append(transform.DOMove(_startPosition + Vector3.left * _horizontalDistance, _moveTime)
            .SetEase(Ease.InOutQuad)
            .SetUpdate(UpdateType.Fixed));
        // 끝 지점에서 대기
        _moveSequence.AppendInterval(_waitTime);
        
        // 6. 시작 위치로 복귀
        _moveSequence.Append(transform.DOMove(_startPosition, _moveTime)
            .SetEase(Ease.InOutQuad)
            .SetUpdate(UpdateType.Fixed));
        // 중앙에서 대기
        _moveSequence.AppendInterval(_waitTime);
        
        // 7. 아래쪽으로 이동 (Y축 방향)
        _moveSequence.Append(transform.DOMove(_startPosition + Vector3.down * _verticalDistance, _moveTime)
            .SetEase(Ease.InOutQuad)
            .SetUpdate(UpdateType.Fixed));
        // 끝 지점에서 대기
        _moveSequence.AppendInterval(_waitTime);
        
        // 8. 시작 위치로 복귀
        _moveSequence.Append(transform.DOMove(_startPosition, _moveTime)
            .SetEase(Ease.InOutQuad)
            .SetUpdate(UpdateType.Fixed));
        // 중앙에서 대기
        _moveSequence.AppendInterval(_waitTime);
        
        // 무한 반복
        _moveSequence.SetLoops(-1, LoopType.Restart);
    }

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