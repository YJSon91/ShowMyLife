using UnityEngine;
using TMPro;
using DG.Tweening; 
using UnityEngine.UI; 

public class IntroUIFlow : MonoBehaviour
{
    [Header("관리할 UI 패널")]
    [SerializeField] private GameObject _startPanel;
    [SerializeField] private GameObject _mainMenuPanel;

    [Header("시작 화면 구성 요소")]
    [SerializeField] private TextMeshProUGUI _pressKeyText;
    [SerializeField] private Image _fadePanel; 

    [Header("연출 시간 설정")]
    [SerializeField] private float _fadeDuration = 0.5f;

    private bool _keyHasBeenPressed = false;
    private Tween _blinkingTween; // 텍스트 점멸 애니메이션을 제어할 변수

    void Start()
    {
        // 시작 시 페이드 패널을 완전히 투명하게 만듭니다.
        if (_fadePanel != null)
        {
            Color fadeColor = _fadePanel.color;
            fadeColor.a = 0;
            _fadePanel.color = fadeColor;
        }

        // 'PRESS ANY KEY' 텍스트 깜빡임 효과 시작
        StartBlinking();
    }

    void Update()
    {
        // 아직 키가 눌리지 않았고, 아무 키나 눌렸다면
        if (!_keyHasBeenPressed && Input.anyKeyDown)
        {
            _keyHasBeenPressed = true;

            // 진행 중이던 점멸 애니메이션을 멈춥니다.
            if (_blinkingTween != null)
            {
                _blinkingTween.Kill();
                _pressKeyText.alpha = 1f; // 텍스트를 완전히 보이게 고정
            }

            // 화면 전환 연출 시작
            TransitionToMainMenu();
        }
    }

    // DOTween으로 텍스트를 깜빡이게 하는 함수
    private void StartBlinking()
    {
        // _pressKeyText의 알파(투명도) 값을 0으로 0.8초 동안 변경했다가,
        // 다시 원래대로 돌아오는 것을 무한 반복 (Yoyo)
        _blinkingTween = _pressKeyText.DOFade(0f, 0.8f)
                                     .SetEase(Ease.InOutQuad)
                                     .SetLoops(-1, LoopType.Yoyo);
    }

    // DOTween의 Sequence 기능으로 화면 전환 연출을 만드는 함수
    private void TransitionToMainMenu()
    {
        Debug.Log("키 입력 감지! DOTween으로 메인 메뉴 전환을 시작합니다...");

        // 여러 애니메이션을 순서대로 실행하기 위해 시퀀스를 생성합니다.
        Sequence transitionSequence = DOTween.Sequence();

        // 1. 검은 화면으로 덮습니다 (Fade-in)
        transitionSequence.Append(_fadePanel.DOFade(1f, _fadeDuration));

        // 2. 검은 화면이 된 직후, 패널들을 교체합니다.
        transitionSequence.AppendCallback(() => {
            _startPanel.SetActive(false);
            _mainMenuPanel.SetActive(true);
        });

        // 3. 다시 밝아지며 메인 메뉴를 보여줍니다 (Fade-out)
        transitionSequence.Append(_fadePanel.DOFade(0f, _fadeDuration));
    }
}
