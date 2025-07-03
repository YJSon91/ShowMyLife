using UnityEngine;
using DG.Tweening;

public class MovingObstacle : BaseObstacle
{
    [Header("이동 설정")]
    [Tooltip("이동할 거리")]
    [SerializeField] private Vector3 _moveTo = Vector3.zero;

    [Tooltip("한 번 이동하는 데 걸리는 시간")]
    [SerializeField] private float _moveTime = 1f;

    private Vector3 _lastPosition;

    private void Start()
    {
        _lastPosition = transform.position;
        StartMoving();
    }

    private void Update()
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

    // 플레이어가 위에 있을 때 위치 이동
    protected void MovePlayerIfOnTop(Vector3 delta)
    {
        if (TryGetPlayerOnTop(out Transform player))
        {
            Rigidbody rb = player.GetComponent<Rigidbody>();
            CharacterController cc = player.GetComponentInChildren<CharacterController>();

            if (rb != null)
                rb.MovePosition(rb.position + delta);
            else if (cc != null)
                cc.Move(delta);
            else
                player.position += delta;
        }
    }
}
