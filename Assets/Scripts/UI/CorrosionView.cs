using System;
using System.Collections.Generic;
using UnityEngine;

public class CorrosionView : MonoBusListener
{
    [SerializeField] private GameObject _corrodedFeedback;
    private HashSet<Guid> _corrodedArenaIDs = new HashSet<Guid>();
    private void Awake()
    {
        ListenToEvent<OnDapEvent>(ODE =>
        {
            _corrodedArenaIDs.Clear();
            HideCorrosionFeedback();
        });
        
        ListenToEvent<OnSubArenaUpdateEvent>(OSAUE =>
        {
            if(!_corrodedArenaIDs.Contains(OSAUE.p_arenaID))
            {
                if (OSAUE.p_overCrowdingPercent >= 1)
                {
                    if (_corrodedArenaIDs.Count <= 0) ShowCorrosionFeedback();
                    _corrodedArenaIDs.Add(OSAUE.p_arenaID);
                }
            }
            else if(OSAUE.p_overCrowdingPercent < 1)
            {
                _corrodedArenaIDs.Remove(OSAUE.p_arenaID);
                if(_corrodedArenaIDs.Count <= 0) HideCorrosionFeedback();
            }
        });
        
        ListenToEvent<OnPlayerSpawnTPEvent>(OPTPE =>
        {
            _corrodedArenaIDs.Clear();
            HideCorrosionFeedback();
        });
    }

    void ShowCorrosionFeedback()
    {
        _corrodedFeedback.SetActive(true);
    }
    void HideCorrosionFeedback()
    {
        _corrodedFeedback.SetActive(false);
    }
}
