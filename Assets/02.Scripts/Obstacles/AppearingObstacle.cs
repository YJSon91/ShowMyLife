using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 플레이어가 닿으면 자식 오브젝트의 모델링이 활성화되는 발판
/// </summary>
public class AppearingObstacle : BaseObstacle
{
    [Header("활성화 설정")]
    [Tooltip("활성화할 자식 오브젝트들")]
    [SerializeField] private List<GameObject> targetChildren = new List<GameObject>();
    
    [Header("사용 제한")]
    [Tooltip("최대 활성화 가능 횟수 (0 = 무제한)")]
    [SerializeField] private int maxActivationCount = 3;
    
    private bool isVisible = false;
    private int activationCount = 0;
    
    private void Start()
    {
        // 시작 시 모든 타겟 오브젝트 비활성화
        foreach (var child in targetChildren)
        {
            if (child != null)
            {
                child.SetActive(false);
            }
        }
    }
    
    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        
        // 플레이어가 감지되면 활성화
        if (IsPlayerObject(other.gameObject) && !isVisible)
        {
            // 최대 활성화 횟수 확인
            if (maxActivationCount <= 0 || activationCount < maxActivationCount)
            {
                ActivateChildren();
                activationCount++;
                
                // 최대 횟수에 도달했는지 로그 출력
                if (maxActivationCount > 0 && activationCount >= maxActivationCount)
                {
                    Debug.Log($"발판 최대 활성화 횟수({maxActivationCount}회)에 도달했습니다.");
                }
            }
        }
    }
    
    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);
        
        // 플레이어가 감지되면 활성화 (최대 횟수 제한 확인)
        if (IsPlayerObject(collision.gameObject) && !isVisible)
        {
            // 최대 활성화 횟수 확인
            if (maxActivationCount <= 0 || activationCount < maxActivationCount)
            {
                ActivateChildren();
                activationCount++;
                
                // 최대 횟수에 도달했는지 로그 출력
                if (maxActivationCount > 0 && activationCount >= maxActivationCount)
                {
                    Debug.Log($"발판 최대 활성화 횟수({maxActivationCount}회)에 도달했습니다.");
                }
            }
        }
    }
    
    /// <summary>
    /// 자식 오브젝트 활성화
    /// </summary>
    private void ActivateChildren()
    {
        // 모든 자식 오브젝트 활성화
        foreach (var child in targetChildren)
        {
            if (child != null)
            {
                child.SetActive(true);
            }
        }
        
        isVisible = true;
        Debug.Log("발판 활성화됨");
    }
    
    /// <summary>
    /// 자식 오브젝트 비활성화
    /// </summary>
    private void DeactivateChildren()
    {
        // 모든 자식 오브젝트 비활성화
        foreach (var child in targetChildren)
        {
            if (child != null)
            {
                child.SetActive(false);
            }
        }
        
        isVisible = false;
        Debug.Log("발판 비활성화됨");
    }
    
    /// <summary>
    /// 외부에서 발판을 강제로 활성화
    /// </summary>
    public void ForceAppear()
    {
        if (!isVisible)
        {
            // 최대 활성화 횟수 확인
            if (maxActivationCount <= 0 || activationCount < maxActivationCount)
            {
                ActivateChildren();
                activationCount++;
            }
        }
    }
    
    /// <summary>
    /// 외부에서 발판을 강제로 비활성화
    /// </summary>
    public void ForceDisappear()
    {
        if (isVisible)
        {
            DeactivateChildren();
        }
    }
    
    /// <summary>
    /// 활성화 횟수 초기화
    /// </summary>
    public void ResetActivationCount()
    {
        activationCount = 0;
    }
    
    /// <summary>
    /// 현재 발판이 활성화 상태인지 확인
    /// </summary>
    public bool IsActive()
    {
        return isVisible;
    }
} 