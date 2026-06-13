using System.Collections;
using MyPrint;
using ScriptableObjectsDefinitions;
using UnityEngine;

namespace Tuto
{
// Event_Dialogue.cs
    public class Event_Dialogue : BaseEvent
    {
        public override string DisplayName => "Dialogue";

        [TextArea(2, 5)]
        public string _dialogue;
        public string _keyVoceline;
        public float _delayAfter = 0f;
        public Speaker speaker;

        public override IEnumerator Execute()
        {
            bool dialogueEnded = false;
            manager.AskForDialogue(_delayAfter, _dialogue, speaker, _keyVoceline, () => dialogueEnded = true);
            yield return new WaitUntil(() => dialogueEnded);
        }
    }
}