 using UnityEngine;
using Cinemachine;
using System.Collections;

// 테마 카메라 연출 제어 스크립트
public class ThemeCameraController : MonoBehaviour
{
    [Tooltip("플레이어 카메라")]
    [SerializeField] private CinemachineVirtualCamera defaultCamera;

    [Tooltip("테마 카메라")]
    [SerializeField] private CinemachineVirtualCamera themeCamera;

    private Coroutine rotateRoutine;
    private Vector3 lastSweepPosition;
    private Quaternion lastSweepRotation;
    public CinemachineVirtualCamera DefaultCamera => defaultCamera;
    public CinemachineVirtualCamera ThemeCamera => themeCamera;
    public Vector3 LastSweepPosition
    {
        get => lastSweepPosition;
        set => lastSweepPosition = value;
    }

    public Quaternion LastSweepRotation
    {
        get => lastSweepRotation;
        set => lastSweepRotation = value;
    }


    // 테마 카메라로 전환
    public void SwitchCameras()
    {
        SetCameraPriority(themeCamera, defaultCamera);
    }

    // 기본 카메라로 복귀
    public void ResetToDefault()
    {
        SetCameraPriority(defaultCamera, themeCamera);
        StartCoroutine(ResetZoomAfterDelay(5f));
    }
    // 줌 리셋
    private IEnumerator ResetZoomAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (themeCamera != null)
            themeCamera.m_Lens.FieldOfView = 60f;
    }

    // 우선순위 조절을 통해 가시 카메라 설정
    private void SetCameraPriority(CinemachineVirtualCamera activeCam, CinemachineVirtualCamera inactiveCam)
    {
        activeCam.Priority = 20;
        inactiveCam.Priority = 10;
    }

    // 테마 카메라 위치 변경
    public void SetPosition(Vector3 newPosition)
    {
        themeCamera.transform.position = newPosition;
    }

    // 회전값을 직접 변경
    public void SetRotation(Vector3 eulerAngles)
    {
        themeCamera.transform.rotation = Quaternion.Euler(eulerAngles);
    }

    // 테마 카메라의 Follow 대상 설정
    public void SetFollow(Transform target)
    {
        themeCamera.Follow = target;
    }

    // 테마 카메라의 LookAt 대상 설정
    public void SetLookAt(Transform target)
    {
        themeCamera.LookAt = target;
    }

    // 부드럽게 회전하며 타겟을 바라보게 만듦
    public void SmoothLookAt(Transform target, float duration = 1f)
    {
        if (rotateRoutine != null)
            StopCoroutine(rotateRoutine);
        rotateRoutine = StartCoroutine(RotateOverTime(target, duration));
    }

    private IEnumerator RotateOverTime(Transform target, float duration)
    {
        if (target == null)
            yield break;

        Quaternion startRot = themeCamera.transform.rotation;
        Vector3 dir = target.position - themeCamera.transform.position;
        Quaternion endRot = Quaternion.LookRotation(dir.normalized);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            themeCamera.transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }

        themeCamera.transform.rotation = endRot;
    }

    // Aim 설정을 특정 타입으로 변경
    public void SetAim<T>() where T : CinemachineComponentBase
    {
        if (themeCamera == null) return;

        themeCamera.DestroyCinemachineComponent<CinemachineComposer>();
        themeCamera.DestroyCinemachineComponent<CinemachinePOV>();
        themeCamera.DestroyCinemachineComponent<CinemachineFramingTransposer>();

        themeCamera.AddCinemachineComponent<T>();
    }

    // Do Nothing 상태로 전환
    public void ClearAim()
    {
        if (themeCamera == null) return;

        themeCamera.DestroyCinemachineComponent<CinemachineComposer>();
        themeCamera.DestroyCinemachineComponent<CinemachinePOV>();
        themeCamera.DestroyCinemachineComponent<CinemachineFramingTransposer>();
    }

    // Body 설정을 특정 타입으로 변경
    public void SetBody<T>() where T : CinemachineComponentBase
    {
        if (themeCamera == null) return;

        themeCamera.DestroyCinemachineComponent<CinemachineTransposer>();
        themeCamera.DestroyCinemachineComponent<CinemachineFramingTransposer>();
        themeCamera.DestroyCinemachineComponent<CinemachineTrackedDolly>();
        themeCamera.DestroyCinemachineComponent<Cinemachine3rdPersonFollow>();

        themeCamera.AddCinemachineComponent<T>();
    }

    // Do Nothing 상태로 전환
    public void ClearBody()
    {
        if (themeCamera == null) return;

        themeCamera.DestroyCinemachineComponent<CinemachineTransposer>();
        themeCamera.DestroyCinemachineComponent<CinemachineFramingTransposer>();
        themeCamera.DestroyCinemachineComponent<CinemachineTrackedDolly>();
        themeCamera.DestroyCinemachineComponent<Cinemachine3rdPersonFollow>();
    }


    public void SetFOV(float fov)
    {
        if (themeCamera != null)
            themeCamera.m_Lens.FieldOfView = fov;
    }

    public float GetFOV()
    {
        return themeCamera != null ? themeCamera.m_Lens.FieldOfView : 60f;
    }

    public void PlayFOVZoom(float fromFOV, float toFOV, float duration, System.Action onComplete = null)
    {
        StartCoroutine(FOVZoomRoutine(fromFOV, toFOV, duration, onComplete));
    }

    private IEnumerator FOVZoomRoutine(float from, float to, float duration, System.Action onComplete)
    {
        if (themeCamera == null)
            yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            themeCamera.m_Lens.FieldOfView = Mathf.Lerp(from, to, t);
            yield return null;
        }

        themeCamera.m_Lens.FieldOfView = to; // 마지막 보정
        onComplete?.Invoke();
    }

    public void PlayMoveCamera(Vector3 from, Vector3 to, float duration, System.Action onComplete = null)
    {
        SwitchCameras();
        ClearAim();
        SetLookAt(null);
        SetFollow(null);
        SetPosition(from);

        StartCoroutine(MoveRoutine(from, to, duration, onComplete));
    }

    private IEnumerator MoveRoutine(Vector3 from, Vector3 to, float duration, System.Action onComplete)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            SetPosition(Vector3.Lerp(from, to, t));
            yield return null;
        }

        SetPosition(to);
        onComplete?.Invoke();
    }

}
