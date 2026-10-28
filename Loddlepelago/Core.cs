using Loddlepelago.Archipelago;
using Loddlepelago.CopyPastas;
using MelonLoader;
using UnityEngine;
using static Loddlepelago.Archipelago.LoddleClient;
using Object = UnityEngine.Object;

[assembly:
    MelonInfo(typeof(Loddlepelago.Core), "Loddlepelago", Loddlepelago.Core.VersionNumber, "SW_CreeperKing", null)]
[assembly: MelonGame("MoonLagoon", "Loddlenaut")]

namespace Loddlepelago;

public class Core : MelonMod
{
    public const string VersionNumber = "0.1.0";
    public const string DataFolder = "Mods/SW_CreeperKing.Loddlepelago/Data";

    public static MelonLogger.Instance Log;
    public static APGui ApGui = new();
    public static DebugWindow DebugWindow;
    public static GameObject PlayButton;
    public static bool ShowDebugWindow;
    public static string Scene;

    public override void OnInitializeMelon()
    {
        Log = LoggerInstance;

        Log.Msg("Reading in Mod Data");

        BiomeRegions = File.ReadAllLines($"{DataFolder}/Biomes.txt");
        RegionIdToName = File.ReadAllLines($"{DataFolder}/RegionIdMap.txt").Select(line => line.Split('|'))
                             .ToDictionary(s => int.Parse(s[0]), s => s[1]);
        BadgeIdToName = File.ReadAllLines($"{DataFolder}/BadgeIdMap.txt").Select(line => line.Split('|'))
                            .ToDictionary(s => int.Parse(s[1]), s => s[0]);
        ItemIdToName = File.ReadAllLines($"{DataFolder}/ItemIdMap.txt").Select(line => line.Split('|'))
                           .ToDictionary(s => int.Parse(s[1]), s => s[0]);
        CookingIdToName = File.ReadAllLines($"{DataFolder}/CookingIdMap.txt").Select(line => line.Split('|'))
                              .ToDictionary(s => int.Parse(s[1]), s => s[0]);
        EvolutionIdToName = File.ReadAllLines($"{DataFolder}/EvolutionIdMap.txt").Select(line => line.Split('|'))
                                .ToDictionary(s => int.Parse(s[1]), s => s[0]);
        RegionNameToId = RegionIdToName.ToDictionary(kv => kv.Value, kv => kv.Key);
        ItemNameToId = ItemIdToName.ToDictionary(kv => kv.Value, kv => kv.Key);
        BiomeCompletionFlag.AddFlags(BiomeRegions);

        Log.Msg("Read Mod Data");

        Init();
        HarmonySetup.Init(MelonAssembly.Assembly, HarmonyInstance);

        Log.Msg("Initialized.");

        Log.Msg("Setting up Debug");
        DebugWindow = new DebugWindow(MelonAssembly.Assembly);
        KeyRegistry.AddKey(KeyCode.F5, () => ShowDebugWindow = !ShowDebugWindow);
        Log.Msg("Debug Initialized");
    }

    public override void OnUpdate()
    {
        Update();
        KeyRegistry.Update();
    }

    public override void OnGUI()
    {
        if (Scene is "Build_MainMenu") ApGui.Render();
        if (ShowDebugWindow) DebugWindow.Render();
    }
    
    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        PlayButton = null;
        Scene = sceneName;
        switch (sceneName)
        {
            case "Build_MainMenu":
                PlayButton = GameObject.Find(
                    "Main Menu Canvas/OuterGameMenu_MainMenu/TopLevelButtonsParent/Play Button"
                );
                PlayButton.SetActive(false);
                GameObject.Find("Main Menu Canvas/OuterGameMenu_MainMenu/Select Save Menu/Save Slot 2")
                          .SetActive(false);
                GameObject.Find("Main Menu Canvas/OuterGameMenu_MainMenu/Select Save Menu/Save Slot 3")
                          .SetActive(false);
                break;
            case "Build_Latest":
                var db = EngineHub.RecipeDatabase;
                foreach (var craftingRecipe in db.GetPrivateField<List<CraftingRecipe>>("craftingRecipeDB")
                                                 .Where(craftingRecipe => !craftingRecipe.isFoodRecipe))
                    db.CallPrivateMethod("UnlockRecipe", craftingRecipe.recipeID, true, false);
                foreach (var shippingOrder in db.GetPrivateField<List<ShippingOrder>>("upgradeOrderDB"))
                    db.CallPrivateMethod("UnlockRecipe", shippingOrder.orderID, false, true);
                foreach (var shippingOrder in db.GetPrivateField<List<ShippingOrder>>("structureOrderDB"))
                    db.CallPrivateMethod("UnlockRecipe", shippingOrder.orderID, false, false);

                foreach (var bot in Object.FindObjectsOfType<RecycleBot>())
                    bot.SetPrivateField("itemsToFillMeter", Data.RecycleCost);

                EngineHub.ShippingMenu.CallPrivateMethod("InitializeEntries", 0, true);
                EngineHub.ShippingMenu.CallPrivateMethod("InitializeEntries", 0, false);
                break;
        }
        // Log.Msg($"Scene Loaded: [{sceneName}]");
    }
}