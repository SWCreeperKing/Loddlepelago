using HarmonyLib;
using iGameAudio.FMODWrapper;
using Loddlepelago.Archipelago;
using Loddlepelago.CopyPastas;
using UnityEngine;

namespace Loddlepelago.Patches.Main;

[PatchAll]
public static class Cook
{
    [HarmonyPatch(typeof(CraftingMenu), "CraftItem"), HarmonyPrefix]
    public static bool OnCook(CraftingMenu __instance, float ___lastItemCounterUIOpenTime,
        float ___ignoreCraftInputPeriod, CraftingMenuEntry ___currentlySelectedEntry,
        CraftingMenuAudioConfig ___audioConfig)
    {
        if (!__instance.menuIsOpen || !__instance.itemCounterUIIsOpen || Time.timeSinceLevelLoad
            <= (double)___lastItemCounterUIOpenTime + ___ignoreCraftInputPeriod) return false;
        
        var isToy = InventoryItemDatabase.IsToyID(___currentlySelectedEntry.itemDetails.itemData.itemID);
        EngineHub.EventManager.Fire(new CraftedItem(___currentlySelectedEntry.recipe.isFoodRecipe, isToy));
        if (isToy) EngineHub.UIManager.AddToObjective(ObjectiveNames.CraftAToy, 1f);
        EngineHub.UIManager.AddToObjective(
            ___currentlySelectedEntry.recipe.isFoodRecipe ? ObjectiveNames.TestCookingModule
                : ObjectiveNames.TestCraftingStation, 1f
        );
        __instance.CallPrivateMethod("RemoveRecipeIngredients");
        __instance.SetPrivateField("itemIsBeingCrafted", true);
        EngineHub.EventManager.Fire(
            new CraftedAnItem(___currentlySelectedEntry.recipe, __instance.numberOfItemsToCraft)
        );
        EngineHub.PlayerInteractionHandler.ExitCurrentStructureMenu();
        AudioSystem.PlaySound(___audioConfig.CraftSound);
        LoddleClient.SendLocation($"Cook: {LoddleClient.ItemIdToName[___currentlySelectedEntry.recipe.recipeID]}");
        return false;
    }
}