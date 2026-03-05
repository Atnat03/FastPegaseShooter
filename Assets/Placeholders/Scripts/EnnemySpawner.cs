using FishNet;
using UnityEngine;

public class EnnemySpawner : MonoBehaviour
{
    public GameObject ennemyPrefab;

    // Update is called once per frame
    void Start()
    {
            Debug.Log("spawning ennemy");
            InstanceFinder.ServerManager.Spawn(Instantiate(ennemyPrefab, transform));
        
    }
}
