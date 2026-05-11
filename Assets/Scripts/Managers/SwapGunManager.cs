using System;
using System.Collections.Generic;
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
    public class SwapGunManager : NetworkBusListener
    {
        [SerializeField] private float _timeToAcceptSwap = 2f;
        public readonly SyncVar<float> _elapsedTime = new SyncVar<float>();
        [SerializeField] private float _swapingTime;
        [SerializeField] private float _cooldownSwap;
        [SerializeField] private float _swapEnergyGain = 20;
        
        private readonly SyncVar<int> _firstPlayerOwnerId = new SyncVar<int>(-1);

        private readonly SyncVar<bool> _canSwap = new SyncVar<bool>(true);
        private readonly SyncVar<float> _elapsedTimeForCombo = new SyncVar<float>();

        [SerializeField] private bool _instantSwapWithoutBroConsentement;
        [SerializeField] private TextMeshProUGUI _cooldownText;
        
        [Header("Polarized")]
        private readonly SyncVar<bool> _isPolarized = new SyncVar<bool>(false);
        private Dictionary<int, int> _playerZones = new Dictionary<int, int>();
        private Dictionary<int, bool> _playerCharges = new Dictionary<int, bool>();

        [Header("Conflict")]
        [SerializeField] private float _conflictTimerMax = 10f;
        [SerializeField] private float _shortCircuitDamageInterval = 2f;
        [SerializeField] private int _shortCircuitDamage = 10;

        private readonly SyncVar<float> _conflictTimer = new SyncVar<float>(0f);
        private readonly SyncVar<bool> _isInConflict = new SyncVar<bool>(false);
        private readonly SyncVar<bool> _isShortCircuit = new SyncVar<bool>(false);

        private float _shortCircuitDamageElapsed = 0f;
        private List<int> _playerIds = new List<int>();
        
        private NetworkObject _player = null;
        private int _firstGunIndex = -1;
        private int _firstGunAmmo = -1;
        private float _displayedTime = 0f;
        
        public Action<float> OnUpdateAskBroSwap;
        public Action<bool> OnChangeAskText;

        public override void OnStartServer()
        {
            base.OnStartServer();

            _elapsedTimeForCombo.Value = 0;
            _elapsedTime.Value = 0;
            
            ListenToEvent<CallSwapGunEvent>(CheckCanSwapServerRpc);
            ListenToEvent<OnPlayerChangeZone>(OnPlayerChangeZone);
            
            ListenToEvent<OnPlayerChangeZone>(OnPlayerChangeZone);
            ListenToEvent<OnPlayerChangeMagneticCharge>(OnPlayerChangeMagneticCharge);
            ListenToEvent<OnPlayerSpawnEvent>(OnPlayerSpawn);

        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            
            _elapsedTime.OnChange += OnElapsedTimeChanged;
            _elapsedTimeForCombo.OnChange += OnElapsedComboTimeChanged;
            _isInConflict.OnChange += OnConflictChanged;
            _isShortCircuit.OnChange += OnShortCircuitChanged;
            _conflictTimer.OnChange += OnConflictTimerChanged;
        }
        
        private void OnPlayerSpawn(OnPlayerSpawnEvent data)
        {
            RegisterPlayer(data.playerId);

            if (!_playerZones.ContainsKey(data.playerId))
                _playerZones[data.playerId] = 0;

            if (!_playerCharges.ContainsKey(data.playerId))
                _playerCharges[data.playerId] = data.isPositiveCharge;

            if (_playerIds.Count >= 2)
                EvaluateConflict();
        }
        
        private void OnPlayerChangeZone(OnPlayerChangeZone data)
        {
            _playerZones[data.playerId] = data.newZone;
            RegisterPlayer(data.playerId);
            EvaluateConflict();
        }

        private void OnPlayerChangeMagneticCharge(OnPlayerChangeMagneticCharge data)
        {
            _playerCharges[data.playerId] = data.isPositiveCharged;
            RegisterPlayer(data.playerId);
            EvaluateConflict();
        }

        private void RegisterPlayer(int id)
        {
            if (!_playerIds.Contains(id))
                _playerIds.Add(id);
        }
        
        private void EvaluateConflict()
        {
            if (_playerIds.Count < 2) return;

            int p1 = _playerIds[0];
            int p2 = _playerIds[1];

            bool zonesKnown = _playerZones.ContainsKey(p1) && _playerZones.ContainsKey(p2);
            bool chargesKnown = _playerCharges.ContainsKey(p1) && _playerCharges.ContainsKey(p2);

            if (!zonesKnown || !chargesKnown) return;

            bool isAligned = _playerCharges[p1] == _playerCharges[p2];
            bool isSameZone = _playerZones[p1] == _playerZones[p2];

            _canSwap.Value = !isAligned;

            bool conflict = (isAligned && !isSameZone) || (!isAligned && isSameZone);

            _isInConflict.Value = conflict;

            _isPolarized.Value = !isAligned;

            NotifyPolarizationObserversRpc(isAligned, isSameZone, conflict);
        }

        private void Update()
        {
            if (IsServerInitialized)
            {
                if (_elapsedTime.Value > 0)
                {
                    _elapsedTime.Value -= Time.deltaTime;
                    if (_elapsedTime.Value <= 0)
                    {
                        ResetTimer();
                    }
                }
                
                if (_elapsedTimeForCombo.Value > 0)
                {
                    _elapsedTimeForCombo.Value -= Time.deltaTime;
                    if (_elapsedTimeForCombo.Value <= 0)
                    {
                        _canSwap.Value = true;
                    }
                }
                
                UpdateConflictTimer();
                UpdateShortCircuitDamage();
            }
            
            if (_displayedTime > 0)
            {
                _displayedTime -= Time.deltaTime;
                if (_displayedTime < 0) 
                    _displayedTime = 0;
            }
        
            OnUpdateAskBroSwap?.Invoke(_displayedTime / _timeToAcceptSwap);
        }
        
        private void UpdateConflictTimer()
        {
            if (_isInConflict.Value)
            {
                if (_isShortCircuit.Value) return; 

                _conflictTimer.Value += Time.deltaTime;
                if (_conflictTimer.Value >= _conflictTimerMax)
                {
                    _conflictTimer.Value = _conflictTimerMax;
                    _isShortCircuit.Value = true;
                }
            }
            else
            {
                _shortCircuitDamageElapsed = 0f;

                if (_conflictTimer.Value > 0)
                {
                    _conflictTimer.Value -= Time.deltaTime;
                    if (_conflictTimer.Value <= 0)
                    {
                        _conflictTimer.Value = 0;
                        _isShortCircuit.Value = false;
                    }
                }
            }
        }

        private void UpdateShortCircuitDamage()
        {
            if (!_isShortCircuit.Value) return;

            _shortCircuitDamageElapsed += Time.deltaTime;
            if (_shortCircuitDamageElapsed >= _shortCircuitDamageInterval)
            {
                _shortCircuitDamageElapsed = 0f;

                foreach (int playerId in _playerIds)
                {
                    ApplyShortCircuitDamageTargetRpc(
                        ServerManager.Clients[playerId],
                        _shortCircuitDamage
                    );
                }
            }
        }

        [TargetRpc]
        private void ApplyShortCircuitDamageTargetRpc(NetworkConnection conn, int damage)
        {
            InvokeEvent(new OnShortCircuitDamage { damage = damage });
        }
        
        [ObserversRpc]
        private void NotifyPolarizationObserversRpc(bool isAligned, bool isSameZone, bool isConflict)
        {
            InvokeEvent(new OnPolarizationStateChanged
            {
                isAligned = isAligned,
                isSameZone = isSameZone,
                isConflict = isConflict
            });
        }
        

        [ServerRpc(RequireOwnership = false)]
        private void CheckCanSwapServerRpc(CallSwapGunEvent data)
        {
            if (_player == data.player) return;
            if (!_canSwap.Value) return;
            
            if (_elapsedTime.Value > 0)
            {
                _elapsedTimeForCombo.Value = _cooldownSwap;
                
                _canSwap.Value = false;

                NotifySwapTargetRpc(_player.Owner, data.gunIndex, data.currentAmmo);
                NotifySwapTargetRpc(data.player.Owner, _firstGunIndex, _firstGunAmmo);

                AddEnergyTargetRpc(_player.Owner);
                AddEnergyTargetRpc(data.player.Owner);
                
                ResetTimer();
            }
            else
            {
                _firstGunAmmo = data.currentAmmo;
                _elapsedTime.Value = _timeToAcceptSwap;
                _player = data.player;
                _firstGunIndex = data.gunIndex;
                _firstPlayerOwnerId.Value = data.player.OwnerId;
            }
        }

        [TargetRpc]
        private void AddEnergyTargetRpc(NetworkConnection conn)
        {
            RequestAddEnergyServerRpc(conn.ClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestAddEnergyServerRpc(int playerId)
        {
            InvokeEvent(new ModifyEnergyEvent
            {
                p_player = playerId,
                p_value = _swapEnergyGain
            });
        }

        [TargetRpc]
        private void NotifySwapTargetRpc(NetworkConnection conn, int newIndex, int currentAmmo)
        {
            
            InvokeEvent(new SwapingGunEvent
            {
                gunIndex = newIndex,
                timeToSwap = _swapingTime,
                currentAmmo = currentAmmo,
            });
        }

        void ResetTimer()
        {
            InvokeEvent(new EndTimerSwapEvent()
            {
                player = _player
            });
            
            _player = null;
            _elapsedTime.Value = 0;
            _firstGunIndex = -1;
            _firstGunAmmo = -1;
            _firstPlayerOwnerId.Value = -1;
        }
        
        private bool CheckPolarized()
        {
            if (_playerZones[0] == _playerZones[1])
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void OnElapsedTimeChanged(float prev, float next, bool asServer)
        {
            if (next > 0)
            {
                bool isRequester = LocalConnection.ClientId == _firstPlayerOwnerId.Value;
                OnChangeAskText?.Invoke(isRequester);
            }

            if (next > prev)
                _displayedTime = next;
        }
        
        private void OnElapsedComboTimeChanged(float prev, float next, bool asServer)
        {
            _cooldownText.gameObject.SetActive(next > 0);
            
            _cooldownText.text = ((int)next).ToString();
        }
        
        // Callbacks clients pour l'UI
        private void OnConflictChanged(bool prev, bool next, bool asServer)
        {
            InvokeEvent(new OnConflictUIUpdate { isConflict = next, isShortCircuit = _isShortCircuit.Value });
        }

        private void OnShortCircuitChanged(bool prev, bool next, bool asServer)
        {
            InvokeEvent(new OnConflictUIUpdate { isConflict = _isInConflict.Value, isShortCircuit = next });
        }

        private void OnConflictTimerChanged(float prev, float next, bool asServer)
        {
            InvokeEvent(new OnConflictTimerUIUpdate
            {
                ratio = next / _conflictTimerMax,
                isShortCircuit = _isShortCircuit.Value
            });
        }
    }
}

public struct OnPlayerIsInSameZone
{
    public bool isSameZone;
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
}