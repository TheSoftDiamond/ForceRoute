using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using ForceRoute.Assets.Scripts;
using System;
using System.Reflection;
using UnityEngine;

namespace ForceRoute
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class ForceRouteBase : BaseUnityPlugin
    {

        internal static ForceRouteBase Instance { get; private set; }

        public ManualLogSource mls;

        private const string GUID = "SoftDiamond.ForceRoute";
        private const string NAME = "ForceRoute";
        private const string VERSION = "1.0.0";


        private readonly Harmony harmony = new Harmony(GUID);

        public static ConfigEntry<string> 
            config_PlanetName;

        public static ConfigEntry<bool>
                   config_ForceOtherDays,
                   config_ForceLastDay;

        private void Awake()
        {
            if (Instance == null) Instance = this;

            mls = BepInEx.Logging.Logger.CreateLogSource(GUID);

            ConfigSetup();

            harmony.PatchAll();

            mls.LogInfo($"ForceRoute has initialized!");

            NetcodePatcherAwake();
        }

        private void ConfigSetup()
        {
            config_PlanetName = Config.Bind("Planet Name(s)", "Names", "", "What planets should be forced? Uses SelectableLevel.PlanetName variable.");
            config_ForceOtherDays = Config.Bind("Force Other Days", "Force Other Days?", true, "Should the mod affect other days than just the first day?");
            config_ForceLastDay = Config.Bind("Force Last Day", "Force Last Day?", false, "Should the mod also affect the the Company/Selling Day?");
        }

        private void NetcodePatcherAwake()
        {
            try
            {
                var currentAssembly = Assembly.GetExecutingAssembly();
                var types = currentAssembly.GetTypes();

                foreach (var type in types)
                {
                    var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

                    foreach (var method in methods)
                    {
                        try
                        {
                            // Safely attempt to retrieve custom attributes
                            var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);

                            if (attributes.Length > 0)
                            {
                                try
                                {
                                    // Safely attempt to invoke the method
                                    method.Invoke(null, null);
                                }
                                catch (TargetInvocationException ex)
                                {
                                    // Log and continue if method invocation fails (e.g., due to missing dependencies)
                                    Logger.LogWarning($"Failed to invoke method {method.Name}: {ex.Message}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Handle errors when fetching custom attributes, due to missing types or dependencies
                            Logger.LogWarning($"Error processing method {method.Name} in type {type.Name}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Catch any general exceptions that occur in the process
                Logger.LogError($"An error occurred in NetcodePatcherAwake: {ex.Message}");
            }
        }
    }
}