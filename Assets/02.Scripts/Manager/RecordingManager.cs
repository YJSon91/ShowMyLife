 #if UNITY_EDITOR // 이 스크립트의 모든 내용은 에디터에서만 컴파일됩니다.
using UnityEngine;
using UnityEditor.Recorder; // Unity Recorder를 제어하기 위해 필수
using UnityEditor.Recorder.Input; // Recorder의 입력 설정을 위해 필수

public class RecordingManager : MonoBehaviour
{
    private RecorderController _recorderController;
    private bool _isRecording = false;

    private void Start()
    {
        // GameManager에 자신을 등록합니다.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterRecordingManager(this);
        }
    }

    /// <summary>
    /// 지정된 이름으로 영상 녹화를 시작합니다.
    /// </summary>
    public void StartRecording(string clipName)
    {
        if (_isRecording)
        {
            Debug.LogWarning("[RecordingManager] 이미 다른 녹화가 진행 중입니다.");
            return;
        }

        // 1. 레코더 컨트롤러 설정 객체를 생성합니다.
        var controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
        _recorderController = new RecorderController(controllerSettings);

        // 2. 녹화할 영상의 설정을 만듭니다 (Movie Recorder).
        var movieRecorder = ScriptableObject.CreateInstance<MovieRecorderSettings>();
        movieRecorder.name = "MadMovie Recorder";
        movieRecorder.Enabled = true;

        // 3. 녹화할 화면 소스를 'Game View'로 설정합니다.
        movieRecorder.ImageInputSettings = new GameViewInputSettings
        {
            OutputWidth = 1920, // 녹화 해상도 (가로)
            OutputHeight = 1080 // 녹화 해상도 (세로)
        };

        // 4. 오디오도 함께 녹음하도록 설정합니다.
        movieRecorder.AudioInputSettings.PreserveAudio = true;

        // 5. 저장될 파일 경로와 이름을 설정합니다.
        // Application.persistentDataPath는 게임이 설치된 안전한 경로입니다.
        string savePath = System.IO.Path.Combine(Application.persistentDataPath, $"{clipName}.mp4");
        movieRecorder.OutputFile = savePath;

        // 6. 설정된 녹화기(Recorder)를 컨트롤러에 추가합니다.
        controllerSettings.AddRecorderSettings(movieRecorder);

        // 7. 컨트롤러가 녹화를 준비하도록 합니다.
        _recorderController.PrepareRecording();

        // 8. 녹화를 시작합니다.
        _recorderController.StartRecording();
        _isRecording = true;

        Debug.Log($"[RecordingManager] '{clipName}.mp4' 녹화를 시작합니다. 저장 경로: {savePath}");
    }

    /// <summary>
    /// 현재 진행 중인 녹화를 중지합니다.
    /// </summary>
    public void StopRecording()
    {
        if (!_isRecording || _recorderController == null) return;

        // 녹화를 중지하고, 컨트롤러를 정리합니다.
        _recorderController.StopRecording();
        _recorderController = null; // 컨트롤러 참조 해제
        _isRecording = false;

        Debug.Log("[RecordingManager] 녹화를 중지했습니다.");
    }
}
#endif
