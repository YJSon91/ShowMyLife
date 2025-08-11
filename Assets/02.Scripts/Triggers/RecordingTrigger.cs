 using UnityEngine;

public class RecordingTrigger : MonoBehaviour
{
    [Tooltip("이 트리거가 시작 트리거인지, 종료 트리거인지 설정")]
    [SerializeField] private bool _isStartTrigger = true;
    [Tooltip("저장될 영상 클립의 이름 (예: Clip_01)")]
    [SerializeField] private string _clipName;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.CompareTag("Player"))
            {               
#if UNITY_EDITOR
                var recorder = GameManager.Instance.RecordingManager;
                if (recorder == null) return;

                if (_isStartTrigger)
                {
                    recorder.StartRecording(_clipName);
                }
                else
                {
                    recorder.StopRecording();
                }
#endif
               gameObject.SetActive(false);
            }
        }
    }
}
