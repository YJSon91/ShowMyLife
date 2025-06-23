using UnityEditor;
using UnityEngine;
using System.IO;

public class DebugWindow : EditorWindow
{
    [MenuItem("도구/디버그")]
    public static void OpenWindow()
    {
        GetWindow<DebugWindow>("디버그");
    }

    private void OnGUI()
    {
        GUILayout.Label("디버그 도구", EditorStyles.boldLabel);
        GUILayout.Space(5);

        if (GUILayout.Button("저장된 위치로 이동하기"))
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("실행 중에서만 사용할 수 있습니다.");
                return;
            }

            if (SaveManager.Load(out string saveId) is Vector3 position)
            {
                GameObject player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    SavePoint.SaveDisableUntil = Time.time + 1f;

                    Rigidbody rb = player.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.velocity = Vector3.zero;
                        rb.MovePosition(position);
                    }
                    else
                    {
                        player.transform.position = position;
                    }

                    Debug.Log($"[디버그] {saveId} 위치로 이동 완료: {position}");
                }
                else
                {
                    Debug.LogWarning("'Player' 태그가 지정된 오브젝트를 찾을 수 없습니다.");
                }
            }
            else
            {
                Debug.LogWarning("저장된 데이터가 없습니다.");
            }
        }

        if (GUILayout.Button("저장 파일 내용 확인"))
        {
            string path = Path.Combine(Application.persistentDataPath, "SaveData.json");
            if (File.Exists(path))
            {
                string content = File.ReadAllText(path);
                Debug.Log($"저장 파일 내용:\n{content}");
            }
            else
            {
                Debug.LogWarning("저장 파일이 존재하지 않습니다.");
            }
        }

        if (GUILayout.Button("저장 파일 삭제"))
        {
            string path = Path.Combine(Application.persistentDataPath, "SaveData.json");
            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log("저장 파일이 삭제되었습니다.");
            }
            else
            {
                Debug.LogWarning("삭제할 저장 파일이 없습니다.");
            }
        }
    }
}
