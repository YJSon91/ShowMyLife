using UnityEngine;
using System.Collections.Generic;
using Cinemachine;

// 연출설정 스크립트
public class EmotionDirector : MonoBehaviour
{
    [Tooltip("ThemeCameraController 스크립트")]
    [SerializeField] private ThemeCameraController themeCamera;

    [Tooltip("PostProcessing 스크립트")]
    [SerializeField] private PostProcessingManager postProcessing;

    [Tooltip("TimeEffect 스크립트")]
    [SerializeField] private TimeEffectManager timeEffect;

    [Tooltip("연출용 타겟 리스트")]
    [SerializeField] private List<Transform> emotionLookTargets = new List<Transform>();

    // 슬로우모션 연출
    public void PlayTopDownEmotion()
    {
        postProcessing?.ResetToDefault();                                         // 화면 필터 초기화
        timeEffect?.StartSlowMotion(0.2f, 3f);                    // 시간 느리게 재생 (0.2배속, 3초간)
    }

    // 테마 시점 + 세피아 톤 연출
    public void PlaySepiaEmotion()
    {
        if (emotionLookTargets.Count <= 1 || themeCamera == null)
            return;

        Transform target = emotionLookTargets[1];
        if (target == null) return;

        themeCamera.SwitchCameras();                                                         // 카메라 전환
        themeCamera.ClearAim();                                                              // Aim 제거
        themeCamera.SetLookAt(null);                                                   // LookAt 비우기
        themeCamera.SetFollow(null);                                                   // Follow도 제거

        Vector3 offset = new Vector3(0, 1f, -2f);
        Vector3 targetPos = target.position + offset;
        Vector3 direction = target.position - targetPos;

        themeCamera.SetPosition(targetPos);                                                 // 위치 직접 설정
        themeCamera.SetRotation(Quaternion.LookRotation(direction.normalized).eulerAngles); // 회전 수동 설정

        themeCamera.SetZoom(50f);                                                      // 줌 설정
        postProcessing?.ApplyColorFilter(new Color(1f, 0.8f, 0.5f));             // 세피아 톤
    }

    // 테마 시점 + 파란색 톤
    public void PlaySadness()
    {
        if (emotionLookTargets.Count == 0 || themeCamera == null)
            return;

        Transform target = emotionLookTargets[0];
        if (target == null) return;

        themeCamera.SwitchCameras();                                                     // 카메라 전환
        themeCamera.SetAim<CinemachineComposer>();                                      // Composer 적용
        themeCamera.SetLookAt(target);                                                  // LookAt 설정
        themeCamera.SetFollow(target);                                                  // Follow 설정
        themeCamera.SetZoom(5f);                                                   // 줌 아웃
        postProcessing?.ApplyColorFilter(new Color(0.2f, 0.2f, 1f));          // 파란색 톤
    }


    // 모든 연출 효과를 원래 상태로 되돌림
    public void ResetEmotion()
    {
        themeCamera?.ResetToDefault();                                                 // 카메라 복귀
        postProcessing?.ResetToDefault();                                              // 색상 복원
        timeEffect?.ResetTimeScale();                                                  // 시간 복원
    }
}
