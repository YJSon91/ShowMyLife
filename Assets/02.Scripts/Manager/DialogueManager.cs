// DialogueManager.cs
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System;

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
        // JSON 파일의 정확한 경로를 확인합니다.
        string filePath = Path.Combine(Application.streamingAssetsPath, "dialogue.json");

        Debug.Log($"[DialogueManager] 대사 파일을 로드합니다: {filePath}");

        // 파일이 존재하는지 확인합니다.
        if (File.Exists(filePath))
        {
            // 파일 내용을 읽어옵니다.
            string jsonString = File.ReadAllText(filePath);

            // 파일 내용이 비어있지 않은지 확인합니다.
            if (string.IsNullOrEmpty(jsonString))
            {
                Debug.LogError("[DialogueManager] dialogue.json 파일의 내용이 비어있습니다!");
                return;
            }

            try
            {
                // JSON 변환을 시도합니다.
                _dialogueDatabase = JsonConvert.DeserializeObject<Dictionary<DialogueTriggerType, List<Dialogue>>>(jsonString);

                // 변환 후 데이터베이스가 null이 아닌지 최종 확인합니다.
                if (_dialogueDatabase != null)
                {
                    Debug.Log("<color=lime>[DialogueManager] JSON 대사 파일 로드 및 변환 성공!</color>");
                }
                else
                {
                    Debug.LogError("[DialogueManager] JSON 변환에 실패했습니다. 파일 형식을 확인해주세요.");
                }
            }
            catch (Exception e)
            {
                // JSON 형식 오류 등 변환 중 발생하는 모든 오류를 잡아냅니다.
                Debug.LogError($"[DialogueManager] JSON 파싱 중 오류 발생: {e.Message}");
            }
        }
        else
        {
            Debug.LogError("[DialogueManager] dialogue.json 파일을 찾을 수 없습니다! 경로를 확인해주세요.");
        }
    }

    public void ShowRandomDialogueByType(DialogueTriggerType type)
    {
        // 4. 데이터베이스에서 대사를 찾아오는 방식도 더 간단해집니다.
        if (_dialogueDatabase.TryGetValue(type, out List<Dialogue> dialogues) && dialogues.Count > 0)
        {
            Dialogue randomDialogue = dialogues[UnityEngine.Random.Range(0, dialogues.Count)];
            var dialogueUI = GameManager.Instance.UIManager.Get<DialogueUI>();
            if (dialogueUI != null)
            {
                dialogueUI.SetText(randomDialogue.text);
                GameManager.Instance.UIManager.Show<DialogueUI>(true);
            }
        }
    }
    /// <summary>
    /// 지정된 ID를 가진 대사를 출력하도록 요청합니다.
    /// </summary>
    public void ShowDialogueByID(string dialogueID)
    {
        // 모든 대사 목록을 순회하며 일치하는 ID를 찾습니다.
        foreach (var dialogueList in _dialogueDatabase.Values)
        {
            foreach (var dialogue in dialogueList)
            {
                if (dialogue.id == dialogueID)
                {
                    // 일치하는 ID를 찾으면, 대사를 출력하고 함수를 종료합니다.
                    ShowDialogueUI(dialogue.text);
                    return;
                }
            }
        }

        // 모든 목록을 찾아도 ID가 없다면 경고 메시지를 출력합니다.
        Debug.LogWarning($"[DialogueManager] ID '{dialogueID}'에 해당하는 대사를 찾을 수 없습니다.");
    }
    /// <summary>
    /// UIManager를 통해 대사 UI를 활성화하고 텍스트를 설정합니다. (코드 중복 방지)
    /// </summary>
    private void ShowDialogueUI(string message)
    {
        var dialogueUI = GameManager.Instance.UIManager.Get<DialogueUI>();
        if (dialogueUI != null)
        {
            dialogueUI.SetText(message);
            GameManager.Instance.UIManager.Show<DialogueUI>(true);
        }
    }
}
