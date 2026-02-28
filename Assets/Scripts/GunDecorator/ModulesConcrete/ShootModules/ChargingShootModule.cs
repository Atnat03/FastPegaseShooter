using System.Linq;
using UnityEngine;

namespace GunDecorator
{
    public class ChargingShootModule : TemplateShootModule
    {
        [SerializeField] private int _damage;
        [SerializeField] private float _fireRate;
        

        public override void Shooting()
        {
            base.Shooting();
            
            Debug.Log("Charging Shoot Module : " + _damage);
        }
        
        //Empecher d'ajouter le component si il y a deja un autre component particulier 
        private void OnValidate()
        {
            INoiseModule[] shootModules = GetComponents<MonoBehaviour>().OfType<INoiseModule>().ToArray();
            if (shootModules.Length > 0)
            {
                Debug.LogError("Vous ne pouvez pas ajouté un module de --Noise-- car il y a déja un module de --FragShoot-- !");
            }
        }
    }
}