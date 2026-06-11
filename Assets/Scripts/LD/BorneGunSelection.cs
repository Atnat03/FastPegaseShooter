using System;
using System.Collections.Generic;
using Controller;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using MyPrint;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class BorneGunSelection : NetworkBusListener
{
    #region Properties

    #endregion


    #region Variables

    [Header("Zone")]
    [SerializeField] private Vector3 _zoneSize;
    [SerializeField] private Transform _zoneMesh;
    [SerializeField] private bool _showGIZMOS = true;
    private List<PlayerVisuelBridge> _playerList = new List<PlayerVisuelBridge>();
    private BoxCollider _collider;
    private readonly SyncVar<int> _numberPlayer = new SyncVar<int>(0);
    private readonly SyncVar<bool> _canOpenSelect = new SyncVar<bool>(false);
    
    #endregion


    #region Fonctions

    private void Awake()
    {
        Cons.Print("[BorneGunSelection] Awake — init collider", ColorConsole.Yellow);
        _collider = GetComponent<BoxCollider>();
        _collider.size = _zoneSize;

        if (_zoneMesh == null)
            Cons.Print("[BorneGunSelection] Awake — _zoneMesh est NULL !", ColorConsole.Red);
        else
            _zoneMesh.localScale = _zoneSize;

        Cons.Print($"[BorneGunSelection] Awake — zoneSize={_zoneSize}", ColorConsole.Yellow);
    }
    
    public override void OnStartServer()
    {
        Cons.Print("[BorneGunSelection] OnStartServer — abonnement OnNumberPlayerChange", ColorConsole.Yellow);
        _numberPlayer.OnChange += OnNumberPlayerChange;
    }

    public override void OnStartClient()
    {
        Cons.Print("[BorneGunSelection] OnStartClient — écoute OnPlayerInteract", ColorConsole.Yellow);
        ListenToEvent<OnPlayerInteract>(PlayerInteract);
    }

    private void PlayerInteract(OnPlayerInteract data)
    {
        Cons.Print($"[BorneGunSelection] PlayerInteract — _canOpenSelect={_canOpenSelect.Value}  IsServer={IsServerInitialized}", ColorConsole.Cyan);

        if (_canOpenSelect.Value)
        {
            if (IsServerInitialized)
            {
                Cons.Print("[BorneGunSelection] PlayerInteract — appel direct ObserversRpc (serveur)", ColorConsole.Cyan);
                AllPlayerInZoneObserversRpc();
            }
            else
            {
                Cons.Print("[BorneGunSelection] PlayerInteract — appel ServerRpc (client)", ColorConsole.Cyan);
                AllPlayerInZoneServerRpc();
            }
        }
        else
        {
            Cons.Print("[BorneGunSelection] PlayerInteract — bloqué, _canOpenSelect est false", ColorConsole.Red);
        }
    }

    private void OnNumberPlayerChange(int prev, int next, bool asServer)
    {
        Cons.Print($"[BorneGunSelection] OnNumberPlayerChange — prev={prev}  next={next}  asServer={asServer}", ColorConsole.Yellow);

        if (asServer)
        {
            int totalClients = InstanceFinder.ServerManager.Clients.Count;
            Cons.Print($"[BorneGunSelection] OnNumberPlayerChange — joueurs en zone={next} / clients connectés={totalClients}", ColorConsole.Yellow);

            if (next == totalClients)
            {
                Cons.Print("[BorneGunSelection] OnNumberPlayerChange — TOUS les joueurs sont en zone → _canOpenSelect=true", ColorConsole.Green);
                _canOpenSelect.Value = true;
            }
            else
            {
                Cons.Print($"[BorneGunSelection] OnNumberPlayerChange — pas encore tous là ({next}/{totalClients}) → _canOpenSelect=false", ColorConsole.Red);
                _canOpenSelect.Value = false;
            }

            Cons.Print($"[BorneGunSelection] OnNumberPlayerChange — envoi CanInteractToOpenObserversRpc({_canOpenSelect.Value})", ColorConsole.Yellow);
            CanInteractToOpenObserversRpc(_canOpenSelect.Value);
        }
        else
        {
            Cons.Print("[BorneGunSelection] OnNumberPlayerChange — ignoré (pas serveur)", ColorConsole.Yellow);
        }
    }

    [ServerRpc]
    private void AllPlayerInZoneServerRpc()
    {
        Cons.Print("[BorneGunSelection] AllPlayerInZoneServerRpc — reçu par le serveur, relay vers ObserversRpc", ColorConsole.Cyan);
        AllPlayerInZoneObserversRpc();
    }

    [ObserversRpc]
    void AllPlayerInZoneObserversRpc()
    {
        Cons.Print("[BorneGunSelection] AllPlayerInZoneObserversRpc — invoke OnAllPlayerAtBorne", ColorConsole.Cyan);
        InvokeEvent(new OnAllPlayerAtBorne());
    }

    [ObserversRpc]
    void CanInteractToOpenObserversRpc(bool isOpen)
    {
        Cons.Print($"[BorneGunSelection] CanInteractToOpenObserversRpc — isOpen={isOpen}", ColorConsole.Cyan);
        InvokeEvent(new OnAllPlayerCanSelectGun { p_open = isOpen });
    }
    
    public void OnTriggerEnter(Collider other)
    {
        Cons.Print($"[BorneGunSelection] OnTriggerEnter — objet={other.name}  IsServer={IsServerInitialized}", ColorConsole.Yellow);

        if (!IsServerInitialized)
        {
            Cons.Print("[BorneGunSelection] OnTriggerEnter — ignoré (pas serveur)", ColorConsole.Yellow);
            return;
        }
        
        if (other.TryGetComponent(out PlayerVisuelBridge player))
        {
            Cons.Print($"[BorneGunSelection] OnTriggerEnter — PlayerVisuelBridge trouvé : {other.name}  → _numberPlayer={_numberPlayer.Value + 1}", ColorConsole.Green);
            _playerList.Add(player);
            _numberPlayer.Value++;
        }
        else
        {
            Cons.Print($"[BorneGunSelection] OnTriggerEnter — pas de PlayerVisuelBridge sur {other.name}", ColorConsole.Yellow);
        }
    }
    
    public void OnTriggerExit(Collider other)
    {
        Cons.Print($"[BorneGunSelection] OnTriggerExit — objet={other.name}  IsServer={IsServerInitialized}", ColorConsole.Yellow);

        if (!IsServerInitialized)
        {
            Cons.Print("[BorneGunSelection] OnTriggerExit — ignoré (pas serveur)", ColorConsole.Yellow);
            return;
        }
        
        if (other.TryGetComponent(out PlayerVisuelBridge player))
        {
            if (_playerList.Contains(player))
            {
                Cons.Print($"[BorneGunSelection] OnTriggerExit — joueur retiré : {other.name}  → _numberPlayer={_numberPlayer.Value - 1}", ColorConsole.Green);
                _playerList.Remove(player);
                _numberPlayer.Value--;
            }
            else
            {
                Cons.Print($"[BorneGunSelection] OnTriggerExit — joueur {other.name} pas dans la liste, ignoré", ColorConsole.Red);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (_showGIZMOS)
        {
            Gizmos.color = Color.cornflowerBlue;
            Gizmos.DrawWireCube(transform.position, _zoneSize);
        }
    }

    #endregion
}

public struct OnAllPlayerAtBorne
{ }

public struct OnAllPlayerCanSelectGun
{
    public bool p_open;
}