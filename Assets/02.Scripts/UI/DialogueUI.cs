using UnityEngine;
using TMPro;
using System.Collections;

// 이 스크립트는 DialoguePanel 게임 오브젝트에 붙여줍니다.
public class DialogueUI : UiBase
{
    [SerializeField] private TextMeshProUGUI _dialogueText;
    [SerializeField] private float _displayTime = 3f;

    private Coroutine _hideCoroutine;

    // UiBase의 Init 함수를 구현합니다.
    public override void Init()
    {
        // UIManager에 자기 자신을 'DialogueUI' 타입으로 등록합니다.
        GameManager.Instance.UIManager.Add<DialogueUI>(this);
    }

    // 외부(DialogueTrigger)에서 대사 내용을 설정할 수 있도록 public 함수를 만듭니다.
    public void SetText(string message)
    {
        if (_dialogueText != null)
        {
            _dialogueText.text = message;
        }
    }

    // UiBase의 Show 함수를 재정의(override)하여 추가 기능을 넣습니다.
    public override void Show(bool show)
    {
        base.Show(show); // 먼저 부모의 Show 함수를 호출하여 패널을 켜거나 끕니다.

        // 만약 패널을 '보여주는' 경우라면
        if (show)
        {
            // 이전에 실행되던 숨기기 코루틴이 있다면 멈춥니다. (연속 호출 방지)
            if (_hideCoroutine != null)
            {
                StopCoroutine(_hideCoroutine);
            }
            // 지정된 시간 후에 자동으로 숨겨지는 코루틴을 새로 시작합니다.
            _hideCoroutine = StartCoroutine(HideAfterDelay(_displayTime));
        }
    }

    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // UIManager에게 다시 나를 숨겨달라고 요청합니다.
        GameManager.Instance.UIManager.Show<DialogueUI>(false);
    }
}
