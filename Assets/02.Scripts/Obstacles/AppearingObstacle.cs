using System.Collections;
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
    
    private bool isVisible = false;
    
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
            ActivateChildren();
        }
    }
    
    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);
        
        // 플레이어가 감지되면 활성화
        if (IsPlayerObject(collision.gameObject) && !isVisible)
        {
            ActivateChildren();
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
    }
    
    /// <summary>
    /// 외부에서 발판을 강제로 활성화
    /// </summary>
    public void ForceAppear()
    {
        if (!isVisible)
        {
            ActivateChildren();
        }
    }
    
    /// <summary>
    /// 외부에서 발판을 강제로 비활성화
    /// </summary>
    public void ForceDisappear()
    {
        if (isVisible)
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
        }
    }
} 