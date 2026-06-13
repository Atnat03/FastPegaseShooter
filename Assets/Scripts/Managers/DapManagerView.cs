using System;
using System.Collections;
using MyPrint;
using ScriptableObjectsDefinitions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class DapManagerView : MonoBusListener
{
    [SerializeField] private DapManager _dapManager;

    [Header("UI Bar")]
    [SerializeField] private Image _energyBar;
    [SerializeField] private TextMeshProUGUI _textPercentage;
    
    [Header("Messages")]
    [SerializeField] private TextMeshProUGUI _textMessage;
    [SerializeField] private string[] _messages;
    [SerializeField] private GameObject _dapNotification;

    [Header("Dapping")]
    [SerializeField] private GameObject _dappingExplosion;
    [SerializeField] private VideoPlayer _videoClipDap;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private SoundsDataSO _dataSound;

    private bool alreadyPlaySound;

    private void OnEnable()
    {
        _dapManager.OnPercentageChange += UpdateUI;
        _dapManager.OnMessageUpdate += UpdateMessages;
        _dapManager.OnDapping += Dapping;
    }

    private void OnDisable()
    {
        _dapManager.OnPercentageChange -= UpdateUI;
        _dapManager.OnMessageUpdate -= UpdateMessages;
        _dapManager.OnDapping -= Dapping;
    }

    private void Dapping(Vector3 pos)
    {
        Cons.Print("Dapping effect !! ");

        _dapNotification.SetActive(false);
        alreadyPlaySound = false;
        
        StartCoroutine(DappingCoroutine(pos));
    }

    IEnumerator DappingCoroutine(Vector3 pos)
    {
        _videoClipDap.gameObject.SetActive(true);
        
        SoundManager.PlaySound(_dataSound, "Dap", _audioSource);

        _dapManager.SetGlobalCanvaOrder(1000);
        
        yield return new WaitForSeconds((float)_videoClipDap.clip.length);
        
        _dapManager.SetGlobalCanvaOrder(2);
        
        AfterDapVideo(pos);
    }

    void AfterDapVideo(Vector3 pos)
    {
        InvokeEvent(new AfterDapVideoEvent());
        _videoClipDap.gameObject.SetActive(false);
        
        Destroy(Instantiate(_dappingExplosion, pos, Quaternion.identity), 5f);
    }
    
    private void UpdateMessages(int idMessage)
    {
        if (_textMessage == null)
            return;

        _textMessage.gameObject.SetActive(idMessage > -1);

        if (_textMessage.gameObject.activeSelf)
            _textMessage.text = _messages[idMessage];
    }

    private void UpdateUI(float fillAmount)
    {
        _energyBar.fillAmount = fillAmount;
        _textPercentage.text = Mathf.RoundToInt(fillAmount * 100f) + "%";

        _dapNotification.SetActive(fillAmount >= 1f);

        if (_dapNotification.activeSelf && !alreadyPlaySound)
        {
            alreadyPlaySound = true;
            InvokeEvent(new PlayUISound{keySound = "CanDap"});
        }
    }
}

public struct AfterDapVideoEvent{}