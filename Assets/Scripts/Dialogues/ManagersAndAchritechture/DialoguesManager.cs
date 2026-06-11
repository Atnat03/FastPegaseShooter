
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

    [Header("UI Elements")] 
    public Image speakerImage;
    public TextMeshProUGUI dialogueText;
    public Image txtBackGround;
    public GameObject[] EverythingRelated;

    [Header("audio Elements")] 
    public AudioSource audioSource;

    [Header("References")] 
    [SerializeField]private GunSwitching gunSwitch;

    private bool dialogueRunning;
    private Coroutine dialogueCoroutine;
    private DialogueListener selfColor;

    private bool AdressedToMe(DialogueLine line) => (line.listener == DialogueListener.Both || line.listener == selfColor);

    
    #region Setup
    void OnEnable()
    {
        gunSwitch.OnSwapGun += OnSwapGun;
    }

    void OnDisable()
    {
        gunSwitch.OnSwapGun -= OnSwapGun;

    }

    private void OnSwapGun(bool isPos)
    {
        selfColor = isPos ? DialogueListener.Red : DialogueListener.Blue;
    }
    
    #endregion

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        ListenToEvent<OnDialogueStart>(StartDialogue);

        CleanDialogue();
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
        foreach (GameObject go in EverythingRelated)
        {
            go.SetActive(true);
        }
        foreach (DialogueLine line in data.dialogueData.lines)
        {
            if (AdressedToMe(line))
            {
                DisplayLine(line);
                yield return new WaitForSeconds(line.audioClip.length);
                CleanDialogue();
                yield return new WaitForSeconds(line.DelayBeforeNextLine);
            }
        }

        dialogueRunning = false;
        CleanDialogue();
    }
    
    void DisplayLine(DialogueLine line)
    {
        if(!AdressedToMe(line)) return;

        foreach (GameObject go in EverythingRelated)
        {
            go.SetActive(true);
        }
        
        switch (line.speaker)
        {
            case DialogueSpeaker.IA :
                speakerImage.sprite = IAIcon;
                txtBackGround.color = IAColor;
                break;
            case DialogueSpeaker.Red :
                speakerImage.sprite = redIcon;
                txtBackGround.color = redColor;
                break;
            case DialogueSpeaker.Blue :
                speakerImage.sprite = blueIcon;
                txtBackGround.color = blueColor;
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
        
        foreach (GameObject go in EverythingRelated)
        {
            go.SetActive(false);
        }
    }
}

public struct OnDialogueStart
{
    public DialoguesDataSO dialogueData;
}