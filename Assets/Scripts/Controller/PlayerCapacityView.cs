using System;
using MyPrint;
using TMPro;
using Tuto;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct CapacityUI
{
    public GameObject p_rootGO;
    public Image p_currentImage;
    public TextMeshProUGUI p_numberCapacityText;
    public ParticleSystem p_particleGetCharge;
}

public class PlayerCapacityView : NetworkBusListener
{
    [Header("References")]
    [SerializeField] private PlayerCapacity _playerCapacity;
    [SerializeField] private PlayerTuto _playerTuto;

    [SerializeField] private Color[] _colorFill;
    
    [Header("Data capacity")]
    [SerializeField] private CapacityUI _uiChargedShoot;
    [SerializeField] private CapacityUI _uiDrone;
    [SerializeField] private CapacityUI _uiHeal;
    
    void OnEnable()
    {
        _playerCapacity.OnUpdateCapacity += CheckUIToUpdate;
        _playerCapacity.OnUseCapacity += UseCapacity;

        _playerTuto.OnUnlockCapa += UnlockCapa;
        
        ListenToEvent<OnPlayerOk>(SetUpFillColor);
    }

    void OnDisable()
    {
        _playerCapacity.OnUpdateCapacity -= CheckUIToUpdate;
        _playerCapacity.OnUseCapacity -= UseCapacity;
    }
    
    private void SetUpFillColor(OnPlayerOk data)
    {
        if(Owner.ClientId != data.playerID)
            return;
        
        Color c = data.IsPositive ? _colorFill[0] : _colorFill[1];

        _uiChargedShoot.p_currentImage.color = c;
        _uiDrone.p_currentImage.color = c;
        _uiHeal.p_currentImage.color = c;
    }

    private void CheckUIToUpdate(CapacityData data)
    {
        switch (data.p_capacity)
        {
            case Capacity.ChargedShoot:
                UpdateUI(ref data, ref _uiChargedShoot);
                break;
            case Capacity.Drone:
                UpdateUI(ref data, ref _uiDrone);
                break;
            case Capacity.Heal:
                UpdateUI(ref data, ref _uiHeal);
                break;
        }
    }

    private void UpdateUI(ref CapacityData data, ref CapacityUI ui)
    {
        ui.p_currentImage.fillAmount = data.p_currentPercentageCapacity / 100;
        ui.p_numberCapacityText.text = data.p_currentNumberCapacity.ToString();
    }
    
    private void UseCapacity(CapacityData data)
    {
        switch (data.p_capacity)
        {
            case Capacity.ChargedShoot:
                PlayerVfxUse(ref data, ref _uiChargedShoot);
                break;
            case Capacity.Drone:
                PlayerVfxUse(ref data, ref _uiDrone);
                break;
            case Capacity.Heal:
                PlayerVfxUse(ref data, ref _uiHeal);
                break;
        }
    }
    
    private void PlayerVfxUse(ref CapacityData data, ref CapacityUI ui)
    {
        ui.p_particleGetCharge.Play();
    }
    
    private void UnlockCapa(Capacity_TUTO capa, bool state)
    {
        switch (capa)
        {
            case Capacity_TUTO.ChargedShoot:
                _uiChargedShoot.p_rootGO.SetActive(state);
                break;
            case Capacity_TUTO.Drone:
                _uiDrone.p_rootGO.SetActive(state);
                break;
            case Capacity_TUTO.Heal:
                _uiHeal.p_rootGO.SetActive(state);
                break;
            default:
                break;
        }
    }

}
