 using UnityEngine;

public abstract class UiBase : MonoBehaviour
{
    // 각 UI 패널의 원래 위치를 저장할 변수
    private Vector3 _originalAnchoredPosition;

    // 이 UI가 초기화될 때 한번만 호출되는 가상 함수
    public virtual void Init()
    {
        // 시작할 때 자신의 원래 위치를 저장해 둡니다.
        _originalAnchoredPosition = GetComponent<RectTransform>().anchoredPosition;
    }

    /// <summary>
    /// UI를 보여주거나 원래 위치로 되돌립니다.
    /// </summary>
    public virtual void Show(bool show)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null) return;

        if (show)
        {
            // UI를 보여줄 때는, 오브젝트를 활성화하고 위치를 화면 중앙(0, 0, 0)으로 이동시킵니다.
            gameObject.SetActive(true);
            rectTransform.anchoredPosition = Vector3.zero;
        }
        else
        {
            // UI를 숨길 때는, 위치를 원래 저장해두었던 자리로 되돌려 놓습니다.
            // 이렇게 하면 씬 뷰에서 작업하기 편한 위치에 계속 머물게 됩니다.
            rectTransform.anchoredPosition = _originalAnchoredPosition;
            gameObject.SetActive(false);
        }
    }
}
