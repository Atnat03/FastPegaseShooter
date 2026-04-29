using UnityEngine;

public class PVBagViewer : MonoBehaviour
{
    [SerializeField] private EnemyLifeModule _enemyLifeModule;

    void Awake()
    {
        _enemyLifeModule.OnDeathViewer += WeakPointDestroyedObserverRPC;
    }

    void WeakPointDestroyedObserverRPC()
    {
        //callBack when weakpoint destroyed
        _enemyLifeModule.gameObject.SetActive(false);
    }
}
