using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SubArenaGaugeView : MonoBusListener
{
    [SerializeField] private SubArenaGauge SubArenaGaugePrefab;
    [SerializeField] private Transform SubArenaGaugeParent;

    private Dictionary<Guid, SubArenaGauge> _idToInfos = new();

    private void Awake()
    {
        ListenToEvent<OnSubArenaUpdateEvent>(OSAUE =>
        {
            if (!_idToInfos.ContainsKey(OSAUE.p_arenaID))
            {
                _idToInfos.Add(OSAUE.p_arenaID, Instantiate(SubArenaGaugePrefab, SubArenaGaugeParent));
            }

            _idToInfos[OSAUE.p_arenaID].p_gauge.fillAmount = OSAUE.p_overCrowdingPercent;
            _idToInfos[OSAUE.p_arenaID].p_gauge.color = OSAUE.p_state.p_color;
            
            _idToInfos[OSAUE.p_arenaID].p_icon.sprite = OSAUE.p_state.p_icon;
            
            _idToInfos[OSAUE.p_arenaID].p_nameTMP.text = $"{OSAUE.p_arenaName} - {OSAUE.p_state.p_name}";
            _idToInfos[OSAUE.p_arenaID].p_nameTMP.color = OSAUE.p_state.p_color;
            
            
            _idToInfos[OSAUE.p_arenaID].p_overCrowded.SetActive(OSAUE.p_overCrowdingPercent >= 1);
        });
        
        ListenToEvent<OnDapEvent>(ODE =>
        {
            foreach (var SubArenaInfoPair in _idToInfos)
            {
                Destroy(SubArenaInfoPair.Value.gameObject);
            }
            _idToInfos.Clear();
        });
    }
}