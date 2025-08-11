🧑‍💻 손영준 (SonYeongJun) - 개발

🚀 주요 기여 및 역할
저는 Show My Life 프로젝트에서 개발팀의 일원으로서 프로젝트의 핵심 아키텍처를 설계하고, 성능 최적화와 주요 시스템의 구현 및 안정화를 주도적으로 담당했습니다. Unity 프로파일러를 기반으로 한 데이터 중심의 문제 해결을 통해 프로젝트의 기술적 완성도를 높이고, 팀원들과의 긴밀한 협업을 통해 발생한 여러 버그를 해결하며 안정적인 플레이 경험을 구축하는 데 기여했습니다.

<details>
<summary><b>🏛️ 아키텍처 및 시스템 설계 (Architecture & System Design)</b></summary>

❗️ 문제 사항
게임의 복잡한 상태와 다수의 하위 시스템을 체계적으로 관리할 중앙 컨트롤 타워가 부재하여, 시스템 간의 의존성이 높아지고 유지보수가 어려워질 위험이 있었습니다.

🆕 개선 방식
매니저 시스템 설계: GameManager를 중심으로 각자의 명확한 책임을 가진 8개의 하위 매니저 클래스를 설계하여 관심사 분리(SoC) 원칙을 적용했습니다.

GameManager (상태 머신): 싱글톤 패턴과 상태 머신을 적용하여 게임의 전체 흐름을 총괄하고, 하위 매니저들이 스스로를 등록하는 시스템으로 **느슨한 결합(Loose Coupling)**을 유지했습니다.

UIManager (모듈식 UI): 제네릭 메서드(Show<T>, Hide<T>)와 UiBase 기반 클래스를 활용하여 13개의 UI 스크립트를 모듈화하여 관리의 효율성과 확장성을 극대화했습니다.

DialogueTool (에디터 툴): EditorWindow를 상속받아 기획자가 JSON 데이터를 쉽게 수정하고 게임 내 프리팹과 동기화할 수 있는 커스텀 에디터 툴을 제작하여 작업 효율성을 높였습니다.

⭐ 개선 결과
각 시스템의 책임이 명확하고, 확장이 용이하며, 유지보수가 편리한 안정적인 프로젝트 아키텍처를 구축했습니다.

코드 샘플

GameManager: 상태 머신
```
// GameManager.cs
// 게임의 상태를 변경하고, 상태에 맞는 로직을 실행하는 핵심 메서드
public void UpdateGameState(GameState newState)
{
    if (CurrentState == newState) return;
    CurrentState = newState;

    // switch 문을 사용하여 각 상태에 따른 동작을 명확하게 분리
    switch (newState)
    {
        case GameState.Playing:
            // 플레이 중일 때는 플레이어 입력만 활성화하고 커서를 잠금
            PlayerControls?.UI.Disable();
            PlayerControls?.Player.Enable();
            Cursor.lockState = CursorLockMode.Locked;
            break;

        case GameState.Paused:
            // 일시정지 시에는 UI 입력만 활성화하고 커서 잠금을 해제
            PlayerControls?.Player.Disable();
            PlayerControls?.UI.Enable();
            Cursor.lockState = CursorLockMode.None;
            break;
    }
    // 상태 변경 사실을 다른 시스템에 알림 (이벤트 기반)
    OnGameStateChanged?.Invoke(newState);
}
```

UIManager: 제네릭 기반 UI 관리
```
// UIManager.cs
// 제네릭을 사용하여 어떤 타입의 UI든 일관된 방식으로 보여주는 메서드
public void Show<T>(bool show) where T : UiBase
{
    // 딕셔너리에서 요청된 타입(T)의 UI를 찾음
    if (_uiInstances.TryGetValue(typeof(T), out UiBase ui))
    {
        // 해당 UI의 Show 함수를 호출
        ui.Show(show);
    }
}
```
KeyBinding: 키 리바인딩
```
// KeyBindingRowUI.cs
// 키 변경을 시작하고, 완료되면 PlayerPrefs에 저장하는 로직
private void StartRebinding()
{
    // 기존 바인딩 작업을 취소하고, 새로운 리바인딩 작업을 시작
    _rebindingOperation?.Cancel();
    _rebindingOperation = _inputAction.PerformInteractiveRebinding()
        .OnComplete(operation =>
        {
            operation.Dispose();
            // 리바인딩이 완료되면 전체 키 설정을 JSON으로 변환하여 저장
            string rebinds = GameManager.Instance.PlayerControls.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString("AllKeyRebinds", rebinds);
            PlayerPrefs.Save();
        })
        .Start();
}
```

UI Effect: 버튼 호버 효과
```
// ButtonHoverEffect.cs
// 마우스 포인터 이벤트에 따라 DOTween을 이용해 버튼 크기를 조절하는 로직
public void OnPointerEnter(PointerEventData eventData)
{
    // 마우스가 버튼 위에 올라오면 0.2초 동안 1.1배로 커짐
    transform.DOScale(1.1f, 0.2f).SetUpdate(true);
}

public void OnPointerExit(PointerEventData eventData)
{
    // 마우스가 버튼에서 벗어나면 0.2초 동안 원래 크기로 돌아옴
    transform.DOScale(1.0f, 0.2f).SetUpdate(true);
}
```

</details>

<details>
<summary><b>📊 성능 최적화 (메모리 및 렌더링 부하 감소)</b></summary>

❗️ 문제 사항
프로젝트 초기, 5.45GB에 달하는 높은 메모리 사용량을 확인했으며, 이로 인한 잠재적인 성능 저하 및 시스템 요구 사양 증가가 우려되었습니다.

Memory Profiler 분석 결과, 고해상도 텍스처(3.06GB) 및 렌더 텍스처(MSAA, 고해상도 그림자, SSAO)가 메모리 점유의 주된 원인임을 파악했습니다.

🆕 개선 방식
데이터 기반의 단계적 최적화: Unity 프로파일러와 프레임 디버거를 활용하여 가장 큰 병목 지점부터 순차적으로 해결하는 과학적인 접근법을 채택했습니다.

텍스처 최적화: Max Texture Size 조절 및 Crunch Compression을 적용하여 텍스처 메모리를 대폭 감소시켰습니다.

렌더링 최적화: MSAA(4x→2x), 그림자 해상도(4096→512), SSAO 품질(High→Medium) 등 URP 설정을 하향 조정하여 GPU 부하를 줄였습니다.

⭐ 개선 결과
총 메모리 사용량을 5.45GB에서 2.34GB로 약 57% 절감하여, 더 넓은 범위의 PC 사양에서 쾌적하게 플레이할 수 있는 안정적인 성능을 확보했습니다.

</details>

<details>
<summary><b>🐛 시스템 디버깅 및 안정화</b></summary>

❗️ 문제 사항
씬 전환 시 BGM이 초기화되는 현상, 재시작 시 오브젝트 풀 참조 오류, 스크립트 실행 순서에 따른 NullReferenceException 등 프로젝트의 안정성을 저해하는 다수의 버그가 발생했습니다.

🆕 개선 방식
BGM 끊김: GameManager의 상태 변화 로직을 분석하여, Paused 상태에서 돌아올 때는 BGM 재생 명령을 내리지 않도록 수정하여 문제를 해결했습니다.

오브젝트 풀 오류: SceneManager.sceneUnloaded 이벤트를 구독하여 씬이 닫힐 때 오브젝트 풀이 자동으로 초기화되도록 구현, 파괴된 오브젝트에 대한 참조 오류를 근본적으로 해결했습니다.

실행 순서 문제: Unity 생명주기에 대한 깊은 이해를 바탕으로 Awake, Start, 코루틴 등을 활용하여 NullReferenceException이 간헐적으로 발생하는 문제를 해결했습니다.

⭐ 개선 결과
주요 버그들을 체계적으로 해결하여 프로젝트의 전반적인 안정성과 완성도를 크게 향상시켰습니다.

</details>
