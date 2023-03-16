using UnityEngine;

public class PlayerController : MonoBehaviour
{
    class UserInput
    {
        public float x;
        public bool jump;
    }

    public enum CollisionType
    {
        None,
        Positive,
        Negtive,
        Both
    }

    #region Variables
    [SerializeField] PlayerData data;
    [SerializeField] LayerMask groundLayerMask;
    [SerializeField] Collider2D airColl;
    [SerializeField, ReadOnly] CollisionType verticalColl;
    [SerializeField, ReadOnly] CollisionType horizontalColl;
    [SerializeField, ReadOnly] CollisionType airHorizontalColl;
    [SerializeField, ReadOnly] float coyoteTime;
    [SerializeField, ReadOnly] float lastPressedJumpTime;

    PlayerStatusManager stMgr;
    UserInput input = new();
    Rigidbody2D rb;
    Collider2D coll;
    SpriteRenderer sr;
    PlayerStatus ust => stMgr.userStatus;
    #endregion

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        coll = GetComponent<Collider2D>();
        stMgr = GetComponent<PlayerStatusManager>();
    }

    // Update is called once per frame
    void Update()
    {
        SimulateFriction();
        GatherInput();
        DoCollisionCheck();
        DecideUserStatus();
        UpdateTimer();
        ChangeUserFacing();
        PerformAction();
    }

    void SimulateFriction()
    {
        var currVelX = rb.velocity.x;
        var decelerationForce = data.HorizontalFriction * Physics2D.gravity.y * Mathf.Sign(currVelX);
        MDebug.Log("decelerationForce: {0}", decelerationForce);
        var finalVelX = currVelX + decelerationForce * Time.deltaTime / rb.mass;

        if (Mathf.Sign(currVelX) == Mathf.Sign(finalVelX))
        {
            rb.velocity = new Vector2(finalVelX, rb.velocity.y);
            return;
        }

        rb.velocity = new Vector2(0, rb.velocity.y);
    }

    void GatherInput()
    {
        input.x = Input.GetAxisRaw("Horizontal");
        input.jump = Input.GetKeyDown(KeyCode.Space);
    }

    void DoCollisionCheck()
    {
        verticalColl = VerticalCollideCheck();
        horizontalColl = HorizontalCollideCheck();
    }

    void DecideUserStatus()
    {
        var nextStatus = ust;

        if (IsGrounded() && (IsRunningTowardsWall() || rb.velocity.x != 0))
            nextStatus = PlayerStatus.RUNNING;
        else if (IsGrounded() && rb.velocity.x == 0)
            nextStatus = PlayerStatus.IDLE;
        else if (!IsGrounded() && ust != PlayerStatus.DOUBLE_JUMPING)
            if (rb.velocity.y >= 0)
                nextStatus = PlayerStatus.RISING;
            else
                nextStatus = PlayerStatus.FALLING;

        stMgr.ChangeUserStatus(nextStatus);
    }

    void UpdateTimer()
    {
        coyoteTime -= Time.deltaTime;
        lastPressedJumpTime -= Time.deltaTime;

        if (ust == PlayerStatus.IDLE || ust == PlayerStatus.RUNNING)
            coyoteTime = data.CoyoteTime;

        if (input.jump)
            lastPressedJumpTime = data.JumpBufferTime;
    }

    void PerformAction()
    {
        if (CanJump() && (input.jump || lastPressedJumpTime > 0))
        {
            PlayerJump();
        }
        else if (CanDoubleJump() && input.jump)
        {
            PlayerDoubleJump();
            stMgr.ChangeUserStatus(PlayerStatus.DOUBLE_JUMPING);
        }

        if (CanRun())
            PlayerRun();
    }

    void ChangeUserFacing()
    {
        if (input.x == 0)
        {
            return;
        }
        sr.flipX = input.x < 0;
    }

    #region Player Movements
    void PlayerJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0); // set Vy to be 0 will make we jump the same height every time
        rb.AddForce(data.JumpForce * Vector2.up, ForceMode2D.Impulse);
    }

    void PlayerDoubleJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0); // set Vy to be 0 will make we jump the same height every time
        rb.AddForce(data.DoubleJumpForce * Vector2.up, ForceMode2D.Impulse);
    }

    void PlayerRun()
    {
        if (input.x == 0)
            return;

        var accelerationForce = input.x * data.AccelerationForce;
        var force = accelerationForce * Time.deltaTime;
        var expectedXSpeed = data.MaxRunningSpeed * input.x;

        var currVelX = rb.velocity.x;
        var finalVelX = currVelX + force / rb.mass;
        if (Mathf.Sign(currVelX) == Mathf.Sign(expectedXSpeed) && Mathf.Abs(finalVelX) > Mathf.Abs(expectedXSpeed))
        {
            rb.velocity = new Vector2(expectedXSpeed, rb.velocity.y);
            return;
        }
        
        rb.AddForce(force * Vector2.right, ForceMode2D.Impulse);
    }
    #endregion

    #region Check movement enabled or not
    bool CanJump()
    {
        return data.EnableJump && (ust == PlayerStatus.IDLE || ust == PlayerStatus.RUNNING || (ust == PlayerStatus.FALLING && coyoteTime > 0));
    }

    bool CanDoubleJump()
    {
        return data.EnableDoubleJump && (ust == PlayerStatus.FALLING || ust == PlayerStatus.RISING);
    }

    bool CanRun()
    {
        return ust != PlayerStatus.WALL_HANGING;
    }

    bool IsGrounded()
    {
        return verticalColl == CollisionType.Negtive || verticalColl == CollisionType.Both;
    }

    bool IsRunningTowardsWall()
    {
        if (input.x == 0)
            return false;

        if (horizontalColl == CollisionType.None)
            return false;

        if (horizontalColl == CollisionType.Both)
            return true;

        return (horizontalColl == CollisionType.Positive && input.x > 0) || (horizontalColl == CollisionType.Negtive && input.x < 0);
    }
    #endregion

    #region Collide Check
    CollisionType VerticalCollideCheck()
    {
        var res = CollisionType.None;

        if (Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0, Vector2.up, data.CollisionDetectDistance.y, groundLayerMask))
        {
            res = CollisionType.Positive;
        }
        if (Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0, Vector2.down, data.CollisionDetectDistance.y, groundLayerMask))
        {
            res = (res == CollisionType.Positive) ? CollisionType.Both : CollisionType.Negtive;
        }

        return res;
    }
    CollisionType HorizontalCollideCheck()
    {
        var res = CollisionType.None;

        if (Physics2D.BoxCast(airColl.bounds.center, airColl.bounds.size, 0, Vector2.right, data.CollisionDetectDistance.x, groundLayerMask))
        {
            res = CollisionType.Positive;
        }
        if (Physics2D.BoxCast(airColl.bounds.center, airColl.bounds.size, 0, Vector2.left, data.CollisionDetectDistance.x, groundLayerMask))
        {
            res = (res == CollisionType.Positive) ? CollisionType.Both : CollisionType.Negtive;
        }

        return res;

    }
    #endregion
}

