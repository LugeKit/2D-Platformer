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
}
