using System;
using FishNet.Object;
using ScriptableObjectsDefinitions;
using UnityEngine;

public class TestSound : NetworkBehaviour
{
	#region Properties

	#endregion


	#region Variables

	[SerializeField] public SoundsDataSO _soundsData;
	[SerializeField] private KeyCode _keySound = KeyCode.Space;

	[SerializeField] private AudioSource _source;
	
	private EventBus _bus;
	private Action _unsubscribeAction;
	
	#endregion

	#region Fonctions

	private void Awake()
	{
		_bus = EventBusInitialiser.instance.Bus;

		_unsubscribeAction = _bus.Subscribe<S_Shoot>(PlaySound);
	}

	private void OnDestroy()
	{
		_unsubscribeAction?.Invoke();
	}

	void PlaySound(S_Shoot _data)
	{
		if (_data.player != NetworkObject) return;
		
		AudioClip clip = SoundManager.GetAudioClip(_data.data, "test");
		SoundManager.PlaySound(clip, _source);
		Debug.Log("Play sound " + transform.name);
	}
	

	#endregion
}

public struct S_Shoot : INetworkEvent
{
	public NetworkObject player { get; set; }
	
	public SoundsDataSO data;
}
