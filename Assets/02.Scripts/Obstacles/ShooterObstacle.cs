using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterObstacle : BaseObstacle
{
    [Header("투사체 발사 설정")]
    [Tooltip("발사할 투사체 프리팹")]
    [SerializeField] private GameObject _projectilePrefab;
    [Tooltip("발사 위치 (Transform)")]
    [SerializeField] private Transform _shootPoint;
    [Tooltip("발사 간격(초)")]
    [SerializeField] private float _shootInterval = 2f;
    [Tooltip("투사체 속도")]
    [SerializeField] private float _projectileSpeed = 12f;
    [Tooltip("투사체 발사 방향(로컬)")]
    [SerializeField] private Vector3 _shootDirection = Vector3.forward;
    [Tooltip("정지/재시작에 사용")]
    private bool _isPaused = false;

    private float _timer = 0f;

    private void Start()
    {
        _timer = 0f;
    }

    private void Update()
    {
        if (_isPaused) return;

        _timer += Time.deltaTime;
        if (_timer >= _shootInterval)
        {
            _timer = 0f;
            FireProjectile();
        }
    }

    private void FireProjectile()
    {
        if (_shootPoint == null) return;

        GameObject proj = ObjectPool.Get("Projectile");
        if (proj != null)
        {
            proj.transform.position = _shootPoint.position;
            proj.transform.rotation = _shootPoint.rotation;

            Rigidbody rb = proj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = _shootPoint.TransformDirection(_shootDirection.normalized) * _projectileSpeed;
            }
        }
    }
}
