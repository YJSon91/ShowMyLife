using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleChildCollider : MonoBehaviour
{
    private BaseObstacle parentObstacle;

    void Awake()
    {
        parentObstacle = GetComponentInParent<BaseObstacle>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && parentObstacle != null)
        {
            parentObstacle.NotifyPlayerOnPlatform(collision.transform, collision.gameObject.GetComponent<Rigidbody>());
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && parentObstacle != null)
        {
            parentObstacle.NotifyPlayerExitPlatform(collision.transform);
        }
    }
}
