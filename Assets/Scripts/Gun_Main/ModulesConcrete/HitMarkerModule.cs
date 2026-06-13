using System;
using System.Collections;
using MyPrint;
using UnityEngine;
using UnityEngine.UI;

namespace GunDecorator
{
    public class HitMarkerModule : GunModule, IHitMarkerModule
    {
        [SerializeField, Tooltip("Le parent est le réticule du joueur")] 
        private Transform _hitMarkerParent;
        
        [Header("Hit")]
        [SerializeField, Tooltip("Prefab visuel a instancié quand on réussi un tir normal")] private GameObject _hitMarkerPrefab;
        [SerializeField, Tooltip("Couleur du hitMarker normal")] private Color _hitMarkerColor = Color.white;
        
        [Header("Hit critique")]
        [SerializeField, Tooltip("Prefab visuel a instancié quand on réussi un tir critique")] private GameObject _hitMarkerCritiquePrefab;
        [SerializeField, Tooltip("Couleur du hitMarker critique")] private Color _hitMarkerCritiqueColor = Color.red;

        [Header("Kill")] 
        [SerializeField] private GameObject _killMarkerPrefab;
        GameObject _currentKillMarker = null;
        
        [Header("Audio Buffer")]
        [SerializeField] private float _hitSoundCooldown = 0.05f;
        private float _lastHitSoundTime;

        private void Start()
        {
            ListenToEvent<OnPlayerDoKill>(PlayerDoKill);
            ListenToEvent<OnPlayerDoDamage>(MakeHitMarker);
        }

        private void MakeHitMarker(OnPlayerDoDamage data)
        {
            if(data.p_ownerId != _gunController.OwnerId)
                return;

            if (!data.p_critical)
            {
                HitMark();
            }
            else
            {
                HitMarkCritique();
            }
        }

        public void HitMark()
        {
            Image s = Instantiate(_hitMarkerPrefab, _hitMarkerParent).GetComponent<Image>();
            if (s != null)
            {
                s.color = _hitMarkerColor;
            }

            TryPlayHitSound();

            Destroy(s.gameObject, 0.5f);
        }

        public void HitMarkCritique()
        {
            Image s = Instantiate(_hitMarkerCritiquePrefab, _hitMarkerParent).GetComponent<Image>();
            if (s != null)
            {
                s.color = _hitMarkerCritiqueColor;
            }

            TryPlayHitSound();

            Destroy(s.gameObject, 0.5f);
        }
        
        private void TryPlayHitSound()
        {
            if (Time.time - _lastHitSoundTime < _hitSoundCooldown)
                return;

            _lastHitSoundTime = Time.time;

            SoundManager.PlaySound(_gunController._soundData, "HitMark", _gunController._source);
        }
        
        private void PlayerDoKill(OnPlayerDoKill data)
        {
            if (_killMarkerPrefab == null) return;
            if (data.p_owerId != _gunController.OwnerId) return;
            
            GameObject s = Instantiate(_killMarkerPrefab, _hitMarkerParent);
            _currentKillMarker = s;
            s.transform.position = _hitMarkerParent.position;
            
            SoundManager.PlaySound(_gunController._soundData, "Kill", _gunController._source);
            
            Destroy(_currentKillMarker.gameObject, 2f);
        }
    }
}