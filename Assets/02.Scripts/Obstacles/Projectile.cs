using DG.Tweening;
using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("투사체 밀림 설정")]
    [Tooltip("플레이어에게 가할 힘의 크기")]
    [SerializeField] private float pushForce = 10f;
    [Tooltip("Y축(수직) 추가 힘")]
    [SerializeField] private float upwardForce = 2f;
    [Tooltip("밀림 지속 시간")]
    [SerializeField] private float pushDuration = 0.4f;
    [Tooltip("입력 저감(1=완전 불가, 0=완전 가능)")]
    [SerializeField] private float inputReduction = 0.8f;
    [Tooltip("밀림 종료 후 투사체 삭제까지의 딜레이(초)")]
    [SerializeField] private float destroyDelayAfterPush = 0.0f;
    [Tooltip("자동 삭제 시간(초)")]
    [SerializeField] private float lifeTime = 3f;
    [Tooltip("힘 감소 커브(없으면 Ease.OutQuad)")]
    [SerializeField] private AnimationCurve pushCurve;
    [Tooltip("플레이어 입력 비활성화")]
    [SerializeField] private bool disablePlayerInput = true;
    [Tooltip("입력 비활성화 시간")]
    [SerializeField] private float inputDisableDuration = 0.3f;

    [Header("사운드 설정")]
    [Tooltip("투사체 충돌 사운드 재생 여부")]
    [SerializeField] private bool playProjectileSound = true;

    [Tooltip("투사체 충돌 사운드 볼륨 (0~1)")]
    [SerializeField] private float projectileSoundVolume = 1.0f;

    [Tooltip("투사체 충돌 사운드 쿨타임 (초)")]
    [SerializeField] private float soundCooldownTime = 0.2f;

    private Tween _currentPushTween;
    private bool _hasPushed = false; // 1회만 발동
    private Coroutine _autoDestroyCoroutine;
    private PlayerMovementController _pushedPlayerController;
    
    // 마지막 사운드 재생 시간
    private float _lastSoundTime = -10f;
    // SoundManager 참조
    private SoundManager _soundManager;

    private void Start()
    {
        // 감속이 자연스러운 커브 기본값 제공 (인스펙터에서 없을 때만 세팅)
        if (pushCurve == null || pushCurve.keys.Length == 0)
            pushCurve = new AnimationCurve(
                new Keyframe(0f, 1f),     // 시작(100%)
                new Keyframe(0.1f, 0.6f), // 아주 빠르게 감소
                new Keyframe(0.3f, 0.25f),
                new Keyframe(0.6f, 0.08f),
                new Keyframe(1f, 0f)      // 끝(멈춤)
            );

        _autoDestroyCoroutine = StartCoroutine(AutoDestroyTimer());
        
        // SoundManager 참조 가져오기
        if (GameManager.Instance != null)
        {
            _soundManager = GameManager.Instance.SoundManager;
        }
    }

    private IEnumerator AutoDestroyTimer()
    {
        yield return new WaitForSeconds(lifeTime);
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_hasPushed) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        Player player = collision.gameObject.GetComponent<Player>()
                     ?? collision.gameObject.GetComponentInParent<Player>();
        if (player == null) return;

        _hasPushed = true;

        // 투사체 충돌 사운드 재생
        PlayProjectileSound();

        // 충돌 시 자동삭제 예약 해제
        if (_autoDestroyCoroutine != null)
        {
            StopCoroutine(_autoDestroyCoroutine);
            _autoDestroyCoroutine = null;
        }

        _pushedPlayerController = player.MovementController;

        // 투사체 진행 방향(velocity)으로 밀기 (y는 upwardForce 적용)
        Vector3 pushDir = GetComponent<Rigidbody>().velocity;
        pushDir.y = 0;
        if (pushDir.sqrMagnitude < 0.01f)
            pushDir = transform.forward; // 예외 fallback

        pushDir.Normalize();
        Vector3 finalPushDir = (pushDir + Vector3.up * upwardForce).normalized;

        // 이전 트윈이 있으면 Kill
        _currentPushTween?.Kill();

        // 플레이어 속도 초기화(물리)
        player.MovementController.Rigidbody.velocity = Vector3.zero;

        // 플레이어 입력 잠시 제한 (선택)
        if (disablePlayerInput && player.InputReader != null)
            StartCoroutine(DisablePlayerInputTemporarily(player.InputReader));

        // Slide 효과 시작
        player.MovementController.ActivateObstacleSlide(finalPushDir, pushForce, pushDuration, inputReduction);

        // DOTween을 통해 힘 점진적 감소 → 연출 끝나면 투사체 삭제
        _currentPushTween = DOVirtual.Float(pushForce, 0f, pushDuration, (force) => {
            _pushedPlayerController.UpdateObstacleSlideSpeed(force);
        })
        .SetEase(pushCurve) // 곡선 적용!
        .OnComplete(() => {
            _pushedPlayerController.DeactivateObstacleSlide();
            StartCoroutine(DestroyAfterDelay(1.0f));
        });
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    private IEnumerator DisablePlayerInputTemporarily(InputReader inputReader)
    {
        inputReader.DisableInput();
        yield return new WaitForSeconds(inputDisableDuration);
        inputReader.EnableInput();
    }

    private void OnDestroy()
    {
        _currentPushTween?.Kill();
        _currentPushTween = null;

        // 혹시라도 슬라이드 효과가 남아있는 상황 방지
        _pushedPlayerController?.DeactivateObstacleSlide();
        _pushedPlayerController = null;
    }

    /// <summary>
    /// 투사체 충돌 사운드 재생
    /// </summary>
    private void PlayProjectileSound()
    {
        // 사운드 재생이 비활성화되어 있으면 무시
        if (!playProjectileSound) return;

        // 개별 쿨타임 체크 제거 (SoundManager에서 전역 관리)
        // if (Time.time - _lastSoundTime < soundCooldownTime) return;
        // _lastSoundTime = Time.time;

        // SoundManager가 있으면 사운드 재생 (쿨타임은 SoundManager에서 관리)
        if (_soundManager != null)
        {
            _soundManager.PlaySFX(SfxType.Projectile, projectileSoundVolume);
            Debug.Log($"[Projectile] 투사체 충돌 사운드 재생 요청 (볼륨: {projectileSoundVolume})");
        }
        else
        {
            Debug.LogWarning("[Projectile] SoundManager를 찾을 수 없어 사운드를 재생할 수 없습니다.");
        }
    }
}
