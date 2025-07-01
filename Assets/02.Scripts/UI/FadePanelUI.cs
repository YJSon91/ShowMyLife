using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System; // Action 사용

public class FadePanelUI : UiBase
{
    private Image _fadeImage;

    public override void Init()
    {
        GameManager.Instance.UIManager.Add<FadePanelUI>(this);
        _fadeImage = GetComponent<Image>();
    }

    // 이 함수는 UIManager가 호출할 것입니다.
    public void FadeOut(float duration, Action onComplete = null)
    {
        Show(true); // 페이드 패널을 켜고
        _fadeImage.DOFade(0f, duration) // 점점 투명하게
                  .OnComplete(() => {
                      Show(false); // 애니메이션 끝나면 패널 끄기
                      onComplete?.Invoke(); // 추가 작업 실행
                  });
    }

    public void FadeIn(float duration, Action onComplete = null)
    {
        Show(true);
        _fadeImage.color = new Color(0, 0, 0, 0); // 완전 투명에서 시작
        _fadeImage.DOFade(1f, duration) // 점점 불투명하게
                  .OnComplete(() => onComplete?.Invoke());
    }
}
