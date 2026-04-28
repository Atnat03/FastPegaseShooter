using System;
using UnityEngine;

public class ReticulesManager : MonoBehaviour
{
	#region Properties

	#endregion


	#region Variables

	[SerializeField] private GameObject[] _reticules;
	
	#endregion


	#region Fonctions

	private void Start()
	{
		ActivateReticules(0);
	}

	public void ActivateReticules(int id)
	{
		for (int i = 0; i < _reticules.Length; i++)
		{
			if (i == id)
			{
				_reticules[i].SetActive(true);
			}
			else
			{
				_reticules[i].SetActive(false);
			}
		}
	}
	
	#endregion
}
