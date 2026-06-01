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
        [SerializeField] private List<TriggerBoxBridge> _sceneProxies = new();
 
        private void Start()
        {
            SetUpBridge();
            InitializeTriggers();
            StartCoroutine(RunTutorial());
        }
 
        private void SetUpBridge()
        {
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
                        Debug.LogWarning($"Aucun proxy avec l'index {boxTrigger.proxyIndex} trouvé dans la scène.");
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

        public void ActivateDoor()
        {
            Cons.Print("Activating door");
        }

        /*public void AskForOpenDoor(int actionToDo, int doorIndex)
        {
            if (IsServerInitialized)
            {
                AskForOpenDoorObserversRpc(actionToDo, doorIndex);
            }else
            {
                AskForOpenDoorServerRpc(actionToDo, doorIndex);
            }
        }
        
        [ServerRpc]
        void AskForOpenDoorServerRpc(int actionToDo, int doorIndex)
        {
            Cons.Print("AskForOpenDoorServerRpc");
            
            AskForOpenDoorObserversRpc(actionToDo, doorIndex);
        }
        
        [ObserversRpc]
        void AskForOpenDoorObserversRpc(int actionToDo, int doorIndex)
        {
            Cons.Print("Ask for open door");
            
            InvokeEvent(new OnDoorOpen_TUTO
            {
                action = actionToDo, 
                indexDoor = doorIndex
            });
        }*/
    }
    
    public struct OnDoorOpen_TUTO
    {
        public int action;
        public int indexDoor;
    }

}