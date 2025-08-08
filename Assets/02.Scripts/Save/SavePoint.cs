using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
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
            return;

        if (GameManager.Instance == null || GameManager.Instance.SaveManager == null)
            return;

        Vector3 savePosition = other.transform.position;
        GameManager.Instance.SaveManager.Save(savePosition, saveId);

        hasSaved = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            hasSaved = false;
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
    }

}
