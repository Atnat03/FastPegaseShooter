using System;
using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Controller
{
    public class GunSurcharge : MonoBusListener
    {
        [SerializeField] private GunBridgePlayer _gunBridge;

        [Header("Overloading")] 
        [SerializeField] private int _numberChargedShootWhenOverload = 2;
        [SerializeField] private bool _isOverload = false;
        [SerializeField] private float _elapsedTimeOverload = 0;
        private float _currentOverloadTimer = 0;

        
        public Action<bool, float> OnOverloadTimeUpdate;
        public Action<Color> OnInfoOverloadSetColor;
        
        private void Update()
        {
            OverloadTimer();
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
                    InvokeEvent(new EndOverloadEvent());
                    _isOverload = false;
                }
            }

            OnOverloadTimeUpdate?.Invoke(_isOverload, _elapsedTimeOverload/_currentOverloadTimer);
        }

    }

    public struct EndOverloadEvent{}
}