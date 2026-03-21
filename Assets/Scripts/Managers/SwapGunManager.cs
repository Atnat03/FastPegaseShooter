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
    
    public class SwapGunManager : NetworkBehaviour
    {
        [SerializeField] private float _timeToAcceptSwap = 2f;
        private readonly SyncVar<float> _elapsedTime = new SyncVar<float>();
        [SerializeField] private float _swapingTime;
        
        [SerializeField] 
        private List<SurchargeData> _damageSurchargeData = new List<SurchargeData>();
        private readonly SyncVar<int> _currentSurchargeLevel = new SyncVar<int>();
        private readonly SyncVar<int> _firstPlayerOwnerId = new SyncVar<int>(-1);
        
        [Header("Combo")]
        [SerializeField] private bool _isCombo = false;
        [SerializeField] private Image _infoCombo;
        private readonly SyncVar<float> _elapsedTimeForCombo = new SyncVar<float>();
        
        [Header("UI")]
        [SerializeField] private GameObject _barUI;
        [SerializeField] private Image _valueImage;
        [SerializeField] private TextMeshProUGUI _textSwapUI;
        [SerializeField] private string _youAskSwapMessage;
        [SerializeField] private string _broAskyouSwapMessage;
        
        private NetworkObject _player = null;
        private int _firstGunIndex = -1;
        private int _firstGunAmmo = -1;
        private float _displayedTime = 0f;
        private float _targetTime = 0f;
        
        private EventBus _bus;

        public override void OnStartServer()
        {
            base.OnStartServer();

            _elapsedTimeForCombo.Value = 0;
            _elapsedTime.Value = 0;
            
            InitBus();
            _bus.Subscribe((CallSwapGunEvent data) => CheckCanSwapServerRpc(data));
            _bus.Subscribe((EndOverloadEvent data) =>
            {
                _elapsedTimeForCombo.Value = _damageSurchargeData[_currentSurchargeLevel.Value].timeToCombo;
            });
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            
            InitBus();
            _elapsedTime.OnChange += OnElapsedTimeChanged;
            _elapsedTimeForCombo.OnChange += OnElapsedComboTimeChanged;
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
        
            _valueImage.fillAmount = _displayedTime / _timeToAcceptSwap;
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
                _firstPlayerOwnerId.Value = data.player.OwnerId;
            }
        }
        
        [TargetRpc]
        private void NotifySwapTargetRpc(NetworkConnection conn, int newIndex, int currentAmmo)
        {
            _bus.InvokeEvent(new SwapingGunEvent
            {
                dataSurcharge = _damageSurchargeData[_currentSurchargeLevel.Value],
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
            _firstGunAmmo = -1;
            _firstPlayerOwnerId.Value = -1;
        }
        
        private void OnElapsedTimeChanged(float prev, float next, bool asServer)
        {
            _targetTime = next;
            _barUI.SetActive(next > 0);

            if (next > 0)
            {
                bool isRequester = LocalConnection.ClientId == _firstPlayerOwnerId.Value;
                _textSwapUI.text = isRequester ? _youAskSwapMessage : _broAskyouSwapMessage;
            }

            if (next > prev)
                _displayedTime = next;
        }
        
        private void OnElapsedComboTimeChanged(float prev, float next, bool asServer)
        {
            _infoCombo.color = _damageSurchargeData[_currentSurchargeLevel.Value].colorJauge;
            _infoCombo.gameObject.SetActive(next > 0 && _currentSurchargeLevel.Value < _damageSurchargeData.Count-1);
        }
    }
}