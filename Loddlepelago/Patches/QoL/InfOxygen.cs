using HarmonyLib;
using Loddlepelago.CopyPastas;

namespace Loddlepelago.Patches.QoL;

[PatchAll]
public static class InfOxygen
{
    [DebugVariableMarker("Inf Oxygen")]
    public static bool TurnOnInfOxy = false;
    
    [HarmonyPatch(typeof(PlayerMeterManager), "DepleteOxygen"), HarmonyPrefix]
    public static bool InfOxy() => !TurnOnInfOxy;
}