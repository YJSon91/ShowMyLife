using UnityEngine;
using DG.Tweening;

public class MovingObstacle : BaseObstacle
{
    [Header("이동 설정")]
    [Tooltip("이동할 거리")]
    [SerializeField] private Vector3 _moveTo = Vector3.zero;

    [Tooltip("한 번 이동하는 데 걸리는 시간")]
    [SerializeField] private float _moveTime = 1f;

    [Tooltip("플레이어 이동 시 적용할 힘 배율")]
    [SerializeField] private float _forceMultiplier = 1.0f;

    private Vector3 _lastPosition;

    private void Start()
    {
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
        transform.DOMove(transform.position + _moveTo, _moveTime)
            .SetEase(Ease.InOutQuad)
            .SetLoops(-1, LoopType.Yoyo)
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
}
