using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// 화면 생상 조정 스크립트
public class PostProcessingManager : MonoBehaviour
{
    [Tooltip("볼륨 오브잭트")]
    [SerializeField] private Volume postProcessVolume;

    private ColorAdjustments colorAdjustments; // 색상 조정 효과를 참조
    private Coroutine colorLerpRoutine;        // 색상 보간 코루틴을 추적

    private void Awake()
    {
        if (postProcessVolume != null && postProcessVolume.profile.TryGet(out ColorAdjustments ca))
        {
            colorAdjustments = ca;
        }
    }

    // 색상 필터를 지정된 색으로 부드럽게 전환
    public void ApplyColorFilter(Color targetColor)
    {
        if (colorAdjustments != null)
        {
            if (colorLerpRoutine != null)
            {
                StopCoroutine(colorLerpRoutine);
            }
            colorLerpRoutine = StartCoroutine(LerpColorFilter(targetColor, 1f));
        }
    }

    // 색상 필터를 기본 값으로 되돌림
    public void ResetToDefault()
    {
        if (colorAdjustments != null)
        {
            if (colorLerpRoutine != null)
            {
                StopCoroutine(colorLerpRoutine);
            }
            colorLerpRoutine = StartCoroutine(LerpColorFilter(Color.white, 1f));
        }
    }

    // 색상 보간
    private System.Collections.IEnumerator LerpColorFilter(Color targetColor, float duration)
    {
        Color startColor = colorAdjustments.colorFilter.value;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            colorAdjustments.colorFilter.value = Color.Lerp(startColor, targetColor, elapsed / duration);
            yield return null;
        }

        colorAdjustments.colorFilter.value = targetColor;
    }
}
