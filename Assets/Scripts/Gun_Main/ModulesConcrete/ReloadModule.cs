using System.Collections;
using UnityEngine;

namespace GunDecorator
{
    [DisallowMultipleComponent]
    public class ReloadModule : GunModule, IReloadModule
    {
        public int CurrentAmmo => _currentAmmo;
        public bool AutoReload => _autoReload;

        [SerializeField] private bool _autoReload = true;
        [SerializeField] private int _magazineSize = 30;
        [SerializeField] private float reloadDuration = 3f;
        private int _currentAmmo = 0;

        public override void Initialize(GunController gun)
        {
            base.Initialize(gun);
            Reload();
        }

        public void Reload() => StartCoroutine(ReloadCoroutine());
        

        public IEnumerator ReloadCoroutine()
        {
            yield return new WaitForSeconds(reloadDuration);
            SetAmmo(_magazineSize);
        }

        public void SetAmmo(int value) => _currentAmmo = value;
        
    }
}