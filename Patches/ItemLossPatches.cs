using System.Collections.Generic;
using HarmonyLib;
using Logging;

namespace FortifySkillsRedux.Patches;

[HarmonyPatch]
internal static class ItemLossPatches
{

    [HarmonyPrefix]
    [HarmonyPriority(50)]
    [HarmonyPatch(typeof(Player), nameof(Player.CreateTombStone))]
    private static void StoreItems(Player __instance, out Dictionary<ItemDrop.ItemData, bool> __state)
    {
        __state = [];
        if (!__instance)
        {
            return;
        }

        bool keepAll = FortifySkillsRedux.Instance.KeepAllItemsOnDeath.Value;
        bool keepEquipped = FortifySkillsRedux.Instance.KeepEquippedItemsOnDeath.Value;
        if (!keepAll && !keepEquipped)
        {
            return;
        }

        List<ItemDrop.ItemData> itemsToDrop = [];
        string reason = keepAll ? "KeepAllItems setting" : "KeepEquippedItems setting";
        foreach (ItemDrop.ItemData item in __instance.m_inventory.m_inventory)
        {
            string prefabName = Utils.GetPrefabName(item.m_dropPrefab);
            string itemLocation = (item.m_gridPos.y == 0) ? "hotbar" : "inventory";

            if (keepAll || (keepEquipped && item.m_equipped))
            {
                __state.Add(item, item.m_equipped);
                Log.LogDebug($"Keeping {prefabName} in {itemLocation} due to the {reason}.");
            }
            else
            {
                itemsToDrop.Add(item);
                Log.LogDebug($"Dropping {prefabName} from {itemLocation}.");
            }
        }

        __instance.m_inventory.m_inventory = itemsToDrop;
    }

    [HarmonyPostfix]
    [HarmonyPriority(750)]
    [HarmonyPatch(typeof(Player), nameof(Player.CreateTombStone))]
    private static void RestoreItems(Player __instance, Dictionary<ItemDrop.ItemData, bool> __state)
    {
        if (!__instance || __state.Count == 0)
        {
            return;
        }
        foreach (KeyValuePair<ItemDrop.ItemData, bool> item in __state)
        {
            Log.LogDebug($"Adding {item.Key.m_dropPrefab.name} back to inventory.");
            __instance.m_inventory.m_inventory.Add(item.Key);
            Log.LogDebug($"Item {item.Key.m_dropPrefab.name} was equipped: {item.Value}");
            if (item.Value && (!item.Key.m_dropPrefab.name.StartsWith("BBH") || !item.Key.m_dropPrefab.name.EndsWith("Quiver")))
            {
                __instance.EquipItem(item.Key, triggerEquipEffects: false);
            }
        }
        __instance.m_inventory.Changed();
    }
}
