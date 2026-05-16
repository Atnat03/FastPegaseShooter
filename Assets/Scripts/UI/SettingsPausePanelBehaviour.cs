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
    
    //sound temporary
    MusicManager _musicManager;
    [SerializeField] Slider _musicVolumeSlider;
    [SerializeField] TMP_Text _musicVolumeText;

    Resolution[] _resolutions;
    private List<Resolution> _selectedResolutions = new();

    public override void Init()
    {
        ListenToEvent<OnMusicManagerLinkage>(OnMusicManagerSignal);
        
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
        
        //resolution
        if (PlayerPrefs.HasKey("ResolutionIndex")) _resolutionDropdown.value = PlayerPrefs.GetInt("ResolutionIndex", 0);
        else _resolutionDropdown.value = _resolutionDropdown.options.Count - 1;
        _resolutionDropdown.RefreshShownValue();

        //fullscreen ?
        bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        _fullscreenToggle.isOn = isFullscreen;

        //sensibilité
        float savedSens;
        if(PlayerPrefs.HasKey("MouseSensitivity"))savedSens = PlayerPrefs.GetFloat("MouseSensitivity");
        else savedSens = 50;
        _mouseSensitivitySlider.value = savedSens;
        _mouseSensitivityText.text = savedSens.ToString("F0");
        _fpsController.mouseSensitivity = Mathf.Lerp(0, _mouseSensitivityMaxValue, _mouseSensitivitySlider.value / _mouseSensitivitySlider.maxValue);
        
        // volume sonore
        ChangeMusicVolume(PlayerPrefs.GetFloat("MusicVolume", 100));
        _musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 100) * 100;
        _musicVolumeText.text = _musicVolumeSlider.value.ToString("F0")  + "%";
        
        ChangeResolution();

        InvokeEvent<OnPausePanelInit>(new OnPausePanelInit());
        gameObject.SetActive(false);
    }

    void OnMusicManagerSignal(OnMusicManagerLinkage data)
    {
        _musicManager = data.musicManager;
    }

    public override void OnPause(bool isPause)
    {
        gameObject.SetActive(false);
    }

    public void ChangeResolution()
    {
        Screen.SetResolution(_selectedResolutions[_resolutionDropdown.value].width,
            _selectedResolutions[_resolutionDropdown.value].height, _fullscreenToggle.isOn);

        PlayerPrefs.SetInt("ResolutionIndex", _resolutionDropdown.value);
        PlayerPrefs.SetInt("Fullscreen", _fullscreenToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ChangeMouseSensibility()
    {
        _fpsController.mouseSensitivity = Mathf.Lerp(0, _mouseSensitivityMaxValue,
            _mouseSensitivitySlider.value / _mouseSensitivitySlider.maxValue);
        _mouseSensitivityText.text = _mouseSensitivitySlider.value.ToString("F0");

        PlayerPrefs.SetFloat("MouseSensitivity", _mouseSensitivitySlider.value);
        PlayerPrefs.Save();
    }

    
    public void ChangeMusicVolume() => ChangeMusicVolume(_musicVolumeSlider.value);
    void ChangeMusicVolume(float newVolume)
    {
        if(!_musicManager)return;
        _musicVolumeText.text = newVolume.ToString("F0");
        newVolume /= 100;
        PlayerPrefs.SetFloat("MusicVolume", newVolume);
        _musicManager.SetVolume(newVolume);
    } 

    public void QuitPanel() => gameObject.SetActive(false);

    [ContextMenu("ResetAllPlayerPrefs")]
    public void ResetSetting()
    {
        PlayerPrefs.DeleteAll();
    }
}

public struct OnPausePanelInit
{
    
}

public struct OnMusicManagerLinkage
{
    public MusicManager musicManager;
}