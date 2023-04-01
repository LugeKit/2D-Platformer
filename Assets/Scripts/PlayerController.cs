using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour, IAttackee
{
    class UserInput
    {
        public float x;
        public bool jump;
        public bool dodge;
        public bool attack;
        public bool defense;
    }

    public enum CollisionType
    {
        None,
        Positive,
        Negtive,
        Both
    }

    // TODO@k1 implement jump cut and fast falling in the future

    #region Variables
    [SerializeField] PlayerMovementSetting movementSetting;
    [SerializeField] PlayerCombatSetting combatSetting;
    [SerializeField] LayerMask groundLayerMask;
    [SerializeField] Collider2D airColl;
    [SerializeField, ReadOnly] CollisionType verticalColl;
    [SerializeField, ReadOnly] CollisionType horizontalColl;
    [SerializeField, ReadOnly] float coyoteTime;
    [SerializeField, ReadOnly] float lastPressedJumpTime;

    PlayerStatusManager stMgr;
    UserInput input = new();
    Rigidbody2D rb;
    Collider2D coll;
    SpriteRenderer sr;
    PlayerStatus ust => stMgr.userStatus;

    // dodgeSpeed: use to keep the speed during dodging is constant
    float dodgeSpeed;

    // coroutine for resuming from attack state
    Coroutine resumeFromAtk;
    int attackIndex = -1;
    float curAtkBeginTime = 0;

    // defense related
    float startDefenseTime;

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
        SimulateDrag();
        GatherInput();
        DoCollisionCheck();
        UpdateTimer();
        
        if (ust == PlayerStatus.IDLE || ust == PlayerStatus.RUNNING || ust == PlayerStatus.RISING || ust == PlayerStatus.FALLING || ust == PlayerStatus.DOUBLE_JUMPING)
            ChangeUserFacing();

        PerformAction();
    }

    private void LateUpdate()
    {
        DecideUserStatus();
    }

    void SimulateDrag()
    {
        var currVelX = rb.velocity.x;
        var dragV = Mathf.Max(Mathf.Abs(currVelX), movementSetting.MinHorizontalDragVelocity);
        var dragForce = -1 * Mathf.Sign(currVelX) * dragV * movementSetting.HorizontalDrag * Time.deltaTime;
        var finalVelX = currVelX + dragForce / rb.mass;


        if (Mathf.Sign(finalVelX) == Mathf.Sign(currVelX))
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
        input.dodge = Input.GetKeyDown(KeyCode.LeftShift);
        input.attack = Input.GetKeyDown(KeyCode.J);
        input.defense = Input.GetKey(KeyCode.K);
    }

    void DoCollisionCheck()
    {
        verticalColl = VerticalCollideCheck();
        horizontalColl = HorizontalCollideCheck();
    }

    void DecideUserStatus()
    {
        var nextStatus = ust;

        if (ust == PlayerStatus.DODGE || ust == PlayerStatus.ATTACK || ust == PlayerStatus.DEFENSE)
            // these states is changed by input or event, not by velocity or collide
            nextStatus = ust;
        else if (IsGrounded() && (IsRunningTowardsWall() || rb.velocity.x != 0))
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
        curAtkBeginTime += Time.deltaTime;

        if (ust == PlayerStatus.DEFENSE)
            startDefenseTime += Time.deltaTime;

        coyoteTime -= Time.deltaTime;
        if (ust == PlayerStatus.IDLE || ust == PlayerStatus.RUNNING)
            coyoteTime = movementSetting.CoyoteTime;

        lastPressedJumpTime -= Time.deltaTime;
        if (input.jump)
            lastPressedJumpTime = movementSetting.JumpBufferTime;
    }

    void PerformAction()
    {
        // Change status when defense holding up. Maybe some problems here? If works just let it be.
        if (ust == PlayerStatus.DEFENSE && !input.defense)
            stMgr.ChangeUserStatus(PlayerStatus.IDLE);

        if (CanAttack())
        {
            var nextAtk = ProceedToNextAtk();

            if (nextAtk != null)
            {
                // TODO@k1 this should check delay. Now the hitbox appears when the player hits attack instantly, which is not good.
                PlayerAttackHitCheck(nextAtk);

                curAtkBeginTime = 0;
                stMgr.ChangeUserStatus(PlayerStatus.ATTACK);
                stMgr.TriigerNextAttack(); // for animation transition
                SetAttackResume(StartCoroutine(AttackResume(nextAtk.EndAttackDelaySec)));
            }
        }

        if (CanDefense())
        {
            startDefenseTime = 0;
            stMgr.ChangeUserStatus(PlayerStatus.DEFENSE);
        }

        if (CanDodge())
        {
            dodgeSpeed = Mathf.Sign(input.x) * combatSetting.DodgeSpeed;
            stMgr.ChangeUserStatus(PlayerStatus.DODGE);
            Invoke(nameof(PlayerDodgeResume), combatSetting.DodgeTime);
        }

        if (ust == PlayerStatus.DODGE)
            // remove the effects of all drag/force/else
            rb.velocity = new Vector2(dodgeSpeed, rb.velocity.y);

        if (CanJump())
        {
            PlayerJump();
        }
        else if (CanDoubleJump() && input.jump)
        {
            PlayerDoubleJump();
            stMgr.ChangeUserStatus(PlayerStatus.DOUBLE_JUMPING);
        }

        if (CanHorizontalMove())
            PlayerHorizontalMove();
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
        rb.AddForce(movementSetting.JumpForce * Vector2.up, ForceMode2D.Impulse);
    }

    void PlayerDodgeResume()
    {
        stMgr.ChangeUserStatus(PlayerStatus.IDLE);
    }

    void PlayerDoubleJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0); // set Vy to be 0 will make we jump the same height every time
        rb.AddForce(movementSetting.DoubleJumpForce * Vector2.up, ForceMode2D.Impulse);
    }

    void PlayerHorizontalMove()
    {
        rb.AddForce(input.x * movementSetting.AccelerationForce * Time.deltaTime * Vector2.right, ForceMode2D.Impulse);
    }

    void PlayerAttackHitCheck(AttackElement e)
    {
        var hit = Physics2D.OverlapCircle(e.HitboxCenterOffset, e.HitboxRadius, combatSetting.EnemyLayer);
        if (hit != null)
            MDebug.Log("hit successfully");
    }

    void SetAttackResume(Coroutine c)
    {
        if (resumeFromAtk != null)
            StopCoroutine(resumeFromAtk);
        resumeFromAtk = c;
    }

    IEnumerator AttackResume(float resumeTime)
    {
        yield return new WaitForSeconds(resumeTime);
        stMgr.ChangeUserStatus(PlayerStatus.IDLE);
        attackIndex = -1;
    }
    #endregion

    #region Check movement enabled or not
    bool CanDodge()
    {
        return combatSetting.EnableDodge 
            && input.dodge 
            && input.x != 0
            && (ust == PlayerStatus.IDLE || ust == PlayerStatus.RUNNING);
    }

    bool CanJump()
    {
        return movementSetting.EnableJump
            && (input.jump || lastPressedJumpTime > 0)
            && (ust == PlayerStatus.IDLE || ust == PlayerStatus.RUNNING || (ust == PlayerStatus.FALLING && coyoteTime > 0 && coyoteTime > lastPressedJumpTime));
    }

    bool CanDoubleJump()
    {
        return movementSetting.EnableDoubleJump && (ust == PlayerStatus.FALLING || ust == PlayerStatus.RISING);
    }

    bool CanHorizontalMove()
    {
        return input.x != 0
            && (ust == PlayerStatus.IDLE || ust == PlayerStatus.RUNNING || ust == PlayerStatus.FALLING || ust == PlayerStatus.RISING || ust == PlayerStatus.DOUBLE_JUMPING);
    }

    bool CanAttack()
    {
        return input.attack && combatSetting.EnableAttack
            && (ust == PlayerStatus.IDLE || ust == PlayerStatus.RUNNING || ust == PlayerStatus.ATTACK);
    }

    bool CanDefense()
    {
        return input.defense && combatSetting.EnableDefense && (ust == PlayerStatus.IDLE || ust == PlayerStatus.RUNNING);
    }

    AttackElement ProceedToNextAtk()
    {
        if (attackIndex == -1)
        {
            if (combatSetting.Attacks.Length <= 0)
                return null;
            attackIndex = 0;
            return combatSetting.Attacks[0];
        }

        if (attackIndex < 0 || attackIndex >= combatSetting.Attacks.Length)
        {
            MDebug.Log("[Error] attackIndex out of bound. Current index: {0}, max index: {1}", attackIndex, combatSetting.Attacks.Length);
            return null;
        }

        var curAtkEle = combatSetting.Attacks[attackIndex];
        if (curAtkBeginTime <= curAtkEle.NextAttackDelaySec)
            return null;

        attackIndex++;
        if (attackIndex == combatSetting.Attacks.Length)
        {
            attackIndex = -1;
            return null;
        }

        return combatSetting.Attacks[attackIndex];
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

        if (Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0, Vector2.up, movementSetting.CollisionDetectDistance.y, groundLayerMask))
        {
            res = CollisionType.Positive;
        }
        if (Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0, Vector2.down, movementSetting.CollisionDetectDistance.y, groundLayerMask))
        {
            res = (res == CollisionType.Positive) ? CollisionType.Both : CollisionType.Negtive;
        }

        return res;
    }
    CollisionType HorizontalCollideCheck()
    {
        var res = CollisionType.None;

        if (Physics2D.BoxCast(airColl.bounds.center, airColl.bounds.size, 0, Vector2.right, movementSetting.CollisionDetectDistance.x, groundLayerMask))
        {
            res = CollisionType.Positive;
        }
        if (Physics2D.BoxCast(airColl.bounds.center, airColl.bounds.size, 0, Vector2.left, movementSetting.CollisionDetectDistance.x, groundLayerMask))
        {
            res = (res == CollisionType.Positive) ? CollisionType.Both : CollisionType.Negtive;
        }

        return res;

    }

    public void Hit(float damage)
    {
        MDebug.Log("Got hit! Damage: {0}", damage);
    }
    #endregion
}

