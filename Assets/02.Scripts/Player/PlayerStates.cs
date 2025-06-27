using UnityEngine;

/// <summary>
/// 플레이어 애니메이션 상태를 정의하는 열거형
/// </summary>
public enum PlayerAnimationState
{
    Base,
    Movement,
    Jump,
    Fall,
    Crouch
}

/// <summary>
/// 플레이어 걸음걸이 상태를 정의하는 열거형
/// </summary>
public enum PlayerGaitState
{
    Idle,
    Walk,
    Run,
    Sprint
} 