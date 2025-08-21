global using Config = WaitAndChillReborn.Configs.Config;
using System;
using Exiled.API.Features;
using WaitAndChillReborn.Configs;

namespace WaitAndChillReborn
{
    public class WaitAndChillReborn : Plugin<Config, Translation>
    {
        public static WaitAndChillReborn Instance;
        
        public override void OnEnabled()
        {
            Instance = this;
            
            EventHandlers.RegisterEvents();
            
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            EventHandlers.UnRegisterEvents();
            
            Instance = null;

            base.OnDisabled();
        }

        public override string Name => "WaitAndChillReborn";
        public override string Author => "Michal78900";
        public override Version Version => new Version(5, 2, 0);
        public override Version RequiredExiledVersion => new Version(9, 8, 1);
    }
}
