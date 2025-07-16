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

        GUI.enabled = Application.isPlaying;

        if (GUILayout.Button("GameManager 임시 생성 및 연결"))
        {
            if (GameManager.Instance == null)
            {
                GameObject go = new GameObject("GameManager_Debug");
                GameManager gm = go.AddComponent<GameManager>();

                // 씬에 존재하는 매니저 찾기
                var sound = FindObjectOfType<SoundManager>() ?? go.AddComponent<SoundManager>();
                var save = FindObjectOfType<SaveManager>();
                var ui = FindObjectOfType<UIManager>() ?? go.AddComponent<UIManager>();
                var cam = FindObjectOfType<CameraManager>();
                var dia = FindObjectOfType<DialogueManager>() ?? go.AddComponent<DialogueManager>();
                var stage = FindObjectOfType<StageManager>() ?? go.AddComponent<StageManager>();
                var obs = FindObjectOfType<ObstacleManager>() ?? go.AddComponent<ObstacleManager>();

                if (save == null || cam == null)
                {
                    Debug.LogError("[DebugWindow] 필수 매니저(SaveManager, CameraManager)가 씬에 없습니다.");
                    return;
                }

                gm.InitializeManually(sound, save, ui, cam, dia, stage, obs);

                DontDestroyOnLoad(go);
                Debug.Log("[DebugWindow] GameManager 임시 생성 및 매니저 연결 완료.");
            }
            else
            {
                Debug.Log("[DebugWindow] 이미 GameManager 인스턴스가 존재합니다.");
            }
        }

        if (GUILayout.Button("저장된 위치로 이동하기"))
        {
            if (GameManager.Instance == null || GameManager.Instance.SaveManager == null)
            {
                Debug.LogWarning("[DebugWindow] SaveManager가 초기화되지 않았습니다.");
                return;
            }

            if (GameManager.Instance.SaveManager.TryLoad(out Vector3 pos, out string saveId))
            {
                GameObject player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    SavePoint.SaveDisableUntil = Time.time + 1f;

                    Rigidbody rb = player.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.velocity = Vector3.zero;
                        rb.MovePosition(pos);
                    }
                    else
                    {
                        player.transform.position = pos;
                    }

                    Debug.Log($"[디버그] {saveId} 위치로 이동 완료: {pos}");
                }
                else
                {
                    Debug.LogWarning("Player 태그 오브젝트를 찾을 수 없습니다.");
                }
            }
            else
            {
                Debug.LogWarning("저장된 위치 데이터가 없습니다.");
            }
        }

        GUI.enabled = true;
        GUILayout.Space(10);

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
