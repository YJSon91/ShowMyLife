using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : BaseObstacle
{
    [Header("점프 패드 옵션")]
    [SerializeField] private float jumpForce = 30f;
    [SerializeField] private Vector3 forceDirection = Vector3.up;
    [SerializeField] private float padCooldown = 0.3f;

    private float _lastActivateTime = -10f;

    // Collision 방식만 처리
    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);

        // 내 방식대로 처리
        if (_senseMode != SenseMode.Collision) return;
        if (!enablePlayerCarry) return;
        if (!IsPlayerObject(collision.gameObject)) return;

        // 여러 접촉점 검사
        foreach (var contact in collision.contacts)
        {
            Vector3 padUp = transform.up;
            Vector3 contactNormal = contact.normal;

            // padUp(장애물 위쪽)과 -contactNormal(플레이어 아래쪽)이 비슷해야 '위에서 밟았다'
            float dot = Vector3.Dot(padUp, -contactNormal);

            // 60도 이내, 즉 거의 위에서 밟았을 때만
            if (dot > 0.7f)
            {
                // 쿨타임 체크
                if (Time.time - _lastActivateTime < padCooldown) return;
                _lastActivateTime = Time.time;

                var playerMoveCtrl = collision.gameObject.GetComponent<PlayerMovementController>();
                if (playerMoveCtrl != null)
                {
                    // 오로지 위에서만 점프!
                    playerMoveCtrl.ExternalJump(forceDirection.normalized * jumpForce);
                }
                break;
            }
        }
    }
}
