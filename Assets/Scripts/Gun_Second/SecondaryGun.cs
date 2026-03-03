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

	#endregion
}
