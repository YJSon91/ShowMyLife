// DialogueManager.cs
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System;
using System.Linq;

public class DialogueManager : MonoBehaviour
{
    // JSON 데이터를 담을 딕셔너리 <트리거타입, 대사목록>
    // 2. 이제 Dictionary도 바로 JSON으로 변환할 수 있습니다!
    private Dictionary<DialogueTriggerType, List<Dialogue>> _dialogueDatabase;

    private DialogueData _dialogueData;
    // 높이 기준점을 인스펙터에서 설정할 수 있도록 추가합니다.
    [SerializeField] private float _middleHeightThreshold = 50f;
    [SerializeField] private float _highHeightThreshold = 100f;

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
    /// <summary>
    /// 트리거의 높이를 기준으로 Fall 타입의 랜덤 대사를 출력합니다.
    /// </summary>
    public void ShowFallDialogueByHeight(float triggerHeight)
    {
        // 1. 열쇠를 문자열 "Fall" 대신, enum 타입인 DialogueTriggerType.Fall 로 변경합니다.
        if (!_dialogueDatabase.ContainsKey(DialogueTriggerType.Fall)) return;

        // 2. 데이터를 가져올 때도 동일하게 enum 타입을 사용합니다.
        List<Dialogue> fallDialogues = _dialogueDatabase[DialogueTriggerType.Fall];
        List<Dialogue> targetDialogues;

        // 높이에 따라 ID에 포함된 키워드로 대사를 필터링합니다.
        if (triggerHeight >= _highHeightThreshold)
        {
            targetDialogues = fallDialogues.Where(d => d.id.Contains("High")).ToList();
        }
        else if (triggerHeight >= _middleHeightThreshold)
        {
            targetDialogues = fallDialogues.Where(d => d.id.Contains("Middle")).ToList();
        }
        else
        {
            targetDialogues = fallDialogues.Where(d => d.id.Contains("Low")).ToList();
        }

        // 필터링된 목록에서 랜덤 대사를 출력합니다.
        if (targetDialogues.Count > 0)
        {
            Dialogue randomDialogue = targetDialogues[UnityEngine.Random.Range(0, targetDialogues.Count)];
            ShowDialogueUI(randomDialogue.text);
        }
    }
}
