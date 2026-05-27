using System;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class XpOrbManager : NetworkBusListener
{
    [Header("------ Orb Logic ------")]
    [SerializeField] private float _playerDetectionDistance = 10f;
    [SerializeField] private float _playerCollectDistance = 1f;
    [SerializeField] private float _orbSpeed = 2f;

    [Header("------ Render ------")]
    [SerializeField] private ParticleSystem _ps;
    [SerializeField] private Color _negativeColor;
    [SerializeField] private Color _positiveColor;
    [SerializeField] private float _maxXpInOrb = 5f;

    [Header("------ Optimisation ------")]
    [SerializeField] private float _recomputeFrequence = 0.2f;

    
    
    private ParticleSystem.Particle[] _particles = Array.Empty<ParticleSystem.Particle>();

    
    private readonly List<OrbData> _xpOrbData = new();
    private readonly List<int> _movingOrbIndices = new();
    private readonly HashSet<int> _movingOrbLookup = new();

    
    
    private float _timeSinceRecompute;
    private float _playerDetectionDistanceSqr;
    private float _playerCollectDistanceSqr;

    private readonly SyncVar<Vector3> _positivePlayerPos = new();
    private readonly SyncVar<Vector3> _negativePlayerPos = new();
    
    private int _positivePlayerId, _negativePlayerId;

    
    
    private void Awake()
    {
        _playerDetectionDistanceSqr = _playerDetectionDistance * _playerDetectionDistance;
        _playerCollectDistanceSqr = _playerCollectDistance * _playerCollectDistance;
    }

    
    
    public override void OnStartServer()
    {
        base.OnStartServer();

        ListenToEvent<OnEnemyDieEvent>(OEDE =>
        {
            if (OEDE.p_energyToDropInOrb == 0)
                return;

            SpawnOrbObserverRpc(
                OEDE.p_enemy.transform.position,
                OEDE.p_energyToDropInOrb);
        });

        ListenToEvent<PlayerPositionUpdateEvent>(PPUE =>
        {
            if (PPUE.p_playerId == 0)
            {
                _positivePlayerPos.Value = PPUE.p_playerPosition;
                _positivePlayerId = PPUE.p_playerId;
            }
            else
            {
                _negativePlayerPos.Value = PPUE.p_playerPosition;
                _negativePlayerId = PPUE.p_playerId;
            }
        });
    }
    
    [ObserversRpc(BufferLast = false)]
    private void SpawnOrbObserverRpc(Vector3 position, float rawAmount)
    {
        AddXpOrbs(position, rawAmount);
    }
    
    private void AddXpOrbs(Vector3 position, float rawAmount)
    {
        float amount = Mathf.Abs(rawAmount);

        while (amount > _maxXpInOrb)
        {
            _xpOrbData.Add(
                new OrbData(
                    position,
                    rawAmount < 0
                        ? -_maxXpInOrb
                        : _maxXpInOrb));

            amount -= _maxXpInOrb;
        }

        _xpOrbData.Add(
            new OrbData(
                position,
                rawAmount < 0
                    ? -amount
                    : amount));
    }

    
    
    private void Update()
    {
        if (_timeSinceRecompute >= _recomputeFrequence)
        {
            _timeSinceRecompute = 0f;

            RecomputeMovingOrbs();
        }

        _timeSinceRecompute += Time.deltaTime;
    }

    
    
    private void RecomputeMovingOrbs()
    {
        for (int i = 0; i < _xpOrbData.Count; i++)
        {
            if (_movingOrbLookup.Contains(i))
                continue;

            if (CanOrbMove(i))
            {
                _movingOrbIndices.Add(i);
                _movingOrbLookup.Add(i);
            }
        }
    }

    
    
    private void FixedUpdate()
    {
        for (int i = _movingOrbIndices.Count - 1; i >= 0; i--)
        {
            int orbIndex = _movingOrbIndices[i];

            if (!CanOrbMove(orbIndex))
            {
                RemoveMovingOrb(i, orbIndex);
                continue;
            }

            if (IsServerInitialized && CanOrbBeCollected(orbIndex))
            {
                InvokeEvent(new ModifyEnergyEvent
                {
                    p_player = _xpOrbData[orbIndex].p_value > 0 ? _positivePlayerId : _negativePlayerId,
                    p_value = Mathf.Abs(_xpOrbData[orbIndex].p_value)
                });
                RemoveOrbObserverRpc(orbIndex);
                continue;
            }

            OrbData orb = _xpOrbData[orbIndex];

            Vector3 targetPos =
                orb.p_value > 0
                    ? _positivePlayerPos.Value
                    : _negativePlayerPos.Value;

            orb.p_position = Vector3.MoveTowards(
                orb.p_position,
                targetPos,
                _orbSpeed * Time.fixedDeltaTime);

            _xpOrbData[orbIndex] = orb;
        }
    }

    
    
    private void RemoveMovingOrb(int movingIndex, int orbIndex)
    {
        _movingOrbLookup.Remove(orbIndex);

        int lastIndex = _movingOrbIndices.Count - 1;

        _movingOrbIndices[movingIndex] =
            _movingOrbIndices[lastIndex];

        _movingOrbIndices.RemoveAt(lastIndex);
    }

    [ObserversRpc]
    private void RemoveOrbObserverRpc(int orbIndex)
    {
        _movingOrbIndices.Remove(orbIndex);
        _movingOrbLookup.Remove(orbIndex);
        _xpOrbData.RemoveAt(orbIndex);
    }

    
    
    private void LateUpdate()
    {
        if (_xpOrbData.Count > _particles.Length)
        {
            _particles =
                new ParticleSystem.Particle[_xpOrbData.Count];
        }

        for (int i = 0; i < _xpOrbData.Count; i++)
        {
            OrbData orb = _xpOrbData[i];

            _particles[i].position = orb.p_position;

            _particles[i].startSize = 0.2f;

            _particles[i].startColor =
                orb.p_value < 0
                    ? _negativeColor
                    : _positiveColor;
        }

        _ps.SetParticles(_particles, _xpOrbData.Count);
    }

    
    
    private bool CanOrbMove(int index)
    {
        OrbData orb = _xpOrbData[index];

        Vector3 playerPos =
            orb.p_value > 0
                ? _positivePlayerPos.Value
                : _negativePlayerPos.Value;

        return (orb.p_position - playerPos).sqrMagnitude
               <= _playerDetectionDistanceSqr;
    }
    private bool CanOrbBeCollected(int index)
    {
        OrbData orb = _xpOrbData[index];

        Vector3 playerPos =
            orb.p_value > 0
                ? _positivePlayerPos.Value
                : _negativePlayerPos.Value;

        return (orb.p_position - playerPos).sqrMagnitude
               <= _playerCollectDistanceSqr;
    }
}

public struct OrbData
{
    public Vector3 p_position;
    public float p_value;

    public OrbData(Vector3 position, float value)
    {
        p_position = position;
        p_value = value;
    }
}