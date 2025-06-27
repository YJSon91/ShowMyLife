using UnityEngine;


    public class CameraController : MonoBehaviour
    {
        private const int _LAG_DELTA_TIME_ADJUSTMENT = 20;

        [Tooltip("캐릭터 게임 오브젝트")]
        [SerializeField] private GameObject _Character;
        [Tooltip("플레이어 시점을 위한 메인 카메라")]
        [SerializeField] private Camera _mainCamera;

        [SerializeField] private Transform _playerTarget;
        [SerializeField]
        private Transform _lockOnTarget;

        [SerializeField] private bool _invertCamera;
        [SerializeField] private bool _hideCursor;
        [SerializeField] private bool _isLockedOn;
        [SerializeField] private bool _isAiming;
        [SerializeField] private float _mouseSensitivity = 5f;
        [SerializeField] private float _cameraDistance = 5f;
        [SerializeField] private float _cameraHeightOffset;
        [SerializeField] private float _cameraHorizontalOffset;
        [SerializeField] private float _cameraTiltOffset;
        private Vector2 _cameraTiltBounds = new Vector2(-10f, 45f);
        [SerializeField]
        private float _positionalCameraLag = 1f;
        [SerializeField]
        private float _rotationalCameraLag = 1f;
        private float _cameraInversion;

        private InputReader _inputReader;
        private float _lastAngleX;
        private float _lastAngleY;

        private Vector3 _lastPosition;

        private float _newAngleX;

        private float _newAngleY;
        private Vector3 _newPosition;
        private float _rotationX;
        private float _rotationY;

        private Transform _camera;
        
        // 카메라 회전 관련 변수 추가
        private Vector3 _currentCameraRotation;
        private Vector3 _previousCameraRotation;

        /// <summary>
        /// 시작 시 호출됩니다.
        /// </summary>
        private void Start()
        {
            _camera = gameObject.transform.GetChild(0);

            _inputReader = _Character.GetComponent<InputReader>();
            _playerTarget = _Character.transform.Find("Player_LookAt");
            _lockOnTarget = _Character.transform.Find("TargetLockOnPos");

            if (_hideCursor)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }

            _cameraInversion = _invertCamera ? 1 : -1;

            transform.position = _playerTarget.position;
            transform.rotation = _playerTarget.rotation;

            _lastPosition = transform.position;

            _camera.localPosition = new Vector3(_cameraHorizontalOffset, _cameraHeightOffset, _cameraDistance * -1);
            _camera.localEulerAngles = new Vector3(_cameraTiltOffset, 0f, 0f);
            
            // 카메라 회전 초기화
            _currentCameraRotation = transform.eulerAngles;
            _previousCameraRotation = _currentCameraRotation;
        }

        /// <summary>
        /// 매 프레임마다 호출됩니다.
        /// </summary>
        private void Update()
        {
            float positionalFollowSpeed = 1 / (_positionalCameraLag / _LAG_DELTA_TIME_ADJUSTMENT);
            float rotationalFollowSpeed = 1 / (_rotationalCameraLag / _LAG_DELTA_TIME_ADJUSTMENT);

            _rotationX = _inputReader._mouseDelta.y * _cameraInversion * _mouseSensitivity;

            _rotationY = _inputReader._mouseDelta.x * _mouseSensitivity;

            _newAngleX += _rotationX;
            _newAngleX = Mathf.Clamp(_newAngleX, _cameraTiltBounds.x, _cameraTiltBounds.y);
            _newAngleX = Mathf.Lerp(_lastAngleX, _newAngleX, rotationalFollowSpeed * Time.deltaTime);

            if (_isLockedOn)
            {
                Vector3 aimVector = _lockOnTarget.position - _playerTarget.position;
                Quaternion targetRotation = Quaternion.LookRotation(aimVector);
                targetRotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationalFollowSpeed * Time.deltaTime);
                _newAngleY = targetRotation.eulerAngles.y;
            }
            else
            {
                _newAngleY += _rotationY;
                _newAngleY = Mathf.Lerp(_lastAngleY, _newAngleY, rotationalFollowSpeed * Time.deltaTime);
            }

            _newPosition = _playerTarget.position;
            _newPosition = Vector3.Lerp(_lastPosition, _newPosition, positionalFollowSpeed * Time.deltaTime);

            transform.position = _newPosition;
            transform.eulerAngles = new Vector3(_newAngleX, _newAngleY, 0);

            _camera.localPosition = new Vector3(_cameraHorizontalOffset, _cameraHeightOffset, _cameraDistance * -1);
            _camera.localEulerAngles = new Vector3(_cameraTiltOffset, 0f, 0f);

            _lastPosition = _newPosition;
            _lastAngleX = _newAngleX;
            _lastAngleY = _newAngleY;
            
            // 카메라 회전 업데이트
            _previousCameraRotation = _currentCameraRotation;
            _currentCameraRotation = transform.eulerAngles;
        }

        /// <summary>
        ///     카메라가 특정 타겟을 향하도록 고정합니다.
        /// </summary>
        /// <param name="enable">타겟 고정 활성화 여부</param>
        /// <param name="newLockOnTarget">고정할 타겟</param>
        public void LockOn(bool enable, Transform newLockOnTarget)
        {
            _isLockedOn = enable;

            if (newLockOnTarget != null)
            {
                _lockOnTarget = newLockOnTarget;
            }
        }

        /// <summary>
        /// 카메라의 위치를 가져옵니다.
        /// </summary>
        /// <returns>카메라의 위치</returns>
        public Vector3 GetCameraPosition()
        {
            return _mainCamera.transform.position;
        }

        /// <summary>
        /// 카메라의 전방 벡터를 가져옵니다.
        /// </summary>
        /// <returns>카메라의 전방 벡터</returns>
        public Vector3 GetCameraForward()
        {
            return _mainCamera.transform.forward;
        }

        /// <summary>
        /// Y값이 0인 카메라의 전방 벡터를 가져옵니다.
        /// </summary>
        /// <returns>Y값이 0인 카메라의 전방 벡터</returns>
        public Vector3 GetCameraForwardZeroedY()
        {
            return new Vector3(_mainCamera.transform.forward.x, 0, _mainCamera.transform.forward.z);
        }

        /// <summary>
        /// Y값이 0인 정규화된 카메라의 전방 벡터를 가져옵니다.
        /// </summary>
        /// <returns>Y값이 0인 정규화된 카메라의 전방 벡터</returns>
        public Vector3 GetCameraForwardZeroedYNormalised()
        {
            return GetCameraForwardZeroedY().normalized;
        }


        /// <summary>
        /// Y값이 0인 카메라의 우측 벡터를 가져옵니다.
        /// </summary>
        /// <returns>Y값이 0인 카메라의 우측 벡터</returns>
        public Vector3 GetCameraRightZeroedY()
        {
            return new Vector3(_mainCamera.transform.right.x, 0, _mainCamera.transform.right.z);
        }

        /// <summary>
        /// Y값이 0인 정규화된 카메라의 우측 벡터를 가져옵니다.
        /// </summary>
        /// <returns>Y값이 0인 정규화된 카메라의 우측 벡터</returns>
        public Vector3 GetCameraRightZeroedYNormalised()
        {
            return GetCameraRightZeroedY().normalized;
        }

        /// <summary>
        /// 카메라 기울기의 X값을 가져옵니다.
        /// </summary>
        /// <returns>카메라 기울기의 X값</returns>
        public float GetCameraTiltX()
        {
            return _mainCamera.transform.eulerAngles.x;
        }
        
        /// <summary>
        /// 현재 카메라 회전을 가져옵니다.
        /// </summary>
        /// <returns>현재 카메라 회전</returns>
        public Vector3 GetCameraRotation()
        {
            return _currentCameraRotation;
        }
        
        /// <summary>
        /// 이전 카메라 회전을 가져옵니다.
        /// </summary>
        /// <returns>이전 카메라 회전</returns>
        public Vector3 GetPreviousCameraRotation()
        {
            return _previousCameraRotation;
        }
    }
