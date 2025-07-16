using UnityEngine;
using DG.Tweening;

public class BookFallTrigger : MonoBehaviour
{
    [Tooltip("책")]
    [SerializeField] private Transform bookObject;

    [Tooltip("회전 시간")]
    [SerializeField] private float finalDuration = 1f;

    [Tooltip("발판 오브젝트")]
    [SerializeField] private GameObject objectToActivate2;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || !other.CompareTag("Player")) return;
        hasTriggered = true;

        if (objectToActivate2 != null)
            objectToActivate2.SetActive(true);

        Vector3 currentRotation = bookObject.localEulerAngles;

        bookObject.DOLocalRotate(
            new Vector3(90f, currentRotation.y, currentRotation.z),
            finalDuration
        ).SetEase(Ease.OutBounce);

        gameObject.SetActive(false);
    }

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
