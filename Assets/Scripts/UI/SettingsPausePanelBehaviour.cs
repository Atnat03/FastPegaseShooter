using System;
using System.Collections.Generic;
using FishNet.Example.Scened;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsPausePanelBehaviour : PausePanel
{
    [SerializeField] PlayerPause _playerPause;
    [SerializeField] FPSController _fpsController;
    [SerializeField] DialoguesManager _dialogueManager;
    [SerializeField] RectTransform crossair;
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] Slider _mouseSensitivitySlider;
    [SerializeField] TMP_Text _mouseSensitivityText;
    [SerializeField] float _mouseSensitivityMaxValue;
    [SerializeField] TMP_Dropdown _resolutionDropdown;
    [SerializeField] Toggle _fullscreenToggle;
    [SerializeField] private Toggle _dialoguesActivated;
    [SerializeField] private Slider _dialoguesAudioVolumeSlider;
    [SerializeField] private TMP_Text _dialoguesAudioVolumeText;
    [SerializeField] private Slider _crossairSizeSlider;
    [SerializeField] private TMP_Text _crossairSizeText;
    
    //sound temporary
    MusicManager _musicManager;
    [SerializeField] Slider _musicVolumeSlider;
    [SerializeField] TMP_Text _musicVolumeText;
    [SerializeField] Slider _SFXVolumeSlider;
    [SerializeField] TMP_Text _SFXVolumeText;

    Resolution[] _resolutions;
    private List<Resolution> _selectedResolutions = new();
    private Vector3 defaultCrossairScale;

    public override void Init()
    {
        defaultCrossairScale = crossair.localScale;
        
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
        _resolutionDropdown.value = PlayerPrefs.GetInt("ResolutionIndex", _resolutionDropdown.options.Count - 1);
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
        if (audioMixer.SetFloat("Music", Mathf.Lerp(-20,20,_musicVolumeSlider.value))) ;
        
        ChangeVFXVolume(PlayerPrefs.GetFloat("SFXVolume", 100));
        _SFXVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 100) * 100;
        _SFXVolumeText.text = _SFXVolumeSlider.value.ToString("F0")  + "%";
        if (audioMixer.SetFloat("SFX", Mathf.Lerp(-20,20,_SFXVolumeSlider.value))) ;

        
        //dialogues
        bool dialoguesOn = PlayerPrefs.GetInt("dialoguesActivated", 1) == 1;
        _dialoguesActivated.isOn = dialoguesOn;
        ActivateDialogues(dialoguesOn);
        
        ChangeDialoguesVolume(PlayerPrefs.GetFloat("DialoguesVolume", 100) * 100);
        _dialoguesAudioVolumeSlider.value = PlayerPrefs.GetFloat("DialoguesVolume", 100) * 100;
        _dialoguesAudioVolumeText.text = _dialoguesAudioVolumeSlider.value.ToString("F0") + "%";
        
        //crossair
        float crossairSize = PlayerPrefs.GetFloat("CrossairSize", 100);
        _crossairSizeSlider.value = crossairSize;
        _crossairSizeText.text = _crossairSizeSlider.value.ToString("F0")+ "%";
        crossair.localScale = defaultCrossairScale * (crossairSize / 100);
        
        
        ChangeResolution();

        InvokeEvent<OnPausePanelInit>(new OnPausePanelInit());
        gameObject.SetActive(false);
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
        _mouseSensitivityText.text = _mouseSensitivitySlider.value.ToString("F0")+ "%";

        PlayerPrefs.SetFloat("MouseSensitivity", _mouseSensitivitySlider.value);
        PlayerPrefs.Save();
    }
    
    public void ChangeMusicVolume() => ChangeMusicVolume(_musicVolumeSlider.value);
    void ChangeMusicVolume(float newVolume)
    {
        if(!audioMixer)return;
        _musicVolumeText.text = newVolume.ToString("F0") + "%";
        newVolume /= 100;
        PlayerPrefs.SetFloat("MusicVolume", newVolume);
        if (audioMixer.SetFloat("Music", Mathf.Lerp(-20,20,newVolume))) ;
        else Debug.LogError("Music does not exist");
    }

    public void ChangeVFXVolume() => ChangeVFXVolume(_SFXVolumeSlider.value);
    void ChangeVFXVolume(float newValue)
    {
        if(!audioMixer)return;
        _SFXVolumeText.text = newValue.ToString("F0") + "%";
        newValue /= 100;
        PlayerPrefs.SetFloat("SFXVolume", newValue);
        if (audioMixer.SetFloat("SFX", Mathf.Lerp(-20,20,newValue))) ;
        else Debug.LogError("SFX does not exist");
    }

    public void ActivateDialogues() => ActivateDialogues(_dialoguesActivated.isOn);
    void ActivateDialogues(bool activate)
    {
        if (!_dialogueManager) return;
        _dialogueManager.dialoguesActivated  = activate;
        PlayerPrefs.SetInt("dialoguesActivated", _dialoguesActivated.isOn ? 1 : 0);
    }

    public void ChangeDialoguesVolume() => ChangeDialoguesVolume(_dialoguesAudioVolumeSlider.value);
    void ChangeDialoguesVolume(float newVolume)
    {
        if (!_dialogueManager) return;
        _dialoguesAudioVolumeText.text = newVolume.ToString("F0") + "%";
        newVolume /= 100;
        PlayerPrefs.SetFloat("DialoguesVolume", newVolume);
        _dialogueManager.dialoguesAudioVolume = newVolume;
    }

    public void ChangeCrossairSize()=> ChangeCrossairSize(_crossairSizeSlider.value);
    void ChangeCrossairSize(float newSize)
    {
        if (!crossair) return;
        _crossairSizeText.text = newSize.ToString("F0") + "%";
        newSize /= 100;
        PlayerPrefs.SetFloat("CrossairSize", newSize);
        crossair.localScale = defaultCrossairScale *  newSize;
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