using UnityEngine;
using DG.Tweening; // DOTween 사용
using TMPro;

public class StartPanelUI : UiBase
{
    [SerializeField] private TextMeshProUGUI _pressKeyText;
    private Tween _blinkingTween;

    public override void Init()
    {
        // UIManager에 자신을 등록
        GameManager.Instance.UIManager.Add<StartPanelUI>(this);
    }

    // 패널이 보일 때, 텍스트 점멸 효과 시작
    public override void Show(bool show)
    {
        base.Show(show); // 부모의 Show 함수(SetActive) 호출

        if (show)
        {
            // 이전에 실행되던 트윈이 있다면 안전하게 제거
            _blinkingTween?.Kill();
            // 텍스트 점멸 애니메이션 시작
            _blinkingTween = _pressKeyText.DOFade(0f, 0.8f)
                                         .SetEase(Ease.InOutQuad)
                                         .SetLoops(-1, LoopType.Yoyo);
        }
        else
        {
            // 패널이 숨겨질 때, 점멸 효과 중지
            _blinkingTween?.Kill();
        }
    }
}
