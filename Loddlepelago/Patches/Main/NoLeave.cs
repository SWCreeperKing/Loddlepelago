using HarmonyLib;
using Loddlepelago.Archipelago;
using Loddlepelago.CopyPastas;

namespace Loddlepelago.Patches.Main;

[PatchAll]
public static class NoLeave
{
    [HarmonyPatch(typeof(SpaceshipInteractor), "UnlockLeavePlanetInteraction"), HarmonyPrefix]
    public static bool StopFromLeaving() => LoddleClient.Client.HasGoaled;
}