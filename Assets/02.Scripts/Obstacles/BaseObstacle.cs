using UnityEngine;

public class BaseObstacle : MonoBehaviour
{
    [Header("플레이어 감지 옵션")]
    [Tooltip("플레이어를 감지해서 장애물 동작에 반영할지 여부")]
    [SerializeField] protected bool enablePlayerCarry = true; // Inspector에서 체크 On/Off

    // 플레이어가 올라온 상태를 저장 (자식에서 사용 가능)
    protected Transform _playerOnPlatform;
    protected Rigidbody _playerRigidbody;

    /// <summary>
    /// 플레이어가 장애물에 올라탔을 때 감지
    /// </summary>
    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (!enablePlayerCarry) return; // 감지 Off면 무시
        if (collision.gameObject.CompareTag("Player"))
        {
            _playerOnPlatform = collision.transform;
            _playerRigidbody = collision.gameObject.GetComponent<Rigidbody>();
            //Debug.Log($"플랫폼 접촉: {_playerOnPlatform.name}");
        }
    }

    /// <summary>
    /// 플레이어가 장애물에서 내려왔을 때 감지 해제
    /// </summary>
    protected virtual void OnCollisionExit(Collision collision)
    {
        if (!enablePlayerCarry) return; // 감지 Off면 무시
        if (collision.gameObject.CompareTag("Player"))
        {
            if (_playerOnPlatform == collision.transform)
            {
                _playerOnPlatform = null;
                _playerRigidbody = null;
                //Debug.Log("플랫폼 이탈");
            }
        }
    }

    public void NotifyPlayerOnPlatform(Transform player, Rigidbody rb)
    {
        if (!enablePlayerCarry) return;
        _playerOnPlatform = player;
        _playerRigidbody = rb;
    }

    public void NotifyPlayerExitPlatform(Transform player)
    {
        if (_playerOnPlatform == player)
        {
            _playerOnPlatform = null;
            _playerRigidbody = null;
        }
    }

    /// <summary>
    /// 플레이어가 장애물 위에 올라와있는지 판정
    /// </summary>
    protected bool IsPlayerOnPlatform()
    {
        if (!enablePlayerCarry) return false; // 감지 Off면 항상 false
        return _playerOnPlatform != null && _playerRigidbody != null;
    }

    /// <summary>
    /// 올라온 플레이어 반환 (없으면 null)
    /// </summary>
    protected Transform GetPlayerOnPlatform()
    {
        return enablePlayerCarry ? _playerOnPlatform : null;
    }

    /// <summary>
    /// 올라온 플레이어의 Rigidbody 반환 (없으면 null)
    /// </summary>
    protected Rigidbody GetPlayerRigidbody()
    {
        return enablePlayerCarry ? _playerRigidbody : null;
    }
}
