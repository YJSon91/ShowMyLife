using UnityEngine;
using UnityEditor; // 에디터 스크립트를 위해 필수!
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json; // Newtonsoft.Json 패키지가 설치되어 있어야 합니다.

public class DialogueTool : EditorWindow
{
    // 메뉴 아이템을 추가하여, 에디터 상단 메뉴에서 이 툴을 열 수 있게 합니다.
    [MenuItem("Tools/Dialogue Manager/Sync Dialogue Triggers")]
    public static void ShowWindow()
    {
        // 툴 창을 엽니다.
        GetWindow<DialogueTool>("Dialogue Sync");
    }

    private void OnGUI()
    {
        // 툴 창에 설명과 버튼을 그립니다.
        GUILayout.Label("Dialogue Data Synchronization", EditorStyles.boldLabel);
        GUILayout.Label("dialogue.json 파일과 DialogueTrigger 프리팹을 동기화합니다.");

        if (GUILayout.Button("Sync Prefabs with JSON"))
        {
            SyncDialogueTriggers();
        }
    }

    private void SyncDialogueTriggers()
    {
        // 1. JSON 파일 로드
        string filePath = Path.Combine(Application.streamingAssetsPath, "dialogue.json");
        if (!File.Exists(filePath))
        {
            Debug.LogError("dialogue.json 파일을 찾을 수 없습니다! 경로: " + filePath);
            return;
        }

        string jsonString = File.ReadAllText(filePath);
        var database = JsonConvert.DeserializeObject<Dictionary<DialogueTriggerType, List<Dialogue>>>(jsonString);

        if (database == null)
        {
            Debug.LogError("JSON 파싱에 실패했습니다. dialogue.json 파일의 형식을 확인해주세요.");
            return;
        }

        // 2. JSON에 있는 모든 대사 ID 목록을 만듭니다.
        HashSet<string> idsInJson = new HashSet<string>();
        foreach (var dialogueList in database.Values)
        {
            foreach (var dialogue in dialogueList)
            {
                // ID가 비어있지 않은 경우에만 추가
                if (!string.IsNullOrEmpty(dialogue.id))
                {
                    idsInJson.Add(dialogue.id);
                }
            }
        }
        Debug.Log($"JSON에서 {idsInJson.Count}개의 고유 ID를 찾았습니다.");


        // 3. 지정된 폴더에서 모든 DialogueTrigger 프리팹을 찾습니다.
        string searchPath = "Assets/03.Prefabs/Triggers";
        string[] guids = AssetDatabase.FindAssets("t:prefab", new[] { searchPath });
        Dictionary<string, GameObject> prefabsInProject = new Dictionary<string, GameObject>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            // 프리팹에 DialogueTrigger 컴포넌트가 있는지 확인
            DialogueTrigger trigger = prefab.GetComponent<DialogueTrigger>();
            if (trigger != null && !string.IsNullOrEmpty(trigger.DialogueID))
            {
                // 중복된 ID가 있을 경우 경고를 표시하고, 처음 찾은 것만 사용
                if (!prefabsInProject.ContainsKey(trigger.DialogueID))
                {
                    prefabsInProject.Add(trigger.DialogueID, prefab);
                }
                else
                {
                    Debug.LogWarning($"중복된 Dialogue ID를 가진 프리팹이 있습니다: '{trigger.DialogueID}'. 경로: {path}");
                }
            }
        }
        Debug.Log($"'{searchPath}' 폴더에서 {prefabsInProject.Count}개의 DialogueTrigger 프리팹을 찾았습니다.");


        // 4. 데이터 비교 및 프리팹 생성/경고
        string prefabFolderPath = "Assets/03.Prefabs/Triggers"; // 프리팹을 생성할 경로
        if (!Directory.Exists(prefabFolderPath))
        {
            Directory.CreateDirectory(prefabFolderPath);
        }

        int createdCount = 0;
        int warningCount = 0;

        // JSON에는 있는데 프로젝트에는 없는 ID -> 새 프리팹 생성
        foreach (string id in idsInJson)
        {
            if (!prefabsInProject.ContainsKey(id))
            {
                // 임시 게임 오브젝트 생성
                GameObject newTriggerObj = new GameObject(id);

                // DialogueTrigger 컴포넌트 추가 및 ID 설정
                DialogueTrigger newTrigger = newTriggerObj.AddComponent<DialogueTrigger>();
                newTrigger.SetDialogueID(id);

                // 트리거로 작동하기 위한 BoxCollider 추가
                BoxCollider collider = newTriggerObj.AddComponent<BoxCollider>();
                collider.isTrigger = true;
                collider.size = new Vector3(1, 1, 1); // 기본 크기 설정

                // 지정된 경로에 프리팹으로 저장
                string prefabPath = $"{prefabFolderPath}/{id}.prefab";
                PrefabUtility.SaveAsPrefabAsset(newTriggerObj, prefabPath);

                // 씬에 남은 임시 오브젝트 즉시 제거
                DestroyImmediate(newTriggerObj);
                createdCount++;
            }
        }

        // 프로젝트에는 있는데 JSON에는 없는 ID -> 경고 메시지 출력
        foreach (var pair in prefabsInProject)
        {
            if (!idsInJson.Contains(pair.Key))
            {
                Debug.LogWarning($"JSON에 없는 ID를 가진 프리팹 발견: '{pair.Key}'. 삭제를 고려해보세요. 경로: {AssetDatabase.GetAssetPath(pair.Value)}");
                warningCount++;
            }
        }

        Debug.Log($"동기화 완료! {createdCount}개의 새 프리팹 생성, {warningCount}개의 불일치 항목(경고) 발견.");
    }
}
