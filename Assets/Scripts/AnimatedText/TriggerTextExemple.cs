using System;
using MyPrint;
using UnityEngine;
using UnityEngine.Events;

public class TriggerTextExemple : MonoBehaviour
{
    public UnityEvent onTrigger;
    
    [ContextMenu("Trigger Effect")]
    public void TestTrigger()
    {
        Cons.Print("Trigger Effect", ColorConsole.Black); 
        onTrigger?.Invoke();
    }
}
