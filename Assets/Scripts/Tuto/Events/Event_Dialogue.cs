using System.Collections;
using MyPrint;
using UnityEngine;

namespace Tuto
{
    public class Event_Dialogue : BaseEvent
    {
        public override string DisplayName => "Dialogue";
 
        [TextArea(2, 5)]
        public string _dialogue;
        public AudioClip _voiceline;
        public float _duration = 3f;
        public Speaker speaker;
 
        public override IEnumerator Execute()
        {
            Cons.Print("Start dialogue", ColorConsole.Blue);
            
            yield return new WaitForSeconds(_duration);
            
            Cons.Print("End dialogue", ColorConsole.Blue);
        }
    }
}