using UnityEngine;

public class PlayerController : MonoBehaviour
{
    class UserInput
    {
        public float x;
        public bool jump;
    }

    [SerializeField] float runningSpeed = 10f;
    [SerializeField] float jumpingForce = 10f;
    [SerializeField] float wallJumpForce = 10f;
    [SerializeField] float groundDetectUnit = .1f;
    [SerializeField] float wallDetectUnit = .1f;
    [SerializeField] PlayerStatusManager statusManager;
    [SerializeField] LayerMask groundLayerMask;

    UserInput input = new();
    Rigidbody2D rb;
    Collider2D coll;
    SpriteRenderer sr;
    PlayerStatus ust => statusManager.userStatus;

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
        DecideUserStatus();
        ChangeUserFacing();
        MovePlayer();
    }

    void GatherInput()
    {
        input.x = Input.GetAxisRaw("Horizontal");
        input.jump = Input.GetKeyDown(KeyCode.Space);
    }

    void DecideUserStatus()
    {
        if (IsGrounded())
        {
            if (rb.velocity.x == 0)
            {
                statusManager.ChangeUserStatus(PlayerStatus.IDLE);
            } else
            {
                statusManager.ChangeUserStatus(PlayerStatus.RUNNING);
            }

            return;
        }

        if (!IsGrounded() && ust != PlayerStatus.DOUBLE_JUMPING && ust != PlayerStatus.WALL_HANGING)
        {
            if (rb.velocity.y >= 0)
            {
                statusManager.ChangeUserStatus(PlayerStatus.RISING);
            } else
            {
                statusManager.ChangeUserStatus(PlayerStatus.FALLING);
            }

            return;
        }
    }

    void MovePlayer()
    {
        Vector2 velBefore = rb.velocity;
        if (CanJump() && input.jump)
        {
            PlayerJump();
        }
        else if (CanDoubleJump() && input.jump)
        {
            PlayerDoubleJump();
            statusManager.ChangeUserStatus(PlayerStatus.DOUBLE_JUMPING);
        }

        if (input.x != 0)
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
        return statusManager.userStatus == PlayerStatus.FALLING || statusManager.userStatus == PlayerStatus.RISING;
    }

    bool IsGrounded()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0, Vector2.down, .1f, groundLayerMask);
    }
    #endregion

}
