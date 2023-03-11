using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatusManager : MonoBehaviour
{

    public Animator playerAnim;
    public PlayerStatus userStatus { get; private set; }

    public void ChangeUserStatus(PlayerStatus newStatus)
    {
        Debug.Log(string.Format("Old user status: {0}, new user status: {1}", userStatus, newStatus));
        if (userStatus == newStatus)
        {
            return;
        }

        userStatus = newStatus;
        playerAnim.SetInteger("userStatus", (int)userStatus);
    }


}
public enum PlayerStatus
{
    IDLE,
    RUNNING,
    RISING,
    FALLING,
    DOUBLE_JUMPING,
    WALL_HANGING
}
