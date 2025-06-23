using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;

public class RotatingObstacle : MonoBehaviour
{
    [SerializeField] private Vector3 rotAmount = new Vector3(0f, 90f, 0f); // 회전량
    [SerializeField] private float rotTime = 1f;                           // 회전 시간
    [SerializeField] private float pauseTime = 1f;                         // 멈춤 시간

    private void Start()
    {
        StartCoroutine(RotLoop());
    }

    private IEnumerator RotLoop()
    {
        while (true)
        {
            // 한 번 회전량만큼 추가 회전 (누적 회전은 Unity가 처리)
            transform.DOLocalRotate(rotAmount, rotTime, RotateMode.WorldAxisAdd)
                .SetEase(Ease.InOutQuad)
                .SetUpdate(UpdateType.Fixed);

            if (pauseTime > 0f)
            {
                yield return new WaitForSeconds(rotTime + pauseTime);
            }
            else
            {
                yield return new WaitForSeconds(rotTime);
            }
        }
    }
}
