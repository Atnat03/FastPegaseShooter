using UnityEngine;

public class FreezeEffectGunView : MonoBehaviour
{

	#region Variables
	
	[SerializeField] private FreezeEffectGun _freezeEffect;
	[SerializeField] private ParticleSystem _freezeEffectParticle;
	
	#endregion


	#region Fonctions

	void OnEnable()
	{
		_freezeEffect.OnApplyEffect += ApplyViewEffect;
	}

	private void ApplyViewEffect(Vector3 rotation)
	{
		Destroy(Instantiate(_freezeEffectParticle, transform.position + rotation * 0.01f, Quaternion.Euler(rotation)), 1f);
	}

	#endregion
}
