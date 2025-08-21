using Exiled.API.Features;

namespace WaitAndChillReborn
{
    using System.Collections.Generic;
    using Configs;
    using CustomPlayerEffects;
    using Exiled.API.Enums;
    using Exiled.API.Extensions;
    using Exiled.API.Features.Pickups;
    using Exiled.API.Features.Roles;
    using Exiled.Events.EventArgs.Interfaces;
    using Exiled.Events.EventArgs.Player;
    using GameCore;
    using InventorySystem.Items.Pickups;
    using InventorySystem.Items.ThrowableProjectiles;
    using MEC;
    using Mirror;
    using UnityEngine;
    using static API.API;
    using Log = Exiled.API.Features.Log;
    using Object = UnityEngine.Object;
    using PlayerEvent = Exiled.Events.Handlers.Player;
    using ServerEvent = Exiled.Events.Handlers.Server;
    using MapEvent = Exiled.Events.Handlers.Map;
    using Player = Exiled.API.Features.Player;
    using Scp106Event = Exiled.Events.Handlers.Scp106;
    using Server = Exiled.API.Features.Server;

    public static class EventHandlers
    {
        public static Vector3? MapChosen = null;
        
        public static void RegisterEvents()
        {
            ServerEvent.WaitingForPlayers += OnWaitingForPlayers;
            PlayerEvent.Verified += OnVerified;
            
            PlayerEvent.SpawningRagdoll += OnDeniableEvent;
            PlayerEvent.IntercomSpeaking += OnDeniableEvent;
            PlayerEvent.DroppingItem += OnDeniableEvent;
            PlayerEvent.DroppingAmmo += OnDeniableEvent;
            PlayerEvent.InteractingDoor += OnDeniableEvent;
            PlayerEvent.InteractingElevator += OnDeniableEvent;
            PlayerEvent.InteractingLocker += OnDeniableEvent;
            MapEvent.ChangingIntoGrenade += OnDeniableEvent;

            // Scp106Event.CreatingPortal += OnDeniableEvent;
            Scp106Event.Teleporting += OnDeniableEvent;
            ServerEvent.RoundStarted += OnRoundStarted;
            ServerEvent.OnRoundStarted();
        }

        public static void UnRegisterEvents()
        {
            ServerEvent.WaitingForPlayers -= OnWaitingForPlayers;

            PlayerEvent.Verified -= OnVerified;
            
            PlayerEvent.SpawningRagdoll -= OnDeniableEvent;
            PlayerEvent.IntercomSpeaking -= OnDeniableEvent;
            PlayerEvent.DroppingItem -= OnDeniableEvent;
            PlayerEvent.DroppingAmmo -= OnDeniableEvent;
            PlayerEvent.InteractingDoor -= OnDeniableEvent;
            PlayerEvent.InteractingElevator -= OnDeniableEvent;
            PlayerEvent.InteractingLocker -= OnDeniableEvent;
            MapEvent.ChangingIntoGrenade -= OnDeniableEvent;

            // Scp106Event.CreatingPortal -= OnDeniableEvent;
            Scp106Event.Teleporting -= OnDeniableEvent;

            ServerEvent.RoundStarted -= OnRoundStarted;
        }

        public static void OnWaitingForPlayers()
        {
            var startRound = GameObject.Find("StartRound");
            if (startRound.TryGetComponent<Setuped>(out var setuped)) return;
            startRound.AddComponent<Setuped>();
        }
        
        public static void OnVerified(VerifiedEventArgs ev)
        {
            var startRound = GameObject.Find("StartRound");
            if (startRound is not null && !startRound.TryGetComponent<Setuped>(out var setuped))
                startRound.AddComponent<Setuped>();

            ev.Player.GameObject.AddComponent<PlayerLobby>();
        }

        public static void OnDeniableEvent(IExiledEvent ev)
        {
            if (IsLobby && ev is IDeniableEvent deniableEvent)
                deniableEvent.IsAllowed = false;
        }
        
        public static void OnRoundStarted()
        {
            foreach (ThrownProjectile throwable in Object.FindObjectsOfType<ThrownProjectile>())
            {
                if (throwable.TryGetComponent(out Rigidbody rb) && rb.velocity.sqrMagnitude <= 1f)
                    continue;

                throwable.transform.position = Vector3.zero;
                Timing.CallDelayed(1f, () => NetworkServer.Destroy(throwable.gameObject));
            }

            foreach (Player player in Player.List)
                player.DisableAllEffects();

            if (Config.TurnedPlayers)
            {
                Scp096Role.TurnedPlayers.Clear();
                Scp173Role.TurnedPlayers.Clear();
            }

            if (Server.FriendlyFire)
                FriendlyFireConfig.PauseDetector = false;

            foreach (Pickup pickup in LockedPickups)
            {
                try
                {
                    PickupSyncInfo info = pickup.Base.NetworkInfo;
                    info.Locked = false;
                    pickup.Base.NetworkInfo = info;

                    pickup.Base.GetComponent<Rigidbody>().isKinematic = false;
                }
                catch (System.Exception)
                {
                    // ignored
                }
            }

            LockedPickups.Clear();
        }

        public static readonly HashSet<Pickup> LockedPickups = new();
        public static readonly LobbyConfig Config = WaitAndChillReborn.Instance.Config.LobbyConfig;
    }
}