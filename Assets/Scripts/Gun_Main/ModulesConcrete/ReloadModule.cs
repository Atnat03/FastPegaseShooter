using System.Collections;
using UnityEngine;

namespace GunDecorator
{
    [DisallowMultipleComponent]
    public class ReloadModule : GunModule, IReloadModule
    {
        public int CurrentAmmo => _currentAmmo;
        public bool AutoReload => _autoReload;
        
        public bool IsReloading => _isReloading;

        [SerializeField] private bool _autoReload = true;
        [SerializeField] private int _magazineSize = 30;
        [SerializeField] private float reloadDuration = 3f;
        private int _currentAmmo = 0;
        private bool _isReloading = false;

        public override void Initialize(GunController gun)
        {
            base.Initialize(gun);
            SetAmmo(_magazineSize);
        }

        public void Reload() => StartCoroutine(ReloadCoroutine());
        

        public IEnumerator ReloadCoroutine()
        {
            _isReloading = true;
            yield return new WaitForSeconds(reloadDuration);
            _isReloading = false;
            SetAmmo(_magazineSize);
        }

        public void SetAmmo(int value) => _currentAmmo = value;
        
    }
}