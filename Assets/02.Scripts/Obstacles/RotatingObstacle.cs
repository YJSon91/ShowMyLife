using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;

public class RotatingObstacle : MonoBehaviour
{
    [SerializeField] private Vector3 _rotAxis = Vector3.up; //회전축
    [SerializeField] private float _rotSpeed = 90f; //회전 속도
    [SerializeField] private bool _clockwise = true; //시계 방향 회전 여부

    private Quaternion _lastRotation; //이전 회전값 저장
    private float _currentAngle = 0f; //현재 회전 각도

    private void Start()
    {
        _lastRotation = transform.rotation; //시작 시 회전값 저장
        StartRotation();
    }

    private void FixedUpdate()
    {
        //현재 회전값과 이전 회전값의 차이를 계산
        Quaternion currentRot = transform.rotation;
        Quaternion deltaRot = currentRot * Quaternion.Inverse(_lastRotation);

        //장애물과 충동하는 플레이어 감지
        Collider[] hits = Physics.OverlapBox(
            transform.position + Vector3.up * 0.5f, //박스 중심
            transform.localScale / 2f + new Vector3(0f, 0.1f, 0f), //박스 크기
            transform.rotation); //현재 회전 반영

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                Transform player = hit.transform;

                //플레이어와 오브젝트의 회전 변화량만큼 회전시킴
                Vector3 dir = player.position - transform.position;
                dir = deltaRot * dir;
                Vector3 newPos = transform.position + dir;

                //rigidbody가 있다면 MovePosition으로 자연스럽게 이동
                Rigidbody rb = player.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.MovePosition(newPos);
                }
                else
                {
                    //rigidbody가 없으면 단순 위치 이동
                    player.position = newPos;
                }
            }
        }

        _lastRotation = currentRot; //현재 회전값을 다음 프레임을 위해 저장
    }

    private void StartRotation()
    {
        // 회전 방향 설정
        float direction = _clockwise ? 1f : -1f;

        // DOTween을 이용해 _currentAngle값을 지속 증가시켜 회전 적용
        DOTween.To(() => _currentAngle, x =>
        {
            _currentAngle = x;
            transform.localRotation = Quaternion.AngleAxis(_currentAngle, _rotAxis.normalized);
        },
        360f * 1000f, // 충분히 큰 회전
        (360f * 1000f) / (_rotSpeed * Mathf.Abs(direction))) // 속도에 맞게 시간 설정
        .SetEase(Ease.Linear) //일정한 속도 회전
        .SetLoops(-1, LoopType.Restart) //무한 반복
        .SetUpdate(UpdateType.Fixed); //FixedUpdate에 맞춰 실행
    }
}
