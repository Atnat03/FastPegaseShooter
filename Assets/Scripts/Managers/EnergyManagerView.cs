using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnergyManagerView : MonoBehaviour
{
	#region Properties

	#endregion


	#region Variables

	[Header("References")]
	[SerializeField] private EnergyManager _energyManager;
	
	[Header("UI")]
	[SerializeField] private Image _imageBarPrefab;
	[SerializeField] private Sprite[] _energyBarSprites;
	[SerializeField] private Transform _barParent;
    
	[SerializeField] private Color _energyBarColorFull;
	[SerializeField] private Color _energyBarColorNotFull;
	
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
				Debug.Log("First");
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

	/*private void OnEnable()
	{
		_energyManager.OnCreateBarUI += CreateUI;
		_energyManager.OnUpdateUI += UpdateUI;
	}


	private void OnDisable()
	{
		_energyManager.OnCreateBarUI -= CreateUI;
		_energyManager.OnUpdateUI -= UpdateUI;
	}*/

	#endregion
}
