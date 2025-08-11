 using UnityEngine;
using System.Collections;

public class EnterStage2_2 : MonoBehaviour
{
    [Tooltip("연출매니저")]
    [SerializeField] private EmotionDirector emotionDirector;

    [Tooltip("첫번째 연출 속도")]
    [SerializeField] private float zoomDuration = 7f;

    [Tooltip("중간 대기 시간")]
    [SerializeField] private float pauseDuration = 2f;

    [Tooltip("두번째 연출 속도")]
    [SerializeField] private float sweepDuration = 10f;

    [Tooltip("훑기 각도")]
    [SerializeField] private float sweepAngle = 90f;

    [Tooltip("훑기 시작 거리")]
    [SerializeField] private float sweepStartDistance = 10f;

    private bool hasTriggered = false;
    private Transform player;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || !other.CompareTag("Player")) return;
        hasTriggered = true;

        player = other.transform;
        emotionDirector.DisablePlayerControl(player);

        StartCoroutine(PlayEmotionSequence());
    }

    private IEnumerator PlayEmotionSequence()
    {
        if (emotionDirector == null)
            yield break;

        // 줌인 연출 (플레이어 위치에서 타겟 방향으로 이동 + 줌)
        emotionDirector.PlayZoomFromPlayer(player, 3, zoomDuration);
        yield return new WaitForSeconds(zoomDuration);

        yield return new WaitForSeconds(pauseDuration);

        // 현재 카메라 위치 기준 훑기 시작 위치 계산
        Vector3 sweepPos = emotionDirector.GetStopPosition(3, sweepStartDistance, Camera.main.transform.position);
        emotionDirector.PlaySweepEmotion(sweepPos, 45f, sweepAngle, sweepDuration);
        yield return new WaitForSeconds(sweepDuration);

        // 복원 및 종료
        emotionDirector.ResetEmotion();
        emotionDirector.EnablePlayerControl(player);
        emotionDirector.ResetToDefault();

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
