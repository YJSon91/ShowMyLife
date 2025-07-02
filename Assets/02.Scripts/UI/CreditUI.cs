using UnityEngine;
using TMPro;
using DG.Tweening; 

public class CreditUI : UiBase
{
    [SerializeField] private RectTransform _creditTextRect; // 스크롤될 텍스트의 RectTransform
    [SerializeField] private TextMeshProUGUI _playtimeText;
    [SerializeField] private float _scrollDuration = 30f; // 전체 스크롤 시간

    public override void Init()
    {
        GameManager.Instance.UIManager.Add<CreditUI>(this);
    }

    public override void Show(bool show)
    {
        base.Show(show);
        if (show)
        {
            // TODO: GameManager로부터 실제 플레이 시간 받아오기
            _playtimeText.text = "00:25:30"; // 임시 플레이 시간
            StartScrolling();
        }
    }

    private void StartScrolling()
    {
        // 텍스트를 시작 위치(화면 아래)로 초기화
        _creditTextRect.anchoredPosition = new Vector2(0, -_creditTextRect.rect.height);

        // DOTween을 사용하여 정해진 시간 동안 목표 위치(화면 위)까지 부드럽게 이동
        _creditTextRect.DOAnchorPosY(_creditTextRect.rect.height, _scrollDuration)
                     .SetEase(Ease.Linear)
                     .OnComplete(() => {
                         // 스크롤이 끝나면 게임 상태를 MainMenu로 변경하도록 요청
                         GameManager.Instance.UpdateGameState(GameManager.GameState.MainMenu);
                     });
    }
}
