using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerIconLobby : MonoBehaviour
{
	#region Properties

	#endregion

	#region Variables

	[SerializeField] private Image _logo;
	[SerializeField] private Image _checkReady;
	[SerializeField] private Sprite[] _spriteList;
	
	#endregion
	
	#region Fonctions

	private void OnEnable()
	{
		IsReady(false);
	}

	public void ChangeLogo(int newCharaId)
	{
		//_logo.sprite = _spriteList[newCharaId];
	}

	public void IsReady(bool state)
	{
		_checkReady.gameObject.SetActive(state);
	}
	
	#endregion
}
