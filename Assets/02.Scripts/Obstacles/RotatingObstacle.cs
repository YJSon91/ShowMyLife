using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;

public class RotatingObstacle : MonoBehaviour
{
    [SerializeField] private Vector3 _rotAxis = Vector3.up;
    [SerializeField] private float _rotSpeed = 90f; // degrees per second
    [SerializeField] private bool _clockwise = true;

    private Quaternion _lastRotation;
    private float _currentAngle = 0f;

    private void Start()
    {
        _lastRotation = transform.rotation;
        StartRotation();
    }

    private void FixedUpdate()
    {
        Quaternion currentRotation = transform.rotation;
        Quaternion deltaRotation = currentRotation * Quaternion.Inverse(_lastRotation);

        Collider[] hits = Physics.OverlapBox(
            transform.position + Vector3.up * 0.5f,
            transform.localScale / 2f + new Vector3(0f, 0.1f, 0f),
            transform.rotation);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                Transform player = hit.transform;

                Vector3 dir = player.position - transform.position;
                dir = deltaRotation * dir;
                Vector3 newPos = transform.position + dir;

                Rigidbody rb = player.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.MovePosition(newPos);
                }
                else
                {
                    player.position = newPos;
                }
            }
        }

        _lastRotation = currentRotation;
    }

    private void StartRotation()
    {
        float direction = _clockwise ? 1f : -1f;

        // 회전 각도를 추적해서 직접 회전 적용
        DOTween.To(() => _currentAngle, x =>
        {
            _currentAngle = x;
            transform.localRotation = Quaternion.AngleAxis(_currentAngle, _rotAxis.normalized);
        },
        360f * 1000f, // 충분히 큰 회전
        (360f * 1000f) / (_rotSpeed * Mathf.Abs(direction)))
        .SetEase(Ease.Linear)
        .SetLoops(-1, LoopType.Restart)
        .SetUpdate(UpdateType.Fixed);
    }
}
