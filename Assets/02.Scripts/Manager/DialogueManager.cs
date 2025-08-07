// DialogueManager.cs
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections;


public class DialogueManager : MonoBehaviour
{
    [Header("낙하 대사 시간 기준")]
    [SerializeField] private float _middleFallTimeThreshold = 2.5f; // 중간 낙하 기준 시간
    [SerializeField] private float _highFallTimeThreshold = 4.0f;   // 높은 낙하 기준 시간
    // JSON 데이터를 담을 딕셔너리 <트리거타입, 대사목록>
    // Dictionary도 바로 JSON으로 변환할 수 있습니다!
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
    public void SubscribeToPlayerAnimationEvents(PlayerAnimationEventHandler eventHandler)
    {
        if (eventHandler == null) return;

        // 전달받은 '인스턴스'의 이벤트에 구독합니다.
        eventHandler.OnLandingAnimationEvent += HandleNormalLanding;
        eventHandler.OnLandingHardAnimationEvent += HandleHardLanding;
    }
    public void UnsubscribeFromPlayerAnimationEvents(PlayerAnimationEventHandler eventHandler)
    {
        if (eventHandler == null) return;
        eventHandler.OnLandingAnimationEvent -= HandleNormalLanding;
        eventHandler.OnLandingHardAnimationEvent -= HandleHardLanding;
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
                // 1. JSON 배열을 List로 먼저 읽어옵니다.
                List<Dialogue> allDialogues = JsonConvert.DeserializeObject<List<Dialogue>>(jsonString);

                // 2. 읽어온 List를 순회하며 Dictionary로 재구성합니다.
                _dialogueDatabase = new Dictionary<DialogueTriggerType, List<Dialogue>>();

                foreach (var dialogue in allDialogues)
                {
                    if (string.IsNullOrEmpty(dialogue.type)) continue;

                    // 문자열 type을 enum type으로 변환
                    if (Enum.TryParse<DialogueTriggerType>(dialogue.type, true, out DialogueTriggerType typeEnum))
                    {
                        // 딕셔너리에 해당 키가 없으면 새로 리스트를 만들어 추가
                        if (!_dialogueDatabase.ContainsKey(typeEnum))
                        {
                            _dialogueDatabase.Add(typeEnum, new List<Dialogue>());
                        }
                        // 해당 키의 리스트에 대사 추가
                        _dialogueDatabase[typeEnum].Add(dialogue);
                    }
                }
            }
            catch (Exception ex)
            {
                // try-catch로 감싸주면 파싱 중 에러가 나도 게임이 멈추지 않고 원인을 파악하기 좋습니다.
                Debug.LogError($"[DialogueManager] JSON 파싱 중 오류 발생: {ex.Message}");
            }
        
    }
        else
        {
            Debug.LogError("[DialogueManager] dialogue.json 파일을 찾을 수 없습니다! 경로를 확인해주세요.");
        }
    }

    public void ShowRandomDialogueByType(DialogueTriggerType type)
    {        
        if (_dialogueDatabase.TryGetValue(type, out List<Dialogue> dialogues) && dialogues.Count > 0)
        {
            Debug.Log($"<color=cyan>3. [DialogueManager] '{type}' 타입의 대사를 찾았습니다. 출력합니다: ");

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
    public void ShowDialogueUI(string message)
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
    private IEnumerator SequentialDialogueRoutine(string[] dialogueIDs)
    {
        // 1. 플레이어의 조작을 잠시 멈춥니다 (선택 사항).
       // GameManager.Instance.PlayerControls.Player.Disable();

        foreach (string id in dialogueIDs)
        {
            // 2. ID에 해당하는 대사를 찾아서 화면에 표시합니다.
            ShowDialogueByID(id);

            // 3. 대사가 끝나기를 기다립니다. 
            //    (여기서는 간단히 3초 + 키 입력 대기로 처리)
            yield return new WaitForSeconds(3.0f);
           // yield return new WaitUntil(() => Input.anyKeyDown);
        }

        // 4. 모든 대사가 끝나면 UI를 숨기고 플레이어 조작을 다시 활성화합니다.
        GameManager.Instance.UIManager.Hide<DialogueUI>();
        //GameManager.Instance.PlayerControls.Player.Enable();
    }
    /// </summary>
    public void StartSequentialDialogue(string[] dialogueIDs)
    {
        // 이미 다른 대사 시퀀스가 실행 중이라면 중복 실행을 막습니다.
        StopAllCoroutines();
        StartCoroutine(SequentialDialogueRoutine(dialogueIDs));
    }
    private void HandlePlayerFall(float fallDuration)
    {
        DialogueTriggerType fallType;

        // 낙하 시간에 따라 대사 타입을 결정합니다.
        if (fallDuration >= _highFallTimeThreshold)
        {
            fallType = DialogueTriggerType.Fall_High; // 이 enum 멤버들을 추가해야 합니다.
        }
        else if (fallDuration >= _middleFallTimeThreshold)
        {
            fallType = DialogueTriggerType.Fall_Middle;
        }
        else if (fallDuration >= 1.5f) // 1.5초 이상일 때만 Low로 간주
        {
            fallType = DialogueTriggerType.Fall_Low;
        }
        else
        {
            return; // 1.5초 미만의 짧은 낙하는 대사를 출력하지 않음
        }

        ShowRandomDialogueByType(fallType);
    }
    // "일반 착지" 애니메이션 방송을 들었을 때
    private void HandleNormalLanding(PlayerAnimationEventHandler handler)
    {
        Debug.Log("<color=yellow>2. [DialogueManager] 일반 착지 신호 수신! 대사를 출력합니다.</color>");
        ShowRandomDialogueByType(DialogueTriggerType.Fall);
    }

    // "세게 착지" 애니메이션 방송을 들었을 때
    private void HandleHardLanding(PlayerAnimationEventHandler handler)
    {
        Debug.Log("<color=red>2. [DialogueManager] 세게 착지 신호 수신! 대사를 출력합니다.</color>");
        // 여기서는 예시로 Middle과 High 대사 중 하나를 보여줍니다.
        ShowRandomDialogueByType(DialogueTriggerType.Fall);
    }
}
