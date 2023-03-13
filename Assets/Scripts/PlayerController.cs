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
    const int CollideNone = 0;
    const int CollidePositive = 1;
    const int CollideNegtive = 2;
    const int CollideBoth = 3;

    [SerializeField] float runningSpeed = 10f;
    [SerializeField] float jumpingForce = 10f;
    [SerializeField] float wallJumpForce = 10f;
    [SerializeField] float groundDetectUnit = .1f;
    [SerializeField] float wallDetectUnit = .1f;
    [SerializeField] float wallJumpInputColdSec = .3f;
    [SerializeField] PlayerStatusManager statusManager;
    [SerializeField] LayerMask groundLayerMask;
    [SerializeField] Collider2D airColl;
    [SerializeField, ReadOnlyAttributes] int isVertColl;
    [SerializeField, ReadOnlyAttributes] int isHoriColl;
    [SerializeField, ReadOnlyAttributes] int isAirHoriColl;

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
        GatherInput();

        if (input.reload)
        {
            GameManager.instance.ReloadThisLevel();
        }

        DoCollideCheck();

        DecideUserStatus();

        ChangeUserFacing();

        MovePlayer();
    }

    void GatherInput()
    {
        input.x = Input.GetAxisRaw("Horizontal");
        input.jump = Input.GetKeyDown(KeyCode.Space);
        input.reload = Input.GetKeyDown(KeyCode.R);
    }

    void DoCollideCheck()
    {
        isVertColl = VerticalCollideCheck();
        isHoriColl = HorizontalCollideCheck();
        isAirHoriColl = AirHorizontalCollideCheck(input.x);
    }

    void DecideUserStatus()
    {
        var nextStatus = ust;

        if (IsGrounded() && (IsRunningTowardsWall() || rb.velocity.x != 0))
        {
            nextStatus = PlayerStatus.RUNNING;
        }
        else if (IsGrounded() && rb.velocity.x == 0)
        {
            nextStatus = PlayerStatus.IDLE;
        }
        else if (!IsGrounded() && ust != PlayerStatus.DOUBLE_JUMPING)
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
        if (isAirHoriColl == CollidePositive)
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
        return isAirHoriColl != CollideNone && isAirHoriColl != CollideBoth && (ust == PlayerStatus.FALLING || ust == PlayerStatus.RISING || ust == PlayerStatus.DOUBLE_JUMPING);
    }

    bool CanRun()
    {
        return ust != PlayerStatus.WALL_HANGING && !wallJumpFlag;
    }

    bool IsGrounded()
    {
        return isVertColl == CollideNegtive || isVertColl == CollideBoth;
    }

    bool IsRunningTowardsWall()
    {
        return IsCollideAndInputSameDirection(isHoriColl, input.x);
    }

    bool IsCollideAndInputSameDirection(int collType, float direction)
    {
        if (Mathf.Approximately(direction, 0))
        {
            return false;
        }

        if (collType == CollideNone)
        {
            return false;
        }

        if (collType == CollideBoth)
        {
            return true;
        }

        return (collType == CollidePositive && direction > 0) || (collType == CollideNegtive && direction < 0);
    }
    #endregion

    #region Collide Check
    int VerticalCollideCheck()
    {
        int res = CollideNone;

        if (Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0, Vector2.up, groundDetectUnit, groundLayerMask))
        {
            res = CollidePositive;
        }
        if (Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0, Vector2.down, groundDetectUnit, groundLayerMask))
        {
            res = (res == CollidePositive) ? CollideBoth : CollideNegtive;
        }

        return res;
    }
    int HorizontalCollideCheck()
    {
        int res = CollideNone;

        if (Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0, Vector2.right, wallDetectUnit, groundLayerMask))
        {
            res = CollidePositive;
        }
        if (Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0, Vector2.left, wallDetectUnit, groundLayerMask))
        {
            res = (res == CollidePositive) ? CollideBoth : CollideNegtive;
        }

        return res;
        
    }

    int AirHorizontalCollideCheck(float direction)
    {
        
        int res = CollideNone;

        if (Physics2D.BoxCast(airColl.bounds.center, airColl.bounds.size, 0, Vector2.right, wallDetectUnit, groundLayerMask))
        {
            res = CollidePositive;
        }
        if (Physics2D.BoxCast(airColl.bounds.center, airColl.bounds.size, 0, Vector2.left, wallDetectUnit, groundLayerMask))
        {
            res = (res == CollidePositive) ? CollideBoth : CollideNegtive;
        }

        return res;
    }
    #endregion

    void ResetWallJumpFlag()
    {
        wallJumpFlag = false;
    }
}

