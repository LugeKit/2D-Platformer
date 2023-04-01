using UnityEngine;

public class TriggerAttacker : MonoBehaviour
{
    public float damage;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        var attackee = AttackHelper.GetAttackee(collision.gameObject, damage);
        if (attackee == null)
            return;

        if (attackee.IsInvincible())
            return;

        attackee.Hit(damage);
        Destroy(gameObject);
    }
}
