using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : BaseObstacle
{
    [Header("텔레포트 설정")]
    [Tooltip("플레이어가 이 위치로 고정됩니다 (캐비넷 내부 피벗)")]
    [SerializeField] private Transform _pivot;

    [Tooltip("텔레포트 목적지로 사용할 캐비넷 (다른 CabinetTeleporter)")]
    [SerializeField] private Teleporter _targetCabinet;

    [Tooltip("문 오브젝트의 Transform (Y축 회전만 사용)")]
    [SerializeField] private Transform _door;

    [Tooltip("문이 완전히 닫혔을 때의 각도 (예: 0)")]
    [SerializeField] private float _doorCloseAngle = 0f;

    [Tooltip("문이 완전히 열렸을 때의 각도 (예: 90)")]
    [SerializeField] private float _doorOpenAngle = 90f;

    [Tooltip("문 열고 닫는 속도 (값이 클수록 빠름)")]
    [SerializeField] private float _doorSpeed = 3f;

    private bool _isBusy = false;

    protected override void OnTriggerEnter(Collider other)
    {
        if (_senseMode != SenseMode.Trigger) return;
        if (_isBusy) return;
        if (!IsPlayerObject(other.gameObject)) return;

        _isBusy = true;
        var player = other.GetComponent<Player>();
        if (player == null) player = other.GetComponentInParent<Player>();
        if (player == null) return;

        StartCoroutine(TeleportSequence(player));
    }

    private IEnumerator TeleportSequence(Player player)
    {
        // 입력 봉인 및 위치 고정
        player.InputReader.DisableInput();
        player.MovementController.ResetMovement();
        player.transform.position = _pivot.position;
        player.transform.rotation = _pivot.rotation;

        // 문 닫기 연출
        yield return StartCoroutine(RotateDoor(_doorOpenAngle, _doorCloseAngle));

        // 목적지 캐비넷으로 실제 텔레포트(항상 성공!)
        if (_targetCabinet != null)
            _targetCabinet.ReceiveTeleport(player);

        _isBusy = false;
    }

    public void ReceiveTeleport(Player player)
    {
        StartCoroutine(ReceiveSequence(player));
    }

    private IEnumerator ReceiveSequence(Player player)
    {
        // 목적지 도착 위치 고정 & 문 열기 연출
        player.InputReader.DisableInput();
        player.MovementController.ResetMovement();
        player.transform.position = _pivot.position;
        player.transform.rotation = _pivot.rotation;

        // 도착 후 문 열기
        yield return StartCoroutine(RotateDoor(_doorCloseAngle, _doorOpenAngle));

        // 연출 후 입력 해제
        yield return new WaitForSeconds(0.3f);
        player.InputReader.EnableInput();
    }

    private IEnumerator RotateDoor(float fromAngle, float toAngle)
    {
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * _doorSpeed;
            float angle = Mathf.Lerp(fromAngle, toAngle, t);
            _door.localRotation = Quaternion.Euler(0, angle, 0);
            yield return null;
        }
        _door.localRotation = Quaternion.Euler(0, toAngle, 0);
    }
}
