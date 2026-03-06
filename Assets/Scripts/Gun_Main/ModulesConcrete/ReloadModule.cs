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

        [SerializeField] private bool _autoReload = true;
        [SerializeField] private int _magazineSize = 30;
        [SerializeField] private float reloadDuration = 3f;
        private int _currentAmmo = 0;
        private bool _isReloading = false;
        
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI _ammoText;
        [SerializeField] private Image _imageReload;
        
        public Coroutine p_reloadCoroutine = null;

        public override void Initialize(GunController gun)
        {
            base.Initialize(gun);
            SetAmmo(_magazineSize);
        }

        private void Update()
        {
            _ammoText.text = CurrentAmmo +  "/" + _magazineSize;
        }

        public void StopReload()
        {
            if (p_reloadCoroutine != null)
            {
                StopCoroutine(p_reloadCoroutine);
                p_reloadCoroutine = null;
                _imageReload.gameObject.SetActive(false);
            }
        }

        public void Reload()
        {
            if(p_reloadCoroutine == null)
                StartCoroutine(ReloadCoroutine());
        }

        IEnumerator ReloadCoroutine()
        {
            _imageReload.gameObject.SetActive(true);
            _imageReload.fillAmount = 1;
            
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
        }

        public void SetAmmo(int value) => _currentAmmo = Mathf.Min(value, _magazineSize);        
    }
}