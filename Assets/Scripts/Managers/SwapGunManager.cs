using System;
using System.Collections.Generic;
using Controller;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Managers
{
    [Serializable]
    public struct SurchargeData
    {
        public float damageMultiplier;
        public float cadenceMultiplier;
        public float overloadDuration;
        public float timeToCombo;
        public Color colorJauge;
    }
    
    public class SwapGunManager : NetworkBusListener
    {
        [SerializeField] private float _timeToAcceptSwap = 2f;
        public readonly SyncVar<float> _elapsedTime = new SyncVar<float>();
        [SerializeField] private float _swapingTime;
        
        [SerializeField] 
        private List<SurchargeData> _damageSurchargeData = new List<SurchargeData>();
        private readonly SyncVar<int> _currentSurchargeLevel = new SyncVar<int>();
        private readonly SyncVar<int> _firstPlayerOwnerId = new SyncVar<int>(-1);

        private bool _isCombo = false;
        private readonly SyncVar<float> _elapsedTimeForCombo = new SyncVar<float>();

        private NetworkObject _player = null;
        private int _firstGunIndex = -1;
        private int _firstGunAmmo = -1;
        private float _displayedTime = 0f;
        private float _targetTime = 0f;
        
        public Action<float> OnUpdateAskBroSwap;
        public Action<bool> OnChangeAskText;
        public Action<bool, Color> OnComboUpdate;

        public override void OnStartServer()
        {
            base.OnStartServer();

            _elapsedTimeForCombo.Value = 0;
            _elapsedTime.Value = 0;
            
            ListenToEvent<CallSwapGunEvent>(CheckCanSwapServerRpc);
            ListenToEvent<EndOverloadEvent>(data => _elapsedTimeForCombo.Value = _damageSurchargeData[_currentSurchargeLevel.Value].timeToCombo);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            
            _elapsedTime.OnChange += OnElapsedTimeChanged;
            _elapsedTimeForCombo.OnChange += OnElapsedComboTimeChanged;
        }
        
        private void Update()
        {
            if (IsServerInitialized)
            {
                if (_elapsedTime.Value > 0)
                {
                    _elapsedTime.Value -= Time.deltaTime;
                    if (_elapsedTime.Value <= 0)
                        ResetTimer();
                }
                
                if (_elapsedTimeForCombo.Value > 0)
                {
                    _elapsedTimeForCombo.Value -= Time.deltaTime;
                    _isCombo = true;
                    if (_elapsedTimeForCombo.Value <= 0)
                    {
                        _isCombo = false;
                        _currentSurchargeLevel.Value = 0;
                    }
                }
            }
            
            if (_displayedTime > 0)
            {
                _displayedTime -= Time.deltaTime;
                if (_displayedTime < 0) _displayedTime = 0;
            }
        
            OnUpdateAskBroSwap?.Invoke(_displayedTime / _timeToAcceptSwap);
        }

        [ServerRpc(RequireOwnership = false)]
        private void CheckCanSwapServerRpc(CallSwapGunEvent data)
        {
            if (_player == data.player) return;
            
            if (_elapsedTime.Value > 0)
            {
                if (_isCombo)
                {
                    _currentSurchargeLevel.Value++;

                    if (_currentSurchargeLevel.Value >= _damageSurchargeData.Count)
                    {
                        _currentSurchargeLevel.Value = 0;
                    }
                }
                else
                {
                    _elapsedTimeForCombo.Value = 0;
                }
                    
                _elapsedTimeForCombo.Value = 0;

                NotifySwapTargetRpc(_player.Owner, data.gunIndex, data.currentAmmo, _damageSurchargeData[_currentSurchargeLevel.Value].colorJauge);
                NotifySwapTargetRpc(data.player.Owner, _firstGunIndex, _firstGunAmmo, _damageSurchargeData[_currentSurchargeLevel.Value].colorJauge);
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
        private void NotifySwapTargetRpc(NetworkConnection conn, int newIndex, int currentAmmo, Color color)
        {
            InvokeEvent(new SwapingGunEvent
            {
                dataSurcharge = _damageSurchargeData[_currentSurchargeLevel.Value],
                gunIndex = newIndex,
                timeToSwap = _swapingTime,
                currentAmmo = currentAmmo,
                color = color,
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
        
        private void OnElapsedTimeChanged(float prev, float next, bool asServer)
        {
            _targetTime = next;

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
            bool a = next > 0 && _currentSurchargeLevel.Value < _damageSurchargeData.Count - 1;
            Color c = _damageSurchargeData[_currentSurchargeLevel.Value].colorJauge;
            
            OnComboUpdate?.Invoke(a, c);
        }
    }
}