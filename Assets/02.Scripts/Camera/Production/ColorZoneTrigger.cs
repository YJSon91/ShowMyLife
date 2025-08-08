using UnityEngine;

public class ColorZoneTrigger : MonoBehaviour
{
    [Tooltip("연출 매니저")]
    [SerializeField] private EmotionDirector emotionDirector;

    [Tooltip("적용할 색상")]
    [SerializeField] private Color targetColor = Color.red;

    [Tooltip("전환 시간")]
    [SerializeField] private float transitionDuration = 1f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        emotionDirector.PostProcessing.ApplyColorFilter(targetColor, transitionDuration);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        emotionDirector.PostProcessing.ResetToDefault(transitionDuration);
    }

    // 범위 시각화
    private void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col is BoxCollider box && box.isTrigger)
        {
            Gizmos.color = new Color(0.64f, 0.64f, 0.64f, 0.45f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.color = new Color(0.64f, 0.64f, 0.64f, 1f);
            Gizmos.DrawWireCube(box.center, box.size);
        }
    }
}
