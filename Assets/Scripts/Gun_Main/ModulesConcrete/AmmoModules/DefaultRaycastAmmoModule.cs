using System;
using Unity.VisualScripting;
using UnityEngine;

namespace GunDecorator.AmmoModules
{
    
    
    public class DefaultRaycastAmmoModule : GunModule , IAmmoModule
    {
        #region variables

        public GameObject p_markPrefab;
        private GameObject _currentMark;
        private Transform _camTransform;
        
        #endregion


        void Start()
        {
            _camTransform = Camera.main.transform;
        }
        
        public void SpawnBullet()
        {
            if (Physics.Raycast(_camTransform.position + transform.forward * .3f, transform.forward, out RaycastHit hit, LayerMask.GetMask("Owner")))
            {
                if (_currentMark != null)
                {
                    Destroy(_currentMark);
                }
            
                _currentMark = Instantiate(p_markPrefab, hit.point + hit.normal * 0.1f, Quaternion.LookRotation(hit.normal));
            }
        }
    }
}
