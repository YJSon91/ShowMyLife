using UnityEngine;
using TMPro;
using DG.Tweening;

// StartPanel을 제어하는, UiBase를 상속받는 전문 스크립트
public class StartPanelUI : UiBase
{
    [Header("시작 화면 구성 요소")]
    [SerializeField] private TextMeshProUGUI _pressKeyText;
    [SerializeField] private FadePanelUI _fadePanel; // 페이드 연출을 위해 FadePanelUI 참조

    [Header("연출 시간 설정")]
    [SerializeField] private float _fadeDuration = 0.5f;

    private bool _keyHasBeenPressed = false;
    private Tween _blinkingTween;

    public override void Init()
    {
        // UIManager에 자신을 등록합니다.
        GameManager.Instance.UIManager.Add<StartPanelUI>(this);
        // FadePanel도 UIManager를 통해 찾아옵니다.
        _fadePanel = GameManager.Instance.UIManager.Get<FadePanelUI>();
    }

    // UiBase의 Show 함수를 재정의하여 추가 기능을 넣습니다.
    public override void Show(bool show)
    {
        base.Show(show); // 부모의 Show 함수(SetActive) 호출

        if (show)
        {
            // 키 입력 대기 상태로 초기화
            _keyHasBeenPressed = false;
            // 텍스트 점멸 효과 시작
            StartBlinking();
        }
        else
        {
            // 패널이 숨겨질 때, 점멸 효과 중지
            _blinkingTween?.Kill();
        }
    }

    private void Update()
    {
        // 이 UI가 활성화되어 있고, 아직 키가 눌리지 않았을 때만 입력을 감지
        if (gameObject.activeInHierarchy && !_keyHasBeenPressed && Input.anyKeyDown)
        {
            _keyHasBeenPressed = true;

            // 진행 중이던 점멸 애니메이션을 멈춥니다.
            _blinkingTween?.Kill();
            _pressKeyText.alpha = 1f;

            // 화면 전환 연출 시작
            TransitionToMainMenu();
        }
    }

    // DOTween으로 텍스트를 깜빡이게 하는 함수
    private void StartBlinking()
    {
        _pressKeyText.alpha = 1f; // 시작 시 알파값 초기화
        _blinkingTween = _pressKeyText.DOFade(0f, 0.8f)
                                     .SetEase(Ease.InOutQuad)
                                     .SetLoops(-1, LoopType.Yoyo);
    }

    // 화면 전환 연출 함수
    private void TransitionToMainMenu()
    {
        if (_fadePanel != null)
        {
            // 페이드인(어두워짐) 후, GameManager에 상태 변경을 요청
            _fadePanel.FadeIn(_fadeDuration, () => {
                GameManager.Instance.UpdateGameState(GameManager.GameState.MainMenu);
            });
        }
        else
        {
            // 페이드 패널이 없다면 즉시 상태 변경 요청
            GameManager.Instance.UpdateGameState(GameManager.GameState.MainMenu);
        }
    }
}
