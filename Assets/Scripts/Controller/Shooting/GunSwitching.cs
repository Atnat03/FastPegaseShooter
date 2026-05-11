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
	public GameObject CurrentMainGun => _mainGunsList[_currentMainGun.Value];
	public IGun IGunMain => _currentMainIGun;
	public ISurcharge ISurchargeMain => _currentISurcharge;
	
	public int CurrentMainGunIndex => _currentMainGun.Value;

	public bool IsSwitching => !_canSwitch;
	
	public bool IsPositive => _isPositiveChargedPlayer.Value;
	
	#endregion
	
	#region Variables

	private readonly SyncVar<bool> _isMainGun = new SyncVar<bool>(true);
	private readonly SyncVar<bool> _isPositiveChargedPlayer = new SyncVar<bool>(false);
	
	[Header("References")]
	[SerializeField] private GameObject _mainGunParent;
	[SerializeField] private GrenadeThrower _throwerGrenade;
	[SerializeField] private DroneThrower _throwerDrone;
	[SerializeField] private ReticulesManager _reticuleManager;
	
	[Header("Settings")]
	[SerializeField] private float _cooldownChangeMagnetic = 5f;
	
	private bool _canSwitch = true;
	private bool _canChangemagnetic = true;
	private List<GameObject> _mainGunsList;
	
	private readonly SyncVar<int> _currentMainGun = new SyncVar<int>(0);
	
	//Actions
	public Action OnStartSwitchGun;
	public Action OnEndSwitchGun;
	public Action<bool> OnSwapGun;

	private IGun _currentMainIGun;
	private ISurcharge _currentISurcharge;
	
	#endregion

	#region Fonctions

	public override void OnStartNetwork()
	{
		_currentMainGun.OnChange += OnCurrentGunMainChange;
		
		_mainGunsList = new List<GameObject>();
		
		foreach (Transform gun in _mainGunParent.transform)
		{
			_mainGunsList.Add(gun.gameObject);
		}
	}

	public void Initialize(int startIndex)
	{
		Cons.Print("Connected", ColorConsole.Green);
		
		_currentMainGun.Value = startIndex;
		
		ActivateCurrentGun(_mainGunsList, startIndex);
		
		_currentMainIGun = CurrentMainGun.GetComponent<IGun>();
		_currentISurcharge = CurrentMainGun.GetComponent<ISurcharge>();

		_isPositiveChargedPlayer.Value = startIndex == 0;
		
		_currentMainIGun.SetChargedPlayer(_isPositiveChargedPlayer.Value);
		
		OnSwapGun?.Invoke(_isPositiveChargedPlayer.Value);
	}

	[ServerRpc]
	public void RequestChangeMagneticCharge(int playerId)
	{
		ChangeMagneticCharge(playerId);
	}
	
	public void ChangeMagneticCharge(int pId)
	{
		if (!IsServerInitialized) return;
		if (!_canChangemagnetic) return;
		
		_isPositiveChargedPlayer.Value = !_isPositiveChargedPlayer.Value;
		
		_currentMainIGun.SetChargedPlayer(_isPositiveChargedPlayer.Value);

		InvokeEvent(new OnPlayerChangeMagneticCharge
		{
			playerId = pId,
			isPositiveCharged = _isPositiveChargedPlayer.Value
		});

		_canChangemagnetic = false;
		
		UpdateUIChargeObserversRpc();
	}

	[ObserversRpc]
	private void UpdateUIChargeObserversRpc()
	{
		OnSwapGun?.Invoke(_isPositiveChargedPlayer.Value);

		StartCoroutine(CooldownChargeMagnetic());
	}

	IEnumerator CooldownChargeMagnetic()
	{
		yield return new WaitForSeconds(_cooldownChangeMagnetic);

		_canChangemagnetic = true;
	}

	private void ActivateCurrentGun(List<GameObject> list, int index)
	{
		for (int i = 0; i < list.Count; i++)
		{
			if(i == index)
				list[i].gameObject.SetActive(true);
			else
				list[i].gameObject.SetActive(false);
		}

		IGunMain?.SetReticule(_reticuleManager);
	}

	public void DesactivateAllMainGun()
	{
		foreach (GameObject t in _mainGunsList)
		{
			t.gameObject.SetActive(false);
		}
	}
	
	[ServerRpc]
	public void ChangeCurrentGun_Main_ServerRpc(int newIndex)
	{
		if (!IsMainGun) return;
		
		Cons.Print("change gun", ColorConsole.Orange);
		
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
		_currentMainGun.Value = newIndex;
		IGunMain.SetChargedPlayer(_isPositiveChargedPlayer.Value);
	}
		
	private void OnCurrentGunMainChange(int prev, int next, bool asServer)
	{
		if (_mainGunsList == null || _mainGunsList.Count == 0) return;
       
		if (next < _mainGunsList.Count)
		{
			_currentMainIGun = CurrentMainGun.GetComponent<IGun>();
			_currentISurcharge = CurrentMainGun.GetComponent<ISurcharge>();
			
			_currentISurcharge.StopReload();
			
			if (IsOwner)
			{
				CurrentMainGun.GetComponent<GunController>().p_authorizedToShoot = true;
				CurrentMainGun.GetComponent<GunController>().StopReload();
			}
          
			if (!asServer)
			{
				ActivateCurrentGun(_mainGunsList, next);
			}
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

		ActivateCurrentGun(_mainGunsList, _currentMainGun.Value);

		GunController g = CurrentMainGun.GetComponent<GunController>();
		
		Cons.PrintBool(g != null, "Gun controller not null");
		
		if (g.IsFullAuto)
		{
			g.ApplyShoot();
		}
	}

	private void OnEnable()
	{
		_throwerGrenade.OnStartThrow += DesactivateGunWhenThrow;
		_throwerDrone.OnThrowing += DesactivateGunWhenThrow;
		
	}
	
	private void OnDisable()
	{
		_throwerGrenade.OnStartThrow -= DesactivateGunWhenThrow;
		_throwerDrone.OnThrowing -= DesactivateGunWhenThrow;
	}

	#endregion
}

public struct OnPlayerChangeMagneticCharge
{
	public int playerId;
	public bool isPositiveCharged;
}