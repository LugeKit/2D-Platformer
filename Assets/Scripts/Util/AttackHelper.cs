using UnityEngine;

public class AttackHelper
{
    static public IAttackee GetAttackee(GameObject target, float damage)
    {
        var monos = target.GetComponents<MonoBehaviour>();
        foreach (var mono in monos)
        {
            var cvt = mono as IAttackee;
            if (cvt != null)
            {
                return cvt;
            }
        }
        return null;
    }
}
