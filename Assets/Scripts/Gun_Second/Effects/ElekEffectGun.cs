using System;
using UnityEngine;

[RequireComponent(typeof(ElekEffectGunView))]
public class ElekEffectGun : EffectSecondaryGun
{
	#region Variables

	public Action<Vector3> OnApplyEffect;

	#endregion


	#region Fonctions

	protected override void ApplyEffect(IDamagable damagable)
	{
		OnApplyEffect?.Invoke(_hit.normal);
		
		base.ApplyEffect(damagable);
		
		Debug.Log(damagable);
	}
	
	#endregion
}
