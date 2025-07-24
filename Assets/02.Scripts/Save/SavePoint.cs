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
    //범위표시
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 0.4f, 0f, 0.4f);

        Collider col = GetComponent<Collider>();
        if (col is BoxCollider box)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
        }
        else if (col is SphereCollider sphere)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawSphere(sphere.center, sphere.radius);
        }
        UnityEditor.Handles.Label(transform.position + Vector3.up, $"Save: {saveId}");
    }

}
