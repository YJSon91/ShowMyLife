using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : BaseObstacle
{
    [Header("점프 패드 옵션")]
    [Tooltip("튕겨낼 힘의 크기")]
    [SerializeField] private float jumpForce = 30f;

    [Tooltip("튕겨낼 방향(기본 위쪽)")]
    [SerializeField] private Vector3 forceDirection = Vector3.up;

    [Tooltip("플레이어 중복 감지 시 쿨타임(초)")]
    [SerializeField] private float padCooldown = 0.3f;

    private float _lastActivateTime = -10f;

    /// <summary>
    /// 플레이어가 점프패드에 올라올 때 튕겨낸다.
    /// </summary>
    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);

        if (!enablePlayerCarry) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        // 여러 접촉점이 있을 수 있음
        foreach (ContactPoint contact in collision.contacts)
        {
            // 점프패드의 '위쪽' 방향 (로컬 Up)
            Vector3 padUp = transform.up;
            // 충돌한 표면의 법선
            Vector3 contactNormal = contact.normal;

            // 내 위쪽(padUp)과 접촉법선(contactNormal)이 거의 반대 방향(플레이어가 위에서 내려온 경우만)
            float dot = Vector3.Dot(padUp, -contactNormal);

            // dot 값이 0.7~1 사이면 "위에서 밟았다" 판정 (60도 이하)
            if (dot > 0.7f)
            {
                // 쿨타임 등 기타 처리
                if (Time.time - _lastActivateTime < padCooldown) return;
                _lastActivateTime = Time.time;

                Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                    rb.AddForce(forceDirection.normalized * jumpForce, ForceMode.VelocityChange);

                    // 이펙트, 사운드 등
                }
                break; // 한 번만 처리하고 끝!
            }
        }
    }
}
