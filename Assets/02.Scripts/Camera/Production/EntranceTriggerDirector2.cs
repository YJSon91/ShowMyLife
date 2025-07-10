using UnityEngine;
using System.Collections;

public class EntranceTriggerDirector2 : MonoBehaviour
{
    [Tooltip("연출매니저")]
    [SerializeField] private EmotionDirector emotionDirector;

    [Tooltip("플레이어 트랜스폼")]
    [SerializeField] private Transform playerTransform;

    [Tooltip("줌인 연출 시간")]
    [SerializeField] private float zoomDuration = 3f;

    [Tooltip("중간 대기 시간")]
    [SerializeField] private float pauseDuration = 1f;

    [Tooltip("훑기 연출 시간")]
    [SerializeField] private float sweepDuration = 4f;

    [Tooltip("훑기 각도")]
    [SerializeField] private float sweepAngle = 90f;

    [Tooltip("첫 줌인 목표 거리")]
    [SerializeField] private float zoomStopDistance = 150f;

    [Tooltip("훑기 시작 거리")]
    [SerializeField] private float sweepStartDistance = 10f;

    private bool hasTriggered = false;

    // 온 트리거
    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || !other.CompareTag("Player")) return;
        hasTriggered = true;

        // 조작 비활성화
        emotionDirector.DisablePlayerControl(other.transform);

        // 카메라 연출
        StartCoroutine(PlayEmotionSequence());
    }

    // 카메라 연출
    private IEnumerator PlayEmotionSequence()
    {
        if (emotionDirector == null)
            yield break;

        // 멀리서 줌인
        Vector3 camPos = Camera.main.transform.position;
        Vector3 camRot = Camera.main.transform.eulerAngles;
        emotionDirector.PlayFocusZoomFrom(camPos, camRot, 3, zoomDuration, zoomStopDistance);
        yield return new WaitForSeconds(zoomDuration);

        // 중간 대기
        yield return new WaitForSeconds(pauseDuration);

        // 가까이 이동 후 정면 응시
        Vector3 sweepPos = emotionDirector.GetStopPosition(3, sweepStartDistance, camPos);
        Vector3 lookTarget = emotionDirector.GetLookTarget(3);
        Vector3 dir = (lookTarget - sweepPos).normalized;
        Quaternion rot = Quaternion.LookRotation(dir);

        emotionDirector.ThemeCamera.SetPosition(sweepPos);
        emotionDirector.ThemeCamera.SetRotation(rot.eulerAngles);

        // 훑기 연출 실행
        emotionDirector.PlaySweepEmotion(sweepPos, 45f, sweepAngle, sweepDuration);
        yield return new WaitForSeconds(sweepDuration);

        // 연출 리셋 및 조작 복원
        emotionDirector.ResetEmotion();
        emotionDirector.EnablePlayerControl(playerTransform);

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
