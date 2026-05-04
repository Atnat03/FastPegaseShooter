using System;
using System.Collections.Generic;
using MyPrint;
using UnityEngine;

public class PlayerHealthManager : NetworkBusListener
{
	public static PlayerHealthManager Instance { get; private set; }

	private readonly List<PlayerHealth> _registeredPlayers = new();

	public IReadOnlyList<PlayerHealth> RegisteredPlayers => _registeredPlayers;

	public Action OnRegistryUpdated;

	private void Awake()
	{
		Instance = this;
	}

	public void Register(PlayerHealth playerHealth)
	{
		if (!_registeredPlayers.Contains(playerHealth))
		{
			_registeredPlayers.Add(playerHealth);
			OnRegistryUpdated?.Invoke();
		}
	}

	public void Unregister(PlayerHealth playerHealth)
	{
		_registeredPlayers.Remove(playerHealth);
		OnRegistryUpdated?.Invoke();
	}
}

public struct PlayerHealthRegisteredEvent
{
	public PlayerHealth p_playerHealth;
}

public struct PlayerHealthUnregisteredEvent
{
	public PlayerHealth p_playerHealth;
}
