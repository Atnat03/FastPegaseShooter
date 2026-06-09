using System;
using UnityEngine;

public class ArmColorChange : NetworkBusListener
{
	#region Properties

	#endregion


	#region Variables

	[SerializeField] private SkinnedMeshRenderer _renderer;
	[SerializeField] private MeshRenderer _rendererClassic;
	[SerializeField] private Material[] _materialsList;
	
	#endregion
	
	#region Fonctions

	private void Awake()
	{
		ListenToEvent<OnPlayerSpawnEvent>(PlayerSpawn);
	}

	private void PlayerSpawn(OnPlayerSpawnEvent data)
	{
		if(_renderer != null)
			_renderer.material = _materialsList[data.isPositiveCharge ? 0 : 1];
		
		if(_rendererClassic != null)
			_rendererClassic.material = _materialsList[data.isPositiveCharge ? 0 : 1];
	}

	#endregion
}
