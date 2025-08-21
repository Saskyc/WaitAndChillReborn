using System;
using Exiled.API.Features.Doors;

namespace WaitAndChillReborn
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Configs;
    using Exiled.API.Enums;
    using Exiled.API.Extensions;
    using Exiled.API.Features;
    using MapGeneration.Distributors;
    using MEC;
    using PlayerRoles;
    using UnityEngine;
    using static API.API;
    
    internal static class Methods
    {
        internal static void SetupAvailablePositions()
        {
            LobbyAvailableSpawnPoints.Clear();
           
            for (int i = 0; i < Config.LobbyRoom.Count; i++)
                Config.LobbyRoom[i] = Config.LobbyRoom[i].ToUpper();
            
            if (Config.LobbyRoom.Contains("TOWER1")) LobbyAvailableSpawnPoints.Add(new Vector3(39.150f, 314.112f, -31.818f));
            if (Config.LobbyRoom.Contains("TOWER2")) LobbyAvailableSpawnPoints.Add(new Vector3(162.125f, 319.440f, -13f));
            if (Config.LobbyRoom.Contains("TOWER3")) LobbyAvailableSpawnPoints.Add(new Vector3(108.3f, 348.048f, -14.075f));
            if (Config.LobbyRoom.Contains("TOWER4")) LobbyAvailableSpawnPoints.Add(new Vector3(-15.105f, 314.461f, -31.797f));
            if (Config.LobbyRoom.Contains("TOWER5")) LobbyAvailableSpawnPoints.Add(new Vector3(44.137f, 313.065f, -50.931f));
            if (Config.LobbyRoom.Contains("NUKE_SURFACE")) LobbyAvailableSpawnPoints.Add(new Vector3(29.69f, 291.86f, -26.7f));
            
            Dictionary<RoomType, string> roomToString = new ()
            {
                { RoomType.EzShelter, "SHELTER" },
                { RoomType.EzGateA, "GATE_A" },
                { RoomType.EzGateB, "GATE_B" },
            };
            
            foreach (Room room in Room.List)
            {
                if (roomToString.ContainsKey(room.Type) && Config.LobbyRoom.Contains(roomToString[room.Type]))
                {
                    Vector3 roomPos = room.transform.position;
                    LobbyAvailableSpawnPoints.Add(new Vector3(roomPos.x, roomPos.y + 2f, roomPos.z));
                }
            }
            
            if (Config.LobbyRoom.Contains("INTERCOM"))
            {
                Transform transform = Intercom.IntercomDisplay.transform;
                LobbyAvailableSpawnPoints.Add(transform.position + transform.forward * 3f);
            }
            
            Dictionary<string, RoleTypeId> stringToRole = new()
            {
                { "049", RoleTypeId.Scp049 },
                { "106", RoleTypeId.Scp106 },
                { "173", RoleTypeId.Scp173 },
                { "939", RoleTypeId.Scp939 },
            };
            
            foreach (KeyValuePair<string, RoleTypeId> role in stringToRole)
                if (Config.LobbyRoom.Contains(role.Key))
                    LobbyAvailableSpawnPoints.Add(role.Value.GetRandomSpawnLocation().Position);
          
            foreach (Vector3 position in Config.StaticLobbyPositions)
            {
                if (position == -Vector3.one)
                    continue;

                LobbyAvailableSpawnPoints.Add(position);
            }
        
            LobbyChoosedSpawnPoint = LobbyAvailableSpawnPoints.RandomItem();
            Log.Info($"Chosen spawnpoint: {LobbyChoosedSpawnPoint}");
        }

        private static readonly Translation Translation = WaitAndChillReborn.Instance.Translation;
        private static readonly LobbyConfig Config = WaitAndChillReborn.Instance.Config.LobbyConfig;
    }
}

