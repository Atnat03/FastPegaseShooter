using System;
using MyPrint;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DapManagerView : MonoBusListener
{
    [SerializeField] private DapManager _dapManager;

    [Header("UI Bar")]
    [SerializeField] private Image _energyBar;
    [SerializeField] private TextMeshProUGUI _textPercentage;
    [Header("Messages")]
    [SerializeField] private TextMeshProUGUI _textMessage;
    [SerializeField] private string[] _messages;

    [Header("Dapping")]
    [SerializeField] private GameObject _dappingExplosion;

    private void OnEnable()
    {
        _dapManager.OnPercentageChange += UpdateUI;
        _dapManager.OnMessageUpdate += UpdateMessages;
        _dapManager.OnDapping += Dapping;
    }

    private void OnDisable()
    {
        _dapManager.OnPercentageChange -= UpdateUI;
        _dapManager.OnMessageUpdate -= UpdateMessages;
        _dapManager.OnDapping -= Dapping;
    }

    private void Dapping(Vector3 pos)
    {
        Cons.Print("Dapping effect !! ");

        Destroy(Instantiate(_dappingExplosion, pos, Quaternion.identity), 5f);
    }

    private void UpdateMessages(int idMessage)
    {
        _textMessage.gameObject.SetActive(idMessage > -1);

        if (_textMessage.gameObject.activeSelf)
            _textMessage.text = _messages[idMessage];
    }

    private void UpdateUI(int activeBarIndex, float activeFill)
    {
        _energyBar.fillAmount = activeFill;
        _textPercentage.text = ((int)(activeFill * 100)).ToString();
    }
}