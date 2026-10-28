using Archipelago.MultiClient.Net.Enums;
using DG.Tweening;
using HarmonyLib;
using iGameAudio.FMODWrapper;
using Loddlepelago.Archipelago;
using Loddlepelago.CopyPastas;
using UnityEngine;

namespace Loddlepelago.Patches.Main;

[PatchAll]
public static class Orders
{

    [HarmonyPatch(typeof(ShippingMenuEntry), "IsUnderOrderLimit"), HarmonyPrefix]
    public static bool HideIfSent(ShippingMenuEntry __instance, ref bool __result)
    {
        __result = LoddleClient.Client.IsMissingLocation(
            $"Purchase {LoddleClient.ItemIdToName[__instance.entryOrderInfo.orderID]}"
        );
        return false;
    }
    
    [HarmonyPatch(typeof(ShippingMenu), "PlaceOrder"), HarmonyPrefix]
    public static bool PlaceOrder(ShippingMenu __instance, ShippingMenuEntry ___currentlySelectedEntry,
        ShippingMenuAudioConfig ___audioConfig)
    {
        if (___currentlySelectedEntry is null) return false;
        var loc = $"Purchase {LoddleClient.ItemIdToName[___currentlySelectedEntry.entryOrderInfo.orderID]}";
        if (!LoddleClient.Client.IsMissingLocation(loc))
        {
            if (__instance.CallPrivateMethod<bool>(
                    "SubtractMaterialCredits", ___currentlySelectedEntry.entryOrderInfo.orderCost
                ))
            {
                if (!___currentlySelectedEntry.entryOrderInfo.isUpgradeOrder)
                    EngineHub.UIManager.AddToObjective(ObjectiveNames.TestShippingStation, 1f);
                ++___currentlySelectedEntry.entryOrderInfo.ordersMade;
                LoddleClient.SendLocation(loc);
                AudioSystem.PlaySound(___audioConfig.OrderSound);
                EngineHub.PlayerInteractionHandler.ExitCurrentStructureMenu();
            }
            else __instance.CallPrivateMethod("EmphasizeUnaffordableCredits");
        }
        else
        {
            ___currentlySelectedEntry.EmphasizeOrderLimitReached();
            AudioSystem.PlaySound(___audioConfig.OrderRejectLimitSound);
        }

        return false;
    }
    
    [HarmonyPatch(typeof(ShippingMenu), "Update"), HarmonyPostfix]
    public static void SpawnItem(ShippingMenu __instance, ShippingMenuAudioConfig ___audioConfig,
        RectTransform ___packageParentTransform, Vector3 ___packagePunchScale, float ___packagePunchTime,
        int ___packagePunchVibrato, float ___packagePunchElasticity)
    {
        if (__instance.packageIsOnPad)
        {
            var packagePunchTween = __instance.GetPrivateField<Tweener>("packagePunchTween");
            packagePunchTween?.Complete();
            __instance.SetPrivateField(
                "packagePunchTween",
                ___packageParentTransform.DOPunchScale(
                    ___packagePunchScale, ___packagePunchTime, ___packagePunchVibrato, ___packagePunchElasticity
                )
            );
            AudioSystem.PlaySound(___audioConfig.OrderRejectPadFullSound);
            return;
        }

        try
        {
            if (ItemHandler.ItemsToReceive.IsEmpty) return;
            ItemHandler.ItemsToReceive.TryDequeue(out var item);
            ItemHandler.LocalItemsReceived++;

            if (ItemHandler.LocalItemsReceived <= ItemHandler.TotalItemsReceived) return;
            ItemHandler.TotalItemsReceived = ItemHandler.LocalItemsReceived;
            LoddleClient.Client.SendToStorage("item_on", ItemHandler.TotalItemsReceived, Scope.Slot);

            var itemObj = EngineHub.RecipeDatabase.LookUpOrderByID(item);
            itemObj.ordersMade++;
            EngineHub.EventManager.Fire(new PurchasedAPackage(itemObj));
        }
        catch (Exception e)
        {
            Core.Log.Error(e);
        }
    }
}