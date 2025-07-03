using UnityEngine;
using System.Collections;

public class EntranceTriggerDirector : MonoBehaviour
{
    [Tooltip("연출매니저")]
    [SerializeField] private EmotionDirector emotionDirector;
    [Tooltip("플레이어")]
    [SerializeField] private Transform playerTransform;
    [Tooltip("발판 오브젝트")]
    [SerializeField] private GameObject objectToActivate;
    [Tooltip("연출 시간")]
    [SerializeField] private float skyPanDuration = 3f;
    [Tooltip("훑을 각도")]
    [SerializeField] private float sweepAngle = 90f;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || !other.CompareTag("Player")) return;

        hasTriggered = true;

        // 하늘 훑기 연출 시작
        emotionDirector.PlaySkyEmotion(playerTransform, sweepAngle, skyPanDuration);

        // 즉시 플레이어를 뒤로 돌림
        Vector3 back = -playerTransform.forward;
        back.y = 0f;
        playerTransform.rotation = Quaternion.LookRotation(back);

        // 즉시 오브젝트 활성화
        if (objectToActivate != null)
            objectToActivate.SetActive(true);

        // 연출 시간 경과 후 입력 해제 및 자기 자신 비활성화
        StartCoroutine(ReleaseInputAfterDelay(skyPanDuration));
    }

    private IEnumerator ReleaseInputAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false); // 자기 자신 비활성화
    }
    //범위 표시
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
