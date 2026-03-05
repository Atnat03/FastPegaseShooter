using System;
using System.Collections.Generic;
using System.ComponentModel;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public interface IGun
{
    public void TryFire();
}


public interface ISurcharge
{
    public int GetCurrentAmmo();
    public void SetAmmo(int value);
    public void SetSurchargeStat(bool isOverload, float dmgMultiplicator, float cadenceMultiplicator);
    public bool IsOverload { get; }
}

namespace GunDecorator
{
    public class GunController : NetworkBehaviour, IGun, ISurcharge
    {
        public bool IsOverload => _isOverload.Value;
        
        public float SurchargeMultiplierDamage { get; set; }
        public float SurchargeMultiplierRate{ get; set; }
        
        private IShootModule[] _shootModule;
        private IReloadModule _reloadModule;
        private IRecoilModule _recoilModule;

        private readonly SyncVar<bool> _isOverload = new SyncVar<bool>(false);
        
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

        public int GetCurrentAmmo()
        {
            return _reloadModule.CurrentAmmo;
        }

        public void SetAmmo(int value)
        {
            _reloadModule.SetAmmo(value);
        }

        public void SetSurchargeStat(bool isOverload, float dmgMultiplicator, float cadenceMultiplicator)
        {
            _isOverload.Value = isOverload;
            SurchargeMultiplierDamage = dmgMultiplicator;
            SurchargeMultiplierRate = cadenceMultiplicator;
        }

        public void Reload()
        {
            //On appele la fonction reload du module de reload actuellement équipé
            _reloadModule?.Reload();
        }
    }
}