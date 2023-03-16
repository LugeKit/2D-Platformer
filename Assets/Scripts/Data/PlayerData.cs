using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Data", order = 1)]
public class PlayerData : ScriptableObject
{
    [Header("Collision")]
    public Vector2 CollisionDetectDistance;

    [Space(20)]

    [Header("Run")]
    public float MaxRunningSpeed;
    public float AccelerationForce;
    public float HorizontalFriction;

    [Space(20)]

    [Header("Jump")]
    public bool EnableJump;
    public float JumpForce;
    public float CoyoteTime;
    public float JumpBufferTime;

    [Space(20)]

    [Header("Double Jump")]
    public bool EnableDoubleJump;
    public float DoubleJumpForce;

    [Space(20)]

    [Header("Wall Jump")]
    public bool EnableWallJump;
    public Vector2 WallJumpForce;
}
