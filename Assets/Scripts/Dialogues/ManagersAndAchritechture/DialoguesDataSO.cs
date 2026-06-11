using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialoguesDataSO", menuName = "DialoguesDataSO")]
public class DialoguesDataSO : ScriptableObject
{
    public List<DialogueLine> lines;
}

public enum DialogueSpeaker
{
    IA,
    Blue,
    Red
}

public enum DialogueListener
{
    Both,
    Red,
    Blue
}

[Serializable]
public class DialogueLine
{
    public DialogueSpeaker speaker;
    public DialogueListener listener;
    public AudioClip audioClip;
    public string text;
    public float DelayBeforeNextLine;
    public float volume;
}