using System;
using UnityEngine;

public interface IEffectSecondaryGun
{
	public void ApplyEffect();
}

public class SecondaryGun : MonoBehaviour, IGun
{
	#region Properties

	public IEffectSecondaryGun EffectSecondaryGun { get; set; }
	
	#endregion


	#region Variables
		
	private IEffectSecondaryGun _effect;

	#endregion


	#region Fonctions

	private void Start()
	{
		_effect = GetComponent<IEffectSecondaryGun>();
	}

	public void TryFire()
	{
		Debug.Log("Secondary gun fire");
		
		_effect.ApplyEffect();
	}

	public void TryCancelShooting()
	{
		throw new NotImplementedException();
	}

	public void TryReload()
	{
		throw new NotImplementedException();
	}

	public void TriggerHitMark(bool isCritique = false)
	{
		
	}

	public void TryCharging()
	{
		throw new NotImplementedException();
	}

	public void TryShootCharged()
	{
		throw new NotImplementedException();
	}

	public int GetCurrentAmmo()
	{
		return 0;
	}

	public void SetAmmo(int value)
	{ }

	#endregion
}
