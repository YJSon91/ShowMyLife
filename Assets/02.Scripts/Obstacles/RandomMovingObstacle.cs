using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMovingObstacle : BaseObstacle
{
    [Header("이동 설정")]
    [SerializeField] private Vector3 _moveDirection = Vector3.right;
    [SerializeField] private float _moveDistance = 3f;
    [SerializeField] private float _moveTime = 1.5f;

    [Header("A에서 랜덤 대기 (최소~최대)")]
    [SerializeField] private float _minRandomDelay = 0.5f;
    [SerializeField] private float _maxRandomDelay = 2.5f;

    private Vector3 _startPos;
    private Vector3 _endPos;

    private void Start()
    {
        _startPos = transform.position;
        _endPos = _startPos + _moveDirection.normalized * _moveDistance;
        StartCoroutine(MoveLoop());
    }

    private IEnumerator MoveLoop()
    {
        while (true)
        {
            // [A->B]로 이동
            yield return transform.DOMove(_endPos, _moveTime)
                .SetEase(Ease.InOutQuad)
                .SetUpdate(UpdateType.Fixed)
                .WaitForCompletion();

            // [B->A]로 복귀
            yield return transform.DOMove(_startPos, _moveTime)
                .SetEase(Ease.InOutQuad)
                .SetUpdate(UpdateType.Fixed)
                .WaitForCompletion();

            // [A]에 도착하면 랜덤 대기
            float randomWait = Random.Range(_minRandomDelay, _maxRandomDelay);
            yield return new WaitForSeconds(randomWait);
        }
    }

    private void OnDestroy()
    {
        DOTween.Kill(transform); // 안전하게 Tween 종료
    }
}
