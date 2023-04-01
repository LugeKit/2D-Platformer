using UnityEngine;

public class PlayerStatusManager : MonoBehaviour
{

    public Animator playerAnim;
    [field: SerializeField, ReadOnly] public PlayerStatus userStatus { get; private set; }
    public PlayerStatus lastUserStatus { get; private set; }

    public void ChangeUserStatus(PlayerStatus newStatus)
    {
        if (userStatus == newStatus)
            return;

        lastUserStatus = userStatus;
        userStatus = newStatus;
        playerAnim.SetInteger("userStatus", (int)userStatus);
    }

    public void TriigerNextAttack()
    {
        playerAnim.SetTrigger("triggerAttack");
    }


}
public enum PlayerStatus
{
    IDLE,
    RUNNING,
    RISING,
    FALLING,
    DOUBLE_JUMPING,
    WALL_HANGING,
    DODGE,
    ATTACK,
    DEFENSE
}
