using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("대사 설정")]
    [Tooltip("출력할 대사 내용을 입력합니다.")]
    [TextArea(3, 5)]
    [SerializeField] private string _dialogueMessage = "이곳은 유치원이다. 모든 것이 새롭다.";

    private bool _hasBeenTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!_hasBeenTriggered && other.CompareTag("Player"))
        {
            _hasBeenTriggered = true;

            if (GameManager.Instance != null && GameManager.Instance.UIManager != null)
            {                
                // 1. UIManager에게 'DialogueUI' 컴포넌트를 달라고 요청합니다.
                var dialogueUI = GameManager.Instance.UIManager.Get<DialogueUI>();

                if (dialogueUI != null)
                {
                    // 2. 받아온 DialogueUI에 텍스트를 설정합니다.
                    dialogueUI.SetText(_dialogueMessage);

                    // 3. UIManager에게 'DialogueUI'를 화면에 보여달라고 최종 요청합니다.
                    GameManager.Instance.UIManager.Show<DialogueUI>(true);
                }
            }
            else
            {
                Debug.LogWarning("[DialogueTrigger] GameManager 또는 UIManager가 준비되지 않았습니다.");
            }
        }
    }
}
