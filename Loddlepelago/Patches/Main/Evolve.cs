using HarmonyLib;
using Loddlepelago.Archipelago;
using Loddlepelago.CopyPastas;
using UnityEngine;

namespace Loddlepelago.Patches.Main;

[PatchAll]
public static class Evolve
{
    [HarmonyPatch(
         typeof(LoddleCocoon), "SpawnCocoon", typeof(LoddleAI.LoddleType), typeof(string), typeof(Color), typeof(bool),
         typeof(string), typeof(int), typeof(bool), typeof(LoddleLifeCycle.LoddlePersonalityTrait), typeof(int),
         typeof(int), typeof(float), typeof(LoddleLifeCycle.LoddleCosmeticVariant), typeof(float)
     ), HarmonyPostfix]
    public static void EvolveCheck(LoddleAI.LoddleType loddleTypeToSpawn, string babyLoddleName, Color babyColor,
        bool babyHadDarkerDeformColors, string spawnedLoddleMovementZoneID, int favoriteFoodID,
        bool favoriteFoodHasBeenDiscovered, LoddleLifeCycle.LoddlePersonalityTrait babyPersonality,
        int babyCrystalCrispNibbleCount, int babyBloopSequenceRespondCount, float babyFollowTime,
        LoddleLifeCycle.LoddleCosmeticVariant babyCosmeticVariant, float lifetimeOverride)
    {
        var evoKey = (int)loddleTypeToSpawn;
        if (!LoddleClient.EvolutionIdToName.TryGetValue(evoKey, out var value))
        {
            Core.Log.Msg($"Loddle Evolving to: ??? = [{loddleTypeToSpawn}] = [{evoKey}]");
            return;
        }
        
        Core.Log.Msg($"Loddle Evolving to: [{value}] = [{evoKey}]");
        LoddleClient.SendLocation($"Evolve: {value}");
    }
}