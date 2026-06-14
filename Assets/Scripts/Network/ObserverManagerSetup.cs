using FishNet;
using FishNet.Managing.Observing;
using FishNet.Observing;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using FishNet.Component.Observing;
using FishNet.Object;
using UnityEngine;

public class ObserverManagerSetup : MonoBehaviour
{
    [SerializeField] private SceneCondition _sceneCondition;

    private void Start()
    {
        StartCoroutine(WaitForObserverManager());
    }

    private IEnumerator WaitForObserverManager()
    {
        ObserverManager observerManager = null;

        while (observerManager == null)
        {
            observerManager = InstanceFinder.NetworkManager?.GetComponent<ObserverManager>();
            yield return null;
        }

        observerManager.SetUpdateHostVisibility(false, HostVisibilityUpdateTypes.Manager);

        FieldInfo field = typeof(ObserverManager).GetField("_defaultConditions", 
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (field != null)
        {
            var conditions = field.GetValue(observerManager) as List<ObserverCondition>;
            if (conditions != null && !conditions.Contains(_sceneCondition))
                conditions.Add(_sceneCondition);
        }

        // Attendre que tous les joueurs soient spawned
        while (InstanceFinder.ServerManager.Clients.Count == 0)
            yield return null;

        // Laisser un frame de plus pour que tous les spawns soient traités
        yield return new WaitForSeconds(0.5f);
        
    }
}