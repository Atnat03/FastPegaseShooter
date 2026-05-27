using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BroHealthView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image _broHealthBar;
    [SerializeField] private float _fillSpeed = 5f;

    [SerializeField] private PlayerHealth _localPlayerHealth;

    private PlayerHealth _trackedAlly;
    private float _targetFill = 1f;

    private void OnEnable()
    {
        if (PlayerHealthManager.Instance != null)
            PlayerHealthManager.Instance.OnRegistryUpdated += RefreshAlly;
        
        RefreshAlly();
    }

    private void OnDisable()
    {
        if (PlayerHealthManager.Instance != null)
            PlayerHealthManager.Instance.OnRegistryUpdated -= RefreshAlly;

        UnsubscribeAlly();
    }

    private void Update()
    {
        if (_broHealthBar != null)
            _broHealthBar.fillAmount = Mathf.Lerp(
                _broHealthBar.fillAmount, _targetFill, Time.deltaTime * _fillSpeed);
    }

    private void RefreshAlly()
    {
        UnsubscribeAlly();

        foreach (PlayerHealth player in PlayerHealthManager.Instance.RegisteredPlayers)
        {
            if (player == _localPlayerHealth) continue;

            _trackedAlly = player;
            break;
        }

        bool hasAlly = _trackedAlly != null;

        if (hasAlly)
        {
            SubscribeAlly();
            _targetFill = _trackedAlly.CurrentHealth / 100f;
        }
    }

    private void SubscribeAlly()
    {
        if (_trackedAlly == null) return;
        _trackedAlly.OnUpdateHealth += OnAllyHealthChanged;
        _trackedAlly.OnKOPlayer += OnAllyKO;
    }

    private void UnsubscribeAlly()
    {
        if (_trackedAlly == null) return;
        _trackedAlly.OnUpdateHealth -= OnAllyHealthChanged;
        _trackedAlly.OnKOPlayer -= OnAllyKO;
        _trackedAlly = null;
    }

    private void OnAllyHealthChanged(float fillAmount)
    {
        _targetFill = fillAmount;
    }

    private void OnAllyKO(bool isDead, float respawnDuration)
    {
        if (_broHealthBar != null)
            _broHealthBar.color = isDead ? Color.red : Color.white;
    }
}