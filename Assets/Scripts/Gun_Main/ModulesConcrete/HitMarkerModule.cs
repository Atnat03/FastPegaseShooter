using UnityEngine;
using UnityEngine.UI;

namespace GunDecorator
{
    public class HitMarkerModule : GunModule, IHitMarkerModule
    {
        [SerializeField, Tooltip("Le parent est le réticule du joueur")] 
        private Transform _hitMarkerParent;
        
        [Header("Hit")]
        [SerializeField] private GameObject _hitMarkerPrefab;
        [SerializeField] private Color _hitMarkerColor = Color.white;
        
        [Header("Hit critique")]
        [SerializeField] private GameObject _hitMarkerCritiquePrefab;
        [SerializeField] private Color _hitMarkerCritiqueColor = Color.red;
        
        public void HitMark()
        {
            Debug.Log("HitMark");
            Image s = Instantiate(_hitMarkerPrefab, _hitMarkerParent).GetComponent<Image>();
            if (s != null)
            {
                s.color = _hitMarkerColor;
            }
            Destroy(s, 0.1f);
        }

        public void HitMarkCritique()
        {
            Image s = Instantiate(_hitMarkerCritiquePrefab, _hitMarkerParent).GetComponent<Image>();
            if (s != null)
            {
                s.color = _hitMarkerCritiqueColor;
            }
            Destroy(s, 0.1f);
        }
    }
}