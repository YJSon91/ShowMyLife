using UnityEngine;

public class BaseObstacle : MonoBehaviour
{
    [Header("플레이어 감지 옵션")]
    [Tooltip("플레이어를 감지해서 장애물 동작에 반영할지 여부")]
    [SerializeField] protected bool enablePlayerCarry = true;

    // 플레이어가 올라온 상태를 저장 (자식에서 사용 가능)
    protected Transform _playerOnPlatform;
    protected Rigidbody _playerRigidbody;

    /// <summary>
    /// 플레이어가 장애물에 올라탔을 때 감지
    /// </summary>
    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (!enablePlayerCarry) return;
        // 여러 플레이어가 있을 경우 tag 등 추가 분기
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
        if (collision.gameObject.CompareTag("Player"))
        {
            // 내려온 오브젝트만 해제
            if (_playerOnPlatform == collision.transform)
            {
                _playerOnPlatform = null;
                _playerRigidbody = null;
                //Debug.Log("플랫폼 이탈");
            }
        }
    }

    /// <summary>
    /// 플레이어가 장애물 위에 올라와있는지 판정
    /// </summary>
    protected bool IsPlayerOnPlatform()
    {
        return _playerOnPlatform != null && _playerRigidbody != null;
    }

    /// <summary>
    /// 올라온 플레이어 반환 (없으면 null)
    /// </summary>
    protected Transform GetPlayerOnPlatform()
    {
        return _playerOnPlatform;
    }

    /// <summary>
    /// 올라온 플레이어의 Rigidbody 반환 (없으면 null)
    /// </summary>
    protected Rigidbody GetPlayerRigidbody()
    {
        return _playerRigidbody;
    }
}
