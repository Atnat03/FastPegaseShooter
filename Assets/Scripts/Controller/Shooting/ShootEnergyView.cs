using System;
using System.Collections;
using UnityEngine;

public class ShootEnergyView : MonoBehaviour
{
	#region Properties

	#endregion


	#region Variables

	[SerializeField] private ShootEnergy _shootEnergy;
	
	[Header("Model")]
	[SerializeField] private MeshRenderer _modelRenderer;
	[SerializeField] private Material[] _modelMaterial;
	
	[Header("Messages")]
	[SerializeField] private GameObject _textCantThrow;
	[SerializeField] private float _timeMessageStayOnScreen = 1;

	[Header("DetectBro")] 
	[SerializeField] private GameObject _uiTarget;

	#endregion


	#region Fonctions

	private void Start()
	{
		_textCantThrow.SetActive(false);
	}

	private void OnEnable()
	{
		_shootEnergy.OnSetUpColor += SetUpColor;
		_shootEnergy.CantThrowEnergy += CantThrowEnergy;
		_shootEnergy.OnDetectBro += DetectBro;
	}

	private void CantThrowEnergy()
	{
		if(!_textCantThrow.gameObject.activeSelf)
			StartCoroutine(MessageCantThrow());
	}

	IEnumerator MessageCantThrow()
	{
		_textCantThrow.SetActive(true);
		yield return new WaitForSeconds(_timeMessageStayOnScreen);
		_textCantThrow.SetActive(false);
	}

	private void SetUpColor(bool isPositive)
	{
		_modelRenderer.material = isPositive ? _modelMaterial[0] : _modelMaterial[1];
	}
	
	private void DetectBro(bool hasDetect, Vector3 pos)
	{
		_uiTarget.SetActive(hasDetect);
			
		Canvas canvas = _uiTarget.GetComponentInParent<Canvas>();

		RectTransformUtility.ScreenPointToLocalPointInRectangle(
			canvas.transform as RectTransform,
			pos,
			canvas.worldCamera,
			out Vector2 localPos);

		_uiTarget.GetComponent<RectTransform>().localPosition = localPos;
	}

	#endregion
}
