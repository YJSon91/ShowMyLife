using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 플레이어 그림자 시스템을 전역적으로 관리하는 매니저
/// </summary>
public class PlayerShadowManager : MonoBehaviour
{
    [Header("그림자 시스템 설정")]
    [SerializeField] private bool enableShadows = true;
    [SerializeField] private LayerMask groundLayerMask = 1;
    
    [Header("기본 그림자 설정")]
    [SerializeField] private float defaultShadowScale = 1f;
    [SerializeField] private float defaultShadowAlpha = 0.6f;
    [SerializeField] private float defaultMaxDistance = 20f;
    
    [Header("그림자 품질")]
    [SerializeField] private bool useHighQualityShadows = false;
    [SerializeField] private float shadowFadeSpeed = 5f;
    
    private static PlayerShadowManager _instance;
    public static PlayerShadowManager Instance => _instance;
    
    private List<PlayerShadowProjector> activeShadows = new List<PlayerShadowProjector>();
    private bool isInitialized = false;
    
    // 이벤트
    public System.Action<bool> OnShadowSystemToggled;
    public System.Action<float> OnShadowQualityChanged;
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        InitializeShadowSystem();
    }
    
    private void Start()
    {
        // GameManager에 등록
        if (GameManager.Instance != null)
        {
            // GameManager에 PlayerShadowManager 등록 로직이 있다면 여기에 추가
        }
        
        // 씬의 모든 그림자 찾기
        FindAllShadowsInScene();
    }
    
    private void InitializeShadowSystem()
    {
        if (isInitialized) return;
        
        // 기본 설정 적용
        ApplyDefaultSettings();
        
        isInitialized = true;
        Debug.Log("[PlayerShadowManager] 그림자 시스템 초기화 완료");
    }
    
    private void ApplyDefaultSettings()
    {
        // 기본 설정을 모든 활성 그림자에 적용
        foreach (var shadow in activeShadows)
        {
            if (shadow != null)
            {
                shadow.SetShadowScale(defaultShadowScale);
                shadow.SetShadowAlpha(defaultShadowAlpha);
                shadow.SetMaxShadowDistance(defaultMaxDistance);
                shadow.SetShadowFadeSpeed(shadowFadeSpeed);
            }
        }
    }
    
    private void FindAllShadowsInScene()
    {
        // 씬의 모든 PlayerShadowProjector 찾기
        PlayerShadowProjector[] shadows = FindObjectsOfType<PlayerShadowProjector>();
        
        foreach (var shadow in shadows)
        {
            if (shadow != null && !activeShadows.Contains(shadow))
            {
                activeShadows.Add(shadow);
                ConfigureShadow(shadow);
            }
        }
        
        Debug.Log($"[PlayerShadowManager] {activeShadows.Count}개의 그림자 발견");
    }
    
    private void ConfigureShadow(PlayerShadowProjector shadow)
    {
        if (shadow == null) return;
        
        // 기본 설정 적용
        shadow.SetShadowScale(defaultShadowScale);
        shadow.SetShadowAlpha(defaultShadowAlpha);
        shadow.SetMaxShadowDistance(defaultMaxDistance);
        shadow.SetShadowFadeSpeed(shadowFadeSpeed);
        
        // 그림자 활성화/비활성화
        shadow.SetShadowEnabled(enableShadows);
    }
    
    /// <summary>
    /// 그림자 시스템 전체 활성화/비활성화
    /// </summary>
    public void SetShadowSystemEnabled(bool enabled)
    {
        enableShadows = enabled;
        
        foreach (var shadow in activeShadows)
        {
            if (shadow != null)
            {
                shadow.SetShadowEnabled(enabled);
            }
        }
        
        OnShadowSystemToggled?.Invoke(enabled);
        Debug.Log($"[PlayerShadowManager] 그림자 시스템 {(enabled ? "활성화" : "비활성화")}");
    }
    
    /// <summary>
    /// 모든 그림자의 투명도 설정
    /// </summary>
    public void SetAllShadowsAlpha(float alpha)
    {
        defaultShadowAlpha = Mathf.Clamp01(alpha);
        
        foreach (var shadow in activeShadows)
        {
            if (shadow != null)
            {
                shadow.SetShadowAlpha(defaultShadowAlpha);
            }
        }
        
        Debug.Log($"[PlayerShadowManager] 모든 그림자 투명도를 {alpha}로 설정");
    }
    
    /// <summary>
    /// 모든 그림자의 크기 설정
    /// </summary>
    public void SetAllShadowsScale(float scale)
    {
        defaultShadowScale = Mathf.Max(0.1f, scale);
        
        foreach (var shadow in activeShadows)
        {
            if (shadow != null)
            {
                shadow.SetShadowScale(defaultShadowScale);
            }
        }
        
        Debug.Log($"[PlayerShadowManager] 모든 그림자 크기를 {scale}로 설정");
    }
    
    /// <summary>
    /// 모든 그림자의 최대 거리 설정
    /// </summary>
    public void SetAllShadowsMaxDistance(float distance)
    {
        defaultMaxDistance = Mathf.Max(1f, distance);
        
        foreach (var shadow in activeShadows)
        {
            if (shadow != null)
            {
                shadow.SetMaxShadowDistance(defaultMaxDistance);
            }
        }
        
        Debug.Log($"[PlayerShadowManager] 모든 그림자 최대 거리를 {distance}로 설정");
    }
    
    /// <summary>
    /// 그림자 품질 설정
    /// </summary>
    public void SetShadowQuality(bool highQuality)
    {
        useHighQualityShadows = highQuality;
        
        foreach (var shadow in activeShadows)
        {
            if (shadow != null)
            {
                // 그림자 품질에 따른 설정 조정
                if (highQuality)
                {
                    shadow.SetShadowFadeSpeed(shadowFadeSpeed * 1.5f);
                }
                else
                {
                    shadow.SetShadowFadeSpeed(shadowFadeSpeed);
                }
            }
        }
        
        OnShadowQualityChanged?.Invoke(highQuality ? 1f : 0.5f);
        Debug.Log($"[PlayerShadowManager] 그림자 품질을 {(highQuality ? "고품질" : "표준")}로 설정");
    }
    
    /// <summary>
    /// 새로운 그림자 등록
    /// </summary>
    public void RegisterShadow(PlayerShadowProjector shadow)
    {
        if (shadow != null && !activeShadows.Contains(shadow))
        {
            activeShadows.Add(shadow);
            ConfigureShadow(shadow);
            Debug.Log($"[PlayerShadowManager] 새로운 그림자 등록: {shadow.name}");
        }
    }
    
    /// <summary>
    /// 그림자 등록 해제
    /// </summary>
    public void UnregisterShadow(PlayerShadowProjector shadow)
    {
        if (activeShadows.Contains(shadow))
        {
            activeShadows.Remove(shadow);
            Debug.Log($"[PlayerShadowManager] 그림자 등록 해제: {shadow.name}");
        }
    }
    
    /// <summary>
    /// 현재 활성화된 그림자 개수 반환
    /// </summary>
    public int GetActiveShadowCount()
    {
        return activeShadows.Count;
    }
    
    /// <summary>
    /// 그림자 시스템 상태 정보 반환
    /// </summary>
    public bool IsShadowSystemEnabled => enableShadows;
    public bool IsHighQualityEnabled => useHighQualityShadows;
    public float CurrentShadowAlpha => defaultShadowAlpha;
    public float CurrentShadowScale => defaultShadowScale;
    
    // 디버그 정보 표시
    private void OnGUI()
    {
        if (!Application.isPlaying) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("Player Shadow Manager", GUI.skin.box);
        
        GUILayout.Label($"활성 그림자: {GetActiveShadowCount()}");
        GUILayout.Label($"시스템 활성화: {enableShadows}");
        GUILayout.Label($"고품질 모드: {useHighQualityShadows}");
        GUILayout.Label($"투명도: {defaultShadowAlpha:F2}");
        GUILayout.Label($"크기: {defaultShadowScale:F2}");
        
        if (GUILayout.Button("그림자 시스템 토글"))
        {
            SetShadowSystemEnabled(!enableShadows);
        }
        
        if (GUILayout.Button("품질 토글"))
        {
            SetShadowQuality(!useHighQualityShadows);
        }
        
        GUILayout.EndArea();
    }
}
