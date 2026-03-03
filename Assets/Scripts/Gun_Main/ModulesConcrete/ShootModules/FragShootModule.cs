using System.Linq;
using UnityEngine;

namespace GunDecorator
{
    public class FragShootModule : TemplateShootModule
    {
        [SerializeField] private int _damage;
        [SerializeField] private float _fireRate;
        
        public override void Shooting()
        {
            base.Shooting();
            
            Debug.Log("Flag Shoot Module : " + _damage);
        }
    }
}