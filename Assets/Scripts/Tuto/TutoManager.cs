using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using MyPrint;
using Tuto.Triggers;
using UnityEngine;

namespace Tuto
{
    public enum PlayerSide { Red, Blue }
    
    public enum Speaker { Red, Blue, AI }
    public enum NotificationTarget { Red, Blue, Both }
    public enum NotificationDismissMode
    {
        AfterDelay,
        OnFireModeChanged,
        OnLaserFired,
        OnDroneUsed,
        OnHealUsed
    }
    
    public class TutoManager : NetworkBusListener
    {
        [SerializeField] private ScenarioSO _scenarioSequence;
 
        // Glisse ici tous les TriggerBoxProxy présents dans la scène.
        // Leur champ "Proxy Index" doit correspondre à l'index configuré dans le ScenarioSO.
        [SerializeField] private List<TriggerBoxBridge> _sceneProxies = new();
 
        private void Start()
        {
            SetUpBridge();
            InitializeTriggers();
            StartCoroutine(RunTutorial());
        }
 
        // Résout chaque Trigger_BoxCollider → proxy correspondant par index
        private void SetUpBridge()
        {
            // Construit un dictionnaire index → proxy pour la résolution rapide
            Dictionary<int, TriggerBoxBridge> proxyMap = new Dictionary<int, TriggerBoxBridge>();
            foreach (TriggerBoxBridge proxy in _sceneProxies)
                proxyMap[proxy.bridgeIndex] = proxy;
 
            foreach (Scenario scenario in _scenarioSequence._scenarioList)
            {
                if (scenario.trigger is Trigger_BoxCollider boxTrigger)
                {
                    if (proxyMap.TryGetValue(boxTrigger.proxyIndex, out var proxy))
                        boxTrigger.InjectProxy(proxy);
                    else
                        Debug.LogWarning($"[TutoManager] Aucun proxy avec l'index {boxTrigger.proxyIndex} trouvé dans la scène.");
                }
            }
        }
 
        private void InitializeTriggers()
        {
            foreach (Scenario scenario in _scenarioSequence._scenarioList)
                scenario.trigger?.Initialize();
        }
 
        private IEnumerator RunTutorial()
        {
            foreach (Scenario scenario in _scenarioSequence._scenarioList)
            {
                if (scenario.trigger != null)
                    yield return WaitForTrigger(scenario.trigger);
 
                foreach (BaseEvent evt in scenario.eventsList)
                {
                    if (evt == null)
                        continue;

                    evt.SetManager(this);
                    
                    yield return StartCoroutine(evt.Execute());
                }
            }
        }
 
        private IEnumerator WaitForTrigger(BaseTrigger trigger)
        {
            bool fired = false;

            trigger.OnActivated += Handler;
            
            yield return new WaitUntil(() => fired);
            
            trigger.OnActivated -= Handler;
            
            trigger.Dispose();
            
            yield break;

            void Handler() => fired = true;
        }

        [ServerRpc]
        public void AskForOpenDoorServerRpc(Event_OpenDoor.Door actionToDo, int doorIndex)
        {
            AskForOpenDoorObserversRpc(actionToDo, doorIndex);
        }
        
        [ObserversRpc]
        private void AskForOpenDoorObserversRpc(Event_OpenDoor.Door actionToDo, int doorIndex)
        {
            Cons.Print("Ask for open door");
            
            InvokeEvent(new OnDoorOpen_TUTO
            {
                action = actionToDo,
                indexDoor = doorIndex
            });
        }
    }

}