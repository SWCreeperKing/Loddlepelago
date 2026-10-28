using System.Collections.Concurrent;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;

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

        if (LoddleClient.RegionNameToId.ContainsKey(item.ItemName)) RegionsUnlocked.Add(item.ItemName);
        else if (LoddleClient.ItemNameToId.TryGetValue(item.ItemName, out var value)
                 && LocalItemsReceived > TotalItemsReceived) ItemsToReceive.Enqueue(value);

        if (LocalItemsReceived <= TotalItemsReceived) return;
        TotalItemsReceived = LocalItemsReceived;
        LoddleClient.Client.SendToStorage("item_on", TotalItemsReceived, Scope.Slot);
    }
}