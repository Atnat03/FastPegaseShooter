using UnityEngine;
using UnityEngine.Events;

public class GenericTriggerListener : MonoBusListener
{
    [SerializeField] private int _listeningId;
    [SerializeField] private UnityEvent _action;
    private void Awake()
    {
        ListenToEvent<GenericTriggerEvent>(GTE =>
        {
            if (GTE.p_Id == _listeningId)
            {
                _action.Invoke();
            }
        });
    }
}