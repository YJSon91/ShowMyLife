using UnityEngine;
using UnityEngine.SceneManagement; 
using TMPro;                  
using System.Collections;     

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private GameObject _dialoguePanel;

    [Header("UI Texts")]
    [SerializeField] private TextMeshProUGUI _dialogueText;

    [Header("Scene Names")]
    [Tooltip("새 게임 또는 재시작 시 불러올 씬의 이름입니다.")]
    [SerializeField] private string _startSceneName = "SampleScene";

    public static UIManager Instance { get; private set; }

    private bool _isPaused = false;

    private void Awake()
    {       
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {        
        if (_pausePanel != null) _pausePanel.SetActive(false);
        if (_dialoguePanel != null) _dialoguePanel.SetActive(false);
    }

    private void Update()
    {     
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    private void PauseGame()
    {
        _isPaused = true;
        _pausePanel.SetActive(true);
        Time.timeScale = 0f; // 게임의 시간을 멈춥니다.
    }
      
    public void ResumeGame()
    {
        _isPaused = false;
        _pausePanel.SetActive(false);
        Time.timeScale = 1f; // 게임의 시간을 다시 흐르게 합니다.
    }

    public void OnSettingButton()
    {
        // MVP 단계에서는 우선 로그만 출력합니다.
        // 추후에 실제 설정 화면 패널을 열도록 구현할 수 있습니다.
        Debug.Log("설정 버튼 클릭됨!");
    }

    public void OnCreditButton()
    {
        // MVP 단계에서는 우선 로그만 출력합니다.
        Debug.Log("크레딧 버튼 클릭됨!");
    }

    public void OnRestartButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(_startSceneName);
    }

    public void OnQuitGameButton()
    {
        Debug.Log("게임 종료 버튼 클릭됨!");
        // 에디터에서는 동작하지 않지만, 빌드된 게임에서는 어플리케이션을 종료합니다.
        Application.Quit();
    }

    public void ShowDialogue(string message)
    {
        StopAllCoroutines();
        StartCoroutine(ShowDialogueCoroutine(message));
    }

    private IEnumerator ShowDialogueCoroutine(string message)
    {
        _dialoguePanel.SetActive(true);
        _dialogueText.text = message;

        yield return new WaitForSeconds(3f);

        _dialoguePanel.SetActive(false);
    }
}
