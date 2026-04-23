using System;
using System.Linq;
using MyPrint;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GunDecorator
{
    public class NoiseModule : GunModule, ISecondModule
    {
        private IShootModule _shootModule;
        private ISecondModule _next;
        
        [SerializeField] private AnimationCurve _curveNoiseOverTime = new AnimationCurve(new Keyframe(0,  0), new Keyframe(1, 1));
        [SerializeField] private float _timeToAccessMaxNoise = 2f;
        private float _elapsedSpam = 0;
        private bool isShooting = false;
        
        [SerializeField][Tooltip("le décalage en X du tir est determiné aléatoirement entre -_maxOffsetX et _maxOffsetX")] private float _maxOffsetX;
        [SerializeField][Tooltip("le décalage en Y du tir est determiné aléatoirement entre -_maxOffsetY et _maxOffsetY")] private float _maxOffsetY;

        public override void SetVariable(GunSetting setting)
        {
            if (setting is S_NoiseSetting s)
            {
                _maxOffsetX = s.MaxOffsetX;
                _maxOffsetY = s.MaxOffsetY;
                _timeToAccessMaxNoise = s.TimeToAccessMaxNoise;
                _curveNoiseOverTime = s.CurveNoiseOverTime;
            }
        }
        
        public void SetUpModule(IShootModule shootModule)
        {
            _shootModule = shootModule;
        }

        public void SetNext(ISecondModule next) =>  _next = next;

        public void DoAdditionnalEffect()
        {
            isShooting = true;

            float t = _curveNoiseOverTime.Evaluate(_elapsedSpam);

            float x = Random.Range(-_maxOffsetX, _maxOffsetX);
            float y = Random.Range(-_maxOffsetY, _maxOffsetY);
            
            _shootModule.SetDirectionModifier(new Vector3(x, y, 1) * t);
            Shooting();
        }

        public void Shooting()
        {
            if (_next != null)
                _next.Shooting();
            else
                _shootModule.Shooting();
        }

        public void CancelShooting()
        {
            isShooting = false;
            _elapsedSpam = 0;
        }

        private void Update()
        {
            if(isShooting)
                _elapsedSpam += Time.deltaTime / _timeToAccessMaxNoise;
        }
    }
}