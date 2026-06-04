using System;
using UnityEngine;

public class ArmColorChange : NetworkBusListener
{
	#region Properties

	#endregion


	#region Variables

	[SerializeField] private SkinnedMeshRenderer _renderer;
	[SerializeField] private Material[] _materialsList;
	
	#endregion
	
	#region Fonctions

	private void Awake()
	{
		ListenToEvent<OnPlayerSpawnEvent>(PlayerSpawn);
	}

	private void PlayerSpawn(OnPlayerSpawnEvent data)
	{
		_renderer.material = _materialsList[data.isPositiveCharge ? 0 : 1];
	}

	#endregion
}
