 using Newtonsoft.Json; // Newtonsoft.Json 패키지가 설치되어 있어야 합니다.
using System.Collections.Generic;
using System.IO;
using UnityEditor; // 에디터 스크립트를 위해 필수!
using UnityEngine;
using System;

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
        // --- 1. JSON 파일 로드 및 파싱 (수정된 부분) ---
        string filePath = Path.Combine(Application.streamingAssetsPath, "dialogue.json");
        if (!File.Exists(filePath))
        {
            Debug.LogError("dialogue.json 파일을 찾을 수 없습니다! 경로: " + filePath);
            return;
        }

        string jsonString = File.ReadAllText(filePath);

        // JSON 배열을 List<Dialogue>로 직접 읽어옵니다.
        List<Dialogue> allDialogues = JsonConvert.DeserializeObject<List<Dialogue>>(jsonString);

        if (allDialogues == null)
        {
            Debug.LogError("JSON 파싱에 실패했습니다. dialogue.json 파일의 형식을 확인해주세요.");
            return;
        }

        // --- 2. ID와 타입을 함께 저장하는 데이터 구조 생성 (수정된 부분) ---
        var dialoguesInJson = new Dictionary<string, DialogueTriggerType>();
        foreach (var dialogue in allDialogues)
        {
            // id나 type이 비어있는 잘못된 데이터는 건너뜁니다.
            if (!string.IsNullOrEmpty(dialogue.id) && !string.IsNullOrEmpty(dialogue.type))
            {
                // JSON에 있는 문자열 "type"을 DialogueTriggerType enum으로 변환합니다.
                // Enum.TryParse를 사용하면 안전하게 변환할 수 있습니다.
                if (Enum.TryParse<DialogueTriggerType>(dialogue.type, true, out DialogueTriggerType typeEnum))
                {
                    dialoguesInJson[dialogue.id] = typeEnum;
                }
                else
                {
                    Debug.LogWarning($"JSON에 인식할 수 없는 type이 있습니다: '{dialogue.type}' (ID: {dialogue.id})");
                }
            }
        }
        Debug.Log($"JSON에서 {dialoguesInJson.Count}개의 고유 ID와 타입을 찾았습니다.");


        // --- 3. 프로젝트의 프리팹 찾기 (기존 코드와 동일) ---
        string searchPath = "Assets/03.Prefabs/Triggers";
        string[] guids = AssetDatabase.FindAssets("t:prefab", new[] { searchPath });
        Dictionary<string, GameObject> prefabsInProject = new Dictionary<string, GameObject>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            DialogueTrigger trigger = prefab.GetComponent<DialogueTrigger>();
            if (trigger != null && !string.IsNullOrEmpty(trigger.DialogueID))
            {
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


        // --- 4. 데이터 비교 및 프리팹 생성/경고 (기존 코드와 동일) ---
        string prefabFolderPath = "Assets/03.Prefabs/Triggers";
        if (!Directory.Exists(prefabFolderPath))
        {
            Directory.CreateDirectory(prefabFolderPath);
        }

        int createdCount = 0;
        int warningCount = 0;

        // JSON에는 있는데 프로젝트에는 없는 ID -> 새 프리팹 생성
        foreach (var pair in dialoguesInJson)
        {
            string id = pair.Key;
            DialogueTriggerType type = pair.Value;

            if (!prefabsInProject.ContainsKey(id))
            {
                GameObject newTriggerObj = new GameObject(id);
                DialogueTrigger newTrigger = newTriggerObj.AddComponent<DialogueTrigger>();

                newTrigger.SetDialogueID(id);
                newTrigger.SetTriggerType(type);

                BoxCollider collider = newTriggerObj.AddComponent<BoxCollider>();
                collider.isTrigger = true;

                string prefabPath = $"{prefabFolderPath}/{id}.prefab";
                PrefabUtility.SaveAsPrefabAsset(newTriggerObj, prefabPath);
                DestroyImmediate(newTriggerObj);
                createdCount++;
            }
        }

        // 프로젝트에는 있는데 JSON에는 없는 ID -> 경고 메시지 출력
        foreach (var pair in prefabsInProject)
        {
            if (!dialoguesInJson.ContainsKey(pair.Key))
            {
                Debug.LogWarning($"JSON에 없는 ID를 가진 프리팹 발견: '{pair.Key}'. 삭제를 고려해보세요. 경로: {AssetDatabase.GetAssetPath(pair.Value)}");
                warningCount++;
            }
        }

        Debug.Log($"동기화 완료! {createdCount}개의 새 프리팹 생성, {warningCount}개의 불일치 항목(경고) 발견.");
    }
}
