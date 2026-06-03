using System;
using System.Collections.Generic;
using Tuto;
using Tuto.Triggers;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Tuto
{
    [CreateAssetMenu(fileName = "New Scenario", menuName = "Tutoriel/ScenarioSO")]
    public class ScenarioSO : ScriptableObject, ISerializationCallbackReceiver
    {
        public List<Scenario> _scenarioList = new List<Scenario>();

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            HashSet<BaseTrigger> seen = new HashSet<BaseTrigger>(ReferenceEqualityComparer.Instance);

            foreach (Scenario scenario in _scenarioList)
            {
                if (scenario.trigger == null) continue;

                if (!seen.Add(scenario.trigger))
                {
                    scenario.trigger = scenario.trigger.Clone();
                }
            }
        }
    }

    [Serializable]
    public class Scenario
    {
        public string ScenarioName;
        [SerializeReference] public BaseTrigger trigger;
        [SerializeReference] public List<BaseEvent> eventsList = new List<BaseEvent>();
    }
}