using UnityEngine;

// TutorialPanel의 동작을 제어하는 스크립트
public class TutorialPanelUI : UiBase
{
    public override void Init()
    {
        // UIManager에 자신을 등록합니다.
        GameManager.Instance.UIManager.Add<TutorialPanelUI>(this);
    }

    private void Update()
    {
        // 이 패널이 활성화되어 있을 때, ESC 키가 눌리면
        if (gameObject.activeInHierarchy && Input.GetKeyDown(KeyCode.Escape))
        {
            // GameManager에게 튜토리얼이 끝났음을 알립니다.
            GameManager.Instance.EndTutorial();
        }
    }
}
