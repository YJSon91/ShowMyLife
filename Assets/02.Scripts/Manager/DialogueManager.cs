// DialogueManager.cs
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class DialogueManager : MonoBehaviour
{
    // JSON 데이터를 담을 딕셔너리 <트리거타입, 대사목록>
    // 2. 이제 Dictionary도 바로 JSON으로 변환할 수 있습니다!
    private Dictionary<DialogueTriggerType, List<Dialogue>> _dialogueDatabase;

    private void Start()
    {
        GameManager.Instance.RegisterDialogueManager(this);
        LoadDialogueData();
    }

    private void LoadDialogueData()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "08.StreamingAssets", "dialogue.json");
        if (File.Exists(filePath))
        {
            string jsonString = File.ReadAllText(filePath);

            // 3. JsonUtility.FromJson 대신, 이 코드를 사용합니다.
            _dialogueDatabase = JsonConvert.DeserializeObject<Dictionary<DialogueTriggerType, List<Dialogue>>>(jsonString);

            Debug.Log("[DialogueManager] Newtonsoft.Json으로 대사 파일 로드 완료.");
        }
    }

    public void ShowRandomDialogueByType(DialogueTriggerType type)
    {
        // 4. 데이터베이스에서 대사를 찾아오는 방식도 더 간단해집니다.
        if (_dialogueDatabase.TryGetValue(type, out List<Dialogue> dialogues) && dialogues.Count > 0)
        {
            Dialogue randomDialogue = dialogues[Random.Range(0, dialogues.Count)];
            var dialogueUI = GameManager.Instance.UIManager.Get<DialogueUI>();
            if (dialogueUI != null)
            {
                dialogueUI.SetText(randomDialogue.text);
                GameManager.Instance.UIManager.Show<DialogueUI>(true);
            }
        }
    }
}
