using UnityEngine;
using System.Collections;

public class EntranceTriggerDirector : MonoBehaviour
{
    [Tooltip("연출매니저")]
    [SerializeField] private EmotionDirector emotionDirector;
    [Tooltip("플레이어")]
    [SerializeField] private Transform playerTransform;
    [Tooltip("발판 오브젝트")]
    [SerializeField] private GameObject objectToActivate;
    [Tooltip("연출 시간")]
    [SerializeField] private float skyPanDuration = 3f;
    [Tooltip("훑을 각도")]
    [SerializeField] private float sweepAngle = 90f;

    private bool hasTriggered = false;
    private InputReader _playerInputReader; // 플레이어의 InputReader를 저장할 변수
    private PlayerMovementController _playerMovementController; // 플레이어의 MovementController를 저장할 변수
    private bool _wasMovementControllerEnabled; // MovementController의 이전 활성화 상태 저장

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || !other.CompareTag("Player")) return;

        hasTriggered = true;
        
        // 플레이어 오브젝트에서 Player 컴포넌트를 가져옴
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            // InputReader를 저장해두고 비활성화
            if (player.InputReader != null)
            {
                _playerInputReader = player.InputReader;
                _playerInputReader.DisableInput();
                
                // 추가 디버그 로그
                Debug.Log("카메라 연출: 플레이어 입력 비활성화");
            }
            
            // MovementController를 저장해두고 비활성화
            if (player.MovementController != null)
            {
                _playerMovementController = player.MovementController;
                _wasMovementControllerEnabled = _playerMovementController.enabled;
                _playerMovementController.enabled = false;
                
                // 추가 디버그 로그
                Debug.Log("카메라 연출: 플레이어 이동 컨트롤러 비활성화");
            }
        }

        // 하늘 훑기 연출 시작
        emotionDirector.PlaySkyEmotion(playerTransform, sweepAngle, skyPanDuration);

        // 즉시 플레이어를 뒤로 돌림
        Vector3 back = -playerTransform.forward;
        back.y = 0f;
        playerTransform.rotation = Quaternion.LookRotation(back);

        // 즉시 오브젝트 활성화
        if (objectToActivate != null)
            objectToActivate.SetActive(true);

        // 연출 시간 경과 후 입력 해제 및 자기 자신 비활성화
        StartCoroutine(ReleaseInputAfterDelay(skyPanDuration));
    }

    private IEnumerator ReleaseInputAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // 입력 다시 활성화
        if (_playerInputReader != null)
        {
            _playerInputReader.EnableInput();
            Debug.Log("카메라 연출: 플레이어 입력 다시 활성화");
        }
        
        // MovementController 다시 활성화 (이전 상태로 복원)
        if (_playerMovementController != null)
        {
            _playerMovementController.enabled = _wasMovementControllerEnabled;
            Debug.Log("카메라 연출: 플레이어 이동 컨트롤러 다시 활성화");
        }
        
        gameObject.SetActive(false); // 자기 자신 비활성화
    }
    
    //범위 표시
    private void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col is BoxCollider box && box.isTrigger)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.color = new Color(1f, 0.5f, 0f, 1f);
            Gizmos.DrawWireCube(box.center, box.size);
        }
    }
}
