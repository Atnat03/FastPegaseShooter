using System.Collections;
using UnityEngine;

namespace GunDecorator.ChargedModules
{
    public class SalveChargedModule : ChargedParentModule
    {
        [Header("Salve")] 
        [SerializeField] private int _numberBulletInCharge = 10;
        [SerializeField] private float _intervaleCharge = 0.05f;
        
        public override void TryShootCharging()
        {
            if (_charging)
            {
                int numberBulletShoot = (int)Mathf.Lerp(0, _numberBulletInCharge, _charginTimer / _timeToCharge);

                _ammoModule.SetBulletData(new BulletData
                {
                    IsExplosive = _isExplosifAmmo,
                    IsCritical = _gunController.IsOverload,
                    ExplosionRadius = _explosionRadius
                });

                _gunController.RecoilModule.Recoil(_gunController.ModelGun.transform, 0.25f, _recoilChargedMultiplier);

                StartCoroutine(ShootSalve(numberBulletShoot));
            }
            
            ResetCharging();
        }

        IEnumerator ShootSalve(int numberBullet)
        {
            for (int i = 0; i < numberBullet; i++)
            {
                _ammoModule.SpawnBullet(Vector3.zero, Vector3.zero);
                _gunController.SetAmmo(_gunController.GetCurrentAmmo() - 1);
                
                AudioClip clip = SoundManager.GetAudioClip(_gunController._soundData,"Charged");
                SoundManager.PlaySound(clip, _gunController._source, 0.5f);
                
                yield return new WaitForSeconds(_intervaleCharge);
            }
            
            _ammoModule.ResetBulletData();
        }
    }
}