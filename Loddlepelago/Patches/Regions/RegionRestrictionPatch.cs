using HarmonyLib;
using Loddlepelago.Archipelago;
using Loddlepelago.CopyPastas;
using UnityEngine;

namespace Loddlepelago.Patches.Regions;

[PatchAll]
public static class RegionRestrictionPatch
{
    [HarmonyPatch(typeof(CentralGameMenu), "CheckForNewMapSegment", typeof(Vector2)), HarmonyPrefix]
    public static bool RestrictRegions(CentralGameMenu __instance, Vector2 position)
    {
        try
        {
            var gameObject = __instance.RaycastAgainstUI_PUBLIC(__instance.CanvasToScreenSpace_PUBLIC(position));
            if (gameObject == null) return false;
            var mapSegmentIndex = int.Parse(gameObject.name);
            var regionName = LoddleClient.RegionIdToName[mapSegmentIndex];

            if (!ItemHandler.RegionsUnlocked.Contains(regionName))
            {
                EngineHub.FastTravelManager.FastTravel(6);
                return false;
            }

            LoddleClient.SendLocation($"Discovered {regionName}");
            // Core.Log.Msg($"Entered Region: [{mapSegmentIndex}]");
        }
        catch (Exception e) { Core.Log.Error(e); }
        return true;
    }

    // [HarmonyPatch(typeof(FastTravelMenu), "StartFastTraveling"), HarmonyPrefix]
    // public static void FastTravel(int locationIndex) => Core.Log.Msg($"Fast Travel to [{locationIndex}]");
}