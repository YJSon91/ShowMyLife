using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    [Tooltip("마우스 감도")]
    [SerializeField] private float sensitivity = 3f;

    [Tooltip("플레이어 카메라")]
    [SerializeField] private CinemachineVirtualCamera playerCamera;

    [Tooltip("연출용 카메라")]
    [SerializeField] private CinemachineVirtualCamera themeCamera;

    [Tooltip("플레이어 컨트롤 스크립트")]
    [SerializeField] private PlayerMovementController movementController;

    [Tooltip("메인 카메라")]
    [SerializeField] private CinemachineBrain brain;

    private bool isCurrentlyFalling = false;
    private CinemachinePOV pov;

    public float Sensitivity
    {
        get => sensitivity;
        set
        {
            sensitivity = Mathf.Clamp(value, 0.1f, 10f);
            ApplySensitivityToPOV();
        }
    }

    private void Awake()
    {
        sensitivity = PlayerPrefs.GetFloat("CameraSensitivity", 10f);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerCamera == null || themeCamera == null)
        {
            Debug.LogError("[CameraManager] 필수 요소가 누락되었습니다.");
            enabled = false;
            return;
        }

        pov = playerCamera.GetCinemachineComponent<CinemachinePOV>();
        if (pov == null)
        {
            Debug.LogError("[CameraManager] CinemachinePOV 컴포넌트가 없습니다.");
            enabled = false;
            return;
        }

        ApplySensitivityToPOV();
        ApplyVerticalClamp();

        playerCamera.Priority = 10;
        themeCamera.Priority = 0;
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterCameraManager(this);
        }
        else
        {
            Debug.LogWarning("[CameraManager] GameManager 인스턴스를 찾을 수 없습니다.");
        }
    }


    private void FixedUpdate()
    {
        if (movementController == null || brain == null)
            return;

        bool isFalling = !movementController.IsGrounded && movementController.Rigidbody.velocity.y < -0.2f;

        if (isFalling != isCurrentlyFalling)
        {
            isCurrentlyFalling = isFalling;

            if (isFalling)
            {
                brain.m_UpdateMethod = CinemachineBrain.UpdateMethod.LateUpdate;
                brain.m_BlendUpdateMethod = CinemachineBrain.BrainUpdateMethod.LateUpdate;
            }
            else
            {
                brain.m_UpdateMethod = CinemachineBrain.UpdateMethod.FixedUpdate;
                brain.m_BlendUpdateMethod = CinemachineBrain.BrainUpdateMethod.FixedUpdate;
            }
        }
    }

    private void ApplySensitivityToPOV()
    {
        if (pov != null)
        {
            pov.m_HorizontalAxis.m_MaxSpeed = sensitivity * 100f;
            pov.m_VerticalAxis.m_MaxSpeed = sensitivity * 100f;
        }
    }

    private void ApplyVerticalClamp()
    {
        if (pov != null)
        {
            pov.m_VerticalAxis.m_MinValue = -30f;
            pov.m_VerticalAxis.m_MaxValue = 70f;
        }
    }

    public void SwitchToThemeCamera(float duration = 3f)
    {
        StartCoroutine(SwitchToThemeRoutine(duration));
    }

    private System.Collections.IEnumerator SwitchToThemeRoutine(float duration)
    {
        themeCamera.Priority = 20;
        playerCamera.Priority = 0;
        yield return new WaitForSeconds(duration);
        themeCamera.Priority = 0;
        playerCamera.Priority = 10;
    }
}
