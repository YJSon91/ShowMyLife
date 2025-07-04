using UnityEngine;
using UnityEngine.EventSystems; 
using DG.Tweening;       

/// <summary>
/// UI 요소에 마우스를 올렸을 때(Hover), DOTween을 이용해 크기 변경 효과를 줍니다.
/// IPointerEnterHandler, IPointerExitHandler 인터페이스를 상속받습니다.
/// </summary>
public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("호버 효과 설정")]
    [Tooltip("마우스를 올렸을 때 커질 크기 배율")]
    [SerializeField] private float _hoverScale = 1.1f;

    [Tooltip("애니메이션이 재생되는 시간")]
    [SerializeField] private float _animationDuration = 0.2f;

    private Vector3 _originalScale; // 원래 크기를 저장할 변수

    private void Awake()
    {
        // 시작할 때, 이 오브젝트의 원래 크기를 저장해 둡니다.
        _originalScale = transform.localScale;
    }

    /// <summary>
    /// 마우스 포인터가 이 UI 요소 영역 안으로 들어왔을 때 자동으로 호출됩니다.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 진행 중인 다른 크기 변경 애니메이션이 있다면 즉시 중단하고,
        // 목표 크기(_hoverScale)로 부드럽게 확대시킵니다.
        transform.DOKill(); // 이전 애니메이션 중단
        transform.DOScale(_originalScale * _hoverScale, _animationDuration).SetEase(Ease.OutQuad);
        if (GameManager.Instance?.SoundManager != null)
        {
            GameManager.Instance.SoundManager.PlayButtonClickSFX();
        }
    }

    /// <summary>
    /// 마우스 포인터가 이 UI 요소 영역 밖으로 나갔을 때 자동으로 호출됩니다.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        // 진행 중인 다른 크기 변경 애니메이션이 있다면 즉시 중단하고,
        // 원래 크기(_originalScale)로 부드럽게 축소시킵니다.
        transform.DOKill(); // 이전 애니메이션 중단
        transform.DOScale(_originalScale, _animationDuration).SetEase(Ease.OutQuad);
        if (GameManager.Instance?.SoundManager != null)
        {
            GameManager.Instance.SoundManager.PlayButtonClickSFX();
        }
    }

    /// <summary>
    /// 마우스 포인터가 이 UI 요소를 클릭했을 때 자동으로 호출됩니다.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        // GameManager를 통해 SoundManager에게 버튼 클릭 효과음 재생을 요청합니다.
        if (GameManager.Instance?.SoundManager != null)
        {
           GameManager.Instance.SoundManager.PlayButtonClickSFX();
        }
    }
}
