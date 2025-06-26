using UnityEngine;

public class LevelManager : MonoBehaviour
{
    private void Awake()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterLevelManager(this);
        }
    }

    // 앞으로 여기에 레벨 관련 기능들이 추가될 예정입니다.
}
