using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIColor : NetworkBusListener
{
	#region Properties

	#endregion


	#region Variables

	[SerializeField] private GunSwitching _gunSwitching;
	[SerializeField] private Image[] _imageList;
	[SerializeField] private Color[] _colorList;
	
	#endregion


	#region Fonctions

	public override void OnStartNetwork()
	{
		ListenToEvent<OnPlayerSetUp>(SetUp);
	}

	private void SetUp(OnPlayerSetUp data)
	{
		Color c = data.isPositive ? _colorList[0] : _colorList[1];
		
		foreach (Image image in _imageList)
		{
			image.color = c;
		}
	}

	#endregion
}
