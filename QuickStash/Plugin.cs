﻿using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using Wetstone.API;

namespace QuickStash
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("xyz.molenzwiebel.wetstone")]
    [Reloadable]
    public class Plugin : BasePlugin
    {
        public static ManualLogSource Logger;

        public static Keybinding configKeybinding;
        public static ConfigEntry<float> configMaxDistance;
        private Harmony _hooks;


        private void InitConfig()
        {
            configMaxDistance = Config.Bind("Server", "MaxDistance", 50.0f, "The max distance for transfering items. 5 'distance' is about 1 tile.");

            configKeybinding = KeybindManager.Register(new()
            {
                Id = "elmegaard.quickstash.deposit",
                Category = "QuickStash",
                Name = "Deposit",
                DefaultKeybinding = KeyCode.G,
            });
        }

        private void RegisterMessages()
        {
            VNetworkRegistry.RegisterServerboundStruct<MergeInventoriesMessage>((fromCharacter, msg) =>
            {
                QuickStashServer.OnMergeInventoriesMessage(fromCharacter, msg);
            });
        }

        private void ClearMessages()
        {
            VNetworkRegistry.UnregisterStruct<MergeInventoriesMessage>();
        }

        public override void Load()
        {
            Logger = Log;
            InitConfig();
            QuickStashClient.Reset();

            _hooks = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            RegisterMessages();

            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        }

        public override bool Unload()
        {
            Config.Clear();
            KeybindManager.Unregister(configKeybinding);
            _hooks.UnpatchSelf();
            ClearMessages();
            return true;
        }
    }
}
