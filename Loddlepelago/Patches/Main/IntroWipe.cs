using HarmonyLib;
using Loddlepelago.Archipelago;
using Loddlepelago.CopyPastas;

namespace Loddlepelago.Patches.Main;

[PatchAll]
public static class IntroWipe
{
    [HarmonyPatch(typeof(IntroManager), "Start"), HarmonyPrefix]
    public static void IntroStarted() => LoddleClient.InitNewGame();
}