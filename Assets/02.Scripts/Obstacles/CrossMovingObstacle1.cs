using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class CrossMovingObstacle1 : BaseObstacle
{
    [Header("이동 설정")]
    [Tooltip("십자 모양 이동 반경")]
    [SerializeField] private float _moveRadius = 3.0f;

    [Tooltip("전체 이동 경로를 완료하는 데 걸리는 시간")]
    [SerializeField] private float _totalPathTime = 4f;
    
    [Tooltip("플레이어 이동 시 적용할 힘 배율")]
    [SerializeField] private float _forceMultiplier = 1.0f;

    private Vector3 _lastPosition;
    private Vector3 _startPosition;

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
        // 십자 모양의 경로 정의 (중앙->오른쪽->중앙->왼쪽->중앙->위쪽->중앙->아래쪽->중앙)
        Vector3[] path = new Vector3[]
        {
            _startPosition + Vector3.forward * _moveRadius,  // 위쪽
            _startPosition,                                  // 중앙
            _startPosition + Vector3.back * _moveRadius,     // 아래쪽
            _startPosition,                                   // 중앙
            _startPosition + Vector3.right * _moveRadius,    // 오른쪽
            _startPosition,                                  // 중앙
            _startPosition + Vector3.left * _moveRadius,     // 왼쪽
            _startPosition,                                  // 중앙
            
        };

        // 경로를 따라 이동
        transform.DOPath(path, _totalPathTime, PathType.Linear)
            .SetEase(Ease.Linear)  // 일정한 속도로 이동
            .SetLoops(-1, LoopType.Restart)
            .SetUpdate(UpdateType.Fixed);
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
        DOTween.Kill(transform);
    }
} 