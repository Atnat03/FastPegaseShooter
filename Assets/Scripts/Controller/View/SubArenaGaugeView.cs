using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SubArenaGaugeView : MonoBusListener
{
    [SerializeField] private Transform SubArenaGaugeParent;

    private Dictionary<Guid, SubArenaGauge> _idToInfos = new();

    private void Awake()
    {
        ListenToEvent<OnSubArenaStartEvent>(OSASE =>
        {
            _idToInfos.Add(OSASE.p_arenaID, Instantiate(OSASE.p_arenaGaugePrefab, SubArenaGaugeParent));
            
            SubArenaGauge info = _idToInfos[OSASE.p_arenaID];
            
            info.p_gauge.fillAmount = 0;
        });
        
        ListenToEvent<OnSubArenaUpdateEvent>(OSAUE =>
        {
            if (!_idToInfos.TryGetValue(OSAUE.p_arenaID, out var info)) return;

            if(info.p_gauge)
            {
                info.p_gauge.fillAmount = OSAUE.p_overCrowdingPercent;
                info.p_gauge.color = OSAUE.p_state.p_color;
            }
            
            if(info.p_icon) info.p_icon.sprite = OSAUE.p_state.p_icon;
            
            if(info.p_overCrowded) info.p_overCrowded.SetActive(OSAUE.p_overCrowdingPercent >= 1);
        });
        
        //clear all gauges
        ListenToEvent<OnDapEvent>(ODE =>
        {
            foreach (var SubArenaInfoPair in _idToInfos)
            {
                Destroy(SubArenaInfoPair.Value.gameObject);
            }
            _idToInfos.Clear();
        });
        ListenToEvent<OnPlayerSpawnTPEvent>(OPSTPE =>
        {
            foreach (var SubArenaInfoPair in _idToInfos)
            {
                Destroy(SubArenaInfoPair.Value.gameObject);
            }
            _idToInfos.Clear();
        });
    }
}