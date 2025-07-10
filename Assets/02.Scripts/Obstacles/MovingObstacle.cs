using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class MovingObstacle : BaseObstacle
{
    [Header("이동 설정")]
    [Tooltip("이동할 방향들 (순서대로 이동)")]
    [SerializeField] private List<Vector3> _moveDirections = new List<Vector3>();

    [Tooltip("한 방향으로 이동하는 데 걸리는 시간")]
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
        if (_moveDirections.Count == 0)
        {
            Debug.LogWarning("이동 방향이 지정되지 않았습니다. 기본 이동 방향을 추가합니다.");
            _moveDirections.Add(Vector3.forward * 3f); // 기본 이동 방향 추가
        }

        _moveSequence = DOTween.Sequence();
        
        // 각 방향에 대해 이동 후 시작 위치로 돌아오는 패턴 생성
        for (int i = 0; i < _moveDirections.Count; i++)
        {
            // 1. 시작 위치에서 해당 방향으로 이동
            Vector3 targetPosition = _startPosition + _moveDirections[i];
            _moveSequence.Append(transform.DOMove(targetPosition, _moveTime)
                .SetEase(Ease.InOutQuad)
                .SetUpdate(UpdateType.Fixed));
            
            // 도달 후 대기 (옵션)
            if (_useWaitTime && _waitTime > 0)
            {
                _moveSequence.AppendInterval(_waitTime);
            }
            
            // 2. 시작 위치로 복귀
            _moveSequence.Append(transform.DOMove(_startPosition, _moveTime)
                .SetEase(Ease.InOutQuad)
                .SetUpdate(UpdateType.Fixed));
                
            // 시작 위치에서 대기 (옵션)
            if (_useWaitTime && _waitTime > 0)
            {
                _moveSequence.AppendInterval(_waitTime);
            }
        }
        
        // 3. 모든 방향을 순회한 후, 처음부터 다시 반복
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
