using System.Linq;
using UnityEngine;

namespace GunDecorator
{
    public class NoiseModuleModule : GunModule, INoiseModule
    {
        public void ApplyNoise()
        {
            Debug.Log("OMG ça se disperse !!");
        }
        
        //Empecher d'ajouter le component si il y a deja un autre component particulier 
        private void OnValidate()
        {
            FragShootModule[] shootModules = GetComponents<FragShootModule>().ToArray();
            if (shootModules.Length > 0)
            {
                Debug.LogError("Vous ne pouvez pas ajouté un module de --Noise-- car il y a déja un module de --FragShoot-- !");
            }
        }
    }
}