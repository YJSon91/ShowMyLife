using UnityEngine;
using System.Collections;

public class EnterStage : MonoBehaviour
{
    [Tooltip("연출 매니저")]
    [SerializeField] private EmotionDirector emotionDirector;
    [Tooltip("천장응시 시간")]
    [SerializeField] private float lookDuration = 2f;
    [Tooltip("일어나는 시간")]
    [SerializeField] private float moveDuration = 2f;
    [Tooltip("응시 타겟")]
    [SerializeField] private Transform endLookTarget;

    [Tooltip("눈뜨는 시간")]
    [SerializeField] private float fadeDuration = 1f;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || !other.CompareTag("Player")) return;
        hasTriggered = true;

        Transform player = other.transform;
        emotionDirector.DisablePlayerControl(player);
        StartCoroutine(PlayWakeUpSequence(player));
    }

    private IEnumerator PlayWakeUpSequence(Transform player)
    {
        // 0. 화면을 검게 덮음
        emotionDirector.PostProcessing.ApplyColorFilter(Color.black, 0f);
        yield return null;

        // 1. 시작 위치
        Transform startTarget = emotionDirector.GetLookTargetTransform(5);
        if (startTarget == null) yield break;

        emotionDirector.SetPlayerVisible(player, false);

        // 2. 시점 초기화
        emotionDirector.ResetThemeCamera();
        emotionDirector.ThemeCamera.SetPosition(startTarget.position);
        emotionDirector.ThemeCamera.SetRotation(new Vector3(-70f, 0f, 0f));
        yield return new WaitForSeconds(lookDuration);

        // 3. 화면을 점점 밝힘
        emotionDirector.PostProcessing.ResetToDefault(fadeDuration);
        yield return new WaitForSeconds(fadeDuration);

        // 4. 이동 + 회전 복원
        Vector3 from = startTarget.position;
        Vector3 to = from + startTarget.TransformDirection(Vector3.forward * 2f) + Vector3.up * 1f;
        Quaternion fromRot = Quaternion.Euler(-70f, 0f, 0f);
        Quaternion toRot = Quaternion.identity;

        emotionDirector.PlayMoveAndRotateToNeutral(from, to, fromRot, toRot, moveDuration);
        yield return new WaitForSeconds(moveDuration + 0.5f);

        // 5. 응시
        if (endLookTarget != null)
            emotionDirector.ThemeCamera.SmoothLookAt(endLookTarget, 1f);

        yield return new WaitForSeconds(2f);
        // 6. 복원 및 등장
        emotionDirector.ResetToDefault();
        emotionDirector.EnablePlayerControl(player);
        yield return new WaitForSeconds(1.6f);
        emotionDirector.SetPlayerVisible(player, true);
        gameObject.SetActive(false);

    }
    //범위표시
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
