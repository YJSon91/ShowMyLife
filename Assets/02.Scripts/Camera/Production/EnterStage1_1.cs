 using UnityEngine;

public class EnterStage1_1 : MonoBehaviour
{
    [Tooltip("연출 매니저")]
    [SerializeField] private EmotionDirector emotionDirector;

    [Tooltip("트리거 발동 지연 시간")]
    [SerializeField] private float triggerDelay = 0.5f;

    [Tooltip("첫번째 연출속도")]
    [SerializeField] private float sweepDuration = 8.5f;

    [Tooltip("두번째 연출속도")]
    [SerializeField] private float focusDuration = 5f;

    [Tooltip("벽면 오브젝트")]
    [SerializeField] private GameObject objectToActivate;

    private bool hasTriggered = false;
    private Transform playerTransform;
    // 온 트리거
    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || !other.CompareTag("Player")) return;

        hasTriggered = true;
        playerTransform = other.transform;

        // 오브젝트 즉시 활성화
        if (objectToActivate != null)
            objectToActivate.SetActive(true);

        // 0.5초 후 모든 연출 시작
        Invoke(nameof(DelayedTriggerSequence), triggerDelay);
    }
    // 카메라 연출
    private void DelayedTriggerSequence()
    {
        if (emotionDirector == null)
        {
            Debug.LogWarning("[DisableOnPlayerTouch] EmotionDirector가 연결되지 않았습니다.");
            FinishSequence();
            return;
        }

        // 조작 차단 + 연출 시작
        emotionDirector.DisablePlayerControl(playerTransform);

        emotionDirector.PlayLookAroundThenFocus(
            4,
            playerTransform,
            sweepDuration,
            focusDuration,
            FinishSequence
        );
    }

    private void FinishSequence()
    {
        // 조작 복원
        if (playerTransform != null)
            emotionDirector.EnablePlayerControl(playerTransform);

        // 메인 카메라로 복귀
        emotionDirector.ResetToDefault();

        // 자기 자신 비활성화
        gameObject.SetActive(false);
    }

    //범위표시
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
