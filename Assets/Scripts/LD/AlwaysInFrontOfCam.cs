using System;
using FishNet;
using UnityEngine;

public class AlwaysInFrontOfCam : MonoBehaviour
{
	#region Properties

	#endregion


	#region Variables

	Camera cam;
	[SerializeField] float _distanceMax = 100f;
	[SerializeField] private bool _scaleModifier = true;
	
	#endregion


	#region Fonctions

	private void Awake()
	{
		//cam = InstanceFinder.ClientManager.Connection.FirstObject.GetComponent<FPSController>().Camera;
	}

	private void LateUpdate()
	{
		if (cam != null)
		{
			transform.LookAt(cam.transform.position);
			
			if(_scaleModifier)
				transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 10,  Vector3.Distance(cam.transform.position, transform.position) / _distanceMax);
		}
	}

	#endregion
}
