using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MagneticChargeView : MonoBehaviour
{
	#region Properties

	#endregion


	#region Variables

	[SerializeField] private GunSwitching _gunSwitching;
	
	[Header("Particles")]
	[SerializeField] private ParticleSystem _positiveParticles;
	[SerializeField] private ParticleSystem _negativeParticles;
    /*
	[Header("UI")]
	[SerializeField] private Transform _imagePositive;
	[SerializeField] private Image _imageSelectPositive;
	[SerializeField] private Transform _imageNegative;
	[SerializeField] private Image _imageSelectNegative;
	[SerializeField] private Material _materialLine;*/

	#endregion


	#region Fonctions

	private void OnEnable()
	{
		_gunSwitching.OnSwapGun += UpdateUI;
	}

	private void UpdateUI(bool positive)
	{
		_positiveParticles.gameObject.SetActive(positive);
		_negativeParticles.gameObject.SetActive(!positive);
		
		//StartCoroutine(AnimationChargeSwap(positive));
	}

	/*IEnumerator AnimationChargeSwap(bool positive)
	{
		float duration = 0.5f;
		float elapsedTime = 0f;

		_imageSelectPositive.gameObject.SetActive(positive);
		_imageSelectNegative.gameObject.SetActive(!positive);
		
		Vector3 negativeTargetScale = positive ? Vector3.one : Vector3.one * 1.2f;
		Vector3 positiveTargetScale = positive ? Vector3.one * 1.2f : Vector3.one;
		
		while (elapsedTime < duration)
		{
			elapsedTime += Time.deltaTime;
			
			_imagePositive.localScale = Vector3.Lerp(_imagePositive.localScale, positiveTargetScale, elapsedTime / duration);
			_imageNegative.localScale = Vector3.Lerp(_imagePositive.localScale, negativeTargetScale, elapsedTime / duration);
			
			yield return null;
		}

		_materialLine.SetFloat("_Speed", positive ? 1 : -1);
		_materialLine.SetColor("_Color", positive ? Color.red : Color.dodgerBlue);
		
	}*/

	#endregion
}
