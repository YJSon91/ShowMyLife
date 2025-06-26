using UnityEngine;

/// <summary>
/// 모든 UI 스크립트가 상속받아야 할 기본 클래스입니다.
/// </summary>
public abstract class UiBase : MonoBehaviour
{
    /// <summary>
    /// UI가 처음 생성될 때 호출되는 초기화 함수입니다.
    /// </summary>
    public abstract void Init();

    /// <summary>
    /// UI를 보여주거나 숨깁니다.
    /// </summary>
    public virtual void Show(bool show)
    {
        gameObject.SetActive(show);
    }
}
