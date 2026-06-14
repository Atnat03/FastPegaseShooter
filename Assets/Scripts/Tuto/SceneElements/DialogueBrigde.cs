using System;
using System.Collections;
using TMPro;
using Tuto;
using UnityEngine;
using UnityEngine.UI;

public class DialogueBrigde : MonoBusListener
{
	#region Variables

	[SerializeField] private GameObject _ui;
	[SerializeField] private Image _backGroundDialogue;
	[SerializeField] private TextMeshProUGUI _dialogueText;
	public Sprite RedRobotSprite;
	public Sprite BlueRobotSprite;
	public Sprite IASprite;

	#endregion


	#region Fonctions

	private void Awake()
	{
		ListenToEvent<OnDialogue_TUTO>(OnDialogue);
		ListenToEvent<OnDialogueEnd_TUTO>(CloseDialogue);
		
		_ui.SetActive(false);
	}

	private void OnDialogue(OnDialogue_TUTO data)
	{
		_ui.SetActive(true);
		
		_backGroundDialogue.sprite = GetSprite(data.speaker);
		_dialogueText.text = data.dialogue;
		
	}
	
	private void CloseDialogue(OnDialogueEnd_TUTO data)
	{
		_ui.SetActive(false);
	}

	
	private Sprite GetSprite(Speaker dataSpeaker)
	{
		switch (dataSpeaker)
		{
			case Speaker.Red:
				return RedRobotSprite;
			case Speaker.Blue:
				return BlueRobotSprite;
			case Speaker.AI:
				return IASprite;
			default:
				return null;
		}
	}

	#endregion
}
