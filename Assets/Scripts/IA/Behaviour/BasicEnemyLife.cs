using System;
using CustomConsole.Runtime.Logger;
using FishNet;
using FishNet.CodeGenerating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;

public class BasicEnemyLife : NetworkBehaviour, IDamagable
{
    [SerializeField] private int _life;
    public readonly SyncVar<int> p_life = new SyncVar<int>();

    private Guid _gridReaderId;
    private int _enemySpawnCost;
    
    [Header("HitMark")]
    [SerializeField] private Transform _hitMarkerParent;
    [SerializeField] private TextMeshProUGUI _textDmg;
    [SerializeField] private TextMeshProUGUI _textDmgCritique;
    [SerializeField] private int _cumuatifDmg = 0;
    [SerializeField] private float _elapsedCumulativeDmgTime = 0;
    private TextMeshProUGUI _hitMarker;
    
    public override void OnStartServer()
    {
        base.OnStartServer();
        p_life.Value = _life;
        p_life.OnChange += OnLifeChanged;
    }
    
    private void OnLifeChanged(int prev, int next, bool asServer)
    {
        if (next <= 0)
        {
            if (asServer)
            {
                Death(); // serveur uniquement
            }
        }
    }
    
    public void TakeDamage(int damageAmount)
    {
        TakeDamageServerRpc(damageAmount);
        TriggerHitMark(false, damageAmount);
    }
    
    void TriggerHitMark(bool IsCritique, float dmg)
    {
        
        _cumuatifDmg += (int)dmg;
        if(_elapsedCumulativeDmgTime <= 0)
        {
            TextMeshProUGUI text;
            text = IsCritique ? _textDmgCritique : _textDmg;
            _hitMarker = Instantiate(text.gameObject, _hitMarkerParent).GetComponent<TextMeshProUGUI>();
            _elapsedCumulativeDmgTime = 0.5f;
            _hitMarker.SetText((_cumuatifDmg).ToString());
        }
        else
        {
            if (_hitMarker == null)
                _hitMarker.SetText((_cumuatifDmg).ToString());
        }
        
        Destroy(_hitMarker.gameObject, 0.05f);
    }

    [ServerRpc(RequireOwnership = false)]
    void TakeDamageServerRpc(int damageAmount)
    {
        p_life.Value -= damageAmount;
    }

    [Server]
    public void Death()
    {
        InstanceFinder.ServerManager.Despawn(gameObject);
        EventBusInitialiser.instance.Bus.InvokeEvent(new EnemyDyingEvent(_gridReaderId, _enemySpawnCost));
    }

    public void SetInfos(Guid _readerId, int cost)
    {
        _gridReaderId = _readerId;
        _enemySpawnCost = cost;
    }

    private void Update()
    {
        if (_elapsedCumulativeDmgTime > 0)
        {
            _elapsedCumulativeDmgTime -= Time.deltaTime;
        }
        else
        {
            _cumuatifDmg = 0;
        }
    }
}

public struct EnemyDyingEvent
{
    public Guid p_gridReaderId;
    public int p_enemySpawnCost;

    public EnemyDyingEvent(Guid id, int cost)
    {
        p_gridReaderId = id;
        p_enemySpawnCost = cost;
    }
}

