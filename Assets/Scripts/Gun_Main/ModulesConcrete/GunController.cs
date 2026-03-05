using System;
using System.Collections.Generic;
using System.ComponentModel;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using ScriptableObjectsDefinitions;
using UnityEngine;

public interface IGun
{
    public void TryFire();
    public void TryReload();
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
        public float SurchargeMultiplierRate { get; set; }

        private IShootModule[] _shootModule;
        private IReloadModule _reloadModule;
        private IRecoilModule _recoilModule;

        private readonly SyncVar<bool> _isOverload = new SyncVar<bool>(false);
        
        [SerializeField] public AudioSource _source;
        [SerializeField] public SoundsDataSO _soundData;

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

            if (GetCurrentAmmo() > 0 && !_reloadModule.IsReloading)
            {
                foreach (IShootModule s in _shootModule)
                {
                    s?.TryShoot();
                }

                SetAmmo(GetCurrentAmmo() - 1);
                _recoilModule?.Recoil();
            }

            if (GetCurrentAmmo() <= 0)
            {
                if (_reloadModule.AutoReload)
                {
                    TryReload();
                }
            }
        }

        public int GetCurrentAmmo() => _reloadModule.CurrentAmmo;

        public void SetAmmo(int value) => _reloadModule.SetAmmo(value);

        public void SetSurchargeStat(bool isOverload, float dmgMultiplicator, float cadenceMultiplicator)
        {
            _isOverload.Value = isOverload;
            SurchargeMultiplierDamage = dmgMultiplicator;
            SurchargeMultiplierRate = cadenceMultiplicator;
        }

        public void TryReload()
        {
            if (_reloadModule.IsReloading) return;
            
            _reloadModule?.Reload();
            
            AudioClip clip = SoundManager.GetAudioClip(_soundData, "Reload");
            SoundManager.PlaySound(clip, _source);
        }
        
    }
}