using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("플레이어 밀어내는 힘")]
    [SerializeField] private float _pushForce = 15f;
    [Header("투사체 생존 시간")]
    [SerializeField] private float _lifetime = 3f;

    private Rigidbody _rb;
    private bool _isPaused = false;
    private Vector3 _savedVelocity;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        Destroy(gameObject, _lifetime);
    }

    private void FixedUpdate()
    {
        // 정지 시 물리 멈춤
        if (_isPaused && _rb != null)
        {
            _rb.velocity = Vector3.zero;
            _rb.isKinematic = true;
        }
        else if (!_isPaused && _rb != null)
        {
            _rb.isKinematic = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
        if (playerRb == null)
            playerRb = collision.gameObject.GetComponentInChildren<Rigidbody>();

        if (playerRb != null)
        {
            Vector3 forceDir = collision.contacts[0].normal * -1f;
            playerRb.AddForce(forceDir.normalized * _pushForce, ForceMode.VelocityChange);
        }
        Destroy(gameObject);
    }
}
