using System;
using System.Collections.Generic;
using FishNet.Example.Scened;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPausePanelBehaviour : PausePanel
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
    
    public override void Init()
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

        // load des playerprefs
        
        int savedResolution = PlayerPrefs.GetInt("ResolutionIndex", 0);
        _resolutionDropdown.value = savedResolution;
        _resolutionDropdown.RefreshShownValue();
        
        bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        _fullscreenToggle.isOn = isFullscreen;
        
        float savedSens = PlayerPrefs.GetFloat("MouseSensitivity", _mouseSensitivityMaxValue / 2f);
        _mouseSensitivitySlider.value = savedSens;
        _mouseSensitivityText.text = savedSens.ToString("F0");
        _fpsController.mouseSensitivity = Mathf.Lerp(0, _mouseSensitivityMaxValue, _mouseSensitivitySlider.value / _mouseSensitivitySlider.maxValue);;
        
        ChangeResolution();

        gameObject.SetActive(false);
    }

    public override void OnPause(bool isPause)
    {
        gameObject.SetActive(false);
    }

    public void ChangeResolution()
    {
        Screen.SetResolution(_selectedResolutions[_resolutionDropdown.value].width, _selectedResolutions[_resolutionDropdown.value].height, _fullscreenToggle.isOn);
        
        PlayerPrefs.SetInt("ResolutionIndex", _resolutionDropdown.value);
        PlayerPrefs.SetInt("Fullscreen", _fullscreenToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();
    } 

    public void ChangeMouseSensibility()
    {
        _fpsController.mouseSensitivity = Mathf.Lerp(0, _mouseSensitivityMaxValue, _mouseSensitivitySlider.value / _mouseSensitivitySlider.maxValue);
        _mouseSensitivityText.text = _mouseSensitivitySlider.value.ToString("F0");
        
        PlayerPrefs.SetFloat("MouseSensitivity", _mouseSensitivitySlider.value);
        PlayerPrefs.Save();
    }

    public void QuitPanel() => gameObject.SetActive(false);
}