using System;
using UnityEngine;

namespace Controller
{
    public class GunBridgePlayer : MonoBehaviour
    {
        [SerializeField]private GunSwitching _gunSwitching;

        public void TryShootWithCurrentGun()
        { 
            _gunSwitching.CurrentGun.TryFire();
        }
    }
}