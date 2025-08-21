using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using GameCore;
using MEC;
using PlayerRoles;
using UnityEngine;
using UnityEngine.Serialization;

namespace WaitAndChillReborn;

public class PlayerLobby : MonoBehaviour
{
    public Player Player;

    public bool IsSetuping = false;
    
    public string Text {
        get
        {
            StringBuilder stringBuilder = NorthwoodLib.Pools.StringBuilderPool.Shared.Rent();

            if (WaitAndChillReborn.Instance.Config.HintVertPos != 0 && WaitAndChillReborn.Instance.Config.HintVertPos < 0)
                for (int i = WaitAndChillReborn.Instance.Config.HintVertPos; i < 0; i++)
                    stringBuilder.Append("\n");

            stringBuilder.Append(WaitAndChillReborn.Instance.Translation.TopMessage);
            stringBuilder.Append($"\n{WaitAndChillReborn.Instance.Translation.BottomMessage}");

            short networkTimer = GameCore.RoundStart.singleton.NetworkTimer;

            switch (networkTimer)
            {
                case -2: stringBuilder.Replace("{seconds}", WaitAndChillReborn.Instance.Translation.ServerIsPaused); break;

                case -1: stringBuilder.Replace("{seconds}", WaitAndChillReborn.Instance.Translation.RoundIsBeingStarted); break;

                case 1: stringBuilder.Replace("{seconds}", $"{networkTimer} {WaitAndChillReborn.Instance.Translation.OneSecondRemain}"); break;

                case 0: stringBuilder.Replace("{seconds}", WaitAndChillReborn.Instance.Translation.RoundIsBeingStarted); break;

                default: stringBuilder.Replace("{seconds}", $"{networkTimer} {WaitAndChillReborn.Instance.Translation.XSecondsRemains}"); break;
            }

            stringBuilder.Replace("{players}", Player.List.Any() ? $"{Player.List.Count()} {WaitAndChillReborn.Instance.Translation.OnePlayerConnected}" : $"{Player.List.Count()} {WaitAndChillReborn.Instance.Translation.XPlayersConnected}");

            if (WaitAndChillReborn.Instance.Config.HintVertPos != 0 && WaitAndChillReborn.Instance.Config.HintVertPos > 0)
                for (int i = 0; i < WaitAndChillReborn.Instance.Config.HintVertPos; i++)
                    stringBuilder.Append("\n");

            return NorthwoodLib.Pools.StringBuilderPool.Shared.ToStringReturn(stringBuilder);
        } 
    }
    
    public Vector3 SpawnPoint
    {
        get
        {
            return API.API.LobbyChoosedSpawnPoint;
        }
    }

    public RoleTypeId Role => WaitAndChillReborn.Instance.Config.LobbyConfig.RolesToChoose.ToList().RandomItem();
    
    public void Awake()
    {
        Player = Player.Get(gameObject);
        if(Player is null)
            Destroy(this);
        
        Exiled.Events.Handlers.Player.Dying += OnDying;
    }

    public void Update()
    {
        if(!API.API.IsLobby || RoundStart.singleton.NetworkTimer <= 1 && RoundStart.singleton.NetworkTimer != -2) 
            Destroy(this);
        
        if(WaitAndChillReborn.Instance.Config.DisplayWaitMessage)
            Player.ShowHint(Text);
        
        if (Player.IsAlive || IsSetuping) return;
        Setup();
    }
    
    public void OnDestroy()
    {
        if(Player is not null)
            Player.Role.Set(RoleTypeId.Spectator);
        Exiled.Events.Handlers.Player.Dying -= OnDying;
    }
    
    private void OnDying(DyingEventArgs ev)
    {
        if (ev.Player != Player) return;
        Player.ClearInventory();
    }
    
    public void Setup()
    {
        IsSetuping = true;
        Timing.CallDelayed(WaitAndChillReborn.Instance.Config.LobbyConfig.SpawnDelay, () =>
        {
            Player.Role.Set(Role);
            Player.Position = SpawnPoint;

            Effects();
            SetupInventory();
        });
    }

    public void Effects()
    {
        foreach (KeyValuePair<EffectType, byte> effect in WaitAndChillReborn.Instance.Config.LobbyConfig.LobbyEffects)
        {
            Player.EnableEffect(effect.Key);
            Player.ChangeEffectIntensity(effect.Key, effect.Value);
        }
    }

    public void SetupInventory()
    {
        Timing.CallDelayed(
            0.3f,
            () =>
            {
                Exiled.CustomItems.API.Extensions.ResetInventory(Player, WaitAndChillReborn.Instance.Config.LobbyConfig.Inventory);

                foreach (KeyValuePair<AmmoType, ushort> ammo in WaitAndChillReborn.Instance.Config.LobbyConfig.Ammo)
                    Player.Ammo[ammo.Key.GetItemType()] = ammo.Value;
                IsSetuping = false;
            });
    }
}