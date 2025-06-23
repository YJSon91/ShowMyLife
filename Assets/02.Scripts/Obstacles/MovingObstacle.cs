using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MovingObstacle : MonoBehaviour
{
    [SerializeField] Vector3 MoveTo = Vector3.zero; //이동거리 설정
    [SerializeField] float MoveTime = 1f; //걸리는 시간 설정

    private Vector3 _lastPos; // 마지막 위치

    private void Start()
    {
        _lastPos = transform.position; //시작 위치 저장
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
        Vector3 currenPos = transform.position; //현재위치
        Vector3 delta = currenPos - _lastPos; //지난 프레임과의 위치 계산

        if(delta != Vector3.zero) //이동 했을때만 처리
        {
            //장애물 위에 있는 플레이어를 감지하기 위해OverlapBox 사용
            Collider[] hits = Physics.OverlapBox(
                transform.position + Vector3.up * 0.5f, //박스 중심 : 장애물 위
                transform.localScale / 2f + new Vector3(0f, 0.1f, 0f), //박스 크기
                transform.rotation); //회전 고려

            foreach(var hit in hits)
            {
                if (hit.CompareTag("Player")) //플레이어가 감지되면
                {
                    var rb = hit.GetComponent<Rigidbody>();
                    if(rb != null)
                    {
                        //rigidbody가 있으면 MovePosition으로 이동(자연스럽게)
                        rb.MovePosition(rb.position + delta);
                    }
                    else
                    {
                        //rigidbody가 없으면 단순 위치 이동
                        hit.transform.position += delta;
                    }
                }
            }
        }

        _lastPos = currenPos; //다음 이동을 위한 현재위치 저장
    }
}
