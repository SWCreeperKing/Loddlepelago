using HarmonyLib;
using Loddlepelago.Archipelago;
using Loddlepelago.CopyPastas;

namespace Loddlepelago.Patches.Main;

[PatchAll]
public static class HoloBadge
{
    [HarmonyPatch(typeof(CentralGameMenu), "UnlockHoloBadge"), HarmonyPrefix]
    public static void CollectBadge(int badgeIndex)
        => LoddleClient.SendLocation($"{LoddleClient.BadgeIdToName[badgeIndex]}");
}