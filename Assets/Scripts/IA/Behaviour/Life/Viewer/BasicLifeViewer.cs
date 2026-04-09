using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("Life")]
    [SerializeField] private TextMeshProUGUI _lifeTMP;
    [SerializeField] private GameObject _lifeBarParent;
    [SerializeField] private Image _lifeBarImage;
    [SerializeField] private Color _fullLifeColor;
    [SerializeField] private Color _emptyLifeColor;
    
    
    void Awake()
    {
        _enemyLifeModule.OnLifeUpdate += LifeUpdating;
        _lifeBarImage.color = _fullLifeColor;
        
        _lifeBarParent.gameObject.SetActive(false);
        _lifeTMP.gameObject.SetActive(false);
    }
    
    public void LifeUpdating(bool IsCritical, int dmg, int lifeAmount, int fullLife)
    {
        _lifeBarParent.gameObject.SetActive(true);
        _lifeTMP.gameObject.SetActive(true);
        
        _cumulatifDmg += dmg;
        float percentage = lifeAmount / (float)fullLife;
        _lifeTMP.text = $"{lifeAmount}/{fullLife}";
        _lifeBarImage.color = Color.Lerp(_emptyLifeColor, _fullLifeColor, percentage);
        _lifeBarImage.fillAmount = percentage;

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
