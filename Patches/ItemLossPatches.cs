using System.Collections.Generic;
using HarmonyLib;
using Logging;

namespace FortifySkillsRedux.Patches;

[HarmonyPatch]
internal static class ItemLossPatches
{
    private static HashSet<string> AdventureBackpacks = new() {
        "BackpackMeadows", "BackpackBlackForest",  "BackpackSwamp",
        "BackpackMountains", "BackpackPlains", "BackpackMistlands",
        "LegacyIronBackpack", "LegacySilverBackpack"
    };

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
            if (CheckIfCanKeep(item))
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

    private static bool CheckIfCanKeep(ItemDrop.ItemData item)
    {
        bool keepAll = FortifySkillsRedux.Instance.KeepAllItemsOnDeath.Value;
        bool keepEquipped = FortifySkillsRedux.Instance.KeepEquippedItemsOnDeath.Value;

        if (!keepAll && !(keepEquipped && item.m_equipped))
        {
            return false;
        }

        bool keepNonTeleportable = FortifySkillsRedux.Instance.KeepNonTeleportableItemsOnDeath.Value;
        if (!(item.m_shared.m_teleportable) && !keepNonTeleportable)
        {
            return false;
        }

        // explicitly check for adventure backpacks since they aren't containers
        if (!FortifySkillsRedux.Instance.KeepContainersOnDeath.Value && AdventureBackpacks.Contains(item.m_dropPrefab.name))
        {
            return false;
        }
        
        if (item.m_dropPrefab.TryGetComponent(out Container container) && container)
        {
            if (!(FortifySkillsRedux.Instance.KeepContainersOnDeath.Value))
            {
                return false;
            }
            foreach (ItemDrop.ItemData containerItem in container.m_inventory.m_inventory)
            {
                if (!CheckIfCanKeep(containerItem))
                {
                    return false;
                }
            }
        }
        return true;
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
