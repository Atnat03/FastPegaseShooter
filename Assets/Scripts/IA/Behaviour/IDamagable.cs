using FishNet.Object;
using UnityEngine;

public interface IDamagable
{
    public void TakeDamage(int damageAmount);
    
    
    public void Death();
}

public interface IHealable : IDamagable
{
    
    public void HealC(int healingAmount);
}
