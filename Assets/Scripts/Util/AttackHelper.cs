using UnityEngine;

public class AttackHelper
{
    static public bool Attack(GameObject target, float damage)
    {
        var monos = target.GetComponents<MonoBehaviour>();
        foreach (var mono in monos)
        {
            var cvt = mono as IAttackee;
            if (cvt != null)
            {
                // TODO@k1 only check the first match, is this what we want?
                cvt.Hit(damage);
                return true;
            }
        }
        return false;
    }
}
