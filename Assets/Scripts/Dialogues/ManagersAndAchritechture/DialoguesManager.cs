using System;
using System.Collections;
using FishNet.Object;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialoguesManager : MonoBusListener
{
    [Header("parameters")] 
    [HideInInspector] public bool dialoguesActivated;
    [HideInInspector][Range(0,1)] public float dialoguesAudioVolume;
    
    
    [Header("speakers")] public Sprite blueIcon;
    public Color blueColor;
    public Sprite redIcon;
    public Color redColor;
    public Sprite IAIcon;
    public Color IAColor;

    [Header("UI Elements")] public Image speakerImage;
    public TextMeshProUGUI dialogueText;
    public Image txtBackGround;
    public GameObject[] EverythingRelated;

    [Header("audio Elements")] public AudioSource audioSource;

    [Header("References")] [SerializeField]
    private GunSwitching gunSwitch;

    [Header("Juicy")] 
    [SerializeField] private AnimationCurve iconsArrivalScale;
    [SerializeField] private float iconsBounceHeightMultiplier;
    [SerializeField] private float iconsBounceSpeed;

    private bool dialogueRunning;
    private Coroutine dialogueCoroutine;
    private DialogueListener selfColor;
    private Vector3 speakerImageDefaultPosition;
    private float[] _audioSamples = new float[256];


    private bool AdressedToMe(DialogueLine line) =>
        (line.listener == DialogueListener.Both || line.listener == selfColor);


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

    public void Start()
    {
        ListenToEvent<OnDialogueStart>(StartDialogue);
        speakerImageDefaultPosition = speakerImage.rectTransform.localPosition;
        CleanDialogue();
    }

    void StartDialogue(OnDialogueStart data)
    {
        if (!dialoguesActivated) return;
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
            if (AdressedToMe(line))
            {
                DisplayLine(line);
                float elapsedTime = 0f;
                while (elapsedTime < line.audioClip.length)
                {
                    elapsedTime += Time.deltaTime;
                    if (elapsedTime < .2f)
                    {
                        speakerImage.rectTransform.localScale =
                            Vector3.one * iconsArrivalScale.Evaluate(elapsedTime / .2f);
                        txtBackGround.rectTransform.localScale =
                            Vector3.one * iconsArrivalScale.Evaluate(elapsedTime / .2f);
                    }
                    else if (line.audioClip.length - elapsedTime < .2f)
                    {
                        speakerImage.rectTransform.localScale =
                            Vector3.one * iconsArrivalScale.Evaluate((line.audioClip.length - elapsedTime) / .2f);
                        txtBackGround.rectTransform.localScale =
                            Vector3.one * iconsArrivalScale.Evaluate((line.audioClip.length - elapsedTime)  / .2f);
                    }

                    else
                    {
                        float rms = GetRMS();
                        speakerImage.rectTransform.localPosition = speakerImageDefaultPosition + new Vector3(0,
                            Mathf.Abs(Mathf.Sin(iconsBounceSpeed * elapsedTime * Mathf.PI)) *
                            rms * iconsBounceHeightMultiplier, 0);
                    }

                    yield return new WaitForEndOfFrame();
                }

                CleanDialogue();
                yield return new WaitForSeconds(line.DelayBeforeNextLine);
            }
        }

        dialogueRunning = false;
        CleanDialogue();
    }

    void DisplayLine(DialogueLine line)
    {
        if (!AdressedToMe(line)) return;

        //rapport a l'Affichage
        foreach (GameObject go in EverythingRelated)
        {
            go.SetActive(true);
        }

        switch (line.speaker)
        {
            case DialogueSpeaker.IA:
                speakerImage.sprite = IAIcon;
                txtBackGround.color = IAColor;
                break;
            case DialogueSpeaker.Red:
                speakerImage.sprite = redIcon;
                txtBackGround.color = redColor;
                break;
            case DialogueSpeaker.Blue:
                speakerImage.sprite = blueIcon;
                txtBackGround.color = blueColor;
                break;
        }
        
        dialogueText.text = line.text;

        //rapport a l'audioSource
        audioSource.Stop();
        audioSource.clip = line.audioClip;
        audioSource.volume = line.volume * dialoguesAudioVolume;
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
    
    float GetRMS()
    {
        audioSource.GetOutputData(_audioSamples, 0);
        float sum = 0f;
        foreach (var s in _audioSamples) sum += s * s;
        return Mathf.Sqrt(sum / _audioSamples.Length);
    }

}

public struct OnDialogueStart
{
    public DialoguesDataSO dialogueData;
}