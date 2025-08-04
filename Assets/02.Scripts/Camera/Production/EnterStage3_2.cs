using UnityEngine;
using Cinemachine;
using System.Collections;

public class EnterStage3_2 : MonoBehaviour
{
    [Tooltip("EmotionDirector 참조")]
    [SerializeField] private EmotionDirector emotionDirector;

    [Tooltip("돌리 카트")]
    [SerializeField] private CinemachineDollyCart dollyCart;

    [Tooltip("돌리 카트2")]
    [SerializeField] private CinemachineDollyCart dollyCart2;

    [Tooltip("바라볼 대상")]
    [SerializeField] private Transform lookTarget;

    [Tooltip("카트 속도")]
    [SerializeField] private float cartSpeed = 20f;

    [Tooltip("카트2 속도")]
    [SerializeField] private float cartSpeed2 = 20f;

    [Tooltip("카트 이동 시간")]
    [SerializeField] private float rideDuration = 10f;

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
        emotionDirector.DisablePlayerControl(player);
        emotionDirector.ResetEmotion();

        Vector3 camStart = dollyCart.transform.position;
        emotionDirector.ThemeCamera.SetPosition(camStart);
        emotionDirector.ResetThemeCamera();

        var themeCam = emotionDirector.ThemeCamera;

        // Body = Transposer (dollyCart 따라감)
        emotionDirector.SetBody<CinemachineTransposer>();
        themeCam.SetFollow(dollyCart.transform);

        // Aim = Composer (lookTarget 응시)
        emotionDirector.SetAim<CinemachineComposer>();
        themeCam.SetLookAt(lookTarget);

        // 돌리카트 이동 시작
        dollyCart.m_Position = 0f;
        dollyCart.m_Speed = cartSpeed;

        dollyCart2.m_Position = 0f;
        dollyCart2.m_Speed = cartSpeed2;

        yield return new WaitForSeconds(rideDuration);

        dollyCart.m_Speed = 0f;
        dollyCart2.m_Speed = 0f;

        // 카메라 설정 원복
        themeCam.SetFollow(null);
        themeCam.SetLookAt(null);
        emotionDirector.ClearBody();
        emotionDirector.ClearAim();

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

