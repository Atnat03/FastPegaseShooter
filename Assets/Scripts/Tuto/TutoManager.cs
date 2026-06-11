using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using MyPrint;
using ScriptableObjectsDefinitions;
using Tuto.Triggers;
using UnityEngine;

namespace Tuto
{
    public enum PlayerSide { Red, Blue }
    public enum Speaker { Red, Blue, AI }
    public enum NotificationTarget { Red, Blue, Both }
    public enum NotificationDisableAction
    {
        AfterDelay,
        OnFireModeChanged,
        OnLaserFired,
        OnDroneUsed,
        OnHealUsed
    }
    
    public struct OnLocalPlayerReady
    {
        public PlayerSide side;
    }
    
    public class TutoManager : NetworkBusListener
    {
        public DapManager DapManagerScript => _dapManager;
        
        [SerializeField] private ScenarioSO _scenarioSequence;

        [Header("References")] 
        [SerializeField] private DapManager _dapManager;
        
        [Header("LD Elements")]
        [SerializeField] private List<TriggerBoxBridge> _sceneProxies = new();
        [SerializeField] private List<SpawnZoneTutorial> _sceneSpawnZones = new();
        
        [Header("Sound")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private SoundsDataSO _soundsData;

        Dictionary<PlayerSide, NetworkObject> _playerList = new();
        
        //Actions
        public Action OnBothUseHeal;
        public Action OnDapUsed;
        
        public override void OnStartNetwork()
        {
            SetUpBridge();
            InitializeTriggers();
            
            if (IsServerInitialized)
                StartCoroutine(RunTutorial());
            
            ListenToEvent<OnPlayerSpawnEvent>(OnPlayerSpawn);
            ListenToEvent<OnHealUsed_TUTO>(CheckHealUse);
            ListenToEvent<OnDapEvent>(DapUsed);
        }

        private void CheckHealUse(OnHealUsed_TUTO data)
        {
            OnAddPlayerUsedHealServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void OnAddPlayerUsedHealServerRpc()
        {
            OnBothUseHeal?.Invoke();
        }

        private void OnPlayerSpawn(OnPlayerSpawnEvent data)
        {
            NetworkObject no = data.Transform.GetComponent<NetworkObject>();
            if (no == null) return;

            PlayerSide side = no.OwnerId == 0 ? PlayerSide.Red : PlayerSide.Blue;

            if (!_playerList.ContainsKey(side))
                _playerList[side] = no;

            if (no.IsOwner)
                InvokeEvent(new OnLocalPlayerReady { side = side });
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
                }
        
                if (scenario.trigger is Trigger_AllMobsDead mobTrigger)
                {
                    mobTrigger.InjectSpawnZones(_sceneSpawnZones);
                }
            }
        }
 
        private void InitializeTriggers()
        {
            foreach (Scenario scenario in _scenarioSequence._scenarioList)
                scenario.trigger?.Initialize(this);
        }
 
        private IEnumerator RunTutorial()
        {
            List<Coroutine> runningScenarios = new();

            foreach (Scenario scenario in _scenarioSequence._scenarioList)
                runningScenarios.Add(StartCoroutine(RunScenario(scenario)));

            foreach (Coroutine coroutine in runningScenarios)
                yield return coroutine;
        }

        private IEnumerator RunScenario(Scenario scenario)
        {
            if (scenario.trigger != null)
                yield return WaitForTrigger(scenario.trigger);

            foreach (BaseEvent evt in scenario.eventsList)
            {
                if (evt == null) continue;

                evt.SetManager(this);
                yield return StartCoroutine(evt.Execute());
            }
        }
 
        private IEnumerator WaitForTrigger(BaseTrigger trigger)
        {
            bool fired = false;

            trigger.Activate();
            trigger.OnActivated += Handler;
            
            yield return new WaitUntil(() => fired);
            
            trigger.OnActivated -= Handler;
            
            trigger.Dispose();
            
            yield break;

            void Handler() => fired = true;
        }

        #region Events
        
        #region DOOR
        public void AskForOpenDoor(int actionToDo, int doorIndex)
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
        void AskForOpenDoorServerRpc(int actionToDo, int doorIndex) => AskForOpenDoorObserversRpc(actionToDo, doorIndex);
        
        [ObserversRpc]
        void AskForOpenDoorObserversRpc(int actionToDo, int doorIndex)
        {
            InvokeEvent(new OnDoorOpen_TUTO
            {
                action = actionToDo, 
                indexDoor = doorIndex
            });
        }       
        #endregion
        
        #region DIALOGUE
        public void AskForDialogue(float duration, string dialogue, Speaker speaker, string keyVoceline, Action onComplete = null)
        {
            if (IsServerInitialized)
                AskForDialogueObserversRpc(duration, dialogue, speaker, keyVoceline);
            else
                AskForDialogueServerRpc(duration, dialogue, speaker, keyVoceline);

            StartCoroutine(DialogueRoutine(duration, onComplete));
        }

        private IEnumerator DialogueRoutine(float duration, Action onComplete)
        {
            yield return new WaitForSeconds(duration);

            AskForDialogueEndObserversRpc();

            onComplete?.Invoke();
        }

        [ObserversRpc]
        private void AskForDialogueEndObserversRpc()
        {
            InvokeEvent(new OnDialogueEnd_TUTO());
        }

        [ServerRpc]
        private void AskForDialogueServerRpc(float duration, string dialogue, Speaker speaker, string keyVoceline)
        {
            AskForDialogueObserversRpc(duration,dialogue, speaker, keyVoceline);
        }

        [ObserversRpc]
        private void AskForDialogueObserversRpc(float duration, string dialogue, Speaker speaker, string keyVoceline)
        {
            SoundManager.PlaySound(_soundsData, keyVoceline, _audioSource);
            
            InvokeEvent(new OnDialogue_TUTO
            {
                dialogue = dialogue,
                speaker = speaker
            });
        }
        
        #endregion
        
        #region NOTIFICATION
        
        public void AskForNotification(NotificationData data)
        {
            if(IsServerInitialized)
                SendNotificationToTargets(data);
        }

        private void SendNotificationToTargets(NotificationData data)
        {
            List<NetworkObject> targets = data.target switch
            {
                NotificationTarget.Red  => GetPlayer(PlayerSide.Red),
                NotificationTarget.Blue => GetPlayer(PlayerSide.Blue),
                NotificationTarget.Both => GetAllPlayers(),
                _ => new List<NetworkObject>()
            };

            foreach (NetworkObject player in targets)
            {
                if (player != null && player.Owner != null)
                    SendNotificationTargetRpc(player.Owner, data);
            }
        }
        
        [TargetRpc]
        private void SendNotificationTargetRpc(NetworkConnection conn, NotificationData data)
        {
            InvokeEvent(new OnNotification_TUTO
            {
                notificationText = data.text,
                speaker = data.target,
                activated = true,
                disableAction = data.disableAction,
                duration = data.duration
            });

            SoundManager.PlaySound(_soundsData, "Notification", _audioSource);
        }
        
        #endregion

        #region TakeDamage
        
        public void TakeDamage(int damage, int seuil)
        {
            if (IsServerInitialized)
            {
                foreach (NetworkObject player in GetAllPlayers())
                {
                    SendDamageToPlayer(player, damage, seuil);
                }
            }
            else
            {
                TakeDamageServerRpc(damage, seuil);
            }
        }

        [ServerRpc]
        private void TakeDamageServerRpc(int damage, int seuil)
        {
            foreach (NetworkObject player in GetAllPlayers())
            {
                SendDamageToPlayer(player, damage, seuil);
            }
        }
        
        private void SendDamageToPlayer(NetworkObject target, float damage, int seuil)
        {
            if (target == null) return;

            TargetTakeDamageRpc(target.Owner, target, damage, seuil);
        }
        
        [TargetRpc]
        private void TargetTakeDamageRpc(NetworkConnection conn, NetworkObject target, float damage, int seuil)
        {
            target.GetComponent<PlayerHealth>().RequestTakeDamageFromTutoServerRpc(Mathf.RoundToInt(damage), seuil);
        }

        #endregion
        
        #region Fill Amount

        public void FillAmount(float maxAmount, float speed, bool activated, AnimationBar type)
        {
            if (IsServerInitialized)
            {
                AskForFillAmountObserversRpc(maxAmount, speed, activated, type);
            }
            else
            {
                AskForFillAmountServerRpc(maxAmount, speed, activated, type);
            }
        }

        [ServerRpc]
        private void AskForFillAmountServerRpc(float maxAmount, float speed, bool activated, AnimationBar type)
        {
            AskForFillAmountObserversRpc(maxAmount, speed, activated, type);
        }

        [ObserversRpc]
        private void AskForFillAmountObserversRpc(float maxAmount, float speed, bool activated, AnimationBar type)
        {
            //SoundManager.PlaySound(_soundsData, keyVoceline, _audioSource);
            
            InvokeEvent(new OnFillAmount_TUTO
            {
                activated = activated,
                maxPercentage = maxAmount,
                speed = speed,
                type = type
            });
        }

        #endregion
        
        #region Unlock Capa

        public void AskForUnlockCapa(Capacity_TUTO capa)
        {
            if (IsServerInitialized)
            {
                AskForUnlockCapaObserversRpc(capa);
            }
            else
            {
                AskForUnlockCapaServerRpc(capa);
            }
        }

        [ServerRpc]
        private void AskForUnlockCapaServerRpc(Capacity_TUTO capa)
        {
            Cons.Print("AskForUnlockCapaServerRpc");
            AskForUnlockCapaObserversRpc(capa);
        }

        [ObserversRpc]
        private void AskForUnlockCapaObserversRpc(Capacity_TUTO capa)
        {
            Cons.Print("AskForUnlockCapaObserversRpc");
            InvokeEvent(new OnUnlockCapa_TUTO{capa = capa});
        }

        #endregion
        
        private List<NetworkObject> GetPlayer(PlayerSide side)
        {
            if (_playerList.TryGetValue(side, out NetworkObject player))
                return new List<NetworkObject> { player };

            return new List<NetworkObject>();
        }

        private List<NetworkObject> GetAllPlayers()
        {
            return new List<NetworkObject>(_playerList.Values);
        }

        #region Spawners

        public void AskForStartSpawn(List<int> spawnIndices)
        {
            if (IsServerInitialized)
            {
                foreach (int index in spawnIndices)
                    InvokeEvent(new OnStartSpawner_TUTO { spawnIndex = index });
            }
            else
            {
                AskForStartSpawnServerRpc(spawnIndices.ToArray());
            }
        }

        [ServerRpc]
        void AskForStartSpawnServerRpc(int[] spawnIndices)
        {
            foreach (int index in spawnIndices)
                InvokeEvent(new OnStartSpawner_TUTO { spawnIndex = index });
        }

        #endregion
        
        private void DapUsed(OnDapEvent data)
        {
            OnDapUsed?.Invoke();
        }
        
        #endregion

    }
}