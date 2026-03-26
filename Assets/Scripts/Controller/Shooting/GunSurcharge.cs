using System;
using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Controller
{
    public class GunSurcharge : MonoBehaviour
    {
        [SerializeField] private GunBridgePlayer _gunBridge;

        [Header("Overloading")]
        [SerializeField] private bool _isOverload = false;
        [SerializeField] private float _elapsedTimeOverload = 0;
        private float _currentOverloadTimer = 0;

        private EventBus _bus;
        
        public Action<bool, float> OnOverloadTimeUpdate;
        public Action<Color> OnInfoOverloadSetColor;

        private void Awake()
        {
            _bus = EventBusInitialiser.instance.Bus;
        }

        public void SetOverloadStats(bool state, float overloadTime, float dmg_Multi, float rate_Multi, int newAmmoAmount = -1)
        {
            _currentOverloadTimer = overloadTime;
            _elapsedTimeOverload = overloadTime;
            
            _isOverload = state;
            if(newAmmoAmount != -1)
                _gunBridge.CurrentMainSurchargeGun.SetAmmo(newAmmoAmount);
            _gunBridge.CurrentMainSurchargeGun.SetSurchargeStat(state, dmg_Multi, rate_Multi);
        }

        private void Update()
        {
            OverloadTimer();
        }

        public void SetColorImage(Color color)
        {
            OnInfoOverloadSetColor?.Invoke(color);
        }

        private void OverloadTimer()
        {
            if (_isOverload)
            {
                if (_elapsedTimeOverload > 0)
                {
                    _elapsedTimeOverload -= Time.deltaTime;
                }
                else
                {
                    _bus.InvokeEvent(new EndOverloadEvent());
                    _isOverload = false;
                    SetOverloadStats(false, 0, 1, 1);
                }
            }

            OnOverloadTimeUpdate?.Invoke(_isOverload, _elapsedTimeOverload);
        }

    }

    public struct EndOverloadEvent{}
}