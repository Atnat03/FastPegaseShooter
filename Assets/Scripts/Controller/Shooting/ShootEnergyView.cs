using System;
using System.Collections;
using MyPrint;
using TMPro;
using UnityEngine;

public class ShootEnergyView : MonoBehaviour
{

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
	Animator _targetAnimator;
	
	[Header("Laser")]
	[SerializeField] private GameObject[] _lasers;
	[SerializeField] private Transform _laserSpawnPoint;
	private GameObject _assignedLaser;
	private Vector3 _targetLaserPos;

	#endregion


	#region Fonctions

	private void Start()
	{
		_textCantThrow.gameObject.SetActive(false);
		
		_targetAnimator = _uiTarget.GetComponent<Animator>();
		
		SetUpColor(_gunSwitching.IsPositive);
	}

	private void OnEnable()
	{
		_gunSwitching.OnSwapGun += SetUpColor;
		_shootEnergy.CantThrowEnergy += CantThrowEnergy;
		_shootEnergy.OnDetectBro += DetectBro;
		_shootEnergy.OnLaserActivate += ActivatedLaser;
	}

	
	void OnDestroy()
	{
		_gunSwitching.OnSwapGun -= SetUpColor;
		_shootEnergy.CantThrowEnergy -= CantThrowEnergy;
		_shootEnergy.OnDetectBro -= DetectBro;
		_shootEnergy.OnLaserActivate -= ActivatedLaser;
	}
	

	private void ActivatedLaser(bool isActive, Vector3 endPos)
	{
		_targetAnimator.SetBool("IsShooting", isActive);
		_assignedLaser.gameObject.SetActive(isActive);
		
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
		Debug.Log("assigned color color is positiv ? :" + isPositive);
		_modelRenderer.material = isPositive ? _modelMaterial[0] : _modelMaterial[1];
		_assignedLaser = isPositive ? _lasers[0] : _lasers[1];
	}
	
	private void DetectBro(bool hasDetect, Vector3 pos)
	{
		_uiTarget.SetActive(hasDetect);
			
		Canvas canvas = _uiTarget.GetComponentInParent<Canvas>();

		RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, pos, canvas.worldCamera, out Vector2 localPos);

		_uiTarget.GetComponent<RectTransform>().localPosition = localPos;
	}

	/*private void Update()
	{
		if(_assignedLaser.gameObject.activeSelf)
		{
			_assignedLaser.transform.LookAt(_targetLaserPos);
		}	
	}*/

	#endregion
}
