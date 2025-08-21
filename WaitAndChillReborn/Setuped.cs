using Exiled.API.Features;
using Exiled.API.Features.Pickups;
using Exiled.API.Features.Roles;
using InventorySystem.Items.Pickups;
using MEC;
using UnityEngine;

namespace WaitAndChillReborn;

public class Setuped : MonoBehaviour
{
    public void Awake()
    {
        if (!WaitAndChillReborn.Instance.Config.DisplayWaitingForPlayersScreen)
            gameObject.transform.localScale = Vector3.zero;
        
        if (Server.FriendlyFire)
            FriendlyFireConfig.PauseDetector = true;

        Scp173Role.TurnedPlayers.Clear();
        Scp096Role.TurnedPlayers.Clear();

        if (EventHandlers.MapChosen is null)
        {
                
        }
        
        Timing.CallDelayed(0.1f, Methods.SetupAvailablePositions);
            
        Timing.CallDelayed(1f, () =>
        {
            EventHandlers.LockedPickups.Clear();

            foreach (var pickup in Pickup.List)
            {
                try
                {
                    if (!pickup.IsLocked)
                    {
                        PickupSyncInfo info = pickup.Base.NetworkInfo;
                        info.Locked = true;
                        pickup.Base.NetworkInfo = info;

                        pickup.Base.GetComponent<Rigidbody>().isKinematic = true;
                        EventHandlers.LockedPickups.Add(pickup);
                    }
                }
                catch (System.Exception)
                {
                    // ignored
                }
            }
        });
    }
}