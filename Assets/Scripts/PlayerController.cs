using UnityEngine;

public class PlayerController : MonoBehaviour
{
    class UserInput
    {
        public float x;
        public bool jump;
        public bool reload;
    }

    #region Variables
    [SerializeField] float runningSpeed = 10f;
    [SerializeField] float jumpingForce = 10f;
    [SerializeField] float wallJumpForce = 10f;
    [SerializeField] float groundDetectUnit = .1f;
    [SerializeField] float wallDetectUnit = .1f;
    [SerializeField] float wallJumpInputColdSec = .3f;
    [SerializeField] PlayerStatusManager statusManager;
    [SerializeField] LayerMask groundLayerMask;
    [SerializeField] Collider2D wallChecker;
    [SerializeField, ReadOnlyAttributes] bool isGrounded;
    [SerializeField, ReadOnlyAttributes] bool isTowardsWall;
    [SerializeField, ReadOnlyAttributes] int isWallCollide;

    UserInput input = new();
    Rigidbody2D rb;
    Collider2D coll;
    SpriteRenderer sr;
    PlayerStatus ust => statusManager.userStatus;
    PlayerStatus lastUst => statusManager.lastUserStatus;
    bool wallJumpFlag = false;
    #endregion

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        coll = GetComponent<Collider2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 lastVel = rb.velocity;
        GatherInput();
        if (input.reload)
        {
            GameManager.instance.ReloadThisLevel();
        }
        DoCollideCheck();
        DecideUserStatus();
        ChangeUserFacing();
        MovePlayer();
        MDebug.Log("status: {0}/{1}, velocity: {2}/{3}", lastUst, ust, lastVel, rb.velocity);
    }

    void GatherInput()
    {
        input.x = Input.GetAxisRaw("Horizontal");
        input.jump = Input.GetKeyDown(KeyCode.Space);
        input.reload = Input.GetKeyDown(KeyCode.R);
    }

    void DoCollideCheck()
    {
        // TODO@k1 [IMPORTANT] make this part pure and move out to another class
        isGrounded = GroundedCheck();
        isTowardsWall = HeadingTowardsWallCheck(input.x);
        isWallCollide = IsWallCollide();
    }

    void DecideUserStatus()
    {
        PlayerStatus nextStatus = ust;

        if (isGrounded && (isTowardsWall || rb.velocity.x != 0))
        {
            nextStatus = PlayerStatus.RUNNING;
        }
        else if (isGrounded && rb.velocity.x == 0)
        {
            nextStatus = PlayerStatus.IDLE;
        }
        else if (!isGrounded && ust != PlayerStatus.DOUBLE_JUMPING)
        {
            if (rb.velocity.y >= 0)
            {
                nextStatus = PlayerStatus.RISING;
            }
            else
            {
                nextStatus = PlayerStatus.FALLING;
            }
        }

        statusManager.ChangeUserStatus(nextStatus);
    }

    void MovePlayer()
    {
        if (CanJump() && input.jump)
        {
            PlayerJump();
        }
        else if (CanWallJump() && input.jump)
        {
            PlayerWallJump();
            statusManager.ChangeUserStatus(PlayerStatus.DOUBLE_JUMPING);
        }
        else if (CanDoubleJump() && input.jump)
        {
            PlayerDoubleJump();
            statusManager.ChangeUserStatus(PlayerStatus.DOUBLE_JUMPING);
        }

        if (CanRun())
        {
            PlayerRun();
        }
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
        rb.velocity = new Vector2(rb.velocity.x, jumpingForce);
    }

    void PlayerDoubleJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpingForce);
    }

    void PlayerWallJump()
    {
        rb.velocity = new Vector2(wallJumpForce, jumpingForce);
        if (isWallCollide == 1)
        {
            rb.velocity = new Vector2(-wallJumpForce, jumpingForce);
        }
        wallJumpFlag = true;
        Invoke(nameof(ResetWallJumpFlag), wallJumpInputColdSec);
    }

    void PlayerRun()
    {
        rb.velocity = new Vector2(runningSpeed * input.x, rb.velocity.y);
    }

    #endregion

    #region Check Status
    bool CanJump()
    {
        return ust == PlayerStatus.IDLE || ust == PlayerStatus.RUNNING;
    }

    bool CanDoubleJump()
    {
        return ust == PlayerStatus.FALLING || ust == PlayerStatus.RISING;
    }

    bool CanWallJump()
    {
        return isWallCollide != 0 && (CanDoubleJump() || ust == PlayerStatus.DOUBLE_JUMPING);
    }

    bool CanRun()
    {
        return ust != PlayerStatus.WALL_HANGING && !wallJumpFlag;
    }

    #endregion

    #region Collide Check
    bool GroundedCheck()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0, Vector2.down, groundDetectUnit, groundLayerMask);
    }

    bool HeadingTowardsWallCheck(float direction)
    {
        return direction != 0 && Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0, Vector2.right * direction, wallDetectUnit, groundLayerMask);
    }

    int IsWallCollide()
    {
        if (Physics2D.BoxCast(wallChecker.bounds.center, wallChecker.bounds.size, 0, Vector2.right, wallDetectUnit, groundLayerMask))
        {
            return 1;
        }
        else if (Physics2D.BoxCast(wallChecker.bounds.center, wallChecker.bounds.size, 0, Vector2.left, wallDetectUnit, groundLayerMask))
        {
            return -1;
        }
        else
        {
            return 0;
        }

    }
    #endregion

    void ResetWallJumpFlag()
    {
        wallJumpFlag = false;
    }
}

