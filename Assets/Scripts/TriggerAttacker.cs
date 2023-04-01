using UnityEngine;

public class TriggerAttacker : MonoBehaviour
{
    public float damage;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        AttackHelper.Attack(collision.gameObject, damage);
        Destroy(gameObject);
    }
}
