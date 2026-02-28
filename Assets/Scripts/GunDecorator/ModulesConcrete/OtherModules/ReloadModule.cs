using UnityEngine;

namespace GunDecorator
{
    [DisallowMultipleComponent]
    public class ReloadModule : GunModule, IReloadModule
    {
        [SerializeField] private int _magazineSize = 30;

        public override void Initialize(GunController gun)
        {
            base.Initialize(gun);
            Reload();
        }

        public void Reload()
        {
            Debug.Log("Reloading");
        }
    }
}