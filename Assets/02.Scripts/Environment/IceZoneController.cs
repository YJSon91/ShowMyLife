 using UnityEngine;

/// <summary>
/// 빙판 구역의 속성을 제어하는 컴포넌트
/// </summary>
public class IceZoneController : MonoBehaviour
{
    [Tooltip("이 빙판에서의 감속 비율 (낮을수록 오래 미끄러짐, 0.01-0.2 권장)")]
    [Range(0.01f, 0.2f)]
    public float slowdownRate = 0.05f;
    
    [Tooltip("이 빙판에서의 가속 비율 (낮을수록 방향 전환이 어려움, 0.1-1.0 권장)")]
    [Range(0.1f, 1.0f)]
    public float accelerationRate = 0.5f;
    
    [Tooltip("이 빙판에서의 최대 속도 배율 (1.0 = 기본 속도)")]
    [Range(0.5f, 2.0f)]
    public float speedMultiplier = 1.0f;
    
    private void OnValidate()
    {
        // 값 범위 제한
        slowdownRate = Mathf.Clamp(slowdownRate, 0.01f, 0.2f);
        accelerationRate = Mathf.Clamp(accelerationRate, 0.1f, 1.0f);
        speedMultiplier = Mathf.Clamp(speedMultiplier, 0.5f, 2.0f);
    }
}