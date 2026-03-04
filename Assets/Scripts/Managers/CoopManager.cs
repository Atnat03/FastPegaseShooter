using System;
using System.Collections.Generic;
using Controller;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.UI;

namespace Managers
{
    public class CoopManager : NetworkBehaviour
    {
        [SerializeField] private float _timeToAcceptSwap = 2f;
        private readonly SyncVar<float> _elapsedTime = new SyncVar<float>();
        [SerializeField] private float _swapingTime;
        
        [SerializeField] private List<SyncVar<float>> _damageSurchargeList = new List<SyncVar<float>>();
        
        [Header("UI")]
        [SerializeField] private GameObject _barUI;
        [SerializeField] private Image _valueImage;
        
        private NetworkObject _player = null;
        private int _firstGunIndex = -1;
        private int _firstGunAmmo = -1;
        private float _displayedTime = 0f;
        private float _targetTime = 0f;
        
        private EventBus _bus;

        public override void OnStartServer()
        {
            base.OnStartServer();

            InitBus();
            _bus.Subscribe((CallSwapGunEvent data) => CheckCanSwapServerRpc(data));
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            
            InitBus();
            _elapsedTime.OnChange += OnElapsedTimeChanged;
        }
        
        private void InitBus()
        {
            _bus = EventBusInitialiser.instance.Bus;
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
            }
            
            if (_displayedTime > 0)
            {
                _displayedTime -= Time.deltaTime;
                if (_displayedTime < 0) _displayedTime = 0;
            }
        
            _valueImage.fillAmount = _displayedTime / _timeToAcceptSwap;
        }

        [ServerRpc(RequireOwnership = false)]
        private void CheckCanSwapServerRpc(CallSwapGunEvent data)
        {
            if (_player == data.player) return;

            if (_elapsedTime.Value > 0)
            {
                NotifySwapTargetRpc(_player.Owner, data.gunIndex, data.currentAmmo);
                NotifySwapTargetRpc(data.player.Owner, _firstGunIndex, _firstGunAmmo); 
                ResetTimer();
            }
            else
            {
                _firstGunAmmo = data.currentAmmo;
                _elapsedTime.Value = _timeToAcceptSwap;
                _player = data.player;
                _firstGunIndex = data.gunIndex;
            }
        }
        
        [TargetRpc]
        private void NotifySwapTargetRpc(NetworkConnection conn, int newIndex, int currentAmmo)
        {
            _bus.InvokeEvent(new SwapingGunEvent
            {
                gunIndex = newIndex,
                timeToSwap = _swapingTime,
                currentAmmo = currentAmmo
            });
        }

        void ResetTimer()
        {
            _bus.InvokeEvent(new EndTimerSwapEvent()
            {
                player = _player
            });
            
            _player = null;
            _elapsedTime.Value = 0;
            _firstGunIndex = -1;
        }
        
        private void OnElapsedTimeChanged(float prev, float next, bool asServer)
        {
            _targetTime = next;
            _barUI.SetActive(next > 0);
    
            if (next > prev)
                _displayedTime = next;
        }
    }
}