using MyPrint;
using UnityEngine;
using UnityEngine.UI;

public class EnemyCoreViewer : MonoBehaviour
{
    [SerializeField] private EnemyCore _enemyCore;
    
    [Header("View")] 
    [SerializeField] private GameObject _p1_UI;
    [SerializeField] private Image _p1_CurValue;
    [SerializeField] private GameObject _p2_UI;
    [SerializeField] private Image _p2_CurValue;
    [SerializeField] private Color[] _colorsJaugesCharges;
    
    [SerializeField] private ParticleSystem _explosionParticle;

    [Header("Shied")]
    [SerializeField] private MeshRenderer _shied;
    [SerializeField] private Material[] _materialShied;
    
    void Awake()
    {
        _enemyCore.p_OnChargeExplosion += OnChargeExplosion;
        _enemyCore.p_OnPlayer1ChargeChange += OnPositiveChargeChange;
        _enemyCore.p_OnPlayer2ChargeChange += OnNegativeChargeChange;
        
        //Shied
        _enemyCore.OnSetShied += SetShield;
    }

    private void SetShield(EnemyCore.ChargeType hasShield)
    {
        if(_shied == null)
            return;
        
        switch (hasShield)
        {
            case EnemyCore.ChargeType.Negative :
                _shied.gameObject.SetActive(true);
                _shied.material = _materialShied[1];
                break;
            case EnemyCore.ChargeType.Positive :
                _shied.gameObject.SetActive(true);
                _shied.material = _materialShied[0];
                break;
            default:
                _shied.gameObject.SetActive(false);
                break;
        }
    }

    private void OnChargeExplosion()
    {
        Destroy(Instantiate(_explosionParticle, transform.position + Vector3.up, Quaternion.identity), 2f);
    }
    private void OnPositiveChargeChange(bool positive, float ratio)
    {
        _p1_UI.SetActive(ratio > 0);
        _p1_CurValue.fillAmount = ratio;
        
        _p1_CurValue.color = _colorsJaugesCharges[positive ? 0 : 1];
    }
    private void OnNegativeChargeChange(bool positive, float ratio)
    {
        _p2_UI.SetActive(ratio > 0);
        _p2_CurValue.fillAmount = ratio;
        
        _p2_CurValue.color = _colorsJaugesCharges[positive ? 0 : 1];
    }
}
