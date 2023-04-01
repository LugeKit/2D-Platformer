using UnityEngine;

[CreateAssetMenu(fileName = "PlayerCombatSetting", menuName = "CombatSetting", order = 1)]
public class PlayerCombatSetting: ScriptableObject
{
    [Header("Dodge")]
    public bool EnableDodge;
    public float DodgeTime;
    public float DodgeSpeed; // set speed directly

    [Space(20)]
    [Header("Attack")]
    public bool EnableAttack;
    public LayerMask EnemyLayer;
    public AttackElement[] Attacks;
}

[System.Serializable]
public class AttackElement
{
    public Vector2 HitboxCenterOffset; // hitbox relative to player
    public float HitboxRadius; // TODO@k1 maybe this can change to a Prefab, which means we can just spawn that GameObject and resize it to be attack hitbox trigger
    public float HitboxDelaySec; // delay time for hitbox showing after play attack anim
    public float NextAttackDelaySec; // delay time for enabling inputting next attack
    public float EndAttackDelaySec; // delay time for user to regain control (move, jump, etc.)
}
