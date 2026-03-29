using UnityEngine;

public class EliteWeakPointViewer : MonoBehaviour
{
    [SerializeField] private EnemyLifeModule _enemyLifeModule;

    void Awake()
    {
        _enemyLifeModule.OnDeath += WeakPointDestroyedObserverRPC;
    }

    void WeakPointDestroyedObserverRPC()
    {
        //callBack when weakpoint destroyed
        _enemyLifeModule.gameObject.SetActive(false);
    }
}
