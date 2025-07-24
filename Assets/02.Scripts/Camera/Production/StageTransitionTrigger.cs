using UnityEngine;

public class StageTransitionTrigger : MonoBehaviour
{
    [System.Serializable]
    public class ToggleObject
    {
        public GameObject target;
        public bool activate = true;
    }

    [Tooltip("오브젝트")]
    [SerializeField] private ToggleObject[] objectsToToggle;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || !other.CompareTag("Player")) return;
        hasTriggered = true;

        foreach (var item in objectsToToggle)
        {
            if (item.target != null)
                item.target.SetActive(item.activate);
        }

        gameObject.SetActive(false);
    }

    // 범위 표시
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.5f, 0.8f, 1f, 0.4f);

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
