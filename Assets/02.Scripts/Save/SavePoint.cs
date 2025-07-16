using UnityEngine;

public class SavePoint : MonoBehaviour
{
    public static float SaveDisableUntil = 0f;

    [SerializeField] private string saveId;
    private bool hasSaved = false;

    private void Reset()
    {
        saveId = gameObject.name;
    }

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
            Debug.Log("[SavePoint] 저장 차단 중 (쿨타임)");
            return;
        }

        if (GameManager.Instance == null || GameManager.Instance.SaveManager == null)
        {
            Debug.LogWarning("[SavePoint] SaveManager가 초기화되지 않아 저장을 건너뜁니다.");
            return;
        }

        Vector3 savePosition = other.transform.position;
        GameManager.Instance.SaveManager.Save(savePosition, saveId);

        Debug.Log($"[SavePoint] 저장됨 → ID: {saveId}, 위치: {savePosition}");
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
