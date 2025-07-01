using UnityEngine;
using System.Collections;

public class DisableAfterDelay : MonoBehaviour
{
    [SerializeField] float delay = 0.5f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(DisableRoutine());
        }
    }

    IEnumerator DisableRoutine()
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }
}
