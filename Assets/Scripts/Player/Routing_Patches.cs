using System;
using ES3Types;
using GameNetcodeStuff;
using HarmonyLib;
using JetBrains.Annotations;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace ForceRoute.Assets.Scripts
{
    public static class Routing_Patches
    {

        public static string PlanetNames = ForceRouteBase.config_PlanetName.Value;

        public static bool forceOtherDays = ForceRouteBase.config_ForceOtherDays.Value;

        public static bool forceLastDay = ForceRouteBase.config_ForceLastDay.Value;

        public static SelectableLevel SelectableLevelName;

        public static string PlanetName;

        public static string GetRandomMoon(string PlanetName)
        {
            string[] planetList = PlanetName.Split(',');

            return planetList[Math.Abs(System.Guid.NewGuid().GetHashCode()) % planetList.Length].Trim();
        }

        public static void TrySetPlanet(string planetName)
        {
            if (!string.IsNullOrEmpty(planetName))
            {
                try
                {
                    foreach (var level in StartOfRound.Instance.levels)
                    {
                        if (level.PlanetName == planetName)
                        {
                            SelectableLevelName = level;
                            break;
                        }
                    }

                    ForceRouteBase.Instance.mls.LogDebug($"Found Selectable Level: {SelectableLevelName.PlanetName} | {SelectableLevelName.levelID}");
                    StartOfRound.Instance.ChangeLevel(SelectableLevelName.levelID);
                }
                catch (System.Exception e)
                {
                    ForceRouteBase.Instance.mls.LogError($"Error forcing planet to: {planetName} | {e}");
                }
                if (StartOfRound.Instance.gameStats.daysSpent == 0)
                    ForceRouteBase.Instance.mls.LogInfo($"Forcing planet: {planetName}");
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(StartOfRound), "Start")]
        public static void FirstDaySettings(StartOfRound __instance)
        {
            if (NetworkManager.Singleton.IsServer)
            { 
                ForceRouteBase.Instance.mls.LogDebug("Is Server. forcing planet on client.");
            }
            else
            {
                ForceRouteBase.Instance.mls.LogDebug("Is Client. skipping force routing");
                return;
            }

            foreach (var level in StartOfRound.Instance.levels)
            {
                ForceRouteBase.Instance.mls.LogDebug($"Selectable Level Planet Name: {level.PlanetName}");
            }

            ForceRouteBase.Instance.mls.LogDebug("Days Spent: " + StartOfRound.Instance.gameStats.daysSpent);
            if (StartOfRound.Instance.gameStats.daysSpent == 0)
            {
                PlanetName = GetRandomMoon(PlanetNames);
                TrySetPlanet(PlanetName);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TimeOfDay), "OnDayChanged")]
        public static void ResetShip(StartOfRound __instance)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                ForceRouteBase.Instance.mls.LogDebug("Is Server. forcing planet on client.");
            }
            else
            {
                ForceRouteBase.Instance.mls.LogDebug("Is Client. skipping force routing");
                return;
            }

            ForceRouteBase.Instance.mls.LogDebug("Days Spent: " + StartOfRound.Instance.gameStats.daysSpent);
            if (StartOfRound.Instance.gameStats.daysSpent == 0)
            {
                PlanetName = GetRandomMoon(PlanetNames);
                TrySetPlanet(PlanetName);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(StartOfRound), "SetShipReadyToLand")]
        public static void forcePlanetOtherDay(StartOfRound __instance)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                ForceRouteBase.Instance.mls.LogDebug("Is Server. forcing planet on client.");
            }
            else
            {
                ForceRouteBase.Instance.mls.LogDebug("Is Client. skipping force routing");
                return;
            }
            if (!forceLastDay && TimeOfDay.Instance.daysUntilDeadline <= 0f)
            {
                ForceRouteBase.Instance.mls.LogDebug("Not forcing planet on other days. Company Day");
                return;
            }
            if (!forceOtherDays && StartOfRound.Instance.gameStats.daysSpent != 0)
            {
                ForceRouteBase.Instance.mls.LogDebug("Not forcing planet on other days. Config Disabled");
                return;
            }
            PlanetName = GetRandomMoon(PlanetNames);
            TrySetPlanet(PlanetName);
        }
    }
}