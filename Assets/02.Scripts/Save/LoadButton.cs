 using UnityEngine;
using UnityEngine.UI;

public class LoadButton : MonoBehaviour
{
    [SerializeField] private Button loadButton;

    private Transform player;

    private void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            loadButton.interactable = false;
            return;
        }

        if (!GameManager.Instance.SaveManager.Exists())
        {
            loadButton.interactable = false;
            return;
        }

        loadButton.onClick.AddListener(LoadSavedPosition);
    }

    private void LoadSavedPosition()
    {
        if (player == null) return;

        if (GameManager.Instance.SaveManager.TryLoad(out Vector3 pos, out string saveId))
        {
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.MovePosition(pos);
            }
            else
            {
                player.position = pos;
            }

            SavePoint.SaveDisableUntil = Time.time + 1f;
        }
    }
}
