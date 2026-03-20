using FishNet.Object;
using UnityEngine;

public interface IDamagable
{
    /// <summary>
    /// Used to apply damage to the damagable object.
    /// This method should only be called from server side
    /// </summary>
    /// <param name="rawDamageAmount">The base amount of damage inflicted by the weapon</param>
    /// <param name="isCritical">Tell the enemy is supposed to take a critical damage from overload</param>
    /// <returns>whereas the damages were critical or not, may be different from parameter due to internal logic</returns>
    public bool TakeDamage(int rawDamageAmount, bool isCritical = false);
    public void Death(int takenDamages);
}

public interface IHealable : IDamagable
{
    
    public void HealC(int healingAmount);
}
