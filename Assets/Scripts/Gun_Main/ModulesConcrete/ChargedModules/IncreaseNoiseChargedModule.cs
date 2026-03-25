using UnityEngine;

namespace GunDecorator.ChargedModules
{
    public class IncreaseNoiseChargedModule : ChargedParentModule
    {
        [Header("Salve")] 
        [SerializeField] private AnimationCurve _noiseEvolutionCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        [SerializeField] private float _maximumNoiseAngle = 10;
        
        public override void TryShootCharging()
        {
            if (_charging)
            {
                _ammoModule.SetBulletData(new BulletData
                {
                    IsExplosive = _isExplosifAmmo,
                    IsCritical = _gunController.IsOverload,
                    ExplosionRadius = _explosionRadius
                });

                _gunController.RecoilModule.Recoil(_gunController.ModelGun.transform, 0.25f, false, _recoilChargedMultiplier, _recoilX);
                _gunController.RecoilModule?.SetIsRecoil(true);

                ApplyShoot();
            }
            
            ResetCharging();
        }

        private void ApplyShoot()
        {
            int numberBulletShoot = (int)Mathf.Lerp(0, _numberBulletInCharge, _charginTimer / _timeToCharge);

            float angle = Mathf.Lerp(0, _maximumNoiseAngle, _noiseEvolutionCurve.Evaluate(_charginTimer / _timeToCharge));
            
            for (int i = 0; i < numberBulletShoot; i++)
            {
                Vector3 direction = 
                    new Vector3(
                        Random.Range(-angle, angle),
                        Random.Range(-angle, angle),
                        0);
                
                _ammoModule.SpawnBullet(direction, Vector3.zero);
                _gunController.SetAmmo(_gunController.GetCurrentAmmo() - 1);
            }
                            
            AudioClip clip = SoundManager.GetAudioClip(_gunController._soundData,"Charged");
            SoundManager.PlaySound(clip, _gunController._source, 0.5f);
        }
    }
}