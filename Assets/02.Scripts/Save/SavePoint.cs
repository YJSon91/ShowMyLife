using UnityEngine;

public class SavePoint : MonoBehaviour
{
    public static float SaveDisableUntil = 0f;
    private string saveId;
    private bool hasSaved = false;

    private void Awake()
    {
        if (string.IsNullOrEmpty(saveId))
        {
            saveId = gameObject.name;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || hasSaved) return;

        if (Time.time < SaveDisableUntil)
        {
            Debug.Log("[SavePoint] 저장 차단 중");
            return;
        }

        SaveManager.Save(saveId, other.transform.position);
        Debug.Log($"[SavePoint] 저장됨 → ID: {saveId}, 위치: {other.transform.position}");
        hasSaved = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            hasSaved = false;
        }
    }

    private void OnDrawGizmos()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (box == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(box.center, box.size);
        UnityEditor.Handles.Label(transform.position + Vector3.up, $"Save: {saveId}");
    }
}
