using UnityEngine;
using System.Collections;

public class FallSlowTrigger : MonoBehaviour
{
    [Tooltip("연출 매니저")]
    [SerializeField] private EmotionDirector emotionDirector;

    [Tooltip("최종 슬로우 배율")]
    [SerializeField] private float targetTimeScale = 0.1f;

    [Tooltip("감소하는 데 걸리는 시간")]
    [SerializeField] private float transitionDuration = 1.5f;

    [Tooltip("슬로우모션 지속 시간")]
    [SerializeField] private float slowHoldDuration = 8f;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || !other.CompareTag("Player")) return;
        hasTriggered = true;

        Transform player = other.transform;

        // 조작 제한
        emotionDirector.DisablePlayerControl(player);

        // 슬로우 인트로 시작 (SetActive는 루틴 안에서 처리)
        StartCoroutine(GradualSlowRoutine());
    }

    private IEnumerator GradualSlowRoutine()
    {
        float start = 1f;
        float end = targetTimeScale;
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / transitionDuration);

            // 곡선 감속: 처음 빠르게, 뒤로 갈수록 천천히
            float curveT = Mathf.SmoothStep(0f, 1f, t);
            float scale = Mathf.Lerp(start, end, curveT);

            Time.timeScale = scale;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;

            Debug.Log($"[슬로우 테스트] TimeScale: {Time.timeScale}, FixedDeltaTime: {Time.fixedDeltaTime}");

            yield return null;
        }

        // 마지막 값 보정
        Time.timeScale = end;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        // 슬로우 유지 시간 (조작 제한 유지)
        yield return new WaitForSecondsRealtime(slowHoldDuration);

        // ❗코루틴이 끝난 시점에서 비활성화 (이제 끊기지 않음)
        gameObject.SetActive(false);
    }

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

