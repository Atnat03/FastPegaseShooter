using FishNet.Object;
using UnityEngine;

public interface IDamagable
{
    public bool TakeDamage(int damageAmount, bool isCritical = false);
    public void Death();
}

public interface IHealable : IDamagable
{
    
    public void HealC(int healingAmount);
}
