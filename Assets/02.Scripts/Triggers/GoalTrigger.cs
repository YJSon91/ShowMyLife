using UnityEngine;
#if UNITY_EDITOR 
using UnityEditor;
#endif

/// <summary>
/// 플레이어의 목표 지점 도착을 감지하여 StageManager에게 보고하는 트리거입니다.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class GoalTrigger : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 충돌한 오브젝트가 'Player' 태그를 가지고 있다면
        if (other.CompareTag("Player"))
        {
            // GameManager를 통해 StageManager에게 레벨 클리어 사실을 '보고'합니다.
            if (GameManager.Instance != null && GameManager.Instance.StageManager != null)
            {
                GameManager.Instance.StageManager.OnPlayerReachedGoal();
            }

            // 한 번만 작동하도록 트리거를 비활성화합니다.
            gameObject.SetActive(false);
        }
    }
    private void OnDrawGizmos()
    {
        // 기즈모의 색상을 설정합니다.
        Gizmos.color = Color.yellow;

        // 이 오브젝트의 위치를 기준으로 기즈모를 그립니다.
        Gizmos.matrix = transform.localToWorldMatrix;

        // 이 오브젝트에 붙어있는 BoxCollider의 크기에 맞춰 와이어 큐브를 그립니다.
        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
        {
            Gizmos.DrawWireCube(box.center, box.size);
        }

#if UNITY_EDITOR
        // 1. 텍스트 스타일을 정의할 새로운 GUIStyle 객체를 만듭니다.
        GUIStyle style = new GUIStyle();

        // 2. 원하는 스타일을 설정합니다.
        style.normal.textColor = Color.black; // 텍스트 색상을 초록색으로
        style.fontSize = 100;                  // 폰트 크기를 20으로
        style.fontStyle = FontStyle.Bold;     // 폰트를 굵게
        style.alignment = TextAnchor.MiddleCenter; // 텍스트를 중앙 정렬 (선택 사항)

        // 3. Handles.Label을 호출할 때, 마지막 인자로 이 스타일을 넘겨줍니다.
        Handles.Label(transform.position + Vector3.up, "Goal", style);
#endif
    }
}
