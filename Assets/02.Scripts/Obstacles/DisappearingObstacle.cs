using System.Collections;
using UnityEngine;
using DG.Tweening;

public class DisappearingObstacle : MonoBehaviour
{
    [Header("작동 방식 설정")]
    [Tooltip("true면 자동 사라짐 반복, false면 플레이어 밟을 때 작동")]
    [SerializeField] private bool useAutoLoop = false;

    [Header("시간 설정")]
    [Tooltip("사라지기 전 대기 시간")]
    [SerializeField] private float delayBeforeDisappear = 1f;
    [Tooltip("나타나기 전 대기 시간")]
    [SerializeField] private float delayBeforeReappear = 2f;
    [Tooltip("애니메이션 시간")]
    [SerializeField] private float fadeDuration = 0.5f;

    private Renderer rend;
    private Collider col;
    private Color originalColor;
    private bool isProcessing = false;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        col = GetComponent<Collider>();

        if (rend != null)
        {
            rend.material = new Material(rend.material);
            originalColor = rend.material.color;
        }
    }

    private void Start()
    {
        if (useAutoLoop)
        {
            StartCoroutine(AutoLoopRoutine());
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!useAutoLoop && !isProcessing && collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(DisappearRoutine());
        }
    }

    private IEnumerator AutoLoopRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(delayBeforeDisappear);
            Disappear();

            yield return new WaitForSeconds(delayBeforeReappear);
            Reappear();
        }
    }

    private IEnumerator DisappearRoutine()
    {
        isProcessing = true;

        yield return new WaitForSeconds(delayBeforeDisappear);
        Disappear();

        yield return new WaitForSeconds(delayBeforeReappear);
        Reappear();

        isProcessing = false;
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
    }
}
