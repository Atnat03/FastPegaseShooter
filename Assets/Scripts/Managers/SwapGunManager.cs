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
            }
            
            if (_displayedTime > 0)
            {
                _displayedTime -= Time.deltaTime;
                if (_displayedTime < 0) 
                    _displayedTime = 0;
            }
        
            OnUpdateAskBroSwap?.Invoke(_displayedTime / _timeToAcceptSwap);
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
    }
}