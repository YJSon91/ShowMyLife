 using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Video;
using System.Collections.Generic;

public class EndingUI : UiBase
{
    [SerializeField] private TextMeshProUGUI _endingMessageText;
    [SerializeField] private ParticleSystem _backgroundParticles;
    [SerializeField] private VideoPlayer _madMoviePlayer;
    private List<string> _videoClipPaths = new List<string>();
    private int _currentClipIndex = 0;

    [Header("연출 시간 설정")]
    [SerializeField] private float _fadeInDuration = 1f;
    [SerializeField] private float _fadeOutDuration = 1f;
    [SerializeField] private float _messageHoldDuration = 3f;


    private string[] _messages = { "고생했어", "지금까지 잘해왔어..." };

    public override void Init()
    {
        GameManager.Instance.UIManager.Add<EndingUI>(this);
        _madMoviePlayer.loopPointReached += OnVideoFinished;
    }

    public override void Show(bool show)
    {
        base.Show(show);
        if (show)
        {           
                FindAllVideoClips();
                PlayNextClip();
                if (_backgroundParticles != null) _backgroundParticles.Play();
                // 연출 코루틴을 시작합니다.
                StartCoroutine(StartEndingSequence());                
        }
    }
    // 저장된 모든 비디오 클립 파일의 경로를 찾아 리스트에 추가합니다.
    private void FindAllVideoClips()
    {
        _videoClipPaths.Clear();
        // Unity Recorder가 영상을 저장하는 기본 경로
        // "C:/Users/[사용자이름]/AppData/LocalLow/[회사이름]/[게임이름]/" 와 같은 경로입니다.
        string savePath = Application.persistentDataPath;

        // 해당 경로에 파일이 있는지 확인합니다.
        if (System.IO.Directory.Exists(savePath))
        {
            // "Clip_*.mp4" 패턴을 가진 모든 파일의 경로를 찾아 리스트에 추가합니다.
            _videoClipPaths.AddRange(System.IO.Directory.GetFiles(savePath, "Recording_*.mp4"));
            //Debug.Log($"[EndingUI] {_videoClipPaths.Count}개의 녹화 클립을 찾았습니다.");
        }
        else
        {
            //Debug.LogWarning($"[EndingUI] 녹화 파일 저장 경로를 찾을 수 없습니다: {savePath}");
        }
    }
    // 다음 비디오 클립을 재생합니다.
    private void PlayNextClip()
    {
        if (_videoClipPaths.Count > _currentClipIndex)
        {
            _madMoviePlayer.url = _videoClipPaths[_currentClipIndex];
            _madMoviePlayer.Play();
            _currentClipIndex++;
        }
        else
        {
            // 모든 영상 재생이 끝나면 메인 메뉴로 돌아가는 등의 로직
            //Debug.Log("모든 매드무비 클립 재생 완료.");
        }
    }
    // 비디오 재생이 끝났을 때 호출되는 이벤트 핸들러
    private void OnVideoFinished(VideoPlayer source)
    {
        // 다음 클립을 이어서 재생합니다.
        PlayNextClip();
    }

    private void OnDestroy()
    {
        // 오브젝트 파괴 시 이벤트 구독을 해제합니다.
        if (_madMoviePlayer != null)
        {
            _madMoviePlayer.loopPointReached -= OnVideoFinished;
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

