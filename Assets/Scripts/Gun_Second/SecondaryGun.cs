using System;
using FishNet.Object;
using GunDecorator;
using UnityEngine;

public enum Element{ Fire, Ice, Elek }

public class SecondaryGun : NetworkBehaviour, IGun
{
	#region Variables

	[Header("References")]
	[SerializeField] private Element _element;
	[SerializeField] private GameObject _ammoPrefab;
	[SerializeField] private Camera _camera; 
	[SerializeField] private Transform _spawnPoint;
	[SerializeField] private GameObject _model;
	
	[Header("Recoil")]
	private RecoilSecond _recoilModule;
	
	[Header("Settings")]
	[SerializeField] private float _fireRate = 1;
	[SerializeField] private float _maxDistance = 2000f;
	[SerializeField] private float _bulletSpeed = 50;

	
	private bool _canShoot = true;
	private float elapsedTime = 0;
	
	Ray cameraRay;
	RaycastHit hit;

	#endregion
	
	#region Fonctions

	void Start()
	{
		_recoilModule = GetComponent<RecoilSecond>();
	}

	private void Update()
	{
		if (!_canShoot)
		{
			elapsedTime += Time.deltaTime;

			if (elapsedTime >= _fireRate)
			{
				elapsedTime = 0;
				_canShoot = true;
			}
		}
	}

	public void TryFire()
	{
		if (!_canShoot) return;
		
		Debug.Log("Secondary gun fire");

		cameraRay = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

		Vector3 targetPoint;
		float travelTime = 0;
		
		if (Physics.Raycast(cameraRay, out hit, _maxDistance, ~LayerMask.GetMask("Owner", "Other"), QueryTriggerInteraction.Ignore))
		{
			targetPoint = hit.point;
		}
		else
		{
			targetPoint = cameraRay.GetPoint(_maxDistance);
		}
		
		travelTime = Vector3.Distance(_spawnPoint.position, targetPoint) / _bulletSpeed;
		Vector3 direction = _camera.transform.forward.normalized;
		
		_recoilModule?.Recoil();
		
		SpawnVisualBulletServerRpc(direction, targetPoint);
	}

	[ServerRpc]
	private void SpawnVisualBulletServerRpc(Vector3 direction, Vector3 targetPoint)
	{
		SpawnVisualBulletObserverRpc(direction, targetPoint);
	}

	[ObserversRpc]
	private void SpawnVisualBulletObserverRpc(Vector3 direction,Vector3 targetPoint)
	{
		EffectSecondaryGun newBullet = Instantiate(_ammoPrefab, _spawnPoint.position, Quaternion.LookRotation(direction)).GetComponent<EffectSecondaryGun>();
		newBullet.SetUpVariables(_bulletSpeed, targetPoint, (int)_element);
	}


	public void TryCancelShooting()
	{ }

	public void TryReload()
	{ }

	public void TriggerHitMark(bool isCritique = false)
	{ }

	public void TryCharging()
	{ }

	public void TryShootCharged()
	{ }

	public bool IsFullAuto => false;

	public void Disable(bool state)
	{
		_model.SetActive(state);
	}

	public int GetCurrentAmmo()
	{
		return 0;
	}

	public void SetAmmo(int value)
	{ }

	#endregion
}
