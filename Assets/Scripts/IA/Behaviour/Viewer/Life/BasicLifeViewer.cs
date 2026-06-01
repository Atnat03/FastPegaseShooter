using TMPro;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.UI;

public class BasicLifeViewer : MonoBehaviour
{
    private EnemyCore _enemyCore;
    [SerializeField] private EnemyLifeModule _enemyLifeModule;
    
    [Header("HitMark")] //visuals
    [SerializeField] private Transform _hitMarkerParent;
    [SerializeField] private TextMeshProUGUI _textDmg;
    [SerializeField] private TextMeshProUGUI _textDmgCritique;
    private int _cumulatifDmg = 0;
    private float _elapsedCumulativeDmgTime = 0;
    private TextMeshProUGUI _hitMarker;

    [Header("Life")]
    [SerializeField] private TextMeshProUGUI _lifeTMP;
    [SerializeField] private Image _lifeBarImage;
    [SerializeField] private Gradient _NoneAffinityLifeGradient;
    [SerializeField] private float _fillSpeedBarFront = 10f;
    
    [SerializeField] private Image _lifeBarSecondImage;
    [SerializeField] private float _fillSpeedBarBack = 5f;
    [SerializeField] private float _timeBeforeSecondBarUpdate = 0.5f;
    private float _secondBarUpdateTime = 0;
    
    private float _frontFill = 1;
    private float _backFill = 1;
    
    
    void Awake()
    {
        _enemyCore = _enemyLifeModule.gameObject.GetComponent<EnemyCore>();
        
        _enemyLifeModule.OnLifeUpdate += LifeUpdating;
        _lifeBarImage.color = _NoneAffinityLifeGradient.Evaluate(1f);

        
        _lifeTMP.enabled = false;
        _lifeBarImage.enabled = false;
        _lifeBarSecondImage.enabled = false;
    }
    
    private void LifeUpdating(bool IsCritical, int dmg, int lifeAmount, int fullLife)
    {
        _lifeTMP.enabled = true;
        _lifeBarImage.enabled = true;
        _lifeBarSecondImage.enabled = true;
        
        _cumulatifDmg += dmg;
        float percentage = lifeAmount / (float)fullLife;
        _lifeTMP.text = $"{lifeAmount}/{fullLife}";
        _lifeBarImage.color = _NoneAffinityLifeGradient.Evaluate(percentage);

        //Percentage fills
        _frontFill = percentage;

        if (percentage < _backFill)
        {
            _secondBarUpdateTime = _timeBeforeSecondBarUpdate;
        }        _secondBarUpdateTime = _timeBeforeSecondBarUpdate;

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
        _lifeBarImage.fillAmount = Mathf.MoveTowards(
            _lifeBarImage.fillAmount,
            _frontFill,
            _fillSpeedBarFront * Time.deltaTime
        );

        if (_secondBarUpdateTime > 0)
        {
            _secondBarUpdateTime -= Time.deltaTime;
        }
        else
        {
            _lifeBarSecondImage.fillAmount = Mathf.MoveTowards(
                _lifeBarSecondImage.fillAmount,
                _frontFill,
                _fillSpeedBarBack * Time.deltaTime
            );
        }
        
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
