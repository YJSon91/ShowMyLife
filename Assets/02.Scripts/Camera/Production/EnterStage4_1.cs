 using UnityEngine;
using System.Collections;
using Cinemachine;

public class EnterStage4_1 : MonoBehaviour
{
    [Tooltip("연출 매니저")]
    [SerializeField] private EmotionDirector emotionDirector;

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
        emotionDirector.ResetEmotion();
        emotionDirector.DisablePlayerControl(player);

        Vector3 camStart = emotionDirector.GetPlayerEyePosition(player);
        emotionDirector.ResetThemeCamera();
        emotionDirector.ThemeCamera.SetPosition(camStart);

        Transform target10 = emotionDirector.GetLookTargetTransform(10);
        Vector3 toPos = emotionDirector.GetStopPosition(10, 0.01f, camStart);
        Quaternion rot10 = Quaternion.identity;

        if (target10 != null)
        {
            Vector3 dir10 = (target10.position - toPos).normalized;
            rot10 = Quaternion.LookRotation(dir10);
            emotionDirector.ThemeCamera.SetRotation(rot10.eulerAngles);
        }

        bool moved = false;
        emotionDirector.ThemeCamera.PlayMoveCamera(camStart, toPos, moveDuration, () => moved = true);
        while (!moved) yield return null;

        yield return new WaitForSecondsRealtime(0.5f);

        Transform target11 = emotionDirector.GetLookTargetTransform(11);
        yield return StartCoroutine(emotionDirector.RotateLookFromSweep(toPos, rot10, target11, 3f));

        yield return new WaitForSecondsRealtime(0.5f);

        Quaternion rot11 = Quaternion.identity;
        if (target11 != null)
        {
            Vector3 dir11 = (target11.position - toPos).normalized;
            rot11 = Quaternion.LookRotation(dir11);
        }

        Transform target12 = emotionDirector.GetLookTargetTransform(12);
        yield return StartCoroutine(emotionDirector.RotateLookFromSweep(toPos, rot11, target12, 2f));

        yield return new WaitForSecondsRealtime(0.5f);

        emotionDirector.ResetToDefault();
        emotionDirector.EnablePlayerControl(player);
        gameObject.SetActive(false);
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

