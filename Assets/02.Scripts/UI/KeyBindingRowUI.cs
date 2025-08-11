 using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI; // Button을 사용하기 위해 필요

public class KeyBindingRowUI : MonoBehaviour
{
    [Header("연결 정보")]
    [Tooltip("어떤 액션을 리매핑할지 지정 (예: Jump, Move)")]
    [SerializeField] private InputActionReference _targetAction;

    [Tooltip("변경할 바인딩의 인덱스. 키보드는 보통 0 또는 1입니다.")]
    [SerializeField] private int _bindingIndex = 0;

    [Header("UI 요소")]
    [SerializeField] private TextMeshProUGUI _actionNameText;
    [SerializeField] private Button _rebindButton;
    [SerializeField] private TextMeshProUGUI _rebindButtonText;

    private InputAction TargetAction
    {
        get
        {
            if (GameManager.Instance?.PlayerControls == null)
            {
              //  Debug.LogError($"[KeyBindingRowUI] GameManager.PlayerControls가 null입니다!");
                return null;
            }

            var action = GameManager.Instance.PlayerControls.asset.FindAction(_targetAction.action.name);
            if (action == null)
            {
               // Debug.LogError($"[KeyBindingRowUI] 액션 '{_targetAction.action.name}'을 찾을 수 없습니다!");
            }
            return action;
        }
    }
              
    private void OnEnable()
    {
        // UI가 켜질 때마다 버튼 텍스트를 최신 상태로 업데이트
        UpdateUI();
    }

    private void Start()
    {
        // 버튼 클릭 이벤트에 함수 연결
        _rebindButton.onClick.AddListener(StartRebinding);
    }

    // 현재 바인딩된 키를 UI에 표시하는 함수
    private void UpdateUI()
    {
        var action = TargetAction;
        if (action == null) return;

        _actionNameText.text = action.name;
        _rebindButtonText.text = action.GetBindingDisplayString(_bindingIndex);
    }

    // 키 변경을 시작하는 함수
    public void StartRebinding()
    {
        var action = TargetAction;

        _rebindButtonText.text = "Press any key...";
        _rebindButton.interactable = false; // 리바인딩 중에는 버튼 비활성화

        action.Disable();
        

        // 기존 바인딩을 취소하고 새로운 입력을 기다립니다.
        action.PerformInteractiveRebinding(_bindingIndex)
            .OnComplete(operation =>
            {
                // 리바인딩이 완료되면 호출될 부분
                operation.Dispose(); // 메모리 정리

                // ★ 중요: 액션을 다시 활성화합니다
                action.Enable();

                // --- 핵심: GameManager의 PlayerControls 전체를 저장 ---
                string allRebindsJson = GameManager.Instance.PlayerControls.SaveBindingOverridesAsJson();

                //Debug.LogError($"[저장 시도] PlayerPrefs에 저장할 데이터: {allRebindsJson}");

                // 데이터를 저장합니다.
                PlayerPrefs.SetString("AllKeyRebinds", allRebindsJson);
                PlayerPrefs.Save();

                // 즉시 GameManager에서 다시 로드하여 동기화
                GameManager.Instance.LoadAllKeybindings();

                UpdateUI(); // UI 텍스트를 새로운 키로 업데이트
                _rebindButton.interactable = true; // 버튼 다시 활성화
                
            })
            .OnCancel(operation =>
            {
                // 취소될 경우 (예: Esc 키 누름)
                operation.Dispose();
                action.Enable();
                
                UpdateUI();
                _rebindButton.interactable = true;
            })
            .Start();
    }
}
