using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GunDecorator
{
    public class SalveShootModule : GunModule, ISecondModule
    {
        public float numberShootPerSalve;
        private IShootModule _shootModule;

        public void SetUpModule(IShootModule shootModule)
        {
            _shootModule = shootModule;
        }

        public void DoAdditionnalEffect()
        {
            for (int i = 0; i < numberShootPerSalve; i++)
            {
                _shootModule.Shooting();
            }
        }
        
        private void OnValidate()
        {
            ChargingShootModule[] shootModules = GetComponents<ChargingShootModule>().ToArray();
            if (shootModules.Length > 0)
            {
                Debug.LogError("Vous ne pouvez pas ajouté un module de --SalveShootModule-- car il y a déja un module de --ChargingShootModule-- !");
            }
        }
    }
}