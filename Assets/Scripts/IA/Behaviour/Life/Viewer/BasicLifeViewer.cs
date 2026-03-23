using TMPro;
using UnityEngine;

public class BasicLifeViewer : MonoBehaviour
{
    [SerializeField] private EnemyLifeModule _enemyLifeModule;
    
    [Header("HitMark")] //visuals
    [SerializeField] private Transform _hitMarkerParent;
    [SerializeField] private TextMeshProUGUI _textDmg;
    [SerializeField] private TextMeshProUGUI _textDmgCritique;
    [SerializeField] private int _cumulatifDmg = 0;
    [SerializeField] private float _elapsedCumulativeDmgTime = 0;
    private TextMeshProUGUI _hitMarker;
    

    void Awake()
    {
        _enemyLifeModule.OnLifeUpdate += TriggerHitMarkObserversRpc;
    }
    
    public void TriggerHitMarkObserversRpc(bool IsCritical, int dmg)
    {
        _cumulatifDmg += dmg;

        if (_elapsedCumulativeDmgTime <= 0)
        {
            TextMeshProUGUI text = IsCritical ? _textDmgCritique : _textDmg;
            _hitMarker = Instantiate(text.gameObject, _hitMarkerParent).GetComponent<TextMeshProUGUI>();
            _hitMarker.SetText(_cumulatifDmg.ToString());
            _elapsedCumulativeDmgTime = 0.05f;

            Destroy(_hitMarker.gameObject, 0.5f);
        }
        else
        {
            if (_hitMarker != null)
                _hitMarker.SetText(_cumulatifDmg.ToString());
        }
    }

    private void Update()
    {
        if (_elapsedCumulativeDmgTime > 0)
        {
            _elapsedCumulativeDmgTime -= Time.deltaTime;
        }
        else
        {
            _cumulatifDmg = 0;
        }
    }
}
