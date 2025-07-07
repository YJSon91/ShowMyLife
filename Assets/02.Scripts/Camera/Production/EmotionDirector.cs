using UnityEngine;
using System.Collections.Generic;
using Cinemachine;
using System.Collections;

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
        postProcessing?.ResetToDefault();                                                 // 화면 필터 초기화
        timeEffect?.StartSlowMotion(0.2f, 3f);                            // 시간 느리게 재생
    }

    // 테마 시점 + 세피아 톤 연출
    public void PlaySepiaEmotion()
    {
        if (emotionLookTargets.Count <= 1 || themeCamera == null)
            return;

        Transform target = emotionLookTargets[1];
        if (target == null) return;

        themeCamera.SwitchCameras();                                            // 카메라 전환
        themeCamera.ClearAim();                                                 // Aim 제거
        themeCamera.SetLookAt(null);                                      // LookAt 비움
        themeCamera.SetFollow(null);                                      // Follow 제거

        Vector3 offset = new Vector3(0, 1f, -2f);
        Vector3 targetPos = target.position + offset;
        Vector3 direction = target.position - targetPos;

        themeCamera.SetPosition(targetPos);                                                 // 위치 설정
        themeCamera.SetRotation(Quaternion.LookRotation(direction.normalized).eulerAngles); // 회전 설정

        themeCamera.SetZoom(50f);
        postProcessing?.ApplyColorFilter(new Color(1f, 0.8f, 0.5f));              // 세피아 톤
    }

    // 테마 시점 + 파란색 톤
    public void PlaySadness()
    {
        if (emotionLookTargets.Count == 0 || themeCamera == null)
            return;

        Transform target = emotionLookTargets[0];
        if (target == null) return;

        themeCamera.SwitchCameras();
        themeCamera.SetAim<CinemachineComposer>();
        themeCamera.SetLookAt(target);
        themeCamera.SetFollow(target);
        themeCamera.SetZoom(5f);
        postProcessing?.ApplyColorFilter(new Color(0.2f, 0.2f, 1f));
    }

    // 모든 연출 효과를 원래 상태로 되돌림
    public void ResetEmotion()
    {
        themeCamera?.ResetToDefault();
        postProcessing?.ResetToDefault();
        timeEffect?.ResetTimeScale();
    }

    // 유치원 연출 - 하늘 훑기
    public void PlaySkyEmotion(Transform player, float sweepAngle, float duration)
    {
        if (themeCamera == null || player == null) return;

        Vector3 eyePos = player.position + Vector3.up * 1.6f; // 머리 높이

        // 플레이어 입력 비활성화
        Player playerComponent = player.GetComponent<Player>();
        if (playerComponent != null)
        {
            InputReader inputReader = playerComponent.GetComponent<InputReader>();
            if (inputReader != null)
            {
                Debug.Log("카메라 연출 시작: 플레이어 입력 비활성화");
                inputReader.DisableInput();
            }
            
            PlayerMovementController movementController = playerComponent.GetComponent<PlayerMovementController>();
            if (movementController != null)
            {
                Debug.Log("카메라 연출 시작: 플레이어 이동 컨트롤러 비활성화");
                movementController.enabled = false;
            }
        }

        themeCamera.SwitchCameras();
        themeCamera.ClearAim();
        themeCamera.SetLookAt(null);
        themeCamera.SetFollow(null);

        StartCoroutine(SkySweepRoutine(eyePos, sweepAngle, duration, player));
    }

    private IEnumerator SkySweepRoutine(Vector3 position, float sweepAngle, float duration, Transform player)
    {
        float elapsed = 0f;
        float startYaw = -sweepAngle * 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float yaw = Mathf.Lerp(startYaw, startYaw + sweepAngle, t);

            // 고개를 위로 들고 좌우로 훑기
            Quaternion rot = Quaternion.Euler(-22f, yaw + 180f, 0f); // 각도, 좌우, 기울기
            themeCamera.SetPosition(position);
            themeCamera.SetRotation(rot.eulerAngles);

            yield return null;
        }

        // 연출 종료 시 플레이어 입력 다시 활성화
        if (player != null)
        {
            Player playerComponent = player.GetComponent<Player>();
            if (playerComponent != null)
            {
                InputReader inputReader = playerComponent.GetComponent<InputReader>();
                if (inputReader != null)
                {
                    Debug.Log("카메라 연출 종료: 플레이어 입력 활성화");
                    inputReader.EnableInput();
                }
                
                PlayerMovementController movementController = playerComponent.GetComponent<PlayerMovementController>();
                if (movementController != null)
                {
                    Debug.Log("카메라 연출 종료: 플레이어 이동 컨트롤러 활성화");
                    movementController.enabled = true;
                }
            }
        }

        ResetEmotion();
    }
}
