using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using ServerSync;

namespace AnyWood
{
    [BepInPlugin(pluginID, pluginName, pluginVersion)]
    [BepInIncompatibility("randyknapp.mods.itsjustwood")]
    public class AnyWood : BaseUnityPlugin
    {
        public const string pluginID = "shudnal.AnyWood";
        public const string pluginName = "Any Wood";
        public const string pluginVersion = "1.0.2";

        private Harmony _harmony;

        internal static readonly ConfigSync configSync = new ConfigSync(pluginID) { DisplayName = pluginName, CurrentVersion = pluginVersion, MinimumRequiredVersion = pluginVersion };

        internal static AnyWood instance;

        private static ConfigEntry<bool> modEnabled;
        private static ConfigEntry<bool> configLocked;
        private static ConfigEntry<bool> loggingEnabled;

        private static ConfigEntry<Vector3> fineWoodToWood;
        private static ConfigEntry<Vector3> coreWoodToWood;
        private static ConfigEntry<Vector3> ancientBarkToWood;
        private static ConfigEntry<Vector3> yggdrasilWoodToWood;
        private static ConfigEntry<Vector3> ashWoodToWood;

        private static ConfigEntry<bool> fineWoodForFuel;
        private static ConfigEntry<bool> coreWoodForFuel;
        private static ConfigEntry<bool> ancientBarkForFuel;
        private static ConfigEntry<bool> yggdrasilWoodForFuel;
        private static ConfigEntry<bool> ashWoodForFuel;

        private static ConfigEntry<bool> fineWoodForCoal;
        private static ConfigEntry<bool> coreWoodForCoal;
        private static ConfigEntry<bool> ancientBarkForCoal;
        private static ConfigEntry<bool> yggdrasilWoodForCoal;
        private static ConfigEntry<bool> ashWoodForCoal;

        private static ItemDrop wood;
        private static ItemDrop fineWood;
        private static ItemDrop coreWood;
        private static ItemDrop ancientBark;
        private static ItemDrop yggdrasilWood;
        private static ItemDrop ashWood;
        private static ItemDrop coal;

        private static CraftingStation workbench;

        private void Awake()
        {
            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), pluginID);

            instance = this;

            ConfigInit();
            _ = configSync.AddLockingConfigEntry(configLocked);
        }

        private void ConfigInit()
        {
            config("General", "NexusID", 2560, "Nexus mod ID for updates", false);
            modEnabled = config("General", "Enabled", defaultValue: true, "Enable this mod. Relaunch the world to take effect.");
            configLocked = config("General", "Lock Configuration", defaultValue: true, "Configuration is locked and can be changed by server admins only.");
            loggingEnabled = config("General", "Enable logging", defaultValue: false, "Enable logging. [Not Synced with Server]", false);

            fineWoodToWood = config("Transmute", "Fine wood", defaultValue: new Vector3(10, 10, 4), "Fine wood to wood transmute recipe. First is fine wood used, second basic wood result, third - workbench level required." +
                                                                                                    "\nSet 0 to wood to disable recipe. Set 0 to station to disable station requirement.");
            coreWoodToWood = config("Transmute", "Core wood", defaultValue: new Vector3(10, 10, 3), "Core wood to wood transmute recipe. First is core wood used, second basic wood result, third - workbench level required." +
                                                                                                    "\nSet 0 to wood to disable recipe. Set 0 to station to disable station requirement.");
            ancientBarkToWood = config("Transmute", "Ancient bark", defaultValue: new Vector3(10, 10, 4), "Ancient bark to wood transmute recipe. First is ancient bark used, second basic wood result, third - workbench level required." +
                                                                                                          "\nSet 0 to wood to disable recipe. Set 0 to station to disable station requirement.");
            yggdrasilWoodToWood = config("Transmute", "Yggdrasil wood", defaultValue: new Vector3(10, 10, 5), "Yggdrasil wood to wood transmute recipe. First is yggdrasil wood used, second basic wood result, third - workbench level required. " +
                                                                                                              "\nSet 0 to wood to disable recipe. Set 0 to station to disable station requirement.");
            ashWoodToWood = config("Transmute", "Ash wood", defaultValue: new Vector3(10, 10, 5), "Ash wood to wood transmute recipe. First is ash wood used, second basic wood result, third - workbench level required. " +
                                                                                                              "\nSet 0 to wood to disable recipe. Set 0 to station to disable station requirement.");

            fineWoodToWood.SettingChanged += (s, e) => AddRecipes(ObjectDB.instance);
            coreWoodToWood.SettingChanged += (s, e) => AddRecipes(ObjectDB.instance);
            ancientBarkToWood.SettingChanged += (s, e) => AddRecipes(ObjectDB.instance);
            yggdrasilWoodToWood.SettingChanged += (s, e) => AddRecipes(ObjectDB.instance);
            ashWoodToWood.SettingChanged += (s, e) => AddRecipes(ObjectDB.instance);

            fineWoodForFuel = config("Fuel", "Fine wood", defaultValue: false, "Use fine wood as a fuel at cooking station, bath and fireplaces");
            coreWoodForFuel = config("Fuel", "Core wood", defaultValue: true, "Use core wood as a fuel at cooking station, bath and fireplaces");
            ancientBarkForFuel = config("Fuel", "Ancient bark", defaultValue: true, "Use ancient bark as a fuel at cooking station, bath and fireplaces");
            yggdrasilWoodForFuel = config("Fuel", "Yggdrasil wood", defaultValue: false, "Use yggdrasil wood as a fuel at cooking station, bath and fireplaces");
            ashWoodForFuel = config("Fuel", "Ash wood", defaultValue: false, "Use ash wood as a fuel at cooking station, bath and fireplaces");

            fineWoodForCoal = config("Coal", "Fine wood", defaultValue: true, "Use fine wood as a source of coal in charcoal kiln");
            coreWoodForCoal = config("Coal", "Core wood", defaultValue: true, "Use core wood as a source of coal in charcoal kiln");
            ancientBarkForCoal = config("Coal", "Ancient bark", defaultValue: true, "Use ancient bark as a source of coal in charcoal kiln");
            yggdrasilWoodForCoal = config("Coal", "Yggdrasil wood", defaultValue: false, "Use yggdrasil wood as a source of coal in charcoal kiln");
            ashWoodForCoal = config("Coal", "Ash wood", defaultValue: false, "Use ash wood as a source of coal in charcoal kiln");

            fineWoodForCoal.SettingChanged += (s, e) => CheckSmeltersCoalConversion();
            coreWoodForCoal.SettingChanged += (s, e) => CheckSmeltersCoalConversion();
            ancientBarkForCoal.SettingChanged += (s, e) => CheckSmeltersCoalConversion();
            yggdrasilWoodForCoal.SettingChanged += (s, e) => CheckSmeltersCoalConversion();
            ashWoodForCoal.SettingChanged += (s, e) => CheckSmeltersCoalConversion();
        }

        ConfigEntry<T> config<T>(string group, string name, T defaultValue, ConfigDescription description, bool synchronizedSetting = true)
        {
            ConfigEntry<T> configEntry = Config.Bind(group, name, defaultValue, description);

            SyncedConfigEntry<T> syncedConfigEntry = configSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        ConfigEntry<T> config<T>(string group, string name, T defaultValue, string description, bool synchronizedSetting = true) => config(group, name, defaultValue, new ConfigDescription(description), synchronizedSetting);

        private void OnDestroy() => _harmony?.UnpatchSelf();

        public static void LogInfo(object data)
        {
            if (loggingEnabled.Value)
                instance.Logger.LogInfo(data);
        }
        
        private static bool IsItemsReady()
        {
            if (!modEnabled.Value) 
                return false;

            return wood != null && fineWood != null && coreWood != null && ancientBark != null && yggdrasilWood != null && ashWood != null && coal != null && workbench != null;
        }

        private static void InitializeItems(ObjectDB instance)
        {
            if (instance == null)
                return;

            wood = instance.GetItemPrefab("Wood").GetComponent<ItemDrop>();
            fineWood = instance.GetItemPrefab("FineWood").GetComponent<ItemDrop>();
            coreWood = instance.GetItemPrefab("RoundLog").GetComponent<ItemDrop>();
            ancientBark = instance.GetItemPrefab("ElderBark").GetComponent<ItemDrop>();
            yggdrasilWood = instance.GetItemPrefab("YggdrasilWood").GetComponent<ItemDrop>();
            ashWood = instance.GetItemPrefab("Blackwood").GetComponent<ItemDrop>();
            coal = instance.GetItemPrefab("Coal").GetComponent<ItemDrop>();

            workbench = GetWorkbench(instance);
        }

        private static CraftingStation GetWorkbench(ObjectDB instance)
        {
            foreach (var recipe in instance.m_recipes)
                if (recipe.m_craftingStation != null && (recipe.m_craftingStation.m_name == "$piece_workbench" || recipe.m_craftingStation.name == "piece_workbench"))
                    return recipe.m_craftingStation;

            return null;
        }

        private static void AddRecipes(ObjectDB instance)
        {
            if (instance == null)
                return;

            if (!IsItemsReady())
                return;

            AddTransmuteRecipe(instance, itemUsed: coreWood, itemResult: wood, ratio: coreWoodToWood.Value); // First one will be used by recycling mods
            AddTransmuteRecipe(instance, itemUsed: fineWood, itemResult: wood, ratio: fineWoodToWood.Value);
            AddTransmuteRecipe(instance, itemUsed: ancientBark, itemResult: wood, ratio: ancientBarkToWood.Value);
            AddTransmuteRecipe(instance, itemUsed: yggdrasilWood, itemResult: wood, ratio: yggdrasilWoodToWood.Value);
            AddTransmuteRecipe(instance, itemUsed: ashWood, itemResult: wood, ratio: ashWoodToWood.Value);
        }

        private static void AddTransmuteRecipe(ObjectDB instance, ItemDrop itemUsed, ItemDrop itemResult, Vector3 ratio)
        {
            int amountResult = Mathf.CeilToInt(ratio.y);
            int amountUsed = Mathf.CeilToInt(ratio.x);
            int stationLevel = Mathf.CeilToInt(ratio.z);

            if (amountResult == 0 || amountUsed == 0)
                return;

            GameObject itemPrefab = itemUsed.m_itemData.m_dropPrefab;
            if (itemPrefab == null)
            {
                LogInfo($"m_dropPrefab for {itemUsed.m_itemData.m_shared.m_name} is not set");
                string prefabName = itemUsed.GetPrefabName(itemUsed.gameObject.name);
                itemPrefab = instance.GetItemPrefab(prefabName);
            }

            if (itemPrefab == null)
            {
                LogInfo($"Prefab for {itemUsed.m_itemData.m_shared.m_name} is not found");
                return;
            }

            string recipeName = $"{pluginID.Replace(".", "_")}_Transmute_{itemPrefab.name}_Wood";

            if (instance.m_recipes.RemoveAll(rec => rec.name == recipeName) > 0)
                LogInfo($"Removed recipe {recipeName}");

            Recipe recipe = ScriptableObject.CreateInstance<Recipe>();
            recipe.name = recipeName;
            recipe.m_amount = amountResult;
            
            recipe.m_item = itemResult;
            recipe.m_enabled = true;

            recipe.m_resources = recipe.m_resources.AddToArray(new Piece.Requirement
            {
                m_amount = amountUsed,
                m_resItem = itemUsed
            });

            if (stationLevel > 0)
            {
                recipe.m_minStationLevel = stationLevel;
                recipe.m_craftingStation = workbench;
            }

            instance.m_recipes.Add(recipe);

            LogInfo($"Added {recipeName} transmute recipe {itemUsed.m_itemData.m_shared.m_name} => {itemResult.m_itemData.m_shared.m_name}");
        }

        private static ItemDrop GetReplacementFuelItem(Inventory inventory, ItemDrop builtIn)
        {
            if (builtIn != wood)
                return null;

            if (inventory.HaveItem(builtIn.m_itemData.m_shared.m_name))
            {
                return null;
            }

            if (ancientBarkForFuel.Value && inventory.HaveItem(ancientBark.m_itemData.m_shared.m_name))
            {
                LogInfo($"Used {ancientBark.m_itemData.m_shared.m_name} instead of {builtIn.m_itemData.m_shared.m_name}");
                return ancientBark;
            }

            if (coreWoodForFuel.Value && inventory.HaveItem(coreWood.m_itemData.m_shared.m_name))
            {
                LogInfo($"Used {coreWood.m_itemData.m_shared.m_name} instead of {builtIn.m_itemData.m_shared.m_name}");
                return coreWood;
            }

            if (fineWoodForFuel.Value && inventory.HaveItem(fineWood.m_itemData.m_shared.m_name))
            {
                LogInfo($"Used {fineWood.m_itemData.m_shared.m_name} instead of {builtIn.m_itemData.m_shared.m_name}");
                return fineWood;
            }

            if (yggdrasilWoodForFuel.Value && inventory.HaveItem(yggdrasilWood.m_itemData.m_shared.m_name))
            {
                LogInfo($"Used {yggdrasilWood.m_itemData.m_shared.m_name} instead of {builtIn.m_itemData.m_shared.m_name}");
                return yggdrasilWood;
            }

            if (ashWoodForFuel.Value && inventory.HaveItem(ashWood.m_itemData.m_shared.m_name))
            {
                LogInfo($"Used {ashWood.m_itemData.m_shared.m_name} instead of {builtIn.m_itemData.m_shared.m_name}");
                return ashWood;
            }

            return null;
        }

        private static void CheckCoalConversion(Smelter __instance, ItemDrop item, bool addRecipe = false)
        {
            Smelter.ItemConversion itemConversion = __instance.m_conversion.Find(x => x.m_from == item);
            if (itemConversion == null && !addRecipe || itemConversion != null && addRecipe)
                return;

            if (addRecipe)
            {
                itemConversion = new Smelter.ItemConversion()
                {
                    m_from = item,
                    m_to = coal
                };
                __instance.m_conversion.Add(itemConversion);
                LogInfo($"Added {item.m_itemData.m_shared.m_name} to coal conversion");
            }
            else
            {
                __instance.m_conversion.Remove(itemConversion);
                LogInfo($"Removed {item.m_itemData.m_shared.m_name} from coal conversion");
            }
        }

        private static void CheckSmeltersCoalConversion() => Piece.s_allPieces.Do(piece => CheckCoalConversions(piece?.GetComponent<Smelter>()));

        private static void CheckCoalConversions(Smelter smelter)
        {
            if (smelter == null)
                return;

            CheckCoalConversion(smelter, fineWood, fineWoodForCoal.Value);
            CheckCoalConversion(smelter, coreWood, coreWoodForCoal.Value);
            CheckCoalConversion(smelter, ancientBark, ancientBarkForCoal.Value);
            CheckCoalConversion(smelter, yggdrasilWood, yggdrasilWoodForCoal.Value);
            CheckCoalConversion(smelter, ashWood, ashWoodForCoal.Value);
        }

        [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.CopyOtherDB))]
        private static class ObjectDB_CopyOtherDB_TransmuteRecipes
        {
            private static void Postfix(ObjectDB __instance)
            {
                if (!modEnabled.Value)
                    return;

                if (__instance.GetItemPrefab("Wood") == null)
                    return;

                InitializeItems(__instance);
                AddRecipes(__instance);
            }
        }

        [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
        private static class ObjectDB_Awake_TransmuteRecipes
        {
            private static void Postfix(ObjectDB __instance)
            {
                if (!modEnabled.Value)
                    return;

                if (__instance.GetItemPrefab("Wood") == null) 
                    return;

                InitializeItems(__instance);
                AddRecipes(__instance);
            }
        }

        [HarmonyPatch(typeof(Smelter), nameof(Smelter.Awake))]
        private static class Smelter_Awake_CoalConversionRecipe
        {
            private static void Postfix(Smelter __instance)
            {
                if (!IsItemsReady())
                    return;

                if (__instance.m_conversion.Find(x => x.m_from == wood) == null)
                    return;

                CheckCoalConversions(__instance);
            }
        }

        [HarmonyPatch(typeof(Smelter), nameof(Smelter.OnAddFuel))]
        private static class Smelter_OnAddFuel_FuelReplacement
        {
            [HarmonyPriority(Priority.Last)]
            private static void Prefix(Smelter __instance, Humanoid user, ItemDrop.ItemData item, ref ItemDrop __state)
            {
                if (!IsItemsReady())
                    return;

                if (item != null && item.m_shared.m_name == __instance.m_fuelItem.m_itemData.m_shared.m_name)
                    return;

                ItemDrop itemFuelReplacement = GetReplacementFuelItem(user.GetInventory(), __instance.m_fuelItem);
                if (itemFuelReplacement == null)
                    return;

                __state = __instance.m_fuelItem;

                __instance.m_fuelItem = itemFuelReplacement;
            }

            [HarmonyPriority(Priority.First)]
            private static void Postfix(Smelter __instance, ItemDrop __state)
            {
                if (!IsItemsReady())
                    return;

                if (__state == null)
                    return;

                __instance.m_fuelItem = __state;
            }
        }

        [HarmonyPatch(typeof(Fireplace), nameof(Fireplace.Interact))]
        public static class Fireplace_Interact_FuelReplacement
        {
            [HarmonyPriority(Priority.Last)]
            private static void Prefix(Fireplace __instance, Humanoid user, ref ItemDrop __state)
            {
                if (!IsItemsReady())
                    return;

                if (__instance.m_fuelItem != wood)
                    return;

                Inventory inventory = user.GetInventory();
                if (inventory.HaveItem(__instance.m_fuelItem.m_itemData.m_shared.m_name))
                    return;

                ItemDrop itemFuelReplacement = GetReplacementFuelItem(inventory, __instance.m_fuelItem);
                if (itemFuelReplacement == null)
                    return;

                __state = __instance.m_fuelItem;

                __instance.m_fuelItem = itemFuelReplacement;
            }

            [HarmonyPriority(Priority.First)]
            private static void Postfix(Fireplace __instance, ItemDrop __state)
            {
                if (!IsItemsReady())
                    return;

                if (__state == null)
                    return;

                __instance.m_fuelItem = __state;
            }
        }

        [HarmonyPatch(typeof(CookingStation), nameof(CookingStation.OnAddFuelSwitch))]
        public static class CookingStation_OnAddFuelSwitch_FuelReplacement
        {
            [HarmonyPriority(Priority.Last)]
            private static void Prefix(CookingStation __instance, Humanoid user, ItemDrop.ItemData item, ref ItemDrop __state)
            {
                if (!IsItemsReady())
                    return;

                if (item != null && item.m_shared.m_name == __instance.m_fuelItem.m_itemData.m_shared.m_name)
                    return;

                ItemDrop itemFuelReplacement = GetReplacementFuelItem(user.GetInventory(), __instance.m_fuelItem);
                if (itemFuelReplacement == null)
                    return;

                __state = __instance.m_fuelItem;

                __instance.m_fuelItem = itemFuelReplacement;
            }

            [HarmonyPriority(Priority.First)]
            private static void Postfix(CookingStation __instance, ItemDrop __state)
            {
                if (!IsItemsReady())
                    return;

                if (__state == null)
                    return;

                __instance.m_fuelItem = __state;
            }

        }
    }
}
