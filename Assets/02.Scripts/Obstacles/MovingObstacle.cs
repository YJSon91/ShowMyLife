using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MovingObstacle : MonoBehaviour
{
    [SerializeField] Vector3 MoveTo = Vector3.zero; //이동거리 설정
    [SerializeField] float MoveTime = 1f; //걸리는 시간 설정

    private Vector3 _lastPos;

    private void Start()
    {
        _lastPos = transform.position;
        Move();
    }

    private void Move()
    {
        //Ease.InOutQuad 시작,끝 느리게 중간 빠르게  SetLoops루프 -1은 무한 반복 LoopType.Yoyo 끝나면 반대로 실행
        transform.DOMove(transform.position + MoveTo, MoveTime)
            .SetEase(Ease.InOutQuad)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(UpdateType.Fixed);
    }

    private void FixedUpdate()
    {
        Vector3 currenPos = transform.position;
        Vector3 delta = currenPos - _lastPos;

        if(delta != Vector3.zero)
        {
            Collider[] hits = Physics.OverlapBox(
                transform.position + Vector3.up * 0.5f,
                transform.localScale / 2f + new Vector3(0f, 0.1f, 0f),
                transform.rotation);

            foreach(var hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    var rb = hit.GetComponent<Rigidbody>();
                    if(rb != null)
                    {
                        rb.MovePosition(rb.position + delta);
                    }
                    else
                    {
                        hit.transform.position += delta;
                    }
                }
            }
        }

        _lastPos = currenPos;
    }
}
