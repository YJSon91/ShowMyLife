using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어의 모든 컴포넌트를 관리하는 중앙 클래스
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerAnimationController))]
[RequireComponent(typeof(PlayerMovementController))]
[RequireComponent(typeof(PlayerStateController))]
[RequireComponent(typeof(InputReader))]
public class Player : MonoBehaviour
{
    #region 컴포넌트 참조

    [Header("필수 컴포넌트")]
    [Tooltip("플레이어 애니메이션을 제어하는 컴포넌트")]
    [SerializeField] private PlayerAnimationController _animationController;
    [Tooltip("플레이어 이동을 제어하는 Character Controller 컴포넌트")]
    [SerializeField] private CharacterController _characterController;
    [Tooltip("카메라 동작을 제어하는 스크립트")]
    [SerializeField] private CameraController _cameraController;
    [Tooltip("InputReader는 플레이어 입력을 처리합니다")]
    [SerializeField] private InputReader _inputReader;

    #endregion

    #region 상태 관리 컴포넌트
    [Header("상태 관리 컴포넌트")]
    [Tooltip("플레이어 이동을 제어하는 컴포넌트")]
    [SerializeField] private PlayerMovementController _movementController;
    [Tooltip("플레이어 상태를 관리하는 컴포넌트")]
    [SerializeField] private PlayerStateController _stateController;
    #endregion

    #region 속성

    public PlayerAnimationController AnimationController => _animationController;
    public CharacterController CharacterController => _characterController;
    public CameraController CameraController => _cameraController;
    public InputReader InputReader => _inputReader;
    public PlayerMovementController MovementController => _movementController;
    public PlayerStateController StateController => _stateController;

    #endregion

    #region Unity 생명주기

    private void Awake()
    {
        InitializeComponents();
    }
    #endregion

    #region 초기화 메서드

    /// <summary>
    /// 모든 필수 컴포넌트를 초기화합니다
    /// </summary>
    private void InitializeComponents()
    {
        // 컴포넌트가 Inspector에서 할당되지 않은 경우 자동으로 찾기
        if (_characterController == null)
            _characterController = GetComponent<CharacterController>();

        if (_animationController == null)
            _animationController = GetComponent<PlayerAnimationController>();

        if (_inputReader == null)
            _inputReader = GetComponent<InputReader>();

        if (_movementController == null)
            _movementController = GetComponent<PlayerMovementController>();

        if (_stateController == null)
            _stateController = GetComponent<PlayerStateController>();

        // 카메라 컨트롤러는 다른 게임 오브젝트에 있을 수 있으므로 자동으로 찾지 않음

        ValidateComponents();
    }

    /// <summary>
    /// 모든 필수 컴포넌트가 존재하는지 확인합니다
    /// </summary>
    private void ValidateComponents()
    {
        if (_characterController == null)
            Debug.LogError("Player: CharacterController가 할당되지 않았습니다!");

        if (_animationController == null)
            Debug.LogError("Player: PlayerAnimationController가 할당되지 않았습니다!");

        if (_inputReader == null)
            Debug.LogError("Player: InputReader가 할당되지 않았습니다!");

        if (_cameraController == null)
            Debug.LogError("Player: CameraController가 할당되지 않았습니다!");

        if (_movementController == null)
            Debug.LogError("Player: PlayerMovementController가 할당되지 않았습니다!");

        if (_stateController == null)
            Debug.LogError("Player: PlayerStateController가 할당되지 않았습니다!");
    }

    #endregion

    #region 공개 메서드

    /// <summary>
    /// 플레이어 위치를 설정합니다
    /// </summary>
    /// <param name="position">새 위치</param>
    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    /// <summary>
    /// 플레이어 회전을 설정합니다
    /// </summary>
    /// <param name="rotation">새 회전</param>
    public void SetRotation(Quaternion rotation)
    {
        transform.rotation = rotation;
    }

    #endregion
} 