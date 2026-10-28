using HarmonyLib;
using Loddlepelago.CopyPastas;
using UnityEngine;

namespace Loddlepelago.Patches.QoL;

[PatchAll]
public static class TrashLoot
{
    public static Dictionary<string, GameObject> TechObjects = [];

    [HarmonyPatch(typeof(Chunk), "Initialize"), HarmonyPostfix]
    public static void Init(Chunk __instance)
    {
        var data = __instance.chunkData;
        if (data.spawnPool is null) return;
        if (data.chunkType is not ChunkData.ChunkType.TRASH) return;
        if (data.spawnPool.itemsToSpawn.Count == 0) return;
        
        GameObject[] objects =
        [
            .. data.spawnPool.itemsToSpawn
                   .SelectMany(i => i.itemTypeObjectPool)
                   .Where(g =>g is not null && g.name.StartsWith("Tech_")),
        ];

        foreach (var obj in objects) TechObjects[obj.name] = obj;
    }
    
    [HarmonyPatch(typeof(Chunk), "SpawnItems"), HarmonyPrefix]
    public static bool SpawnLoot(Chunk __instance)
    {
        var data = __instance.chunkData;
        if (data.spawnPool is null) return false;
        if (data.chunkType is not ChunkData.ChunkType.TRASH) return true;
        if (data.spawnPool.itemsToSpawn.Count == 0) return true;
        
        foreach (var original in TechObjects.Values)
        {
            var gameObject = UnityEngine.Object.Instantiate(original);
            var component1 = gameObject.GetComponent<Pickup>();
            component1?.RegisterAsInstantiatedPickup();
            var insideUnitSphere = UnityEngine.Random.insideUnitSphere;
            gameObject.transform.position = __instance.transform.position + insideUnitSphere * data.spawnRadius;

            if (!gameObject.TryGetComponent(out Rigidbody component2)) continue;
            component2.isKinematic = false;
            component2.AddForce(
                insideUnitSphere.normalized * UnityEngine.Random.Range(data.spawnForceRange.x, data.spawnForceRange.y),
                ForceMode.Impulse
            );
            var num = UnityEngine.Random.Range(data.spawnTorqueRange.x, data.spawnTorqueRange.y)
                      * BloopTools.RandomSign();
            component2.AddTorque(UnityEngine.Random.onUnitSphere * num);
        }
        
        return false;
    }
}