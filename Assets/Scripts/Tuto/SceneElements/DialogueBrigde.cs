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
		
		_backGroundDialogue.color = GetColor(data.speaker);
		_dialogueText.text = data.dialogue;
		
	}
	
	private void CloseDialogue(OnDialogueEnd_TUTO data)
	{
		_ui.SetActive(false);
	}

	
	private Color GetColor(Speaker dataSpeaker)
	{
		switch (dataSpeaker)
		{
			case Speaker.Red:
				return Color.red;
			case Speaker.Blue:
				return Color.blue;
			case Speaker.AI:
				return Color.purple;
			default:
				return Color.white;
		}
	}

	#endregion
}
