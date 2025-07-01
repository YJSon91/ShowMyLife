using UnityEngine;

// 시간 연출 제어 스크립트
public class TimeEffectManager : MonoBehaviour
{
    private Coroutine slowMotionCoroutine;

    // 슬로우모션 시작
    public void StartSlowMotion(float timeScale, float duration)
    {
        if (slowMotionCoroutine != null)
        {
            StopCoroutine(slowMotionCoroutine);
        }
        slowMotionCoroutine = StartCoroutine(SlowMotionRoutine(timeScale, duration));
    }

    // 지정된 시간 동안 슬로우모션 유지 후 복원
    private System.Collections.IEnumerator SlowMotionRoutine(float targetScale, float duration)
    {
        Time.timeScale = targetScale;                       // 시간 배속 조절
        Time.fixedDeltaTime = 0.02f * Time.timeScale;       // 물리 시간도 배속에 맞게 조절
        yield return new WaitForSecondsRealtime(duration);  // 실시간 기준 대기
        ResetTimeScale();                                   // 시간 배율 복원
    }

    // 시간 배율을 기본값으로 초기화
    public void ResetTimeScale()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }
}
