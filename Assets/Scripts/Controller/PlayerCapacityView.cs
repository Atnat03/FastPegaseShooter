using System;
using MyPrint;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct CapacityUI
{
    public Image p_currentImage;
    public TextMeshProUGUI p_numberCapacityText;
}

public class PlayerCapacityView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerCapacity _playerCapacity;
    
    [Header("Data capacity")]
    [SerializeField] private CapacityUI _uiChargedShoot;
    [SerializeField] private CapacityUI _uiDrone;
    [SerializeField] private CapacityUI _uiHeal;

    void OnEnable()
    {
        _playerCapacity.OnUpdateCapacity += CheckUIToUpdate;
    }
    
    void OnDisable()
    {
        _playerCapacity.OnUpdateCapacity -= CheckUIToUpdate;
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
}
