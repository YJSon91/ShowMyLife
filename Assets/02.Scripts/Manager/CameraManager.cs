using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    [Tooltip("감도")]
    [SerializeField] private float sensitivity = 0.2f;

    [Tooltip("버츄얼 카메라")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    [Tooltip("플레이어 오브젝트")]
    [SerializeField] private Transform playerBody;

    [Tooltip("인풋 처리 스크립트")]
    [SerializeField] private InputReader inputReader;

    private Cinemachine3rdPersonFollow follow;
    private float xRotation = 0f;

    public float Sensitivity
    {
        get => sensitivity;
        set => sensitivity = Mathf.Clamp(value, 0.1f, 1f);
    }

    private void Awake()
    {
        if (virtualCamera == null) return;

        follow = virtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        if (follow == null) return;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

       
    }
    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterCameraManager(this);
        }
        else
        {
            Debug.LogError("[CameraManager] CameraManager가 씬에 존재하지 않습니다!");
        }
    }

    private void LateUpdate()
    {
        Vector2 look = inputReader.LookInput;

        float mouseX = look.x * sensitivity;
        float mouseY = look.y * sensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -40f, 80f);

        virtualCamera.transform.localEulerAngles = new Vector3(xRotation, virtualCamera.transform.localEulerAngles.y, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
