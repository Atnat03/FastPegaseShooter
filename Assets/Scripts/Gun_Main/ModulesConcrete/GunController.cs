using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public interface IGun
{
    public void TryFire();
}

namespace GunDecorator
{
    public class GunController : MonoBehaviour, IGun
    {
        private IShootModule[] _shootModule;
        private IReloadModule _reloadModule;
        private IRecoilModule _recoilModule;
        
        private void Awake()
        {
            //On récupere tout les types de modules possible et potentiellement sur l'arme
            _shootModule = GetComponents<IShootModule>();
            _reloadModule = GetComponent<IReloadModule>();
            _recoilModule = GetComponent<IRecoilModule>();


            //On initialise tout les modules de l'arme
            foreach (GunModule module in GetComponents<GunModule>())
            {
                module.Initialize(this);
            }
        }

        public void TryFire()
        {
            //On appele la fonction shoot du module de shoot actuellement équipé

            foreach (IShootModule s in _shootModule)
            {
                s?.TryShoot();
            }
            _recoilModule?.Recoil();
        }

        public void Reload()
        {
            //On appele la fonction reload du module de reload actuellement équipé
            _reloadModule?.Reload();
        }
    }
}