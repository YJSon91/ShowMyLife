using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Settings")]
    [Tooltip("출력할 대사 내용을 입력합니다.")]
    [SerializeField] private string _dialogueMessage = "이곳은 유치원이다. 모든 것이 새롭다.";

    // 대사가 이미 한 번 출력되었는지 확인하는 변수 (스위치 역할)
    private bool _hasBeenTriggered = false;

    private void OnTriggerEnter(Collider other)
    {        
        if (!_hasBeenTriggered && other.CompareTag("Player"))
        {
            _hasBeenTriggered = true;            
            UIManager.Instance.ShowDialogue(_dialogueMessage);// UIManager를 통해 대사 출력을 요청합니다.

        }
    }
}
