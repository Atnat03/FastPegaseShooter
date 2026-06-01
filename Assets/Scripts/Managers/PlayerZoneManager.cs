using System;
using System.Collections.Generic;
using System.Data.Common;
using Controller;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using MyPrint;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Managers
{
    public class PlayerZoneManager : NetworkBusListener
    {
        [Header("Zone")]
        public Dictionary<int, int> p_playerZones = new Dictionary<int, int>();
        private Dictionary<int, bool> _playerCharges = new Dictionary<int, bool>();
        private List<int> _playerIds = new List<int>();

        public override void OnStartServer()
        {
            base.OnStartServer();
            
            ListenToEvent<OnPlayerChangeZone>(OnPlayerChangeZone);
            
            ListenToEvent<OnPlayerChangeMagneticCharge>(OnPlayerChangeMagneticCharge);
            ListenToEvent<OnPlayerSpawnEvent>(OnPlayerSpawn);
        }
        
        private void OnPlayerSpawn(OnPlayerSpawnEvent data)
        {
            RegisterPlayer(data.playerId);

            if (!p_playerZones.ContainsKey(data.playerId))
                p_playerZones[data.playerId] = -1;

            if (!_playerCharges.ContainsKey(data.playerId))
                _playerCharges[data.playerId] = data.isPositiveCharge;

            if (_playerIds.Count >= 2)
            {
                int p1 = _playerIds[0];
                int p2 = _playerIds[1];
                bool isAligned = _playerCharges[p1] == _playerCharges[p2];
            }
        }
        
        private void OnPlayerChangeZone(OnPlayerChangeZone data)
        {
            p_playerZones[data.playerId] = data.newZone;
            RegisterPlayer(data.playerId);
        }

        private void OnPlayerChangeMagneticCharge(OnPlayerChangeMagneticCharge data)
        {
            _playerCharges[data.playerId] = data.isPositiveCharged;
            RegisterPlayer(data.playerId);
        }

        private void RegisterPlayer(int id)
        {
            if (!_playerIds.Contains(id))
                _playerIds.Add(id);
        }
    }
}
public struct OnPolarizationStateChanged
{
    public bool isAligned;
    public bool isSameZone;
    public bool isConflict;
}

public struct OnConflictUIUpdate
{
    public bool isConflict;
    public bool isShortCircuit;
}

public struct OnConflictTimerUIUpdate
{
    public float ratio;
    public bool isShortCircuit;
}

public struct OnShortCircuitDamage
{
    public int damage;
}

public struct OnPlayerSpawnEvent
{
    public int playerId;
    public bool isPositiveCharge;
    public GunSwitching gunSwitching;
    public Transform Transform;
}