using Archipelago.MultiClient.Net.Enums;
using CreepyUtil.Archipelago;
using CreepyUtil.Archipelago.ApClient;
using JetBrains.Annotations;
using Loddlepelago.CopyPastas;
using Loddlepelago.Patches;
using Newtonsoft.Json;
using Object = UnityEngine.Object;

namespace Loddlepelago.Archipelago;

public static class LoddleClient
{
    public static string[] BiomeRegions = [];
    public static Dictionary<int, string> RegionIdToName = [];
    public static Dictionary<string, int> RegionNameToId = [];
    public static Dictionary<int, string> BadgeIdToName = [];
    public static Dictionary<int, string> ItemIdToName = [];
    public static Dictionary<string, int> ItemNameToId = [];
    public static Dictionary<int, string> CookingIdToName = [];
    public static Dictionary<int, string> EvolutionIdToName = [];
    public static LargeLoseFlag<string> BiomeCompletionFlag = new();

    public static ApClient Client = new(new TimeSpan(0, 1, 0));
    public static ApData Data = new();
    public static string GameUUID = "";

    public static void Init()
    {
        if (File.Exists("ApConnection.json"))
        {
            Data = JsonConvert.DeserializeObject<ApData>(File.ReadAllText("ApConnection.json").Replace("\r", ""));
        }

        Client.OnConnectionLost += () =>
        {
            // if (Core.Scene is "Game") GameUI.Instance.IngameMenuReturnToTitle();
            Core.Log.Error("Lost Connection to Ap");
        };

        Client.OnConnectionEvent += _ =>
        {
            try
            {
                BiomeCompletionFlag.SetFlag();
                ItemHandler.RegionsUnlocked =
                [
                    "Home Cave", "North West of Home Cave", "North of Home Cave", "Ripple Reef",
                    "South of Home Cave", "West of Home Cave",
                ];
                ItemHandler.LocalItemsReceived = 0;
                ItemHandler.LocalPrintedReceived = 0;
                ItemHandler.ItemsToReceive.Clear();
                ItemHandler.PrinterItemsReceived = Client.GetFromStorage("printer_items", Scope.Slot, def: 0L);
                Client.GetFromStorageAsync(
                    "biome_completion", s => BiomeCompletionFlag.SetFlag(s), Scope.Slot, BiomeCompletionFlag.MinFlag
                );

                GameUUID = (string)Client.SlotData["uuid"];

                SavePatch.PathDir = $"{Directory.GetCurrentDirectory()}/Archipelago/{GameUUID}";
                Core.Log.Msg($"Dir: [{SavePatch.PathDir}]");
                if (!Directory.Exists(SavePatch.PathDir)) Directory.CreateDirectory(SavePatch.PathDir);

                var mm = Object.FindAnyObjectByType<MainMenu>();
                foreach (var saveSlot in mm.GetPrivateField<SaveSlotMenu[]>("saveSlots"))
                {
                    try { saveSlot.InitializeFromLoad(); }
                    catch (Exception e) { Core.Log.Error($"Exception during load of Slot {saveSlot.slotIndex}", e); }
                }           
                
                if (BiomeCompletionFlag.IsMaxFlag) Client.TryGoal();
            }
            catch (Exception e) { Core.Log.Error(e); }
        };

        Client.ItemHandlerInitialized += handler =>
        {
            handler.OnItemReceived += (item, _) => ItemHandler.HandleItem(item);
        };

        Client.OnConnectionErrorReceived += (e, _) => Core.Log.Error(e);
    }

    [CanBeNull]
    public static string[] TryConnect(string addressPort, string password, string slotName)
    {
        var addressSplit = addressPort.Split(':');

        if (addressSplit.Length != 2) return ["Address Field is incorrect"];
        if (!int.TryParse(addressSplit[1], out var port)) return ["Port is incorrect"];

        var login = new LoginInfo(port, slotName, addressSplit[0], password);

        return Client.TryConnect(login, "Loddlenaut", ItemsHandlingFlags.AllItems);
    }

    public static void SaveFile() => File.WriteAllText("ApConnection.json", JsonConvert.SerializeObject(Data));

    public static void Update()
    {
        try
        {
            if (Client is null) return;
            Client.UpdateConnection();

            if (!Client.IsConnected) return;
            if (Core.Scene is not "Build_Latest") return;

            Client.UpdateItemHandler();
        }
        catch (Exception e) { Core.Log.Error(e); }
    }

    public static void SendLocation(string loc)
    {
        if (!Client.IsMissingLocation(loc)) return;
        Client.SendLocation(loc);
    }

    public static void InitNewGame()
    {
        Core.Log.Msg("New Game");
        BiomeCompletionFlag.SetFlag();
        ItemHandler.RegionsUnlocked =
        [
            "Home Cave", "North West of Home Cave", "North of Home Cave", "Ripple Reef",
            "South of Home Cave", "West of Home Cave",
        ];
        ItemHandler.LocalItemsReceived = 0;
        ItemHandler.PrinterItemsReceived = 0;
        ItemHandler.ItemsToReceive.Clear();
        Client.SendToStorage("printer_items", 0, Scope.Slot);
        Client.SendToStorage("biome_completion", BiomeCompletionFlag.MinFlag, Scope.Slot);
    }
}