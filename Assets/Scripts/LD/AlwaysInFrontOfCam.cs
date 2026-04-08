using System;
using FishNet;
using FishNet.Object;
using UnityEngine;

public class AlwaysInFrontOfCam : NetworkBehaviour
{
	#region Properties

	#endregion


	#region Variables

	Camera cam;
	[SerializeField] float _distanceMax = 100f;
	[SerializeField] private bool _scaleModifier = true;
	[SerializeField] private Vector2 _scaleMap = new Vector2(0.001f, 0.003f);
	
	#endregion


	#region Fonctions

	public override void OnStartClient()
	{
		if (!IsOwner) return;
		
		cam = InstanceFinder.ClientManager.Connection.FirstObject.GetComponent<FPSController>().Camera;
	}

	private void LateUpdate()
	{
		if (cam != null)
		{
			transform.rotation = Quaternion.LookRotation(cam.transform.position);
			
			if(_scaleModifier)
				transform.localScale = Vector3.Lerp(Vector3.one * _scaleMap.x, Vector3.one * _scaleMap.y,  Vector3.Distance(cam.transform.position, transform.position) / _distanceMax);
		}
	}

	#endregion
}
