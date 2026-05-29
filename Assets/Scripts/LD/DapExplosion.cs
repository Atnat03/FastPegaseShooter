using System;
using System.Collections;
using FishNet;
using MyPrint;
using UnityEngine;

public class DapExplosion : MonoBehaviour
{
    [SerializeField] Transform sphereCollider;
    [SerializeField] private float speed = 20;

    private void Awake()
    {
        StartCoroutine(ScaleAnimation());
    }

    IEnumerator ScaleAnimation()
    {
        float t = 0;
        float duration = 2f;
        
        while (t  < duration)
        {
            t += Time.deltaTime;
            
            sphereCollider.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t / duration);
            
            yield return new WaitForEndOfFrame();
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!InstanceFinder.NetworkManager.IsServerStarted)
            return;
        
        if (other.TryGetComponent(out EnemyCore enemy))
        {
            Cons.Print("Enemy hit", ColorConsole.Red);
            
            enemy.ExplodeOnDapWave();
        }
    }
}