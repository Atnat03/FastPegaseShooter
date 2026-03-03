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
    }
}