 using UnityEngine;
using System.Collections;

public class EnterStage3_1_1 : MonoBehaviour
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

        emotionDirector.DisablePlayerControl(player);

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

            float curveT = Mathf.SmoothStep(0f, 1f, t);
            float scale = Mathf.Lerp(start, end, curveT);

            Time.timeScale = scale;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;

            //Debug.Log($"[슬로우 테스트] TimeScale: {Time.timeScale}, FixedDeltaTime: {Time.fixedDeltaTime}");

            yield return null;
        }

        Time.timeScale = end;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        yield return new WaitForSecondsRealtime(slowHoldDuration);

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

