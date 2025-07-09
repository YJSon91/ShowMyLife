using UnityEngine;

public enum DialogueTriggerType { Fall, Cheer, Process, Reach, Start, End }

public class DialogueTrigger : MonoBehaviour
{
    [Header("트리거 설정")]
    [Tooltip("이 트리거의 종류를 선택합니다.")]
    [SerializeField] private DialogueTriggerType _triggerType;

    [Tooltip("지정 대사를 출력하고 싶을 경우, 여기에 대사 ID를 입력하세요.")]
    [SerializeField] private string _dialogueID = ""; // ID를 저장할 변수
    public string DialogueID => _dialogueID;
    public DialogueTriggerType TriggerType => _triggerType;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // ID가 지정되었다면 지정 대사를, 비어있다면 랜덤 대사를 요청합니다.
            if (!string.IsNullOrEmpty(_dialogueID))
            {
                GameManager.Instance.DialogueManager.ShowDialogueByID(_dialogueID);
            }
            else
            {
                GameManager.Instance.DialogueManager.ShowRandomDialogueByType(_triggerType);
            }

            // 한 번만 작동하도록 비활성화
            gameObject.SetActive(false);
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
