using UnityEngine;

/// <summary>
/// 플레이어가 진입했을 때, UIManager를 통해 특정 대사를 출력하도록 요청하는 트리거입니다.
/// </summary>
public class DialogueTrigger : MonoBehaviour
{
    [Header("대사 설정")]
    [Tooltip("출력할 대사 내용을 입력합니다.")]
    [TextArea(3, 5)] // 인스펙터 창에서 여러 줄로 편하게 입력할 수 있도록 합니다.
    [SerializeField] private string _dialogueMessage = "이곳은 유치원이다. 모든 것이 새롭다.";

    // 대사가 이미 한 번 출력되었는지 확인하는 변수
    private bool _hasBeenTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // 아직 한 번도 발동된 적이 없고, 들어온 오브젝트가 "Player" 태그를 가지고 있다면
        if (!_hasBeenTriggered && other.CompareTag("Player"))
        {
            // 다시는 발동되지 않도록 스위치를 켭니다.
            _hasBeenTriggered = true;

            // GameManager와 UIManager가 준비되었는지 확인하는 안전장치
            if (GameManager.Instance != null && GameManager.Instance.UIManager != null)
            {
                // GameManager를 통해 UIManager에게 대사 출력을 '요청'합니다.
                GameManager.Instance.UIManager.ShowDialogue(_dialogueMessage);
            }
            else
            {
                Debug.LogWarning("[DialogueTrigger] GameManager 또는 UIManager가 준비되지 않았습니다.");
            }
        }
    }
}
