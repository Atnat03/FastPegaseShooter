using System;
using System.Collections.Generic;
using MyPrint;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DapManagerView : MonoBusListener
{
    [SerializeField] private DapManager _dapManager;
    
    [Header("UI Bar")]
    [SerializeField] private Image _imageBarPrefab;
    [SerializeField] private Sprite[] _energyBarSprites;
    [SerializeField] private Transform _barParent;   
    [SerializeField] private Color _energyBarColorFull;
    [SerializeField] private Color _energyBarColorNotFull;
    private float _targetFill;
    
    private List<Image> _energyBarsImageList = new List<Image>();
	
    [Header("Messages")]
    [SerializeField] private TextMeshProUGUI _textMessage;
    [SerializeField] private string[] _messages;
    
    [Header("Dapping")]
    [SerializeField] private GameObject _dappingExplosion;
    
    private void OnEnable()
    {
        _dapManager.OnPercentageChange += UpdateUI;
        _dapManager.OnCreateBarUI += CreateUI;
        _dapManager.OnMessageUpdate += UpdateMessages;
        
        _dapManager.OnDapping += Dapping;
    }

    private void Dapping(Vector3 pos)
    {
        Cons.Print("Dapping effect !! ");
        
        Destroy(Instantiate(_dappingExplosion, pos, Quaternion.identity), 5f);
    }

    private void UpdateMessages(int idMessage)
    {
        _textMessage.gameObject.SetActive(idMessage > -1);
        
        if(_textMessage.gameObject.activeSelf)
            _textMessage.text = _messages[idMessage];
    }

    #region BARS
    
    private void UpdateUI(int activeBarIndex, float activeFill)
    {
        for (int i = 0; i < _energyBarsImageList.Count; i++)
        {
            if (i < activeBarIndex)
            {
                _energyBarsImageList[i].fillAmount = 1f;
                _energyBarsImageList[i].color = _energyBarColorFull;
            }
            else if (i == activeBarIndex)
            {
                _energyBarsImageList[i].fillAmount = activeFill;
                _energyBarsImageList[i].color = activeFill >= 1f ? _energyBarColorFull : _energyBarColorNotFull;
            }
            else
            {
                _energyBarsImageList[i].fillAmount = 0f;
            }
        }
    }
    
    private void CreateUI(int totalBars)
    {
        for (int i = 0; i < totalBars; i++)
        {
            Image newImage = Instantiate(_imageBarPrefab, _barParent);
            
            if(i == 0)
            {
                newImage.sprite = _energyBarSprites[0];
            }
            else if(i == totalBars-1)
            {
                newImage.sprite = _energyBarSprites[2];
            }
            else
            {
                newImage.sprite = _energyBarSprites[1];
            }
            
            _energyBarsImageList.Add(newImage);
        }
    }
    
    
    #endregion
}