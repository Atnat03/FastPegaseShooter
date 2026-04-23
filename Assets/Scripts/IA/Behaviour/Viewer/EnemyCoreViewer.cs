using UnityEngine;
using UnityEngine.UI;

public class EnemyCoreViewer : MonoBehaviour
{
    [SerializeField] private EnemyCore _enemyCore;
    
    [Header("View")] 
    [SerializeField] private GameObject _positiveUI;
    [SerializeField] private Image _positiveCurValue;
    [SerializeField] private GameObject _negativeUI;
    [SerializeField] private Image _negativeCurValue;
    
    [SerializeField] private ParticleSystem _explosionParticle;

    void Awake()
    {
        _enemyCore.p_OnChargeExplosion += OnChargeExplosion;
        _enemyCore.p_OnPositiveChargeChange += OnPositiveChargeChange;
        _enemyCore.p_OnNegativeChargeChange += OnNegativeChargeChange;
    }
    
    private void OnChargeExplosion()
    {
        Destroy(Instantiate(_explosionParticle, transform.position + Vector3.up, Quaternion.identity), 2f);
    }
    private void OnPositiveChargeChange()
    {
        _positiveUI.SetActive(_enemyCore.p_currentPositiveCharge > 0);
        _positiveCurValue.fillAmount = _enemyCore.p_currentPositiveCharge / _enemyCore.p_positiveChargeMax;
    }
    private void OnNegativeChargeChange()
    {
        _negativeUI.SetActive(_enemyCore.p_currentNegativeCharge > 0);
        _negativeCurValue.fillAmount = _enemyCore.p_currentNegativeCharge / _enemyCore.p_negativeChargeMax;
    }
}
