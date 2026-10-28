using HarmonyLib;
using Loddlepelago.CopyPastas;

namespace Loddlepelago.Patches;

[PatchAll]
public static class SavePatch
{
    public static string PathDir = $"{Directory.GetCurrentDirectory()}/ArchipelagoSaveData";

    [HarmonyPatch(typeof(ES3Settings), MethodType.Constructor, typeof(string), typeof(ES3Settings)), HarmonyPrefix]
    public static void ConstructorPatch(ref string path, ref ES3Settings settings)
        => path = $"{PathDir}/{Path.GetFileName(path)}";

    [HarmonyPatch(typeof(SaveWrapper), "GetNewSaveSettings"), HarmonyPrefix]
    public static bool SaveFilePath(string savename, ref ES3Settings __result)
    {
        __result = new ES3Settings(ES3.Location.Cache, ES3.CompressionType.Gzip) { path = $"{PathDir}/{savename}" };
        return false;
    }
}