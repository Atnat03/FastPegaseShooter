using UnityEngine;

public class FireEffectGunView : MonoBehaviour
{

	#region Variables
	
	[SerializeField] private FireEffectGun _fireEffect;
	[SerializeField] private ParticleSystem _fireEffectParticle;
	
	#endregion

	#region Fonctions

	void OnEnable()
	{
		_fireEffect.OnApplyEffect += ApplyViewEffect;
	}

	private void ApplyViewEffect(Vector3 rotation)
	{
		Destroy(Instantiate(_fireEffectParticle, transform.position + rotation * 0.01f, Quaternion.Euler(rotation)), 1f);
	}

	#endregion
}
