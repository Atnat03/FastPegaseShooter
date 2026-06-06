using System;
using System.Collections;
using MyPrint;
using TMPro;
using UnityEngine;

public class ShootEnergyView : MonoBehaviour
{
	#region Properties

	#endregion


	#region Variables

	[SerializeField] private ShootEnergy _shootEnergy;
	[SerializeField] private GunSwitching _gunSwitching;
	
	[Header("Model")]
	[SerializeField] private MeshRenderer _modelRenderer;
	[SerializeField] private Material[] _modelMaterial;
	
	[Header("Messages")]
	[SerializeField] private TextMeshProUGUI _textCantThrow;
	[SerializeField] private float _timeMessageStayOnScreen = 1;
	[SerializeField, Tooltip("0 = pas assez d'energie / 1 = ne vise pas son pote / 2 = ne vise pas son pote + pas assez d'energie")]
	private string[] _messagesCantThrow;

	[Header("DetectBro")] 
	[SerializeField] private GameObject _uiTarget;
	
	[Header("Laser")]
	[SerializeField] private LineRenderer _laser;
	[SerializeField] private Gradient[] _laserColors;
	[SerializeField] private Transform _laserSpawnPoint;
	private Vector3 _targetLaserPos;

	#endregion


	#region Fonctions

	private void Start()
	{
		_textCantThrow.gameObject.SetActive(false);
	}

	private void OnEnable()
	{
		_gunSwitching.OnSwapGun += SetUpColor;
		_shootEnergy.CantThrowEnergy += CantThrowEnergy;
		_shootEnergy.OnDetectBro += DetectBro;
		_shootEnergy.OnLaserActivate += ActivatedLaser;
	}

	private void ActivatedLaser(bool isActive, Vector3 endPos)
	{
		_laser.gameObject.SetActive(isActive);

		if (isActive)
		{
			_targetLaserPos = endPos; // + Vector3.up;
		}
	}

	private void CantThrowEnergy(int index)
	{
		if(!_textCantThrow.gameObject.activeSelf)
		{
			_textCantThrow.text = _messagesCantThrow[index];
			StartCoroutine(MessageCantThrow());
		}
	}

	IEnumerator MessageCantThrow()
	{
		_textCantThrow.gameObject.SetActive(true);
		yield return new WaitForSeconds(_timeMessageStayOnScreen);
		_textCantThrow.gameObject.SetActive(false);
	}

	private void SetUpColor(bool isPositive)
	{
		_modelRenderer.material = isPositive ? _modelMaterial[0] : _modelMaterial[1];
		_laser.colorGradient = isPositive ? _laserColors[0] : _laserColors[1];
	}
	
	private void DetectBro(bool hasDetect, Vector3 pos)
	{
		_uiTarget.SetActive(hasDetect);
			
		Canvas canvas = _uiTarget.GetComponentInParent<Canvas>();

		RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, pos, canvas.worldCamera, out Vector2 localPos);

		_uiTarget.GetComponent<RectTransform>().localPosition = localPos;
	}

	private void Update()
	{
		if(_laser.gameObject.activeSelf)
		{
			_laser.SetPosition(0, Vector3.Lerp(_laser.GetPosition(0), _laserSpawnPoint.position, Time.deltaTime * 25));
			_laser.SetPosition(1, Vector3.Lerp(_laser.GetPosition(1), _targetLaserPos, Time.deltaTime * 25));
		}	
	}

	#endregion
}
