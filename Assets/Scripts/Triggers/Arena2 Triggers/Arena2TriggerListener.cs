using UnityEngine;
using UnityEngine.Events;

public class Arena2TriggerListener : MonoBusListener
{
    public enum TriggerType {Arena1, Arena2, Arena3}
    [SerializeField] private TriggerType _triggerType;
    [SerializeField] private UnityEvent _action;
    private void Awake()
    {
        
        switch (_triggerType)
        {
            case TriggerType.Arena1:
                ListenToEvent<OnArena2FirstEvent>(e => _action?.Invoke());
                break;
            case TriggerType.Arena2:
                ListenToEvent<OnArena2SecondEvent>(e => _action?.Invoke());
                break;
            case TriggerType.Arena3:
                ListenToEvent<OnArena2ThirdEvent>(e => _action?.Invoke());
                break;
        }
    }
}
