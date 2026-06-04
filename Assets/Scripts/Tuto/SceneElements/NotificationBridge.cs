using System;
using System.Collections;
using TMPro;
using Tuto;
using UnityEngine;

public class NotificationBridge : MonoBusListener
{
    [SerializeField] private GameObject _ui;
    [SerializeField] private TextMeshProUGUI _notificationText;

    private Coroutine _disableCoroutine;
    private PlayerSide? _localSide;

    private void Awake()
    {
        ListenToEvent<OnLocalPlayerReady>(OnLocalPlayerReady);
        ListenToEvent<OnNotification_TUTO>(OnNotification);
        ListenToEvent<OnFireModeChanged_TUTO>(OnFireModeChanged);
        ListenToEvent<OnLaserFired_TUTO>(OnLaserFired);
        ListenToEvent<OnDroneUsed_TUTO>(OnDroneUsed);
        ListenToEvent<OnHealUsed_TUTO>(OnHealUsed);

        _ui.SetActive(false);
    }

    private void OnLocalPlayerReady(OnLocalPlayerReady data)
    {
        _localSide = data.side;
    }
    
    private void OnNotification(OnNotification_TUTO data)
    {
        if (_localSide.HasValue && data.speaker != NotificationTarget.Both)
        {
            bool isForMe = (data.speaker == NotificationTarget.Red && _localSide == PlayerSide.Red)
                           || (data.speaker == NotificationTarget.Blue && _localSide == PlayerSide.Blue);
            if (!isForMe) return;
        }

        if (_disableCoroutine != null)
            StopCoroutine(_disableCoroutine);

        _ui.SetActive(data.activated);

        if (data.activated)
        {
            _notificationText.text = data.notificationText;
            _disableCoroutine = StartCoroutine(DisableAfterAction(data.disableAction, data.duration));
        }
    }

    private IEnumerator DisableAfterAction(NotificationDisableAction action, float duration)
    {
        switch (action)
        {
            case NotificationDisableAction.AfterDelay:
                yield return new WaitForSeconds(duration);
                HideNotification();
                break;

            case NotificationDisableAction.OnFireModeChanged:
                yield return new WaitUntil(() => _fireModeChanged);
                _fireModeChanged = false;
                HideNotification();
                break;

            case NotificationDisableAction.OnLaserFired:
                yield return new WaitUntil(() => _laserFired);
                _laserFired = false;
                HideNotification();
                break;

            case NotificationDisableAction.OnDroneUsed:
                yield return new WaitUntil(() => _droneUsed);
                _droneUsed = false;
                HideNotification();
                break;

            case NotificationDisableAction.OnHealUsed:
                yield return new WaitUntil(() => _healUsed);
                _healUsed = false;
                HideNotification();
                break;
        }
    }

    private void HideNotification()
    {
        _ui.SetActive(false);
        _disableCoroutine = null;
    }

    // Flags set by incoming game events
    private bool _fireModeChanged;
    private bool _laserFired;
    private bool _droneUsed;
    private bool _healUsed;

    private void OnFireModeChanged(OnFireModeChanged_TUTO _) => _fireModeChanged = true;
    private void OnLaserFired(OnLaserFired_TUTO _) => _laserFired = true;
    private void OnDroneUsed(OnDroneUsed_TUTO _) => _droneUsed = true;
    private void OnHealUsed(OnHealUsed_TUTO _) => _healUsed = true;
}