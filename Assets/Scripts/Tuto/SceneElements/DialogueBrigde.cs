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
    [SerializeField] private Image _speakerIcon;
    [SerializeField] private TextMeshProUGUI _dialogueText;
    [SerializeField] private Image _txtBackGround;
    [SerializeField] private Color[] _sousTitreBGColorList;
    public Sprite RedRobotSprite;
    public Sprite BlueRobotSprite;
    public Sprite IASprite;

    [Header("Dialogue animations")]
    [SerializeField] private RectTransform animParent;
    [SerializeField] private AnimationCurve iconsArrivalScale;
    [SerializeField] private float iconsBounceHeightMultiplier;
    [SerializeField] private float iconsBounceSpeed;
    [SerializeField] private AudioSource audioSource;

    private bool dialogueRunning;
    private Coroutine dialogueCoroutine;
    private Vector3 _animParentDefaultPosition;
    private Vector3 _speakerIconDefaultPosition;
    private float[] _audioSamples = new float[256];

    #endregion

    #region Fonctions

    private void Awake()
    {
        ListenToEvent<OnDialogue_TUTO>(OnDialogue);
        ListenToEvent<OnDialogueEnd_TUTO>(CloseDialogue);

        _animParentDefaultPosition = animParent.localPosition;
        _speakerIconDefaultPosition = _speakerIcon.rectTransform.localPosition;
        _ui.SetActive(false);
    }

    private void OnDialogue(OnDialogue_TUTO data)
    {
        if (dialogueRunning)
        {
            StopCoroutine(dialogueCoroutine);
            ResetAnimationState();
        }

        _ui.SetActive(true);
        _speakerIcon.sprite = GetSprite(data.speaker);
        _dialogueText.text = data.dialogue;
        
        _txtBackGround.color = _sousTitreBGColorList[(int)data.speaker];

        dialogueCoroutine = StartCoroutine(PlayDialogueAnimation(data.duration));
    }

    private IEnumerator PlayDialogueAnimation(float duration)
    {
        dialogueRunning = true;

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            if (elapsedTime < 0.2f)
            {
                float t = elapsedTime / 0.2f;
                animParent.localScale = Vector3.one * iconsArrivalScale.Evaluate(t);
                _txtBackGround.rectTransform.localScale = Vector3.one * iconsArrivalScale.Evaluate(t);
            }
            else if (duration - elapsedTime < 0.2f)
            {
                float t = (duration - elapsedTime) / 0.2f;
                animParent.localScale = Vector3.one * iconsArrivalScale.Evaluate(t);
                _txtBackGround.rectTransform.localScale = Vector3.one * iconsArrivalScale.Evaluate(t);
            }
            else
            {
                float rms = GetRMS();
                _speakerIcon.rectTransform.localPosition = _speakerIconDefaultPosition + new Vector3( // ← _speakerIcon au lieu de animParent
                    0,
                    Mathf.Abs(Mathf.Sin(iconsBounceSpeed * elapsedTime * Mathf.PI)) * rms * iconsBounceHeightMultiplier,
                    0);
            }

            yield return new WaitForEndOfFrame();
        }

        dialogueRunning = false;
    }

    private void CloseDialogue(OnDialogueEnd_TUTO data)
    {
        if (dialogueRunning)
        {
            StopCoroutine(dialogueCoroutine);
            dialogueRunning = false;
        }

        ResetAnimationState();
        _ui.SetActive(false);
    }

    private void ResetAnimationState()
    {
        animParent.localScale = Vector3.one;
        animParent.localPosition = _animParentDefaultPosition;
        _speakerIcon.rectTransform.localPosition = _speakerIconDefaultPosition;
        if (_txtBackGround != null)
            _txtBackGround.rectTransform.localScale = Vector3.one;
    }

    private Sprite GetSprite(Speaker dataSpeaker)
    {
        switch (dataSpeaker)
        {
            case Speaker.Red:  return RedRobotSprite;
            case Speaker.Blue: return BlueRobotSprite;
            case Speaker.AI:   return IASprite;
            default:           return null;
        }
    }

    private float GetRMS()
    {
        audioSource.GetOutputData(_audioSamples, 0);
        float sum = 0f;
        foreach (var s in _audioSamples) sum += s * s;
        return Mathf.Sqrt(sum / _audioSamples.Length);
    }

    #endregion
}