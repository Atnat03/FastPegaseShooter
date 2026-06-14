using UnityEngine;
using UnityEngine.Events;

public class DapTriggerZone : MonoBusListener
{
    enum DapEventType {OnDap, AfterDapVideo}
    [SerializeField] private DapEventType _dapEventType;
    [SerializeField] private UnityEvent _action;

    void Awake()
    {
        switch (_dapEventType)
        {
            case DapEventType.OnDap:
                ListenToEvent<OnDappEventObserveurs>((ODE) =>
                {
                    _action.Invoke();
                });
                break;
            case DapEventType.AfterDapVideo:
                ListenToEvent<AfterDapVideoEvent>((ADVE) =>
                {
                    _action.Invoke();
                });
                break;
        }
    }
}
