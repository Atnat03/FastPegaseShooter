using System;
using System.Collections;
using MyPrint;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShootEnergyView : MonoBehaviour
{

	#region Variables

	[SerializeField] private ShootEnergy _shootEnergy;
	[SerializeField] private GunSwitching _gunSwitching;
	[SerializeField] private PlayerEnergizedState _playerEnergizedState;
	
	[Header("Messages")]
	[SerializeField] private TextMeshProUGUI _textCantThrow;
	[SerializeField] private float _timeMessageStayOnScreen = 1;
	[SerializeField, Tooltip("0 = pas assez d'energie / 1 = ne vise pas son pote / 2 = ne vise pas son pote + pas assez d'energie")]
	private string[] _messagesCantThrow;

	[Header("DetectBro")] 
	[SerializeField] private GameObject _uiTarget;
	[SerializeField] private Image _imageTarget;
	[SerializeField] private Color[] _targetColors;
	Animator _targetAnimator;
	
	[Header("Laser")]
	[SerializeField] private GameObject[] _lasers;
	[SerializeField] private GameObject[] _lasersTPS;
	[SerializeField] private Transform _laserSpawnPoint;
	private GameObject _assignedLaser;
	private GameObject _assignedLaserTPS;
	private Vector3 _targetLaserPos;
	
	private Coroutine _messageCoroutine;

	#endregion
	
	#region Fonctions

	private void Awake()
	{
		_textCantThrow.gameObject.SetActive(false);
		_targetAnimator = _uiTarget.GetComponent<Animator>();
    
		_assignedLaser = _lasers[0];
		_assignedLaserTPS = _lasersTPS[0];
	}

	private void Start()
	{
		SetUpColor(_gunSwitching.IsPositive);
	}

	private void OnEnable()
	{
		_gunSwitching.OnSwapGun += SetUpColor;
		_shootEnergy.CantThrowEnergy += CantThrowEnergy;
		_shootEnergy.OnDetectBro += DetectBro;
		_shootEnergy.OnLaserActivate += ActivatedLaser;
		_shootEnergy.OnTPSLaserActivate += ActivatedLaserTPS;
	}

	
	void OnDestroy()
	{
		_gunSwitching.OnSwapGun -= SetUpColor;
		_shootEnergy.CantThrowEnergy -= CantThrowEnergy;
		_shootEnergy.OnDetectBro -= DetectBro;
		_shootEnergy.OnLaserActivate -= ActivatedLaser;
		_shootEnergy.OnTPSLaserActivate -= ActivatedLaserTPS;
	}
	

	private void ActivatedLaser(bool isActive, Vector3 endPos)
	{
		if (_assignedLaser == null) return;
		
		_targetAnimator.SetBool("IsShooting", isActive);
		_assignedLaser.gameObject.SetActive(isActive);
		_assignedLaserTPS.gameObject.SetActive(isActive);
		
		_imageTarget.color = isActive ? _targetColors[0] : _targetColors[1];
		
		if (isActive)
		{
			_targetLaserPos = endPos; // + Vector3.up;
		}
	}

	private void CantThrowEnergy(int index)
	{
		if (_messageCoroutine != null)
			StopCoroutine(_messageCoroutine);
    
		_textCantThrow.text = _messagesCantThrow[index];
		_messageCoroutine = StartCoroutine(MessageCantThrow());
	}

	IEnumerator MessageCantThrow()
	{
		_textCantThrow.gameObject.SetActive(true);
		yield return new WaitForSeconds(_timeMessageStayOnScreen);
		_textCantThrow.gameObject.SetActive(false);
		_messageCoroutine = null;
	}

	private void SetUpColor(bool isPositive)
	{
		_assignedLaser = isPositive ? _lasers[0] : _lasers[1];
		_assignedLaserTPS = isPositive ? _lasersTPS[0] : _lasersTPS[1];
	}
	
	private void DetectBro(bool hasDetect, Vector3 pos)
	{
		_uiTarget.SetActive(hasDetect);
			
		Canvas canvas = _uiTarget.GetComponentInParent<Canvas>();

		RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, pos, canvas.worldCamera, out Vector2 localPos);

		_uiTarget.GetComponent<RectTransform>().localPosition = localPos;
	}
	
	private void ActivatedLaserTPS(bool isActive)
	{
		if (_assignedLaserTPS == null) return;
		_assignedLaserTPS.gameObject.SetActive(isActive);
	}

	#endregion
}
