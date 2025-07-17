using UnityEngine;
using System.Collections.Generic;
using System.Collections;


public enum CommonEmotionType
{
    Sweep,
    FOVZoom,
    MoveCamera,
    RotateToTarget,
    TopDownSlow,
    ResetAll,
    EndEmotion,
}

public struct EmotionParams
{
    public Vector3 position;
    public Vector3 fromPosition;
    public Vector3 toPosition;

    public float pitch;
    public float angle;
    public float duration;
    public float baseYaw;

    public float fromFOV;
    public float toFOV;

    public bool sweepLeftToRight;

    public System.Action onComplete;
    public Transform targetTransform;
}

public class EmotionDirector : MonoBehaviour
{
    [Tooltip("ThemeCameraController 스크립트")] [SerializeField]
    private ThemeCameraController themeCamera;

    [Tooltip("PostProcessing 스크립트")] [SerializeField]
    private PostProcessingManager postProcessing;

    [Tooltip("TimeEffect 스크립트")] [SerializeField]
    private TimeEffectManager timeEffect;

    [Tooltip("연출용 타겟 리스트")] [SerializeField]
    private List<Transform> emotionLookTargets = new List<Transform>();

    public ThemeCameraController ThemeCamera => themeCamera;
    public PostProcessingManager PostProcessing => postProcessing;

    public void PlayCommonEmotion(CommonEmotionType type, EmotionParams param)
    {
        switch (type)
        {
            case CommonEmotionType.Sweep:
                PlaySweepEmotion(param.position, param.pitch, param.angle, param.duration, param.baseYaw, param.sweepLeftToRight);
                break;


            case CommonEmotionType.FOVZoom:
                themeCamera?.PlayFOVZoom(param.fromFOV, param.toFOV, param.duration, param.onComplete);
                break;

            case CommonEmotionType.MoveCamera:
                themeCamera?.PlayMoveCamera(param.fromPosition, param.toPosition, param.duration, param.onComplete);
                break;

            case CommonEmotionType.RotateToTarget:
                StartCoroutine(RotateLookDirection(param.fromPosition, param.targetTransform, param.duration));
                break;

            case CommonEmotionType.TopDownSlow:
                PlayTopDownEmotion();
                break;

            case CommonEmotionType.ResetAll:
                ResetEmotion();
                break;

            case CommonEmotionType.EndEmotion:
                EndEmotion(param.onComplete);
                break;
        }
    }



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

    // 연출 종료
    public void EndEmotion(System.Action onComplete = null)
    {
        onComplete?.Invoke();
    }

    // 카메라 리셋
    public void ResetThemeCamera()
    {
        if (themeCamera == null) return;

        themeCamera.SwitchCameras();
        themeCamera.ClearAim();
        themeCamera.SetLookAt(null);
        themeCamera.SetFollow(null);
    }
    // 메인 카메라 복귀
    public void ResetToDefault()
    {
        themeCamera?.ResetToDefault();
    }

    // 플레이어 기준 기본 카메라 시작 위치 반환
    public Vector3 GetPlayerEyePosition(Transform player)
    {
        if (player == null) return Vector3.zero;
        return player.position + Vector3.up * 1.6f;

    }

    // 지정된 각도로 훑기 연출
    public void PlaySweepEmotion(Vector3 position, float pitch, float sweepAngle, float duration,
        float baseYaw = 180f, bool sweepLeftToRight = true)
    {
        if (themeCamera == null) return;

        themeCamera.SwitchCameras();
        themeCamera.ClearAim();
        themeCamera.SetLookAt(null);
        themeCamera.SetFollow(null);

        StartCoroutine(SweepRoutine(position, pitch, sweepAngle, duration, baseYaw, sweepLeftToRight));
    }

    private IEnumerator SweepRoutine(Vector3 fixedPos, float pitch, float sweepAngle, float duration,
        float baseYaw, bool sweepLeftToRight)
    {
        float startYaw = sweepLeftToRight ? -sweepAngle * 0.5f : sweepAngle * 0.5f;
        float endYaw = sweepLeftToRight ? sweepAngle * 0.5f : -sweepAngle * 0.5f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float yaw = Mathf.Lerp(startYaw, endYaw, t);

            Quaternion rot = Quaternion.Euler(pitch, baseYaw + yaw, 0f); // 기준 yaw + 훑기 각도
            themeCamera.SetPosition(fixedPos);
            themeCamera.SetRotation(rot.eulerAngles);

            yield return null;
        }

        // 마지막 상태 저장
        Quaternion lastRot = Quaternion.Euler(pitch, baseYaw + endYaw, 0f);
        themeCamera.LastSweepPosition = fixedPos;
        themeCamera.LastSweepRotation = lastRot;

        themeCamera.SetPosition(fixedPos);
        themeCamera.SetRotation(lastRot.eulerAngles);
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

    // 카메라 회전 LookAt 기반
    public IEnumerator RotateLookDirection(Vector3 fromPosition, Transform target, float duration)
    {
        if (themeCamera == null || target == null)
            yield break;

        var defaultCam = themeCamera.DefaultCamera;
        if (defaultCam == null)
            yield break;

        Quaternion fromRot = defaultCam.transform.rotation;
        Vector3 lookDir = (target.position - fromPosition).normalized;
        Quaternion toRot = Quaternion.LookRotation(lookDir);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            defaultCam.transform.rotation = Quaternion.Slerp(fromRot, toRot, t);
            yield return null;
        }

        var pov = defaultCam.GetCinemachineComponent<Cinemachine.CinemachinePOV>();
        if (pov != null)
        {
            Vector3 finalDir = target.position - defaultCam.transform.position;
            if (finalDir.sqrMagnitude > 0.01f)
            {
                Quaternion lookRot = Quaternion.LookRotation(finalDir.normalized);
                Vector3 euler = lookRot.eulerAngles;

                pov.m_HorizontalAxis.Value = euler.y;
                pov.m_VerticalAxis.Value = -euler.x;
            }
        }
    }

    // 카메라회전 수동
    public IEnumerator RotateLookFromSweep(Vector3 fromPosition, Quaternion fromRotation, Transform target, float duration, float speedMultiplier = 1f)
    {
        if (themeCamera == null || target == null)
            yield break;

        Vector3 lookDir = (target.position - fromPosition).normalized;
        Quaternion toRotation = Quaternion.LookRotation(lookDir);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime * speedMultiplier;
            float t = Mathf.Clamp01(elapsed / duration);
            Quaternion rot = Quaternion.Slerp(fromRotation, toRotation, t);
            themeCamera.SetRotation(rot.eulerAngles);
            yield return null;
        }
    }

    //캐릭터 블라인드
    public void SetPlayerVisible(Transform player, bool visible)
    {
        if (player == null) return;

        var renderers = player.GetComponentsInChildren<Renderer>(true);
        foreach (var renderer in renderers)
        {
            renderer.enabled = visible;
        }
    }
    // 타켓 리스트
    public Transform GetLookTargetTransform(int index)
    {
        if (index < 0 || index >= emotionLookTargets.Count) return null;
        return emotionLookTargets[index];
    }

    #endregion



    #region 플레이어 조작제한

    // 플레이어 조작을 비활성화
    public void DisablePlayerControl(Transform player)
    {
        // GameManager를 통해 Player 참조
        Player playerComponent = GameManager.Instance?.Player;
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
                // 이동 중이던 상태를 완전히 초기화
                movement.ResetMovement();
                Debug.Log("이동 비활성화 및 초기화");
            }
        }
    }

    // 플레이어 조작을 활성화
    public void EnablePlayerControl(Transform player)
    {
        // GameManager를 통해 Player 참조
        Player playerComponent = GameManager.Instance?.Player;
        if (playerComponent != null)
        {
            var movement = playerComponent.GetComponent<PlayerMovementController>();
            if (movement != null)
            {
                // 이동 상태를 초기화하여 이전 입력이 남아있지 않도록 함
                movement.ResetMovement();
                movement.enabled = true;
                Debug.Log("이동 활성화");
            }
            
            var inputReader = playerComponent.GetComponent<InputReader>();
            if (inputReader != null)
            {
                inputReader.EnableInput();
                Debug.Log("입력 활성화");
            }
        }
    }

    #endregion



    #region 유치원 시네마틱
    public void StartFinishSkySweep(float delay, int targetIndex)
    {
        StartCoroutine(FinishSkySweepRoutine(delay, targetIndex));
    }

    private IEnumerator FinishSkySweepRoutine(float delay, int targetIndex)
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

        yield return StartCoroutine(RotateLookDirection(themeCamera.LastSweepPosition, target, 1f));

        yield return new WaitForSeconds(0.5f);
        ResetToDefault();
        ResetEmotion();
    }


    #endregion

    #region 초등학교 시네마틱

    public void PlayZoomFromPlayer(Transform player, int index, float duration, float targetFOV = 40f, float moveDistance = 100f, System.Action onComplete = null)
    {
        if (player == null || index >= emotionLookTargets.Count || emotionLookTargets[index] == null || themeCamera == null)
            return;

        ResetThemeCamera();

        // 시작 위치: 플레이어 기준
        Vector3 fromPos = GetPlayerEyePosition(player);

        // 타겟 방향
        Vector3 dir = (emotionLookTargets[index].position - fromPos).normalized;
        if (dir.sqrMagnitude < 0.01f)
            return;

        // 회전 세팅 + 위치 세팅
        Quaternion rot = Quaternion.LookRotation(dir);
        themeCamera.SetRotation(rot.eulerAngles);
        themeCamera.SetPosition(fromPos);

        // 도착 위치
        Vector3 toPos = fromPos + dir * moveDistance;

        // 현재 FOV
        float startFOV = themeCamera.GetFOV();

        // 이동 + 줌을 동시에 시작
        themeCamera.PlayMoveCamera(fromPos, toPos, duration, null);
        themeCamera.PlayFOVZoom(startFOV, targetFOV, duration, () =>
        {
            EndEmotion(onComplete);
        });
    }




    #endregion

    #region 도로 시네마틱
    public void PlayLookAroundThenFocus(int targetIndex, Transform player, float sweepDuration, float focusDuration,
        System.Action onComplete = null)
    {
        StartCoroutine(LookAroundThenFocusRoutine(targetIndex, player, sweepDuration, focusDuration, onComplete));
    }

    private IEnumerator LookAroundThenFocusRoutine(int targetIndex, Transform player, float sweepDuration,
        float focusDuration, System.Action onComplete)
    {
        if (themeCamera == null || player == null || targetIndex >= emotionLookTargets.Count ||
            emotionLookTargets[targetIndex] == null)
            yield break;

        // 1. 시작 위치 계산
        Vector3 startPos = GetPlayerEyePosition(player);

        // 2. 카메라 초기화
        ResetThemeCamera();
        themeCamera.SetPosition(startPos);

        // 4. 훑기 연출 (오른쪽 → 왼쪽)
        EmotionParams sweepLeft = new EmotionParams
        {
            position = startPos,
            pitch = 0f,
            angle = 130f,
            duration = sweepDuration * 0.5f,
            sweepLeftToRight = false,
        };
        PlayCommonEmotion(CommonEmotionType.Sweep, sweepLeft);
        yield return new WaitForSeconds(sweepDuration * 0.5f);

        // 5. 훑기 연출 (왼쪽 → 오른쪽)
        EmotionParams sweepRight = new EmotionParams
        {
            position = startPos,
            pitch = 0f,
            angle = 130f,
            duration = sweepDuration * 0.5f,
            sweepLeftToRight = true,
        };
        PlayCommonEmotion(CommonEmotionType.Sweep, sweepRight);
        yield return new WaitForSeconds(sweepDuration * 0.5f);

        // 6. 마지막 훑기 위치에서 타겟 응시 회전
        Vector3 camPos = themeCamera.LastSweepPosition;
        Quaternion fromRot = themeCamera.LastSweepRotation;
        Transform target = emotionLookTargets[targetIndex];

        float speedMultiplier = 1.8f;

        yield return StartCoroutine(RotateLookFromSweep(camPos, fromRot, target, focusDuration, speedMultiplier));

        yield return new WaitForSeconds(0.25f);

        // 7. 카메라 이동
        float stopDistance = 35f;
        Vector3 toPos = GetStopPosition(targetIndex, stopDistance, camPos);
        themeCamera.PlayMoveCamera(camPos, toPos, 5f, () =>
        {
            ResetToDefault();
            onComplete?.Invoke();
        });

        yield return new WaitForSeconds(5f);
    }
    #endregion

    #region 시작 시네마틱
    public void PlayMoveAndRotateToNeutral(Vector3 fromPos, Vector3 toPos, Quaternion fromRot, Quaternion toRot, float duration, System.Action onComplete = null)
    {
        StartCoroutine(MoveAndRotateToNeutralRoutine(fromPos, toPos, fromRot, toRot, duration, onComplete));
    }

    private IEnumerator MoveAndRotateToNeutralRoutine(Vector3 fromPos, Vector3 toPos, Quaternion fromRot, Quaternion toRot, float duration, System.Action onComplete)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            ThemeCamera.SetPosition(Vector3.Lerp(fromPos, toPos, t));
            ThemeCamera.SetRotation(Quaternion.Slerp(fromRot, toRot, t).eulerAngles);

            yield return null;
        }

        ThemeCamera.SetPosition(toPos);
        ThemeCamera.SetRotation(toRot.eulerAngles);

        onComplete?.Invoke();
    }

    #endregion
}
