using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GunDecorator
{
    [DisallowMultipleComponent]
    public class ReloadModule : GunModule, IReloadModule
    {
        public int CurrentAmmo => _currentAmmo;
        public bool AutoReload => _autoReload;
        
        public bool IsReloading => _isReloading;

        [SerializeField, Tooltip("Si coché => l'arme recharge automatiquement quand on arrive à 0 balles dans le chargeur")]
        private bool _autoReload = true;
        [SerializeField, Tooltip("Nombre max de munition dans un chargeur")] private int _magazineSize = 30;
        [SerializeField, Tooltip("Temps de rechargement")] private float reloadDuration = 3f;
        private int _currentAmmo = 0;
        private bool _isReloading = false;
        
        [Header("UI")]
        [SerializeField, Tooltip("Text des balles actuelles + max balles")] private TextMeshProUGUI _ammoText;
        [SerializeField, Tooltip("Cercle pour le temps de reload")] private Image _imageReload;
        
        public Coroutine p_reloadCoroutine = null;

        public override void Initialize(GunController gun)
        {
            base.Initialize(gun);
            SetAmmo(_magazineSize);
        }

        private void Update()
        {
            _ammoText.text = CurrentAmmo +  "/" + _magazineSize;
            
            _currentAmmo = Mathf.Clamp(_currentAmmo, 0, _magazineSize);

            if (_autoReload)
            {
                if (_currentAmmo <= 0)
                {
                    Reload();
                }
            }
        }

        public void StopReload()
        {
            if(p_reloadCoroutine != null)
            {
                StopCoroutine(p_reloadCoroutine);
                p_reloadCoroutine = null;
                _imageReload.gameObject.SetActive(false);
                _isReloading = false;
            }
        }

        public void Reload()
        {
            if(p_reloadCoroutine == null)
                p_reloadCoroutine = StartCoroutine(ReloadCoroutine());
        }

        IEnumerator ReloadCoroutine()
        {
            _imageReload.gameObject.SetActive(true);
            _imageReload.fillAmount = 1;
            
            _gunController?._animator.SetTrigger("Reload");
            
            _isReloading = true;
            
            float duration = reloadDuration;
            float elapsedTime = duration;

            while (elapsedTime > 0)
            {
                elapsedTime -= Time.deltaTime;
                _imageReload.fillAmount = elapsedTime / duration;
                yield return null;
            }
            
            _imageReload.gameObject.SetActive(false);
            
            _isReloading = false;
            SetAmmo(_magazineSize);
            
            p_reloadCoroutine = null;
            
            if (_gunController.IsFullAuto)
            {
                _gunController.ApplyShoot();
            }
        }

        public void SetAmmo(int value) => _currentAmmo = Mathf.Min(value, _magazineSize);        
    }
}