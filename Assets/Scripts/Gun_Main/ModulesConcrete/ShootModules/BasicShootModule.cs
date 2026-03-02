using UnityEngine;

namespace GunDecorator
{
    public class BasicShootModule : TemplateShootModule
    {
        [SerializeField] private int _damage;

        public override void Shooting()
        {
            base.Shooting();
            
            Debug.Log("Basic Shoot Module : " + _damage);
        }
    }
}