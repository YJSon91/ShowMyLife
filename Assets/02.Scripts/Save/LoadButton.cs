using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class LoadButton : MonoBehaviour
{
    [SerializeField] private Button loadButton;
    private Transform player;

    private void Start()
    {
        if (player == null)
        {
            GameObject obj = GameObject.FindWithTag("Player");
            if (obj != null)
                player = obj.transform;
            else
                Debug.LogWarning("[LoadButton] Player 태그 오브젝트를 찾을 수 없습니다.");
        }

        string path = Path.Combine(Application.persistentDataPath, "SaveData.json");
        if (!File.Exists(path))
        {
            loadButton.interactable = false;
            return;
        }

        loadButton.onClick.AddListener(LoadSavedPosition);
    }

    private void LoadSavedPosition()
    {
        if (player == null)
        {
            Debug.LogWarning("[LoadButton] Player가 연결되지 않아 위치를 이동할 수 없습니다.");
            return;
        }

        if (SaveManager.Load(out string saveId) is Vector3 pos)
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

            Debug.Log($"[LoadButton] 위치 로드됨 → {saveId} at {pos}");
        }
    }
}

