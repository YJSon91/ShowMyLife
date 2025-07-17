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
        if (_fadeImage == null)
        {
            Debug.LogError("[FadePanelUI] Image 컴포넌트를 찾을 수 없습니다! 이 오브젝트에 Image 컴포넌트가 있는지 확인해주세요.");
        }
        else
        {
            Debug.Log("<color=lime>[FadePanelUI] Init 완료. Image 컴포넌트를 성공적으로 찾았습니다.</color>");
        }
    }
    public override void Show(bool show)
    {
        // 이 함수도 로그를 추가하여 언제 호출되는지 확인합니다.
        Debug.Log($"<color=orange>[{Time.time:.2f}초] UiBase.Show({show}) 호출됨! 오브젝트 상태: {show}</color>");
        base.Show(show);
    }
    /// <summary>
    /// 화면을 어둡게 만듭니다. (Fade In)
    /// </summary>
    /// <param name="duration">애니메이션 시간</param>
    /// <param name="onComplete">애니메이션이 끝난 후 실행할 작업</param>
    public Tween FadeIn(float duration, Action onComplete = null)
    {
        Debug.Log($"<color=cyan>[{Time.time:.2f}초] FadeIn 요청! FadePanel을 활성화합니다.</color>");
        gameObject.SetActive(true);
        _fadeImage.raycastTarget = true;
        _fadeImage.color = new Color(0, 0, 0, 0);
        return _fadeImage.DOFade(1f, duration)
                         .OnComplete(() => {
                             Debug.Log("<color=cyan>[FadePanelUI] FadeIn 애니메이션 완료!</color>");
                             onComplete?.Invoke();
                         });
    }
    /// <summary>
    /// 화면을 밝게 만듭니다. (Fade Out)
    /// </summary>
    public Tween FadeOut(float duration, Action onComplete = null)
    {
        return _fadeImage.DOFade(0f, duration)
                         .OnComplete(() => {
                             _fadeImage.raycastTarget = false;
                             gameObject.SetActive(false);
                             onComplete?.Invoke();
                         });
    }
}
