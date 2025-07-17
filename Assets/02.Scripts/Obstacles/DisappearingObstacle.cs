using System.Collections;
using UnityEngine;
using DG.Tweening;

public class DisappearingObstacle : BaseObstacle
{
    [Header("작동 방식 설정")]
    [SerializeField] private bool useAutoLoop = false;

    [Header("시간 설정")]
    [Tooltip("사라지기 전 대기 시간")]
    [SerializeField] private float delayBeforeDisappear = 1f;
    [Tooltip("나타나기 전 대기 시간")]
    [SerializeField] private float delayBeforeReappear = 2f;
    [Tooltip("애니메이션 시간")]
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("흔들림 옵션")]
    [Tooltip("흔들릴 모델(자식) 오브젝트")]
    [SerializeField] private Transform shakeModel;
    [Tooltip("흔들림 XYZ 범위 (local)")]
    [SerializeField] private Vector3 shakeStrength = new Vector3(0.05f, 0.05f, 0.05f);
    [Tooltip("흔들림 속도")]
    [SerializeField] private float shakeVibrato = 20f;

    private Renderer rend;
    private Collider col;
    private Color originalColor;
    private bool isProcessing = false;
    private bool _wasPlayerOnPlatform = false;
    private Tween _shakeTween;
    private Vector3 _modelOriginPos;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        col = GetComponent<Collider>();

        if (rend != null)
        {
            rend.material = new Material(rend.material);
            originalColor = rend.material.color;
        }
        if (shakeModel != null)
            _modelOriginPos = shakeModel.localPosition;
    }

    private void Start()
    {
        if (useAutoLoop)
        {
            StartCoroutine(AutoLoopRoutine());
        }
    }

    private void Update()
    {
        if (!useAutoLoop && !isProcessing)
        {
            bool isOn = IsPlayerOnPlatform();

            if (isOn && !_wasPlayerOnPlatform)
            {
                StartCoroutine(DisappearRoutine());
            }
            _wasPlayerOnPlatform = isOn;
        }
    }

    private IEnumerator AutoLoopRoutine()
    {
        while (true)
        {
            // 자동 반복: 사라지기 전 2초 동안 흔들림
            if (shakeModel != null)
                StartShake();

            yield return new WaitForSeconds(2f);

            StopShake();
            yield return new WaitForSeconds(delayBeforeDisappear - 2f);

            Disappear();
            yield return new WaitForSeconds(delayBeforeReappear);

            Reappear();
        }
    }

    private IEnumerator DisappearRoutine()
    {
        isProcessing = true;

        // 사라지기 전 2초 동안 흔들림
        if (shakeModel != null)
            StartShake();

        yield return new WaitForSeconds(2f);

        StopShake();

        // 2초 뒤 ~ delayBeforeDisappear까지 대기
        float remain = delayBeforeDisappear - 2f;
        if (remain > 0)
            yield return new WaitForSeconds(remain);

        Disappear();

        yield return new WaitForSeconds(delayBeforeReappear);
        Reappear();

        isProcessing = false;
    }

    private void StartShake()
    {
        // DOTween의 DOShakePosition으로 XYZ축 랜덤하게 흔들림
        if (_shakeTween != null && _shakeTween.IsActive()) _shakeTween.Kill();
        if (shakeModel != null)
        {
            shakeModel.localPosition = _modelOriginPos;
            _shakeTween = shakeModel.DOShakePosition(
                2f,          // duration (2초)
                shakeStrength,
                Mathf.RoundToInt(shakeVibrato),
                90,          // randomness
                false,       // fade out
                true         // snapping
            ).SetEase(Ease.Linear);
        }
    }

    private void StopShake()
    {
        if (_shakeTween != null && _shakeTween.IsActive())
            _shakeTween.Kill();
        if (shakeModel != null)
            shakeModel.localPosition = _modelOriginPos;
    }

    private void Disappear()
    {
        if (rend != null)
        {
            rend.material.DOFade(0f, fadeDuration);
        }

        if (col != null)
        {
            col.enabled = false;
        }

        // 플래그를 강제로 해제!
        _playerOnPlatform = null;
        _playerRigidbody = null;
    }

    private void Reappear()
    {
        if (rend != null)
        {
            rend.material.DOFade(originalColor.a, fadeDuration);
        }

        if (col != null)
        {
            col.enabled = true;
        }

        // 재등장하면 흔들림 초기화
        StopShake();
    }

    private void OnDestroy()
    {
        StopShake();
    }
}
