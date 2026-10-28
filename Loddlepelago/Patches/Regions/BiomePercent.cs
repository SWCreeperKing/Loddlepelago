using Archipelago.MultiClient.Net.Enums;
using HarmonyLib;
using Loddlepelago.Archipelago;
using Loddlepelago.CopyPastas;

namespace Loddlepelago.Patches.Regions;

[PatchAll]
public static class BiomePercent
{
    public static readonly int[] CleanPercents = [10, 25, 50, 75, 100];
    public static Dictionary<string, int> LastRecordedPollution = [];

    [HarmonyPatch(typeof(BiomeManager), "UpdateOverallPollution"), HarmonyPostfix]
    public static void PercentUpdated(BiomeManager __instance, bool skipEffects, bool instantTransition)
    {
        var name = __instance.biomeDisplayName;
        var pol = (int)Math.Truncate(__instance.biomePollution * 100);
        var clean = 100 - pol;

        var prevClean = LastRecordedPollution.GetValueOrDefault(name, clean);
        var remainingPercent = CleanPercents.SkipWhile(i => prevClean > i);
        var toCleanPercent = remainingPercent.TakeWhile(i => clean >= i);

        foreach (var i in toCleanPercent)
        {
            LoddleClient.SendLocation($"Clean {__instance.biomeDisplayName} {i}%");
            // Core.Log.Msg($"[{__instance.biomeDisplayName}] Pollution: [{pol}] : [{clean}]");
            // Core.Log.Msg($"clean milestone: [{i}%]");

            if (i is not 100) continue;
            LoddleClient.BiomeCompletionFlag += __instance.biomeDisplayName;
            LoddleClient.Client.SendToStorage(
                "biome_completion", LoddleClient.BiomeCompletionFlag.GetTrackingFlag, Scope.Slot
            );
            if (LoddleClient.BiomeCompletionFlag.IsMaxFlag) LoddleClient.Client.TryGoal();
            Core.Log.Msg(
                $"Mark [{__instance.biomeDisplayName}] as complete| final: [{LoddleClient.BiomeCompletionFlag.GetTrackingFlag}]"
            );
        }

        LastRecordedPollution[name] = clean;
    }
}