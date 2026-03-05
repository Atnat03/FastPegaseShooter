using UnityEngine;

namespace GunDecorator
{
    [DisallowMultipleComponent]
    public class ReloadModule : GunModule, IReloadModule
    {
        public int CurrentAmmo => _currentAmmo;

        [SerializeField] private int _magazineSize = 30;
        private int _currentAmmo = 0;
        
        public override void Initialize(GunController gun)
        {
            base.Initialize(gun);
            Reload();
        }

        public void Reload()
        {
            Debug.Log("Reloading");
            SetAmmo(_magazineSize);
        }
        
        public void SetAmmo(int value)
        {
            _currentAmmo = value;
        }
    }
}