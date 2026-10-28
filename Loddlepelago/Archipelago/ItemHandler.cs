using System.Collections.Concurrent;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using DG.Tweening;
using iGameAudio.FMODWrapper;
using Loddlepelago.CopyPastas;
using UnityEngine;

namespace Loddlepelago.Archipelago;

public static class ItemHandler
{
    public static ConcurrentQueue<int> ItemsToReceive = [];
    public static List<string> RegionsUnlocked = [];
    public static long TotalItemsReceived = 0;
    public static long LocalItemsReceived = 0;

    public static void HandleItem(ItemInfo item)
    {
        LocalItemsReceived++;
        if (item.ItemName is "Motivational Poster") return;

        if (LoddleClient.RegionNameToId.ContainsKey(item.ItemName))
        {
            RegionsUnlocked.Add(item.ItemName);
            return;
        }

        if (LoddleClient.ItemNameToId.TryGetValue(item.ItemName, out var value))
        {
            ItemsToReceive.Enqueue(value);
            return;
        }
    }

    // public static void RunPrinter()
    // {
    //     var sm = EngineHub.ShippingMenu;
    //
    //     if (sm.packageIsOnPad)
    //     {
    //         var packagePunchTween = sm.GetPrivateField<Tweener>("packagePunchTween");
    //         packagePunchTween?.Complete();
    //         sm.SetPrivateField(
    //             "packagePunchTween",
    //             sm.GetPrivateField<RectTransform>("packageParentTransform").DOPunchScale(
    //                 sm.GetPrivateField<Vector3>("packagePunchScale"), sm.GetPrivateField<float>("packagePunchTime"),
    //                 sm.GetPrivateField<int>("packagePunchVibrato"), sm.GetPrivateField<float>("packagePunchElasticity")
    //             )
    //         );
    //         AudioSystem.PlaySound(sm.GetPrivateField<ShippingMenuAudioConfig>("audioConfig").OrderRejectPadFullSound);
    //         return;
    //     }
    //
    //     try
    //     {
    //         if (ItemsToReceive.IsEmpty) return;
    //         ItemsToReceive.TryDequeue(out var item);
    //         LocalItemsReceived++;
    //
    //         if (LocalItemsReceived <= TotalItemsReceived) return;
    //         TotalItemsReceived = LocalItemsReceived;
    //         LoddleClient.Client.SendToStorage("item_on", TotalItemsReceived, Scope.Slot);
    //
    //         var itemObj = EngineHub.RecipeDatabase.LookUpOrderByID(item);
    //         itemObj.ordersMade++;
    //         EngineHub.EventManager.Fire(new PurchasedAPackage(itemObj));
    //     }
    //     catch (Exception e) { Core.Log.Error(e); }
    // }

}