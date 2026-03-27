using System.Collections;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using GunDecorator.ChargedModules;
using ScriptableObjectsDefinitions;
using UnityEngine;
using UnityEngine.VFX;

public interface IGun
{
    public void TryFire();
    public void TryCancelShooting();
    public void TryReload();
    public void TryCharging();
    public void TryShootCharged();
    public void Disable(bool state);
}


public interface ISurcharge
{
    public int GetCurrentAmmo();
    public void SetAmmo(int value);
    public void SetSurchargeStat(bool isOverload, float dmgMultiplicator, float cadenceMultiplicator);
    public MeshRenderer ModelGun { get; }
    public void StopReload();
}

namespace GunDecorator
{
    public class GunController : NetworkBusListener, IGun, ISurcharge
    {
        public bool IsOverload => _isOverload.Value;
        public float SurchargeMultiplierDamage { get; set; }
        public float SurchargeMultiplierRate { get; set; }
        public bool IsFullAuto => _isFullAuto;
        
        public IRecoilModule RecoilModule => _recoilModule;
        
        public MeshRenderer ModelGun => _model;

        private IShootModule[] _shootModule;
        private IReloadModule _reloadModule;
        private IRecoilModule _recoilModule;
        private IHitMarkerModule _hitMarkerModule;
        private ChargedParentModule _chargedModule;

        private readonly SyncVar<bool> _isOverload = new SyncVar<bool>(false);
        
        [SerializeField, Tooltip("Model 3d de l'arme")] 
        private MeshRenderer _model;
        [SerializeField, Tooltip("Audio Source de l'arme")] 
        public AudioSource p_source;
        [SerializeField, Tooltip("Scriptable Object contenant les Audio Clip de l'arme (exemple dans le dossier Assets/SoudData)")] 
        public SoundsDataSO p_soundData;
        
        [SerializeField, Tooltip("Animation du modele de l'arme")] 
        public Animator p_animator;
        
        [SerializeField, Tooltip("Effet de tir du bout du canon de l'arme")] public VisualEffect _muzzleFlash; // test
        [SerializeField][Tooltip("est ce que le maintient du clic provoque un tir automatique")]private bool _isFullAuto;
        
        private bool ShootingInputPressed = true;

        [HideInInspector] public bool p_authorizedToShoot = true;
        
        private void Awake()
        {
            //On récupere tout les types de modules possible et potentiellement sur l'arme
            _shootModule = GetComponents<IShootModule>();
            _reloadModule = GetComponent<IReloadModule>();
            _recoilModule = GetComponent<IRecoilModule>();
            _hitMarkerModule = GetComponent<IHitMarkerModule>();
            _chargedModule = GetComponent<ChargedParentModule>();

            //On initialise tout les modules de l'arme
            foreach (GunModule module in GetComponents<GunModule>())
            {
                module.Initialize(this);
            }
        }

        public void TryFire()
        {
            if (_chargedModule != null)
                if (_chargedModule.IsCharging) return;
            
            //On appele la fonction shoot du module de shoot actuellement équipé
            ShootingInputPressed = true;

            ApplyShoot();
        }

        public void ApplyShoot()
        {
            if (!ShootingInputPressed) return;

            if (GetCurrentAmmo() > 0 && !_reloadModule.IsReloading && p_authorizedToShoot)
            {
                foreach (IShootModule s in _shootModule)
                {
                    if (IsFullAuto)
                    {
                        StartCoroutine(ShootingCoroutine(s));
                        continue;
                    }
                    s?.TryShoot();
                    
                    _recoilModule?.Recoil(_model.transform, 0.1f, false);
                    _recoilModule?.SetIsRecoil(true);
                    
                    SetAmmo(GetCurrentAmmo() - 1);
                    PlayMuzzleFlash();
                }
            }
        }


        IEnumerator ShootingCoroutine(IShootModule s)
        {
            while (ShootingInputPressed && GetCurrentAmmo() > 0 && !_reloadModule.IsReloading)
            {
                p_authorizedToShoot = false;
                s.TryShoot();
                PlayMuzzleFlash();
                
                _recoilModule?.Recoil(_model.transform, s.FireRate, true);
                _recoilModule?.SetIsRecoil(true);
                
                SetAmmo(GetCurrentAmmo() - 1);
                
                yield return new WaitForSeconds(s.FireRate);
                
                p_authorizedToShoot =  true;
            }
        }
        

        public void TryCancelShooting()
        {
            ShootingInputPressed = false;
            foreach (IShootModule s in _shootModule)
            {
                s?.CancelShooting();
            }
            
            _recoilModule?.SetIsRecoil(false);
        }

        public int GetCurrentAmmo() => _reloadModule.CurrentAmmo;

        public void SetAmmo(int value) => _reloadModule.SetAmmo(value);

        public void SetSurchargeStat(bool isOverload, float dmgMultiplicator, float cadenceMultiplicator)
        {
            if (!IsServerInitialized)
            {
                SetSurchargeStatServerRpc(isOverload, dmgMultiplicator, cadenceMultiplicator);
                return;
            }
            _isOverload.Value = isOverload;
            SurchargeMultiplierDamage = dmgMultiplicator;
            SurchargeMultiplierRate = cadenceMultiplicator;
            foreach (IShootModule s in _shootModule)
                s?.AmmoModule.SetDamage(dmgMultiplicator);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetSurchargeStatServerRpc(bool isOverload, float dmgMultiplicator, float cadenceMultiplicator)
        {
            SetSurchargeStat(isOverload, dmgMultiplicator, cadenceMultiplicator);
        }

        public void TryReload()
        {
            if (_reloadModule.IsReloading) return;
            
            _reloadModule?.Reload();
            PlaySound("Reload");
        }

        public void TriggerHitMark(bool isCritique = false)
        {
            if (!isCritique)
            {
                _hitMarkerModule?.HitMark();
            }
            else
            {
                _hitMarkerModule?.HitMarkCritique();
            }
        }

        public void TryCharging()
        {
            _chargedModule?.TryCharging();
        }

        public void TryShootCharged()
        {
            _chargedModule?.TryShootCharging();
        }

        public void Disable(bool state)
        {
            _model.gameObject.SetActive(state);
        }

        public void PlaySound(string sound)
        {
            AudioClip clip = SoundManager.GetAudioClip(p_soundData,sound);
            SoundManager.PlaySound(clip, p_source, 0.5f);
            
            PlaySoundServerRpc(sound);
        }

        [ServerRpc]
        void PlaySoundServerRpc(string sound)
        {
            PlaySoundObserverRpc(sound);
        }

        [ObserversRpc(ExcludeOwner = true)]
        void PlaySoundObserverRpc(string sound)
        {
            AudioClip clip = SoundManager.GetAudioClip(p_soundData,sound);
            SoundManager.PlaySound(clip, p_source, 0.5f);
        }

        [ObserversRpc]
        private void PlayMuzzleFlash()
        {
            _muzzleFlash.Play();
        }

        public void StopReload() => _reloadModule.StopReload();
    }
}