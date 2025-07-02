using UnityEngine;
using TMPro;
using System.Collections; // 코루틴을 사용하기 위해 필요합니다.

public class EndingUI : UiBase
{
    [SerializeField] private TextMeshProUGUI _endingMessageText;
    [SerializeField] private ParticleSystem _backgroundParticles;

    [Header("연출 시간 설정")]
    [SerializeField] private float _fadeInDuration = 1f;
    [SerializeField] private float _fadeOutDuration = 1f;
    [SerializeField] private float _messageHoldDuration = 3f;

    private string[] _messages = { "고생했어", "지금까지 잘해왔어..." };

    public override void Init()
    {
        GameManager.Instance.UIManager.Add<EndingUI>(this);
    }

    public override void Show(bool show)
    {
        base.Show(show);
        if (show)
        {
            if (_backgroundParticles != null) _backgroundParticles.Play();
            // 연출 코루틴을 시작합니다.
            StartCoroutine(StartEndingSequence());
        }
    }

    // 엔딩 연출을 순서대로 진행하는 코루틴
    private IEnumerator StartEndingSequence()
    {
        _endingMessageText.text = ""; // 텍스트 초기화

        foreach (string msg in _messages)
        {
            // 1. 텍스트를 먼저 설정하고, Fade In 코루틴을 실행합니다.
            _endingMessageText.text = msg;
            yield return StartCoroutine(FadeTextAlpha(1f, _fadeInDuration)); // 100% 보이게

            // 2. 3초 동안 대기합니다.
            yield return new WaitForSeconds(_messageHoldDuration);

            // 3. Fade Out 코루틴을 실행합니다.
            yield return StartCoroutine(FadeTextAlpha(0f, _fadeOutDuration)); // 0% 투명하게
        }

        // 4. 모든 연출이 끝나면 크레딧 UI를 보여줍니다.
        GameManager.Instance.UIManager.Show<CreditUI>(true);
        GameManager.Instance.UIManager.Hide<EndingUI>();
    }

    // 텍스트의 알파(투명도) 값을 부드럽게 변경하는 코루틴
    private IEnumerator FadeTextAlpha(float targetAlpha, float duration)
    {
        float timer = 0f;
        float startAlpha = _endingMessageText.alpha;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            // 시간에 따라 시작 알파값에서 목표 알파값으로 점진적으로 변경
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, timer / duration);
            _endingMessageText.alpha = newAlpha;
            yield return null; // 다음 프레임까지 대기
        }

        _endingMessageText.alpha = targetAlpha; // 마지막에 목표값으로 확실하게 설정
    }
}
