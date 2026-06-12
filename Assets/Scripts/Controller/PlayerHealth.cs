using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CustomConsole.Runtime.Logger;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Managers;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHealth : NetworkBusListener
{
    #region Properties

    public float CurrentHealth => _currentHealth.Value;
    public bool IsDead => _isDead.Value;

    public bool IsCritik => _isCritik;
    public int OwnerId => Owner.ClientId;

    #endregion
    
    #region Variables

    private readonly SyncVar<float> _currentHealth = new SyncVar<float>();
    private readonly SyncVar<bool> _isDead = new SyncVar<bool>(false);
    private readonly SyncVar<float> _respawnTimer = new SyncVar<float>(0);

    [SerializeField] private float _healthBase = 100;
    [SerializeField] public float _timeToRespawn = 5;
    [SerializeField, Range(0f, 1f)] private float _critikStep = 0.5f;
    [SerializeField] private PlayerInput _playerInputAction;
    [SerializeField] private PlayerAnimation _playerAnimation;
    [SerializeField] private GunSwitching _gunSwitching;
    [SerializeField] private NetworkObject healThrowObject;
    [SerializeField] private PlayerCapacity _playerCapacity;
    
    [Header("Invincibility")] [SerializeField]
    private float _invicibilityDuration = 3f;

    private float _respawnTime;
    private bool _isInvincible = false;

    private bool IsInvincible
    {
        get
        {
            //if the player as respawn recently
            float currentTime = Time.time;
            if (_respawnTime + _invicibilityDuration > currentTime) return true;

            return _isInvincible;
        }
        set => _isInvincible = value;
    }

    [Header("Healing throw version")] [SerializeField]
    private bool throwableHeal = false;

    [SerializeField] private PlayerEnergy _playerEnergy;
    public Transform p_healThrowDirection;
    public float throwForce = 10;
    public float maxThrowDistance = 100;
    public float minSize = 3;
    public float healSizeEffectFactor = .05f;
    [SerializeField] private float _minHealthToGive = 15;
    [SerializeField] private float healAmountEffectFactor = .1f;
    public float showLineDelay = .5f;
    [SerializeField] private float _healThrowCost = 20;
    [SerializeField] private LayerMask _throwHitLayerMask;
    [SerializeField] private LayerMask _throwHealLayerMask;

    [Header("Healing Self version")] [SerializeField]
    private float _healAmount = 50;

    private PlayerZoneManager _playerZoneManager; //debug

    private bool _initialized = false;
    private bool _isCritik = false;
    private float _targetHealthFill;
    private Vector3 _startPos;

    private bool _isHealKeyDown;
    private bool _canThrowHeal = true;

    public bool p_unlockCapa = true;

    //Action
    public Action<float> OnUpdateHealth;
    public Action OnStartWarning;
    public Action<bool, float> OnKOPlayer;
    public Action OnTakeDamage;
    public Action OnHeal;

    public Action OnThrowingVisualActivation;
    public Action OnThrowing;
    public Action<Vector3, float, float> OnHealThrowLanding;
    public Action OnThrowKeyReleased;
    public Action OnHealCanceled;

    [SerializeField]private bool invincible = false;

    #endregion


    #region Fonctions

    public override void OnStartServer()
    {
        if (!_initialized)
        {
            _currentHealth.Value = _healthBase;
            _initialized = true;
        }

        ListenToEvent<PlayerTakeDamageEvent>(TakeDamage);
        ListenToEvent<AddHealthToPlayer>(AddHealth);
        ListenToEvent<GetInvincibleEvent>(GetInvincible);

        InvokeEvent(new OnPlayerSpawnEvent
        {
            playerId = Owner.ClientId,
            isPositiveCharge = Owner.ClientId == 0,
            gunSwitching = _gunSwitching,
            Transform = transform
        });

        ListenToEvent<OnCorrosionEvent>(ApplyCorrosionDamage);


        _playerZoneManager = FindAnyObjectByType<PlayerZoneManager>(); // pour du debug, a tej en build finale

        _respawnTime = Time.time;
    }

    public override void OnStartClient()
    {
        _currentHealth.OnChange += OnHealthChange;
        _isDead.OnChange += OnDeadChange;
        _respawnTimer.OnChange += OnRespawnTimerChange;

        if (IsOwner)
        {
            _startPos = transform.position;
            ListenToEvent<OnShortCircuitDamage>(ApplyShortCircuitDamage);

            ListenToEvent<OnPlayerSpawnTPEvent>((OPSTE) =>
            {
                if (!IsOwner) return;

                Rigidbody rb = GetComponent<Rigidbody>();
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.position = OwnerId == 0 ? OPSTE.p_spawnPositions[0] : OPSTE.p_spawnPositions[1];
                transform.position = OwnerId == 0 ? OPSTE.p_spawnPositions[0] : OPSTE.p_spawnPositions[1];
            });
        }

        PlayerHealthManager.Instance?.Register(this);
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        PlayerHealthManager.Instance?.Unregister(this);
    }

    private void OnEnable()
    {
        _playerInputAction.actions["Heal"].performed += HealKeyPerformed;
        _playerInputAction.actions["Heal"].canceled += HealKeyCanceled;
        _playerInputAction.actions["Shoot"].performed += CancelHealThrowing;
        _playerInputAction.actions["Charge"].performed += CancelHealThrowing;
    }

    private void OnDisable()
    {
        _playerInputAction.actions["Heal"].performed -= HealKeyPerformed;
        _playerInputAction.actions["Heal"].canceled -= HealKeyCanceled;
        _playerInputAction.actions["Shoot"].performed -= CancelHealThrowing;
        _playerInputAction.actions["Charge"].performed -= CancelHealThrowing;
    }

    private void Update()
    {
        if (IsServerInitialized)
        {
            if (_isDead.Value)
            {
                if (_respawnTimer.Value > 0)
                    _respawnTimer.Value -= Time.deltaTime;
                else
                    Respawn();
            }
        }
    }

    public void CancelHealThrowing(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;

        _canThrowHeal = false;
        OnHealCanceled?.Invoke();
    }


    void HealKeyPerformed(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;

        if (!p_unlockCapa) return;
        if (!_playerCapacity.CanHeal) return;

        InvokeEvent(new OnHealUsed_TUTO());

        InvokeEvent(new OnUseCapacity
        {
            p_capacityData = Capacity.Heal
        });

        if (throwableHeal)
        {
            OnThrowingVisualActivation?.Invoke();
            _isHealKeyDown = true;
        }
        else
        {
            NotThrowableHealPressedServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void NotThrowableHealPressedServerRpc()
    {
        AddHealth(new AddHealthToPlayer
        {
            p_delay = 0,
            p_playerId = OwnerId,
            p_value = _healAmount
        });
    }

    void HealKeyCanceled(InputAction.CallbackContext ctx)
    {
        if (!(IsOwner || _isHealKeyDown)) return;
        if (!throwableHeal) return;
        OnThrowKeyReleased?.Invoke();

        if (!_playerEnergy.CanThrow(_playerEnergy.p_costThrowHeal))
        {
            return;
        }

        if (_canThrowHeal)
        {
            OnThrowing?.Invoke();

            ThrowHealServerRpc(Owner);
        }

        _isHealKeyDown = false;
        _canThrowHeal = true;
    }

    [Server]
    void TakeDamage(PlayerTakeDamageEvent data)
    {
        //debug clement

        try
        {
            float player1PVs = -1;
            float player2PVs = -1;
            if (PlayerHealthManager.Instance != null)
            {
                player1PVs = PlayerHealthManager.Instance.RegisteredPlayers.Count > 0
                    ? PlayerHealthManager.Instance.RegisteredPlayers[0].CurrentHealth
                    : 0;
                player2PVs = PlayerHealthManager.Instance.RegisteredPlayers.Count > 1
                    ? PlayerHealthManager.Instance.RegisteredPlayers[1].CurrentHealth
                    : 0;
            }

            InvokeEvent(new OnDataLog
            {
                entityName = data.p_attacker.name,
                EntityID = data.p_attacker ? data.p_attacker.ObjectId : -1,
                weapon = "ennemy_Shoot",
                targetName = transform.GetRootTransform().gameObject.name,
                targetID = data.p_playerN.ObjectId,
                damages = data.p_value,
                player1PVs = player1PVs,
                player2PVs = player2PVs,
                ArenaID = (_playerZoneManager != null && _playerZoneManager.p_playerZones.ContainsKey(OwnerId))
                    ? _playerZoneManager.p_playerZones[OwnerId]
                    : -1
            });
        }
        catch (Exception e)
        {
            CustomLogger.CCErrorLog(e.Message);
        }

        //fin du debug
        
        if (data.p_playerN.ObjectId != NetworkObject.ObjectId) return;
        if (IsDead || IsInvincible || invincible) return;

        float newHealth = _currentHealth.Value - data.p_value;

        ApplyVolumeDamagedEffectTargetRpc(Owner);

        if (newHealth <= 0)
        {
            Death();
        }
        else
        {
            _currentHealth.Value = newHealth;
        }
    }

    private void ApplyShortCircuitDamage(OnShortCircuitDamage data)
    {
        RequestTakeDamageServerRpc(data.damage);
    }

    private void ApplyCorrosionDamage(OnCorrosionEvent data)
    {
        TakeDamage(new PlayerTakeDamageEvent
        {
            p_playerN = NetworkObject,
            p_value = data.p_corrosionDamage,
        });
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestTakeDamageFromTutoServerRpc(int damage, int seuil)
    {
        int dmg = damage;

        if (_currentHealth.Value - dmg < seuil)
        {
            _currentHealth.Value = seuil;
        }
        else
        {
            TakeDamage(new PlayerTakeDamageEvent
            {
                p_playerN = NetworkObject,
                p_value = damage,
                p_attacker = null
            });
        }
    }

    [ServerRpc]
    public void RequestTakeDamageServerRpc(int damage)
    {
        TakeDamage(new PlayerTakeDamageEvent
        {
            p_playerN = NetworkObject,
            p_value = damage,
        });
    }

    [TargetRpc]
    private void ApplyVolumeDamagedEffectTargetRpc(NetworkConnection target)
    {
        OnTakeDamage?.Invoke();
        OnUpdateHealth?.Invoke(1f);
    }

    [ServerRpc(RequireOwnership = false)]
    void ThrowHealServerRpc(NetworkConnection throwerConnection)
    {
        InvokeEvent(new ConsumeEnergyEvent()
        {
            p_player = throwerConnection,
            p_value = -(_playerEnergy.p_costThrowHeal * _playerEnergy.EnergyOneBar),
        });

        StartCoroutine(HealThrowCoroutine());
    }

    IEnumerator HealThrowCoroutine()
    {
        Vector3[] positions = HealThrowLine(out float distance);
        NetworkObject throwObject = Instantiate(healThrowObject, positions[0], Quaternion.identity);
        Spawn(throwObject);
        for (int i = 0; i < positions.Length; i += 2)
        {
            throwObject.transform.position = positions[i];
            yield return new WaitForEndOfFrame();
        }

        Despawn(throwObject);
        Destroy(throwObject.gameObject);
        OnHealActivate(positions[^1], distance * healAmountEffectFactor + _minHealthToGive,
            distance * healSizeEffectFactor + minSize);
        ShowHealThrowObserverRpc(positions[^1], distance, 1f);
    }

    [ServerRpc(RequireOwnership = false)]
    void OnHealActivate(Vector3 landingPos, float lifeToAdd, float scale)
    {
        Collider[] colliders = Physics.OverlapSphere(landingPos, scale, _throwHealLayerMask);
        foreach (Collider collider in colliders)
        {
            if (collider != null && collider.TryGetComponent(out PlayerVisuelBridge visualBridge))
            {
                visualBridge.PlayerHealth.AddHealth(new AddHealthToPlayer
                {
                    p_delay = 0,
                    p_playerId = visualBridge.OwnerId,
                    p_value = lifeToAdd
                });
            }
        }
    }

    [Server]
    async void AddHealth(AddHealthToPlayer data)
    {
        //debug clement

        float? player1PVs = -1;
        float? player2PVs = -1;
        float? player1Energy = -1;
        float? player2Energy = -1;
        if (PlayerHealthManager.Instance != null)
        {
            player1PVs = PlayerHealthManager.Instance.RegisteredPlayers.Count > 0
                ? PlayerHealthManager.Instance.RegisteredPlayers[0].CurrentHealth
                : null;
            player2PVs = PlayerHealthManager.Instance.RegisteredPlayers.Count > 1
                ? PlayerHealthManager.Instance.RegisteredPlayers[1].CurrentHealth
                : null;
            player1Energy = PlayerHealthManager.Instance.RegisteredPlayers.Count > 0
                ? PlayerHealthManager.Instance.RegisteredPlayers[0].gameObject.GetComponent<PlayerEnergy>()
                    .CurrentEnergy
                : null;

            player2Energy = PlayerHealthManager.Instance.RegisteredPlayers.Count > 1
                ? PlayerHealthManager.Instance.RegisteredPlayers[1].gameObject.GetComponent<PlayerEnergy>()
                    .CurrentEnergy
                : null;
        }

        InvokeEvent(new OnDataLog
        {
            entityName = name,
            EntityID = ObjectId,
            weapon = "heal",
            targetName = gameObject.name,
            targetID = ObjectId,
            damages = (data.p_value * -1f),
            player1PVs = player1PVs,
            player2PVs = player2PVs,
            player1Energy = player1Energy,
            player2Energy = player2Energy,
            skillUsed = "Heal",
            ArenaID = (_playerZoneManager != null && _playerZoneManager.p_playerZones.ContainsKey(OwnerId))
                ? _playerZoneManager.p_playerZones[OwnerId]
                : -1
        });

        //fin du debug

        if (_isDead.Value || data.p_playerId != OwnerId) return;
        if (data.p_delay != 0 && throwableHeal) await Task.Delay(Mathf.RoundToInt(data.p_delay * 1000));

        float newHealth = (_currentHealth.Value + data.p_value) > _healthBase
            ? _healthBase
            : _currentHealth.Value + data.p_value;
        _currentHealth.Value = newHealth;
        OnHeal?.Invoke();
    }

    private void Death()
    {
        _gunSwitching.IGunMain.TryCancelShooting();
        
        _currentHealth.Value = 0;
        _isDead.Value = true;
        _respawnTimer.Value = _timeToRespawn;

        NotifyDeathRpc(NetworkObject);
    }


    [Server]
    public void Respawn()
    {
        _respawnTimer.Value = 0;
        _respawnTime = Time.time;

        Vector3 respawnPos = Managers.RespawnPointManager.Instance != null
            ? Managers.RespawnPointManager.Instance.GetRespawnPosition(OwnerId)
            : new Vector3(30, 0, -23.5f);

        _currentHealth.Value = _healthBase;

        TeleportOwnerTargetRpc(Owner, respawnPos);

        _isDead.Value = false;

        RespawnObserverRpc();
    }

    [TargetRpc]
    private void TeleportOwnerTargetRpc(NetworkConnection conn, Vector3 respawnPos)
    {
        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = respawnPos;
            transform.position = respawnPos;
        }
        else
        {
            transform.position = respawnPos;
        }

        _gunSwitching.IGunMain.TryCancelShooting();
    }

    [ObserversRpc]
    private void RespawnObserverRpc()
    {
        InvokeEvent(new OnPlayerRespawnEvent { p_playerN = NetworkObject });
    }

    [ObserversRpc]
    private void NotifyDeathRpc(NetworkObject playerN)
    {
        InvokeEvent(new OnPlayerDeathEvent { p_playerN = playerN });
    }


    [ObserversRpc]
    private void ShowHealThrowObserverRpc(Vector3 landingPos, float scale, float duration)
    {
        OnHealThrowLanding?.Invoke(landingPos, scale, duration);
    }

    private void OnHealthChange(float prev, float next, bool asServer)
    {
        _targetHealthFill = next / _healthBase;

        OnUpdateHealth?.Invoke(_targetHealthFill);

        if (!IsOwner) return;

        if (_targetHealthFill <= _critikStep)
        {
            _isCritik = true;
            OnStartWarning?.Invoke();
        }
        else
        {
            _isCritik = false;
        }
    }


    private void OnDeadChange(bool prev, bool next, bool asServer)
    {
        _playerAnimation?.SetDeadAnim(next);
        if (IsOwner)
        {
            OnKOPlayer?.Invoke(next, _timeToRespawn);
        }
    }

    private void OnRespawnTimerChange(float prev, float next, bool asServer)
    {
    }

    #endregion

    public void Interact()
    {
        Debug.Log("Interact");
        RespawnObserverRpc();
    }

    private Vector3 startPos;

    public Vector3[] HealThrowLine(out float distance)
    {
        startPos = transform.position + transform.forward + transform.right;
        float simulatedTime = 0;
        Vector3 previousPos = startPos;
        Vector3 nextPos = GetNewPosition(Time.fixedDeltaTime);
        List<Vector3> posList = new();
        distance = 0;
        RaycastHit hit;
        while (!Physics.Raycast(previousPos, nextPos - previousPos, out hit, (nextPos - previousPos).magnitude,
                   ~LayerMask.GetMask("Owner"), QueryTriggerInteraction.Ignore) && distance < maxThrowDistance)
        {
            simulatedTime += Time.fixedDeltaTime;
            distance += (nextPos - previousPos).magnitude;
            posList.Add(previousPos);
            previousPos = nextPos;
            nextPos = GetNewPosition(simulatedTime);
        }

        if (hit.collider != null)
            posList.Add(hit.point);
        else
            posList.Add(nextPos);
        return posList.ToArray();
    }

    Vector3 GetNewPosition(float overTime)
    {
        Vector3 forward = p_healThrowDirection.forward;
        Vector3 planeNormal = Vector3.up; // plan horizontal

        Vector3 projectedForward = Vector3.ProjectOnPlane(forward, planeNormal);

        float pitch = Vector3.SignedAngle(
            projectedForward,
            forward,
            p_healThrowDirection.right
        );

        Vector3 throwAngle = new Vector3(p_healThrowDirection.forward.x, -pitch * 0.1f + .5f,
            p_healThrowDirection.forward.z);

        return startPos + throwAngle * (throwForce * overTime) + Physics.gravity * (0.5f * overTime * overTime);
    }

    void GetInvincible(GetInvincibleEvent evt)
    {
        invincible = !invincible;
        CustomLogger.ImportantLog($"get  invincible = {invincible}");
    } 
}

public struct AddHealthToPlayer
{
    public int p_playerId;
    public float p_value;
    public float p_delay;
}

public struct PlayerTakeDamageEvent
{
    public NetworkObject p_playerN;
    public float p_value;
    public NetworkObject p_attacker;
}

public struct OnPlayerDeathEvent
{
    public NetworkObject p_playerN;
}

public struct OnPlayerRespawnEvent
{
    public NetworkObject p_playerN;
}

public struct GetInvincibleEvent
{
}