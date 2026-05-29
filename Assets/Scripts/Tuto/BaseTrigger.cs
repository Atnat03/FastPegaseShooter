using System;
using UnityEngine;

namespace Tuto
{
    [Serializable]
    public abstract class BaseTrigger
    {
        public Action OnActivated;
        public abstract string DisplayName { get; }

        // Appelé par TutoManager au démarrage pour que le trigger
        // puisse s'initialiser (s'abonner à des events, trouver des objets…)
        public virtual void Initialize() { }

        // Nettoyage quand le scénario est terminé
        public virtual void Dispose() { }
    }
}