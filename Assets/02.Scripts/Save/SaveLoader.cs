using UnityEngine;

public class SaveLoader : MonoBehaviour
{
    private Transform player;
    private static bool _alreadyLoaded = false;

    private void Awake()
    {
        if (_alreadyLoaded) return;

        if (player == null)
        {
            GameObject obj = GameObject.FindWithTag("Player");
            if (obj != null)
                player = obj.transform;
        }

        if (player == null)
        {
            Debug.LogWarning("[SaveLoader] 플레이어가 연결되지 않았습니다.");
            return;
        }

        if (SaveManager.Load(out string saveId) is Vector3 pos)
        {
            player.position = pos;
            Debug.Log($"[SaveLoader] 위치 로드 완료 → {saveId} at {pos}");
            _alreadyLoaded = true;
        }
    }

}
