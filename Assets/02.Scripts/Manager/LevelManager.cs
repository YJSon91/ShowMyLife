using UnityEngine;

public class LevelManager : MonoBehaviour
{
    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterLevelManager(this);
        }
        else
        {
            Debug.LogError("[LevelManager] LevelManager가 씬에 존재하지 않습니다!");
        }
    }

    // 앞으로 여기에 레벨 관련 기능들이 추가될 예정입니다.
}
