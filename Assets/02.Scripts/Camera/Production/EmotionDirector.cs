using UnityEngine;
using System.Collections.Generic;
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

    public ThemeCameraController ThemeCamera => themeCamera;

    #region 공용 연출
    // 슬로우모션 연출
    public void PlayTopDownEmotion()
    {
        postProcessing?.ResetToDefault();
        timeEffect?.StartSlowMotion(0.2f, 3f);
    }

    // 모든 연출 효과 리셋
    public void ResetEmotion()
    {
        themeCamera?.ResetToDefault();
        postProcessing?.ResetToDefault();
        timeEffect?.ResetTimeScale();
    }

    // 지정된 각도로 훑기 연출
    public void PlaySweepEmotion(Vector3 position, float pitch, float sweepAngle, float duration)
    {
        if (themeCamera == null) return;

        themeCamera.SwitchCameras();
        themeCamera.ClearAim();
        themeCamera.SetLookAt(null);
        themeCamera.SetFollow(null);

        StartCoroutine(SweepRoutine(position, pitch, sweepAngle, duration));
    }

    // 훑기 연출 코루틴
    private IEnumerator SweepRoutine(Vector3 fixedPos, float pitch, float sweepAngle, float duration)
    {
        float elapsed = 0f;
        float startYaw = -sweepAngle * 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float yaw = Mathf.Lerp(startYaw, startYaw + sweepAngle, t);

            Quaternion rot = Quaternion.Euler(pitch, yaw + 180f, 0f);
            themeCamera.SetPosition(fixedPos);
            themeCamera.SetRotation(rot.eulerAngles);

            yield return null;
        }

        // 마지막 상태 저장
        themeCamera.LastSweepPosition = fixedPos;
        themeCamera.LastSweepRotation = Quaternion.Euler(pitch, startYaw + sweepAngle + 180f, 0f);

        themeCamera.SetPosition(themeCamera.LastSweepPosition);
        themeCamera.SetRotation(themeCamera.LastSweepRotation.eulerAngles);
    }

    #endregion


    #region 플레이어 조작제한
    // 플레이어 조작을 비활성화
    public void DisablePlayerControl(Transform player)
    {
        if (player == null) return;

        Player playerComponent = player.GetComponent<Player>();
        if (playerComponent != null)
        {
            var inputReader = playerComponent.GetComponent<InputReader>();
            if (inputReader != null)
            {
                inputReader.DisableInput();
                Debug.Log("입력 비활성화");
            }

            var movement = playerComponent.GetComponent<PlayerMovementController>();
            if (movement != null)
            {
                movement.enabled = false;
                Debug.Log("이동 비활성화");
            }
        }
    }

    // 플레이어 조작을 활성화
    public void EnablePlayerControl(Transform player)
    {
        if (player == null) return;

        Player playerComponent = player.GetComponent<Player>();
        if (playerComponent != null)
        {
            var inputReader = playerComponent.GetComponent<InputReader>();
            if (inputReader != null)
            {
                inputReader.EnableInput();
                Debug.Log("입력 활성화");
            }

            var movement = playerComponent.GetComponent<PlayerMovementController>();
            if (movement != null)
            {
                movement.enabled = true;
                Debug.Log("이동 활성화");
            }
        }
    }
    #endregion


    #region 유치원 시네마틱
    // 하늘 훑기 종료 후 타겟 응시 코루틴 시작
    public void StartFinishSkySweep(float duration, int index)
    {
        StartCoroutine(FinishSkySweepAfter(duration, index));
    }

    // 하늘 훑기 종료 후 타겟 응시
    private IEnumerator FinishSkySweepAfter(float delay, int targetIndex)
    {
        yield return new WaitForSeconds(delay);

        if (targetIndex >= emotionLookTargets.Count || emotionLookTargets[targetIndex] == null)
            yield break;

        Transform target = emotionLookTargets[targetIndex];
        var defaultCam = themeCamera.DefaultCamera;

        defaultCam.transform.position = themeCamera.LastSweepPosition;
        defaultCam.transform.rotation = themeCamera.LastSweepRotation;

        defaultCam.LookAt = null;
        themeCamera.SmoothLookAt(target, 1f);

        var pov = defaultCam.GetCinemachineComponent<Cinemachine.CinemachinePOV>();
        if (pov != null)
        {
            Vector3 dir = target.position - defaultCam.transform.position;
            if (dir.sqrMagnitude > 0.01f)
            {
                Quaternion rot = Quaternion.LookRotation(dir.normalized);
                Vector3 euler = rot.eulerAngles;

                pov.m_HorizontalAxis.Value = euler.y;
                pov.m_VerticalAxis.Value = -euler.x;
            }
        }
        else
        {
            Vector3 dir = target.position - defaultCam.transform.position;
            if (dir.sqrMagnitude > 0.01f)
            {
                Quaternion lookRot = Quaternion.LookRotation(dir.normalized);
                defaultCam.transform.rotation = lookRot;
            }
        }

        yield return new WaitForSeconds(1f);
        ResetEmotion();
    }

    #endregion


    #region 초등학교 시네마틱

    // 줌인 시네마틱 연출 시작
    public void PlayFocusZoomFrom(Vector3 fromPos, Vector3 fromRot, int index, float duration, float stopDistance)
    {
        if (index >= emotionLookTargets.Count || emotionLookTargets[index] == null || themeCamera == null)
            return;

        Transform target = emotionLookTargets[index];

        themeCamera.SwitchCameras();
        themeCamera.ClearAim();
        themeCamera.SetLookAt(null);
        themeCamera.SetFollow(null);

        Vector3 direction = (target.position - fromPos).normalized;
        Vector3 toPos = target.position - direction * stopDistance;
        Vector3 lookTarget = target.position;
        float toFOV = 40f;

        StartCoroutine(FocusZoomRoutine(fromPos, fromRot, toPos, lookTarget, toFOV, duration));
    }

    // 줌인 시네마틱 카메라 이동 처리
    private IEnumerator FocusZoomRoutine(Vector3 fromPos, Vector3 fromRot, Vector3 toPos, Vector3 lookTarget,
        float toFOV, float duration)
    {
        float elapsed = 0f;
        float fromFOV = themeCamera.DefaultCamera.m_Lens.FieldOfView;
        Quaternion fromQ = Quaternion.Euler(fromRot);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            Vector3 currentPos = Vector3.Lerp(fromPos, toPos, t);
            Vector3 dir = (lookTarget - currentPos).normalized;
            Quaternion toQ = Quaternion.LookRotation(dir);

            themeCamera.SetPosition(currentPos);
            themeCamera.SetRotation(Quaternion.Slerp(fromQ, toQ, t).eulerAngles);
            themeCamera.SetZoom(Mathf.Lerp(fromFOV, toFOV, t));

            yield return null;
        }

        themeCamera.SetPosition(toPos);
        themeCamera.SetRotation(Quaternion.LookRotation((lookTarget - toPos).normalized).eulerAngles);
        themeCamera.SetZoom(toFOV);
    }

    // 시점 멈춤 위치 계산
    public Vector3 GetStopPosition(int index, float stopDistance, Vector3 fromPos)
    {
        if (index >= emotionLookTargets.Count || emotionLookTargets[index] == null)
            return Vector3.zero;

        Vector3 dir = (emotionLookTargets[index].position - fromPos).normalized;
        return emotionLookTargets[index].position - dir * stopDistance;
    }

    // 시점 타겟 위치 반환
    public Vector3 GetLookTarget(int index)
    {
        if (index >= emotionLookTargets.Count || emotionLookTargets[index] == null)
            return Vector3.zero;

        return emotionLookTargets[index].position;
    }

    #endregion
}
