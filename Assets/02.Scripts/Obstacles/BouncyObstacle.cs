using UnityEngine;
using DG.Tweening;
using System.Collections;

/// <summary>
/// 플레이어가 부딪히면 반대 방향으로 튕겨내는 장애물
/// </summary>
public class BouncyObstacle : BaseObstacle
{
    [Header("튕겨내기 설정")]
    [Tooltip("튕겨내는 힘의 크기")]
    [SerializeField] private float bounceForce = 10f;

    [Tooltip("튕겨낼 때 Y축 방향으로 추가되는 힘 (양수면 위로, 음수면 아래로)")]
    [SerializeField] private float upwardForce = 2f;

    [Tooltip("튕겨내는 효과가 지속되는 시간 (초)")]
    [SerializeField] private float bounceDuration = 0.5f;

    [Tooltip("튕겨내는 동안 플레이어 입력 무시 정도 (0: 완전 제어 가능, 1: 완전 제어 불가)")]
    [SerializeField] private float inputReduction = 0.8f;

    [Tooltip("튕겨내기 효과 종료 시 감속 시간 (초)")]
    [SerializeField] private float decelerationDuration = 0.3f;

    [Tooltip("튕겨내기 쿨타임 (초)")]
    [SerializeField] private float cooldownTime = 0.5f;

    [Tooltip("튕겨내기 효과 곡선 (비어있으면 기본 OutQuad 사용)")]
    [SerializeField] private AnimationCurve bounceCurve;

    [Header("플레이어 입력 제어")]
    [Tooltip("플레이어 입력을 비활성화할지 여부")]
    [SerializeField] private bool disablePlayerInput = true;

    [Tooltip("플레이어 입력을 비활성화하는 시간 (초)")]
    [SerializeField] private float inputDisableDuration = 0.5f;

    // 현재 활성화된 트윈 저장용
    private Tween _currentBounceTween;
    
    // 마지막 튕겨내기 시간
    private float _lastBounceTime = -10f;

    private void Start()
    {
        // 효과 곡선이 비어있으면 기본 곡선 생성
        if (bounceCurve.keys.Length == 0)
        {
            bounceCurve = new AnimationCurve(
                new Keyframe(0, 1, 0, -2),
                new Keyframe(1, 0, 0, 0)
            );
        }
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);

        if (!collision.gameObject.CompareTag("Player")) return;
        
        // 쿨타임 체크
        if (Time.time - _lastBounceTime < cooldownTime) return;
        _lastBounceTime = Time.time;

        // 플레이어 컴포넌트 가져오기
        Player player = collision.gameObject.GetComponent<Player>();
        if (player == null)
        {
            player = collision.gameObject.GetComponentInParent<Player>();
            if (player == null) return;
        }

        // 충돌 정보에서 반발 방향 계산
        Vector3 bounceDirection = Vector3.zero;
        Vector3 contactPoint = Vector3.zero;
        
        // 모든 접촉점 중 첫 번째 것 사용 (또는 평균 계산 가능)
        if (collision.contacts.Length > 0)
        {
            ContactPoint contact = collision.contacts[0];
            contactPoint = contact.point;
            
            // 충돌 법선의 반대 방향 사용 (플레이어가 부딪힌 방향)
            bounceDirection = -contact.normal;
            
            Debug.Log($"충돌 지점: {contactPoint}, 법선 벡터: {contact.normal}, 반발 방향: {bounceDirection}");
        }
        else
        {
            // 장애물에서 플레이어 방향의 반대 방향
            bounceDirection = (transform.position - player.transform.position).normalized;
            Debug.Log($"접촉점 없음, 계산된 반발 방향: {bounceDirection}");
        }
        
        // 수평 방향만 사용하고 정규화
        Vector3 horizontalBounceDir = new Vector3(bounceDirection.x, 0, bounceDirection.z).normalized;
        
        // 최종 튕겨내기 방향 (수평 방향 + 상향 힘)
        Vector3 finalBounceDir = horizontalBounceDir + Vector3.up * upwardForce;
        finalBounceDir.Normalize();
        
        // 기존 트윈이 있으면 중단
        if (_currentBounceTween != null && _currentBounceTween.IsActive())
        {
            _currentBounceTween.Kill();
        }
        
        // 플레이어 속도 초기화 (선택적)
        player.MovementController.Rigidbody.velocity = Vector3.zero;
        
        // 플레이어 입력 비활성화 (옵션에 따라)
        if (disablePlayerInput && player.InputReader != null)
        {
            StartCoroutine(DisablePlayerInputTemporarily(player.InputReader));
        }
        
        // 플레이어에게 미끄럼틀 효과 적용 (ActivateObstacleSlide 사용)
        player.MovementController.ActivateObstacleSlide(finalBounceDir, bounceForce, 0.5f, inputReduction);
        
        Debug.Log($"플레이어 튕겨내기 시작 - 방향: {finalBounceDir}, 힘: {bounceForce}");
        
        // DOTween을 사용하여 시간에 따라 튕겨내기 힘 감소
        _currentBounceTween = DOVirtual.Float(bounceForce, 0f, bounceDuration, (force) => {
            // 현재 힘 업데이트
            player.MovementController.UpdateObstacleSlideSpeed(force);
        }).SetEase(bounceCurve)
        .OnComplete(() => {
            // 튕겨내기 효과 종료
            player.MovementController.DeactivateObstacleSlide();
            Debug.Log("플레이어 튕겨내기 종료");
        });
    }

    /// <summary>
    /// 일정 시간 동안 플레이어 입력을 비활성화합니다
    /// </summary>
    private IEnumerator DisablePlayerInputTemporarily(InputReader inputReader)
    {
        // 입력 비활성화
        inputReader.DisableInput();
        Debug.Log($"플레이어 입력 비활성화: {inputDisableDuration}초 동안");
        
        // 지정된 시간 동안 대기
        yield return new WaitForSeconds(inputDisableDuration);
        
        // 입력 다시 활성화
        inputReader.EnableInput();
        Debug.Log("플레이어 입력 다시 활성화됨");
    }

    private void OnDestroy()
    {
        // 안전하게 트윈 정리
        if (_currentBounceTween != null && _currentBounceTween.IsActive())
        {
            _currentBounceTween.Kill();
            _currentBounceTween = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 튕겨내기 효과 범위를 시각화
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, 0.5f);
        
        // 콜라이더가 있으면 그 크기도 표시
        Collider collider = GetComponent<Collider>();
        if (collider != null && collider is BoxCollider)
        {
            BoxCollider boxCollider = collider as BoxCollider;
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.1f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(boxCollider.center, boxCollider.size);
        }
    }
} 