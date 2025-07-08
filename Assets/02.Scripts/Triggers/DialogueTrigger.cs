using UnityEngine;

public enum DialogueTriggerType { Fall, Cheer, Process, Reach }

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private DialogueTriggerType _triggerType;
    private bool _hasBeenTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!_hasBeenTriggered && other.CompareTag("Player"))
        {
            _hasBeenTriggered = true;
            GameManager.Instance.DialogueManager.ShowRandomDialogueByType(_triggerType);
        }
    }
}
