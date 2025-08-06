using UnityEngine;
using System.Collections;
using Cinemachine;

public class EnterStage3_1_2 : MonoBehaviour
{
    [Tooltip("연출매니저")]
    [SerializeField] private EmotionDirector emotionDirector;

    [Tooltip("카메라 이동 거리")]
    [SerializeField] private float stopDistance = 35f;

    [Tooltip("카메라 이동 시간")]
    [SerializeField] private float moveDuration = 5f;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || !other.CompareTag("Player")) return;
        hasTriggered = true;

        Transform player = other.transform;
        StartCoroutine(PlaySequence(player));
    }

    private IEnumerator PlaySequence(Transform player)
    {
        CinemachineBrain brain = Camera.main.GetComponent<CinemachineBrain>();
        float originalBlendTime = brain.m_DefaultBlend.m_Time;
        brain.m_DefaultBlend.m_Time = 0.5f;

        emotionDirector.ResetEmotion();

        Vector3 camStart = emotionDirector.GetPlayerEyePosition(player);
        emotionDirector.ResetThemeCamera();
        emotionDirector.ThemeCamera.SetPosition(camStart);

        Transform target6 = emotionDirector.GetLookTargetTransform(6);
        Vector3 toPos = emotionDirector.GetStopPosition(6, stopDistance, camStart);
        Quaternion rot6 = Quaternion.identity;

        if (target6 != null)
        {
            Vector3 dir6 = (target6.position - toPos).normalized;
            rot6 = Quaternion.LookRotation(dir6);
            emotionDirector.ThemeCamera.SetRotation(rot6.eulerAngles);
        }

        bool moved = false;
        emotionDirector.ThemeCamera.PlayMoveCamera(camStart, toPos, moveDuration, () => moved = true);
        while (!moved) yield return null;

        yield return new WaitForSecondsRealtime(0.5f);

        Transform target7 = emotionDirector.GetLookTargetTransform(7);
        yield return StartCoroutine(emotionDirector.RotateLookFromSweep(toPos, rot6, target7, 1f));

        yield return new WaitForSecondsRealtime(0.5f);

        Quaternion rot7 = Quaternion.identity;
        if (target7 != null)
        {
            Vector3 dir7 = (target7.position - toPos).normalized;
            rot7 = Quaternion.LookRotation(dir7);
        }

        Transform target0 = emotionDirector.GetLookTargetTransform(8);
        yield return StartCoroutine(emotionDirector.RotateLookFromSweep(toPos, rot7, target0, 3f));

        yield return new WaitForSecondsRealtime(0.5f);

        yield return StartCoroutine(SmoothTimeScale(1f, 0.5f));

        var defaultCam = emotionDirector.ThemeCamera.DefaultCamera;
        if (defaultCam != null)
        {
            defaultCam.transform.position = emotionDirector.ThemeCamera.transform.position;
            defaultCam.transform.rotation = emotionDirector.ThemeCamera.transform.rotation;
        }

        brain.m_DefaultBlend.m_Time = originalBlendTime;
        emotionDirector.ResetToDefault();
        yield return null;

        if (defaultCam != null)
        {
            defaultCam.LookAt = null;
            yield return null;

            var pov = defaultCam.GetCinemachineComponent<CinemachinePOV>();
            if (pov != null && target0 != null)
            {
                Vector3 playerPos = player.position;
                Vector3 lookPos = target0.position;
                lookPos.y = playerPos.y;

                Vector3 dir = (lookPos - playerPos).normalized;
                if (dir.sqrMagnitude > 0.001f)
                {
                    float yaw = Quaternion.LookRotation(dir).eulerAngles.y;
                    pov.m_HorizontalAxis.Value = yaw;
                    pov.m_VerticalAxis.Value = 0f;
                }
            }
        }

        emotionDirector.EnablePlayerControl(player);
        gameObject.SetActive(false);
    }

    private IEnumerator SmoothTimeScale(float target, float duration)
    {
        float start = Time.timeScale;
        float time = 0f;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(start, target, time / duration);
            yield return null;
        }

        Time.timeScale = target;
    }

    // 범위 표시
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
