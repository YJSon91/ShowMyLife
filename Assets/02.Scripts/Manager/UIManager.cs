using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    // UIManager는 이제 GameManager에 의해 관리되므로 싱글톤이 아닙니다.

    // Type을 Key로 사용하여, 모든 UI 인스턴스를 저장하는 딕셔너리
    private Dictionary<Type, UiBase> _uiDictionary = new();

    private void Start()
    {
        // 1. GameManager에 자신을 등록
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterUIManager(this);
        }
        else
        {
            Debug.LogError("[UIManager] GameManager가 씬에 존재하지 않습니다!");
            return;
        }

        // 2. 자식 UI들을 찾아 초기화 및 등록
        UiBase[] allUIs = GetComponentsInChildren<UiBase>(true);
        foreach (UiBase ui in allUIs)
        {
            ui.Init();
        }

        // 3. 시작 시점의 게임 상태에 맞게 UI를 즉시 설정
        if (GameManager.Instance != null)
        {
            HandleGameStateChanged(GameManager.Instance.CurrentState);
        }
    }

    private void OnEnable()
    {
        // GameManager의 상태 변경 이벤트를 구독합니다.
        GameManager.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        // 오브젝트가 파괴될 때, 이벤트 구독을 반드시 해제합니다.
        GameManager.OnGameStateChanged -= HandleGameStateChanged;
    }
    /// <summary>
    /// GameManager로부터 게임 상태 변경 신호를 받았을 때 호출되는 핵심 함수입니다.
    /// </summary>
    private void HandleGameStateChanged(GameManager.GameState newState)
    {
        // 일시정지 상태가 되거나 풀릴 때는 다른 UI를 끄지 않도록 예외 처리
        Debug.Log($"<color=yellow>[{Time.time:.2f}초] UIManager가 {newState} 상태를 감지했습니다.</color>");
        if (newState == GameManager.GameState.Paused)
        {
            Show<PauseMenu>(true);
            return;
        }
        if (GameManager.Instance.CurrentState == GameManager.GameState.Paused && newState == GameManager.GameState.Playing)
        {
            Hide<PauseMenu>();
            return;
        }

        // 그 외의 상태 변경 시에는 모든 패널을 숨기고 시작합니다.
        HideAll();
        Show<FadePanelUI>(true); // 페이드 패널을 활성화합니다.

        // 새로운 상태에 맞는 UI만 활성화합니다.
        switch (newState)
        {
            case GameManager.GameState.Start:
                Show<StartPanelUI>(true);
                break;

            case GameManager.GameState.Tutorial:
                Show<TutorialPanelUI>(true);
                break;

            case GameManager.GameState.MainMenu:
                // 메인 메뉴로 전환 시, 페이드 연출을 사용합니다.
                var fadePanel = Get<FadePanelUI>();
                if (fadePanel != null)
                {
                    // 화면을 어둡게 한 뒤, 패널을 교체하고 다시 밝게 합니다.
                    fadePanel.FadeIn(0.5f, () => {
                        Hide<StartPanelUI>();
                        Show<MainMenu>(true);
                        fadePanel.FadeOut(0.5f);
                    });
                }
                else
                {
                    // 페이드 패널이 없다면 즉시 교체
                    Hide<StartPanelUI>();
                    Show<MainMenu>(true);
                }
                break;

            case GameManager.GameState.Paused:
                // 'Paused' 상태에서는 일시정지 메뉴를 보여줍니다.
                Show<PauseMenu>(true);
                break;

            case GameManager.GameState.Playing:
                // 'Playing' 상태에서는 모든 메뉴 패널이 꺼지고,
                // 인게임 HUD만 보이게 됩니다. (HUD를 별도 관리하지 않으면 아무것도 켜지 않음)
                break;

            case GameManager.GameState.LevelClear:
                // 레벨 클리어 UI(엔딩)를 보여줍니다.
                Show<EndingUI>(true);
                Debug.Log("레벨 클리어! 엔딩 UI를 표시합니다.");
                break;
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
        if (_uiDictionary.TryGetValue(typeof(T), out UiBase ui))
        {
            // UiBase에 있는 Show 함수를 호출합니다.
            // 이 함수는 이제 위치를 옮기는 역할을 합니다.
            ui.Show(show);

            // UIManager는 추가로 오브젝트 자체의 활성화/비활성화를 제어합니다.
            ui.gameObject.SetActive(show);
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
    public void HideAll()
    {
        Debug.LogWarning($"<color=red>[{Time.time:.2f}초] UIManager가 HideAllPanels()를 호출! 모든 패널을 숨깁니다.</color>");
        foreach (var ui in _uiDictionary.Values)
        {
            ui.Show(false);
        }
    }
}
