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
    private Transform _playerOnPlatform;
    private Rigidbody _playerRigidbody;

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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _playerOnPlatform = collision.transform;
            _playerRigidbody = collision.gameObject.GetComponent<Rigidbody>();
            Debug.Log("플레이어가 장애물에 올라탔습니다.");
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _playerOnPlatform = null;
            _playerRigidbody = null;
            Debug.Log("플레이어가 장애물에서 내려왔습니다.");
        }
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
        // 기존 TryGetPlayerOnTop 메서드 대신 직접 충돌 감지한 플레이어 사용
        if (_playerOnPlatform != null && _playerRigidbody != null)
        {
            Debug.Log($"플레이어 이동 시도 - 델타: {delta}, 현재위치: {_playerRigidbody.position}");                     
            
            // 방법 : MovePosition 사용
            _playerRigidbody.MovePosition(_playerRigidbody.position + delta);                    
            
            Debug.Log($"이동 후 위치: {_playerRigidbody.position}");
        }
        else
        {
            // 기존 방식
            // if (TryGetPlayerOnTop(out Transform player))
            // {
            //     Rigidbody rb = player.GetComponent<Rigidbody>();
                
            //     if (rb != null)
            //     {
            //         Debug.Log($"기존 방식으로 감지된 리지드바디 - 델타: {delta}");
            //         rb.MovePosition(rb.position + delta);
            //     }
            //     else
            //     {
            //         player.position += delta;
            //     }
            // }
        }
    }
}
