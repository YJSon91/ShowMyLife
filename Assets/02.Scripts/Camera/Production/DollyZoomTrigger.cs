using UnityEngine;

public class DollyZoomTrigger : MonoBehaviour
{
    [Tooltip("연출 매니저")]
    [SerializeField] private EmotionDirector emotionDirector;

    [Tooltip("줌 FOV 값")]
    [SerializeField] private float zoomFOV = 40f;

    [Tooltip("줌 연출 시간")]
    [SerializeField] private float zoomDuration = 1f;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || !other.CompareTag("Dolly")) return;
        hasTriggered = true;

        var param = new EmotionParams
        {
            fromFOV = emotionDirector.ThemeCamera.GetFOV(),
            toFOV = zoomFOV,
            duration = zoomDuration
        };

        emotionDirector.PlayCommonEmotion(CommonEmotionType.FOVZoom, param);

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

