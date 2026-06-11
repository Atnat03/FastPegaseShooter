using System;
using System.Collections;
using FishNet.Object;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialoguesManager : NetworkBusListener
{
    [Header("speakers")] public Sprite blueIcon;
    public Color blueColor;
    public Sprite redIcon;
    public Color redColor;
    public Sprite IAIcon;
    public Color IAColor;

    [Header("UI Elements")] public Image speakerImage;
    public TextMeshProUGUI dialogueText;

    [Header("audio Elements")] public AudioSource audioSource;

    private bool dialogueRunning;
    private Coroutine dialogueCoroutine;
    private DialogueListener selfColor;

    private void OnServerInitialized()
    {
        ListenToEvent<OnDialogueStart>(StartDialogue);
    }

    void StartDialogue(OnDialogueStart data)
    {
        if (dialogueRunning)
        {
            StopCoroutine(dialogueCoroutine);
            CleanDialogue();
        }

        dialogueCoroutine = StartCoroutine(DisplayDialogue(data));
    }

    IEnumerator DisplayDialogue(OnDialogueStart data)
    {
        dialogueRunning = true;
        foreach (DialogueLine line in data.dialogueData.lines)
        {
            DisplayLine(line);
            yield return new WaitForSeconds(line.duration);
        }

        dialogueRunning = false;
        CleanDialogue();
    }
    
    void DisplayLine(DialogueLine line)
    {
        if(!(line.listener == DialogueListener.Both || line.listener == selfColor)) return;

        switch (line.speaker)
        {
            case DialogueSpeaker.IA :
                speakerImage.sprite = IAIcon;
                dialogueText.color = IAColor;
                break;
            case DialogueSpeaker.Red :
                speakerImage.sprite = redIcon;
                dialogueText.color = redColor;
                break;
            case DialogueSpeaker.Blue :
                speakerImage.sprite = blueIcon;
                dialogueText.color = blueColor;
                break;
        }
        
        dialogueText.text = line.text;
        audioSource.Stop();
        audioSource.clip = line.audioClip;
        audioSource.volume = line.volume;
        audioSource.Play();
    }
    

    void CleanDialogue()
    {
        speakerImage.sprite = null;
        dialogueText.text = "";
        audioSource.Stop();
    }
}

public struct OnDialogueStart
{
    public DialoguesDataSO dialogueData;
}