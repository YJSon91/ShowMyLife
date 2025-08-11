 using UnityEngine;
using System.Collections;

public class EnterStage1_2 : MonoBehaviour
{
    [Tooltip("연출매니저")]
    [SerializeField] private EmotionDirector emotionDirector;

    [Tooltip("발판 오브젝트")]
    [SerializeField] private GameObject objectToActivate;

    [Tooltip("연출 속도")]
    [SerializeField] private float skyPanDuration = 7f;

    [Tooltip("훑을 각도")]
    [SerializeField] private float sweepAngle = 90f;

    private bool hasTriggered = false;

    // 온 트리거
    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || !other.CompareTag("Player")) return;
        hasTriggered = true;

        // 조작 비활성화
        emotionDirector.DisablePlayerControl(other.transform);

        // 카메라 연출
        StartCoroutine(PlaySkySequence(other.transform));
    }

    // 카메라 연출
    private IEnumerator PlaySkySequence(Transform player)
    {
        // 연출 시작
        Vector3 eyePos = emotionDirector.GetPlayerEyePosition(player);

        // 카메라 세팅 및 훑기 연출 시작
        emotionDirector.PlaySweepEmotion(eyePos, -22f, sweepAngle, skyPanDuration);

        // 플레이어를 즉시 뒤로 회전
        Vector3 back = -player.forward;
        back.y = 0f;
        player.rotation = Quaternion.LookRotation(back);

        // 발판 오브젝트 활성화
        if (objectToActivate != null)
            objectToActivate.SetActive(true);

        // 연출 후 타겟 응시 및 회전 루틴 시작
        emotionDirector.StartFinishSkySweep(skyPanDuration, 2);

        // 연출 시간 대기
        yield return new WaitForSeconds(skyPanDuration);

        // 조작 복원
        emotionDirector.EnablePlayerControl(player);

        // 자기 자신 비활성화
        gameObject.SetActive(false);
    }

    // 범위 표시
    private void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col is BoxCollider box && box.isTrigger)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.color = new Color(1f, 0.5f, 0f, 1f);
            Gizmos.DrawWireCube(box.center, box.size);
        }
    }
}
