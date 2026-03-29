using UnityEngine;

public class ElekEffectGunView : MonoBehaviour
{
	#region Variables
	
	[SerializeField] private ElekEffectGun _elekEffect;
	[SerializeField] private ParticleSystem _elekEffectParticle;
	
	#endregion


	#region Fonctions

	void OnEnable()
	{
		_elekEffect.OnApplyEffect += ApplyViewEffect;
	}

	private void ApplyViewEffect(Vector3 rotation)
	{
		Destroy(Instantiate(_elekEffectParticle, transform.position + rotation * 0.01f, Quaternion.Euler(rotation)), 1f);
	}

	#endregion
}
