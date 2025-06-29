using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    // UIManager는 이제 GameManager에 의해 관리되므로 싱글톤이 아닙니다.

    // Type을 Key로 사용하여, 모든 UI 인스턴스를 저장하는 딕셔너리
    private Dictionary<Type, UiBase> _uiDictionary = new();

    private void Awake()
    {      
        // UIManager의 자식으로 있는 모든 UI들을 자동으로 찾아 초기화 및 등록
        UiBase[] allUIs = GetComponentsInChildren<UiBase>(true); // 비활성화된 자식도 포함
        foreach (UiBase ui in allUIs)
        {
            ui.Init(); // 각 UI의 초기화 함수 호출
        }
    }
    private void Start()
    {
        // GameManager에 자신을 등록
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterUIManager(this);
        }
        else
        {
            Debug.LogError("[UIManager] UIManager가 씬에 존재하지 않습니다!");
        }
    }
    /// <summary>
    /// 딕셔너리에 UI를 등록합니다. 각 UI의 Init()에서 호출됩니다.
    /// </summary>
    public void Add<T>(UiBase ui) where T : UiBase
    {
        Type key = typeof(T);
        if (!_uiDictionary.ContainsKey(key))
        {
            _uiDictionary.Add(key, ui);
        }
    }

    /// <summary>
    /// 특정 타입의 UI를 찾아 반환합니다.
    /// </summary>
    public T Get<T>() where T : UiBase
    {
        Type key = typeof(T);
        if (_uiDictionary.TryGetValue(key, out UiBase ui))
        {
            return ui as T;
        }
        return null;
    }

    /// <summary>
    /// 특정 타입의 UI를 보여주거나 숨깁니다.
    /// </summary>
    public void Show<T>(bool show) where T : UiBase
    {
        Type key = typeof(T);
        if (_uiDictionary.TryGetValue(key, out UiBase ui))
        {
            ui.Show(show);
        }
    }
    /// <summary>
    /// 특정 타입의 UI를 숨깁니다. Show<T>(false)와 동일한 기능입니다.
    /// </summary>
    public void Hide<T>() where T : UiBase
    {
        // 내부적으로는 Show<T>(false)를 호출하여 코드를 재사용합니다.
        Show<T>(false);
    }
}
