using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MovingObstacle : MonoBehaviour
{
    [SerializeField] Vector3 MoveTo = Vector3.zero; //이동거리 설정
    [SerializeField] float MoveTime = 1f; //걸리는 시간 설정

    private void Start()
    {
        Move();
    }

    private void Move()
    {
        //Ease.InOutQuad 시작,끝 느리게 중간 빠르게  SetLoops루프 -1은 무한 반복 LoopType.Yoyo 끝나면 반대로 실행
        transform.DOMove(transform.position + MoveTo, MoveTime).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
    }

    private void OnCollisionEnter(Collision collision) //플레이어 자식객체로 변경
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.transform.SetParent(this.transform);
        }
    }

    private void OnCollisionExit(Collision collision) //플레이어 자식객체 해제
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.transform.SetParent(null);
        }
    }
}
