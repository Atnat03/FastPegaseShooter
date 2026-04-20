using System;
using System.Collections.Generic;
using FishNet.Example.Scened;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelBehaviour : MonoBehaviour
{
    [SerializeField] PlayerPause _playerPause;
    [SerializeField] FPSController _fpsController;
    [SerializeField] Slider _mouseSensitivitySlider;
    [SerializeField] TMP_Text _mouseSensitivityText;
    [SerializeField] float _mouseSensitivityMaxValue;
    [SerializeField] TMP_Dropdown _resolutionDropdown;
    [SerializeField] Toggle _fullscreenToggle;

    Resolution[] _resolutions;
    private List<Resolution> _selectedResolutions = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _resolutions = Screen.resolutions;
        List<string> resolutionListString = new List<string>();
        string newResolutionString;
        foreach (Resolution resolution in _resolutions)
        {
            newResolutionString = resolution.width + "x" + resolution.height;
            if (!resolutionListString.Contains(newResolutionString))
            {
                resolutionListString.Add(newResolutionString);
                _selectedResolutions.Add(resolution);
            }
        }

        _resolutionDropdown.ClearOptions();
        _resolutionDropdown.AddOptions(resolutionListString);

        gameObject.SetActive(false);
    }

    void OnEnable()
    {
        _playerPause.OnPause += OnPause;
    }

    void OnDisable()
    {
        _playerPause.OnPause -= OnPause;
    }

    void OnPause(bool isPause)
    {
        gameObject.SetActive(false);
    }

    public void ChangeResolution()
    {
        Screen.SetResolution(_selectedResolutions[_resolutionDropdown.value].width, _selectedResolutions[_resolutionDropdown.value].height, _fullscreenToggle.isOn);
    } 

    public void ChangeMouseSensibility()
    {
        _fpsController.mouseSensitivity = Mathf.Lerp(0, _mouseSensitivityMaxValue, _mouseSensitivitySlider.value / _mouseSensitivitySlider.maxValue);
        _mouseSensitivityText.text = _mouseSensitivitySlider.value.ToString();
    }

    public void QuitPanel() => gameObject.SetActive(false);
}