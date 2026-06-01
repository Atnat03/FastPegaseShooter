using System.Collections.Generic;
using CustomConsole.Runtime.Logger;
using FishNet;
using FishNet.Object;
using UnityEngine;

public class PuddleEnemyBullet : EnemyBullet
{
    private float _damageDelay;
    private float _timeToShoot;
    
    HashSet<NetworkObject> _playerHits = new HashSet<NetworkObject>();

    public PuddleEnemyBullet(EnemyShootingEvent ESE, Vector3 direction, float spawnTime,
        int bulletId, LayerMask layerMask)
        : base(ESE, direction, spawnTime, bulletId, layerMask)
    {
        if (ESE.p_enemyAttackModule is LobShootingAttackModule LSAModule)
            _damageDelay = LSAModule._lobShootingAttackModuleSo.p_splashDamageDelay;
        else
        {
            CustomLogger.CCErrorLog("Could not load LSAModule, delay defaulted back to 1 secondes");
            _damageDelay = 1;
        }
    }

    public override void UpdateBullet(float serverTime)
    {
        _timeToShoot -= (float)InstanceFinder.TimeManager.TickDelta;
        
        base.UpdateBullet(serverTime);
    }

    protected override Vector3 GetNewPosition(float serverTime)
    {
        //Puddle doesn't move over time
        return _startPos;
    }

    protected override bool DoCollide(Vector3 startPos, Vector3 endPos, out RaycastHit hit)
    {
        //return base.DoCollide(startPos, endPos, out hit);
        
        Vector3 delta = endPos - startPos;
        float length = delta.magnitude;
        Vector3 dir = delta / length;

        Collider[] colliders = Physics.OverlapBox(
            _currentPosition,
            new Vector3(_bulletSize * 0.5f, 0.25f, _bulletSize * 0.5f),
            Quaternion.identity, _layerMask);

        hit = new RaycastHit();
        
        if (colliders.Length > 0)
        {
            bool didHitPlayer = false;
            foreach (Collider collider in colliders)
            {
                if((_currentPosition - collider.transform.position).magnitude <= _bulletSize*0.5f)
                {
                    didHitPlayer = true;
                    _playerHits.Add(collider.gameObject.GetComponent<PlayerVisuelBridge>().NetworkObject);
                }
            }
            if(didHitPlayer) return true;
        }
        
        return false;
        
    }

    protected override void ManageCollision(RaycastHit hit)
    {
        if (_timeToShoot <= 0)
        {
            _timeToShoot = _damageDelay;

            foreach (NetworkObject playerNetworkObject in _playerHits)
            {
                
                EventBus.InvokeEvent(new PlayerTakeDamageEvent
                {
                    p_playerN = playerNetworkObject,
                    p_value = p_bulletDamage,
                    p_attacker = p_attackModule.gameObject.GetComponent<NetworkObject>()
                });
            
                p_attackModule?.p_onHitPlayer?.Invoke(playerNetworkObject.ObjectId, p_bulletDamage);
            }
        }
    }
    public override bool ShouldBeDestroyed(float serverTime)
    {
        //if the bullet was alive for too long
        //or collided with something
        return (serverTime - _spawnTime > _maxLifeTime);
    }
}
