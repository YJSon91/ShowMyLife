using UnityEngine;
using System.Collections;

public class DisableSelfEnableOtherAfterDelay : MonoBehaviour
{
    [SerializeField] private float delay = 0.5f;
    [SerializeField] private GameObject objectToEnable;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(SwitchObjects());
        }
    }

    private IEnumerator SwitchObjects()
    {
        yield return new WaitForSeconds(delay);

        if (objectToEnable != null)
            objectToEnable.SetActive(true);

        gameObject.SetActive(false);
    }
}
