using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using GunDecorator;
using MyPrint;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class GunSwitching : NetworkBusListener
{
	#region Properties
	public bool IsMainGun => _isMainGun.Value;
	public GunController CurrentMainGun => _mainGunsList[_currentMainGun.Value];
	public IGun IGunMain => _currentMainIGun;
	public ISurcharge ISurchargeMain => _currentISurcharge;
	public ShootEnergy ShootEnergy => _shootEnergy;
	public int CurrentMainGunIndex => _currentMainGun.Value;

	public bool IsSwitching => !_canSwitch;
	
	public bool IsPositive => _isPositiveChargedPlayer.Value;
	
	#endregion
	
	#region Variables

	private readonly SyncVar<bool> _isMainGun = new SyncVar<bool>(true);
	private readonly SyncVar<bool> _isPositiveChargedPlayer = new SyncVar<bool>(false);
	
	[Header("References")]
	[SerializeField] private GameObject _mainGunParent;
	[SerializeField] private GameObject _mainGunParentTPS;
	[SerializeField] private ShootEnergy _shootEnergy;
	[SerializeField] private DroneThrower _throwerDrone;
	[SerializeField] private ReticulesManager _reticuleManager;
	
	[Header("Settings")]
	[SerializeField] private float _cooldownChangeMagnetic = 5f;
	
	private bool _forceEnergyMode;
	private bool _canSwitch = true;
	[HideInInspector]public List<GunController> _mainGunsList;
	[HideInInspector]public List<GameObject> _mainGunsListTPS;
	
	private readonly SyncVar<int> _currentMainGun = new SyncVar<int>(0);
	
	//Actions
	public Action<bool> OnSwapGun;
	
	private IGun _currentMainIGun;
	private ISurcharge _currentISurcharge;
	
	#endregion

	#region Fonctions

	public override void OnStartNetwork()
	{
		_currentMainGun.OnChange += OnCurrentGunMainChange;
		
		_mainGunsList = new List<GunController>();
		
		foreach (Transform gun in _mainGunParent.transform)
		{
			_mainGunsList.Add(gun.GetComponent<GunController>());
		}
		
		_mainGunsListTPS = new List<GameObject>();
		
		foreach (Transform gun in _mainGunParentTPS.transform)
		{
			_mainGunsListTPS.Add(gun.gameObject);
		}
	}

	public void Initialize(int startIndex)
	{
		Cons.Print("Connected", ColorConsole.Green);

		_currentMainGun.Value = startIndex;
		
		ActivateCurrentGun(_mainGunsList, startIndex);

		_currentMainIGun = CurrentMainGun.GetComponent<IGun>();
		_currentISurcharge = CurrentMainGun.GetComponent<ISurcharge>();
		
		_isMainGun.Value = true;

		if (IsServerInitialized)
		{
			_isPositiveChargedPlayer.Value = (Owner.ClientId == 0);
		}

		_currentMainIGun.SetChargedPlayer(_isPositiveChargedPlayer.Value);
		
		_shootEnergy.gameObject.SetActive(false);

		OnSwapGun?.Invoke(_isPositiveChargedPlayer.Value);
	}

	public void ActivateCurrentGun(List<GunController> list, int index)
	{
		if (_forceEnergyMode)
			return;
		
		for (int i = 0; i < list.Count; i++)
		{
			bool shouldBeActive = (i == index);
			list[i].CurrentModelGun.gameObject.SetActive(shouldBeActive);
		}
		
		for (int i = 0; i < _mainGunsListTPS.Count; i++)
		{
			bool shouldBeActive = (i == index);
			_mainGunsListTPS[i].gameObject.SetActive(shouldBeActive);
		}

		IGunMain?.SetReticule(_reticuleManager);
	}

	public void DesactivateAllMainGun()
	{
		foreach (GunController gun in _mainGunsList)
		{
			gun.CurrentModelGun.gameObject.SetActive(false);
		}
	}
	
	[ServerRpc]
	public void ChangeCurrentGun_Main_ServerRpc(int newIndex)
	{
		if (!IsMainGun) return;
		
		ChangeCurrentGun_Main(newIndex);
		//ChangeMagneticCharge();

		if (_currentMainIGun != null)
		{
			_currentMainIGun.SetChargedPlayer(_isPositiveChargedPlayer.Value);
		}
	}

	[Server]
	public void ChangeCurrentGun_Main(int newIndex)
	{
		if (_currentMainGun.Value == newIndex)
		{
			ActivateCurrentGunObserversRpc(newIndex);
			return;
		}

		_currentMainGun.Value = newIndex;
	}
	
	[ObserversRpc]
	private void ActivateCurrentGunObserversRpc(int index)
	{
		_currentMainIGun = _mainGunsList[index].GetComponent<IGun>();
		_currentISurcharge = _mainGunsList[index].GetComponent<ISurcharge>();
		ActivateCurrentGun(_mainGunsList, index);
	}
		
	private void OnCurrentGunMainChange(int prev, int next, bool asServer)
	{
		if (_mainGunsList == null || _mainGunsList.Count == 0) return;
		if (next >= _mainGunsList.Count) return;

		_currentMainIGun = CurrentMainGun.GetComponent<IGun>();
		_currentISurcharge = CurrentMainGun.GetComponent<ISurcharge>();
    
		_currentISurcharge.StopReload();
    
		if (IsOwner)
		{
			CurrentMainGun.GetComponent<GunController>().p_authorizedToShoot = true;
			CurrentMainGun.GetComponent<GunController>().StopReload();
		}
    
		if (IsMainGun)
			ActivateCurrentGun(_mainGunsList, next);
	}
	
	[ServerRpc]
	public void ChangeGunServerRpc(bool isMain)
	{
		StartCoroutine(DelaySwitch(isMain));
	}

	IEnumerator DelaySwitch(bool isMain)
	{
		PlayAnimationObserverRpc(isMain);
		
		yield return new WaitForSeconds(0.5f);
		
		SetGunModeObserversRpc(isMain);
	}

	[ObserversRpc]
	private void PlayAnimationObserverRpc(bool isMain)
	{
		Animator gun = CurrentMainGun._animator;
		Animator arm = CurrentMainGun._animatorArm;

		string trigger = isMain ? "Prendre" : "Retirer";
		
		Cons.Print("Switch : " + isMain, ColorConsole.Black);
		
		if(gun)
			gun.SetTrigger(trigger);
		
		if(arm)
			arm.SetTrigger(trigger);
	}

	[ObserversRpc]
	private void SetGunModeObserversRpc(bool isMain)
	{
		if(IsServerInitialized)
			_isMainGun.Value = isMain;
		
		_shootEnergy.gameObject.SetActive(!isMain);

		_forceEnergyMode = !isMain;

		if (!isMain)
		{
			DesactivateAllMainGun();
			_shootEnergy.gameObject.SetActive(true);
		}
		else
		{
			_forceEnergyMode = false;
			ActivateCurrentGun(_mainGunsList, _currentMainGun.Value);
			_shootEnergy.gameObject.SetActive(false);
		}
	}
	
	private void DesactivateGunWhenThrow()
	{
		StartCoroutine(DesactivateGun());
	}

	IEnumerator DesactivateGun()
	{
		DesactivateAllMainGun();
		
		yield return new WaitForSeconds(1f);
		
		if (!IsMainGun)
			yield break;

		ActivateCurrentGun(_mainGunsList, _currentMainGun.Value);

		GunController g = CurrentMainGun.GetComponent<GunController>();
		
		if (g.IsFullAuto)
		{
			g.ApplyShoot();
		}
	}
	
	private void OnEnable()
	{
		_throwerDrone.OnThrowing += DesactivateGunWhenThrow;
		
	}
	
	
	private void OnDisable()
	{
		_throwerDrone.OnThrowing -= DesactivateGunWhenThrow;
	}
	
	#endregion
}

public struct OnPlayerChangeMagneticCharge
{
	public int playerId;
	public bool isPositiveCharged;
}