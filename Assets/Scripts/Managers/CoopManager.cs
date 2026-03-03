using System;
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
        
        [Header("UI")]
        [SerializeField] private GameObject _barUI;
        [SerializeField] private Image _valueImage;
        
        private NetworkObject _player = null;
        private int _firstGunIndex = -1;
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
        }

        [ServerRpc(RequireOwnership = false)]
        private void CheckCanSwapServerRpc(CallSwapGunEvent data)
        {
            if (_player == data.player) return;

            if (_elapsedTime.Value > 0)
            {
                NotifySwapTargetRpc(_player.Owner, data.gunIndex);      
                NotifySwapTargetRpc(data.player.Owner, _firstGunIndex); 
                ResetTimer();
            }
            else
            {
                _elapsedTime.Value = _timeToAcceptSwap;
                _player = data.player;
                _firstGunIndex = data.gunIndex;
            }
        }
        
        [TargetRpc]
        private void NotifySwapTargetRpc(NetworkConnection conn, int newIndex)
        {
            _bus.InvokeEvent(new SwapingGunEvent
            {
                gunIndex = newIndex,
                timeToSwap = _swapingTime
            });
        }

        void ResetTimer()
        {
            _player = null;
            _elapsedTime.Value = 0;
            _firstGunIndex = -1;
        }
        
        private void OnElapsedTimeChanged(float prev, float next, bool asServer)
        {
            _valueImage.fillAmount = next / _timeToAcceptSwap;
            _barUI.SetActive(next > 0);
        }
    }
}