using UnityEngine;

public enum DialogueTriggerType
{
    Fall, Cheer, Process, Reach, Start, End, Mid,Guide,
    Fall_Low, Fall_Middle, Fall_High // 높이별 낙하 타입을 추가
}

public class DialogueTrigger : MonoBehaviour
{
    [Header("트리거 설정")]
    [Tooltip("이 트리거의 종류를 선택합니다.")]
    [SerializeField] private DialogueTriggerType _triggerType;

    [Tooltip("지정 대사를 출력하고 싶을 경우, 여기에 대사 ID를 입력하세요.")]
    [SerializeField] private string _dialogueID = ""; // ID를 저장할 변수

    [Tooltip("여러 대사를 순서대로 출력하려면 여기에 ID 목록을 추가하세요.")]
    [SerializeField] private string[] _dialogueIDs; // 여러 ID를 담을 배열 추가

    [Tooltip("이 트리거를 한 번만 작동시킬지 여부를 설정합니다.")]
    [SerializeField] private bool _isOneTimeTrigger = true;

    [Header("나레이션 설정")]
    [Tooltip("이 대사에 나레이션을 재생할지 여부입니다.")]
    [SerializeField] private bool _playNarration = false;
    [Tooltip("재생할 나레이션 오디오 클립의 파일 이름을 정확히 입력하세요.")]
    [SerializeField] private string _narrationClipName;

    public string DialogueID => _dialogueID;
    public DialogueTriggerType TriggerType => _triggerType;
    private bool _hasBeenTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_isOneTimeTrigger && _hasBeenTriggered) return;

        if (!_hasBeenTriggered && other.CompareTag("Player"))
        {
            _hasBeenTriggered = true;

            var dialogueManager = GameManager.Instance.DialogueManager;
            if (dialogueManager == null) return;

            // 1. 순차 대사 목록이 있는지 최우선으로 확인합니다.
            if (_dialogueIDs != null && _dialogueIDs.Length > 0)
            {
                dialogueManager.StartSequentialDialogue(_dialogueIDs);
            }
            // 2. 순차 대사가 없다면, 지정된 단일 ID가 있는지 확인합니다.
            else if (!string.IsNullOrEmpty(_dialogueID))
            {
                dialogueManager.ShowDialogueByID(_dialogueID);
            }
            // 3. 둘 다 없다면, 타입에 맞는 랜덤 대사를 출력합니다.
            else
            {
                dialogueManager.ShowRandomDialogueByType(_triggerType);
            }
            if (_playNarration)
            {
              GameManager.Instance.SoundManager.PlayNarration(_narrationClipName);
            }

            // 대화가 시작되면 트리거 자체는 비활성화할 수 있습니다.
            //gameObject.SetActive(false);
        }
    }
    /// <summary>
    /// DialogueTool에서 Trigger Type을 설정하기 위한 public 함수
    /// </summary>
    public void SetTriggerType(DialogueTriggerType newType)
    {
        this._triggerType = newType;
    }
    // DialogueTool에서 ID를 설정하기 위한 public 함수
    public void SetDialogueID(string newID)
    {
        this._dialogueID = newID;
    }
   
}
