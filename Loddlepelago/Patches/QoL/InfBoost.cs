using HarmonyLib;
using Loddlepelago.CopyPastas;

namespace Loddlepelago.Patches.QoL;

[PatchAll]
public static class InfBoost
{
    [DebugVariableMarker("Inf Boost")]
    public static bool TurnOnInfBoost = false;
    
    [HarmonyPatch(typeof(PlayerMeterManager), "DepleteBoost"), HarmonyPrefix]
    public static bool InfOxy() => !TurnOnInfBoost;
}