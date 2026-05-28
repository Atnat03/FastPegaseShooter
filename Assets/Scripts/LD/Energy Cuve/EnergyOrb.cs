using System;
using FishNet.Object;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnergyOrb : MonoBusListener, IPoolable
{
    [SerializeField] private float _oscillationSpeed = 0.1f;
    [SerializeField] private float _oscillationAmplitude = 0.1f;
    private MeshRenderer meshRenderer;

    private int _orbId;
    private float _currentEnergyOrb;
    private float _initialY;
    private float _oscillationOffset;
    
    private XpOrbManager _xpOrbManager;


    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void SetUpOrb(int id, float value, Material material, XpOrbManager xpOrbManager)
    {
        _orbId = id;
        
        _currentEnergyOrb = value;

        meshRenderer.material = material;
        
        _xpOrbManager = xpOrbManager;
    }

    public void UpdateOrb()
    {
        Vector3 offset = Vector3.zero;
        offset.y = Mathf.Sin(Time.time * _oscillationSpeed);
        transform.position = new Vector3(
            transform.position.x, 
            _initialY + Mathf.Sin((Time.time+_oscillationOffset) * _oscillationSpeed) * _oscillationAmplitude,
            transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out PlayerVisuelBridge player))
            return;
        
        if(!player.PlayerGun.IsPositive && _currentEnergyOrb > 0 ||
           player.PlayerGun.IsPositive && _currentEnergyOrb < 0) return;

        NetworkObject playerNet = player.transform.root.GetComponent<NetworkObject>();

        if (playerNet == null)
            return;

        InvokeEvent(new ModifyEnergyEvent
        {
            p_player = playerNet.OwnerId,
            p_value = Mathf.Abs(_currentEnergyOrb)
        });

        _xpOrbManager.ReturnOrbToPoolServerRpc(_orbId);
    }

    public void Spawn()
    {
        _initialY = transform.position.y;

        _oscillationOffset = Random.Range(0, 10);
    }

    public void ReturnToPool(){}
}