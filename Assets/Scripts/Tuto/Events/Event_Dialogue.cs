using System.Collections;
using MyPrint;
using ScriptableObjectsDefinitions;
using UnityEngine;

namespace Tuto
{
    public class Event_Dialogue : BaseEvent
    {
        public override string DisplayName => "Dialogue";
 
        [TextArea(2, 5)]
        public string _dialogue;
        public string _keyVoceline;
        public float _duration = 3f;
        public Speaker speaker;
 
        public override IEnumerator Execute()
        {
            manager.AskForDialogue(_duration,_dialogue, speaker, _keyVoceline);
            
            yield return new WaitForSeconds(_duration);
        }
    }
}