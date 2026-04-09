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
        
        public void HitMark()
        {
            Image s = Instantiate(_hitMarkerPrefab, _hitMarkerParent).GetComponent<Image>();
            if (s != null)
            {
                s.color = _hitMarkerColor;
            }
            
            SoundManager.PlaySound(_gunController._soundData, "HitMark", _gunController._source);
            
            Destroy(s, 0.1f);
        }

        public void HitMarkCritique()
        {
            Image s = Instantiate(_hitMarkerCritiquePrefab, _hitMarkerParent).GetComponent<Image>();
            if (s != null)
            {
                s.color = _hitMarkerCritiqueColor;
            }
            
            AudioClip clip = SoundManager.GetAudioClip(_gunController._soundData,"HitMark");
            SoundManager.PlaySound(clip, _gunController._source, 0.1f);
            
            Destroy(s, 0.1f);
        }
    }
}