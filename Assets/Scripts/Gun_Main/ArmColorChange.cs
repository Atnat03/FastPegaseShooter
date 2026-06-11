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

	public override void OnStartNetwork()
	{
		int index = Owner.ClientId % _materialsList.Length;

		if (_renderer != null)
			_renderer.material = _materialsList[index];

		if (_rendererClassic != null)
			_rendererClassic.material = _materialsList[index];
	}
	
	private void Awake()
	{
		ListenToEvent<OnPlayerSpawnEvent>(PlayerSpawn);
	}
	
	private void PlayerSpawn(OnPlayerSpawnEvent data)
	{

	}

	#endregion
}
