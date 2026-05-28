using System;
using System.Collections.Generic;
using MyPrint;
using UnityEngine;
using UnityEngine.UI;

public class PlayerEnergyView : MonoBehaviour
{
	#region Variables

	[Header("References")]
	[SerializeField] private PlayerEnergy _energyPlayer;
	
	[Header("UI")]
	[SerializeField] private Image _imageBarPrefab;
	[SerializeField] private Sprite[] _energyBarSprites;
	[SerializeField] private Transform _barParent;
    
	[SerializeField] private Color[] _energyBarColorFull;
	[SerializeField] private Color[] _energyBarColorNotFull;
	private int _currentIndexCharge = 0;
	
	private List<Image> _energyBarsImageList = new List<Image>();
	
	#endregion

	#region Fonctions
	
	
	private void UpdateUI(int activeBarIndex, float activeFill)
	{
		for (int i = 0; i < _energyBarsImageList.Count; i++)
		{
			if (i < activeBarIndex)
			{
				_energyBarsImageList[i].fillAmount = 1f;
				_energyBarsImageList[i].color = _energyBarColorFull[_currentIndexCharge];
			}
			else if (i == activeBarIndex)
			{
				_energyBarsImageList[i].fillAmount = activeFill;
				_energyBarsImageList[i].color = activeFill >= 1f ? _energyBarColorFull[_currentIndexCharge] : _energyBarColorNotFull[_currentIndexCharge];
			}
			else
			{
				_energyBarsImageList[i].fillAmount = 0f;
			}
		}
	}
	
	
	private void UpdateUIColor(bool isPositive)
	{
		_currentIndexCharge = isPositive ? 0 : 1;
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

	private void OnEnable()
	{
		_energyPlayer.OnCreateBarUI += CreateUI;
		_energyPlayer.OnUpdateUI += UpdateUI;
		_energyPlayer.OnUpdateCharge += UpdateUIColor;
	}


	private void OnDisable()
	{
		_energyPlayer.OnCreateBarUI -= CreateUI;
		_energyPlayer.OnUpdateUI -= UpdateUI;
		_energyPlayer.OnUpdateCharge -= UpdateUIColor;
	}

	#endregion
}
