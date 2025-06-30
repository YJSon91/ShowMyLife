using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    [Tooltip("감도")]
    [SerializeField] private float sensitivity = 2f;

    [Tooltip("버츄얼 카메라")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    [Tooltip("플레이어 오브젝트")]
    [SerializeField] private Transform playerBody;

    [Tooltip("포커스 타겟")]
    [SerializeField] private Transform cameraPitchTarget;

    [Tooltip("인풋 처리 스크립트")]
    [SerializeField] private InputReader inputReader;

    private float xRotation = 0f;

    public float Sensitivity
    {
        get => sensitivity;
        set => sensitivity = Mathf.Clamp(value, 0.1f, 10f);
    }

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (virtualCamera == null || playerBody == null || cameraPitchTarget == null)
        {
            enabled = false;
            return;
        }

        virtualCamera.Follow = cameraPitchTarget;
        virtualCamera.LookAt = cameraPitchTarget;
    }

    private void Start()
    {
        StartCoroutine(WaitAndRegister());
    }

    private System.Collections.IEnumerator WaitAndRegister()
    {
        float waitTime = 0f;
        while (GameManager.Instance == null && waitTime < 2f)
        {
            waitTime += Time.deltaTime;
            yield return null;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterCameraManager(this);
        }
        else
        {
            Debug.LogWarning("[CameraManager] GameManager를 찾지 못했습니다");
        }
    }

    private void LateUpdate()
    {
        Vector2 look = inputReader.LookInput;

        float mouseX = look.x * sensitivity;
        float mouseY = look.y * sensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -30f, 70f);
        cameraPitchTarget.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        playerBody.Rotate(Vector3.up * mouseX);
    }
}
