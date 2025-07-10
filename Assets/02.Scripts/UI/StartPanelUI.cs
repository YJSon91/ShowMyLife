using UnityEngine;
using TMPro;
using DG.Tweening; // DOTween 사용을 위해 필수
using UnityEngine.UI; // Image를 사용하기 위해 필수

public class StartPanelUI : UiBase
{
    [Header("연출 대상 UI 요소")]
    [SerializeField] private Image _backgroundImage;      // 0.5초에 페이드인 될 배경 이미지
    [SerializeField] private TextMeshProUGUI _titleText; // 2.0초에 등장할 'SHOW MY LIFE' 로고
    [SerializeField] private TextMeshProUGUI _pressKeyText;   // 3.0초에 점멸 시작할 안내 문구
    [SerializeField] private FadePanelUI _fadePanel;          // 4.0초에 화면 전환을 위한 페이드 패널

    private bool _isSequencePlaying = false;
    private bool _keyInputEnabled = false;

    public override void Init()
    {
        GameManager.Instance.UIManager.Add<StartPanelUI>(this);
        // UIManager를 통해 다른 UI를 찾아올 수 있습니다.
        _fadePanel = GameManager.Instance.UIManager.Get<FadePanelUI>();
    }

    // StartPanel이 화면에 표시될 때 연출을 시작합니다.
    public override void Show(bool show)
    {
        base.Show(show); // gameObject.SetActive(true) 실행
        if (show)
        {
            StartIntroSequence();
        }
    }

    private void Update()
    {
        // 키 입력이 활성화되었고, 아무 키나 눌렸다면
        if (_keyInputEnabled && Input.anyKeyDown)
        {
            // 한 번만 실행되도록 플래그를 false로 바꿉니다.
            _keyInputEnabled = false;

            Debug.Log("키 입력 감지! 메인 메뉴로 전환합니다.");

            // 페이드 아웃 후 메인메뉴 상태로 전환 요청
            if (_fadePanel != null)
            {
                _fadePanel.FadeIn(0.5f, () =>
                {
                    GameManager.Instance.UpdateGameState(GameManager.GameState.MainMenu);
                });
            }
            else
            {
                GameManager.Instance.UpdateGameState(GameManager.GameState.MainMenu);
            }
        }
    }

    /// <summary>
    /// 기획서 타임라인에 맞는 인트로 연출을 실행합니다.
    /// </summary>
    private void StartIntroSequence()
    {
        if (_isSequencePlaying) return;
        _isSequencePlaying = true;
        _keyInputEnabled = false;

        // --- 초기 상태 설정 ---
        // FadePanel을 검은색 반투명 상태로 시작
        if (_backgroundImage != null)
        {
            _backgroundImage.color = new Color(0, 0, 0, 0); // 완전 투명에서 시작
        }
        if (_titleText != null) _titleText.alpha = 0;
        if (_pressKeyText != null) _pressKeyText.alpha = 0;
        // --- 초기 상태 설정 끝 ---

        // DOTween 시퀀스를 생성합니다.
        Sequence introSequence = DOTween.Sequence();

        // 타임라인에 따라 애니메이션을 순서대로 추가합니다.
        introSequence
            // 2.0초: 로고 등장
            .AppendInterval(2f)
            .AppendCallback(() => {
                if (_titleText != null)
                {
                    _titleText.transform.localScale = Vector3.one * 0.5f;
                    _titleText.transform.DOScale(1f, 1f).SetEase(Ease.OutBack);
                    _titleText.DOFade(1f, 1f);
                }
            })
            // 3.0초: "PRESS ANY KEY" 텍스트 점멸 시작
            .AppendInterval(1f)
            .AppendCallback(() => {
                if (_pressKeyText != null)
                {
                    _pressKeyText.alpha = 1f;
                    _pressKeyText.DOFade(0f, 1.5f).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
                }
            })
            // 4.0초: 1초 후부터 키 입력 활성화
            .AppendInterval(1f)
            .OnComplete(() => {
                _keyInputEnabled = true;
                _isSequencePlaying = false;
            });
    }
}
