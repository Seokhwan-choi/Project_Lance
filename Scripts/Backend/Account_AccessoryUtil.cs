using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Lance
{
    public partial class Account
    {
        public List<(string, int)> CombineAllAccessory(ItemType type)
        {
            var inventory = GetAccessoryInventoryByItemType(type);
            if (inventory == null)
                return (null);

            return inventory.CombineAllItem();
        }

        public (string id, int combineCount) CombineAccessory(string id)
        {
            var inventory = GetAccessoryInventoryByAccessoryId(id);
            if (inventory == null)
                return (id, 0);

            // 데이터가 없는 장비는 합성 불가
            var data = DataUtil.GetAccessoryData(id);
            if (data == null)
                return (id, 0);

            // combineCount가 없다면 합성 진행이 불가능한 것
            if (data.combineCount == 0)
                return (string.Empty, 0);

            // 합성에 필요한 장비 갯수가 충분한지 확인
            AccessoryInst accessoryItem = inventory.GetAccessoryItem(id);
            if (accessoryItem.IsEnoughCount(data.combineCount) == false)
                return (string.Empty, 0);

            return inventory.CombineAndAddItem(id);
        }

        public bool AnyCanAccessoryCombine(ItemType type)
        {
            AccessoryInventory inventory = GetAccessoryInventoryByItemType(type);
            if (inventory == null)
                return false;

            foreach (var accessory in inventory.GetAccessoryItems())
            {
                if (CanAccessoryCombine(accessory))
                    return true;
            }

            return false;
        }

        public bool CanAccessoryCombine(string id)
        {
            var data = DataUtil.GetAccessoryData(id);
            if (data == null)
                return false;

            if (data.combineCount == 0)
                return false;

            var accessoryItem = GetAccessory(id);
            if (accessoryItem == null)
                return false;

            if (accessoryItem.IsEnoughCount(data.combineCount) == false)
                return false;

            return true;
        }

        public bool CanAccessoryCombine(AccessoryInst accessoryItem)
        {
            var data = DataUtil.GetAccessoryData(accessoryItem.GetId());
            if (data == null)
                return false;

            // combineCount가 없다면 합성 진행이 불가능한 것
            if (data.combineCount == 0)
                return false;

            if (accessoryItem.IsEnoughCount(data.combineCount) == false)
                return false;

            return true;
        }

        public bool IsEnoughAccessoryCount(string id, int count)
        {
            AccessoryInventory inventory = GetAccessoryInventoryByAccessoryId(id);
            if (inventory == null)
                return false;

            AccessoryInst accessoryItem = inventory.GetAccessoryItem(id);
            if (accessoryItem == null)
                return false;

            return accessoryItem.IsEnoughCount(count);
        }

        public int GetTotalStackedAccessorysLevelUpCount()
        {
            int totalCount = 0;

            foreach (var inventory in AccessoryInventorys)
            {
                totalCount += inventory.GetStackedAccessorysLevelUpCount();
            }

            return totalCount;
        }

        public int GetTotalStackedAccessorysCombineCount()
        {
            int totalCount = 0;

            foreach (var inventory in AccessoryInventorys)
            {
                totalCount += inventory.GetStackedCombineCount();
            }

            return totalCount;
        }

        public bool EquipAccessory(string id)
        {
            AccessoryInventory inventory = GetAccessoryInventoryByAccessoryId(id);
            if (inventory == null)
                return false;

            return inventory.EquipItem(id);
        }

        public void UnEquipAccessory(string id)
        {
            var inventory = GetAccessoryInventoryByAccessoryId(id);
            if (inventory == null)
                return;

            inventory.UnEquipItem(id);
        }

        public void AllUnEquipAccssory()
        {
            // 어떤 장비를 착용했는지 결과를 가져오자
            foreach (var inventory in AccessoryInventorys)
            {
                inventory.AllUnEquip();
            }
        }

        // 추천 장착
        public List<string> EquipRecomandAccessorys()
        {
            List<string> results = new List<string>();
            // 어떤 장비를 착용했는지 결과를 가져오자
            foreach (var inventory in AccessoryInventorys)
            {
                var result = inventory.EquipRecomandItem();

                results.AddRange(result);
            }

            return results;
        }

        public void AddAccessory(string id, int count)
        {
            AccessoryInventory inventory = GetAccessoryInventoryByAccessoryId(id);
            if (inventory == null)
                return;

            inventory.AddItem(id, count);
        }

        public int GetAccessoryCount(string id)
        {
            AccessoryInventory inventory = GetAccessoryInventoryByAccessoryId(id);
            if (inventory == null)
                return 0;

            AccessoryInst accessory = inventory.GetAccessoryItem(id);
            if (accessory == null)
                return 0;

            return accessory.GetCount();
        }

        public List<AccessoryInst> GetEquippedAccessorys()
        {
            List<AccessoryInst> list = new List<AccessoryInst>();

            foreach (var inventory in AccessoryInventorys)
            {
                list.AddRange(inventory.GetEquippedItems());
            }

            return list;
        }

        public string GetEquippedAccessoryId(ItemType type, int slot)
        {
            AccessoryInventory inventory = GetAccessoryInventoryByItemType(type);
            if (inventory == null)
                return null;

            return inventory?.GetEquippedItemId(slot);
        }

        public AccessoryInst GetAccessory(string id)
        {
            AccessoryInventory inventory = GetAccessoryInventoryByAccessoryId(id);
            if (inventory == null)
                return null;

            return inventory?.GetAccessoryItem(id);
        }

        public IEnumerable<AccessoryInst> GatherAccessorys(ItemType type)
        {
            AccessoryInventory inventory = GetAccessoryInventoryByItemType(type);
            if (inventory == null)
                return null;

            return inventory?.GetAccessoryItems();
        }

        public int GetAccessoryLevel(string id)
        {
            AccessoryInventory inventory = GetAccessoryInventoryByAccessoryId(id);
            if (inventory == null)
                return 0;

            return inventory?.GetItemLevel(id) ?? 0;
        }

        public int GetAccessoryReforgeStep(string id)
        {
            AccessoryInventory inventory = GetAccessoryInventoryByAccessoryId(id);
            if (inventory == null)
                return 0;

            return inventory?.GetItemReforgeStep(id) ?? 0;
        }

        public int GetAccessoryReforgeFailCount(string id)
        {
            AccessoryInventory inventory = GetAccessoryInventoryByAccessoryId(id);
            if (inventory == null)
                return 0;

            return inventory?.GetItemReforgeFailedCount(id) ?? 0;
        }

        AccessoryInventory GetAccessoryInventoryByItemType(ItemType itemType)
        {
            switch (itemType)
            {
                case ItemType.Necklace:
                    return NecklaceInventory;
                case ItemType.Earring:
                    return EarringInventory;
                case ItemType.Ring:
                default:
                    return RingInventory;
            }
        }

        AccessoryInventory GetAccessoryInventoryByAccessoryId(string id)
        {
            AccessoryData data = DataUtil.GetAccessoryData(id);
            if (data == null)
                return null;

            return GetAccessoryInventoryByItemType(data.type);
        }

        public bool IsEquippedAccessory(string id)
        {
            var inventory = GetAccessoryInventoryByAccessoryId(id);
            if (inventory == null)
                return false;

            return inventory.IsEquipped(id);
        }

        public bool IsMaxLevelAccessory(string id)
        {
            var inventory = GetAccessoryInventoryByAccessoryId(id);
            if (inventory == null)
                return false;

            return inventory.IsMaxLevel(id);
        }

        public bool IsMaxReforgeStepAccessory(string id)
        {
            var inventory = GetAccessoryInventoryByAccessoryId(id);
            if (inventory == null)
                return false;

            return inventory.IsMaxReforge(id);
        }

        public bool HaveAccessory(string id)
        {
            foreach (var inventory in AccessoryInventorys)
            {
                if (inventory.HaveItem(id))
                    return true;
            }

            return false;
        }

        public double GetAccessoryUpgradeRequireStones(string id, int upgradeCount)
        {
            // 인벤토리 접근
            AccessoryInventory inventory = GetAccessoryInventoryByAccessoryId(id);
            if (inventory == null)
                return double.MaxValue;

            return DataUtil.GetAccessoryUpgradeRequireStone(DataUtil.GetAccessoryData(id), inventory.GetItemLevel(id), upgradeCount);
        }

        public double GetAccessoryReforgeRequireStones(string id)
        {
            // 인벤토리 접근
            AccessoryInventory inventory = GetAccessoryInventoryByAccessoryId(id);
            if (inventory == null)
                return double.MaxValue;

            var data = DataUtil.GetAccessoryData(id);
            if (data == null)
                return double.MaxValue;

            return DataUtil.GetAccessoryReforgeRequireStone(data.grade, inventory.GetItemReforgeStep(id));
        }

        public bool CanUpgradeAccessory(string id, int upgradeCount)
        {
            // 인벤토리 접근
            AccessoryInst accessoryItem = GetAccessory(id);
            if (accessoryItem == null)
                return false;

            return CanUpgradeAccessory(accessoryItem, upgradeCount);
        }

        public bool CanUpgradeAccessory(AccessoryInst accessoryItem, int upgradeCount)
        {
            if (upgradeCount <= 0)
                return false;

            if (accessoryItem.GetLevel() + upgradeCount > accessoryItem.GetMaxLevel())
                return false;

            double requireStones = accessoryItem.GetUpgradeRequireStones(upgradeCount);
            if (IsEnoughUpgradeStones(requireStones) == false)
                return false;

            return true;
        }

        public int GetAccessoryMaxLevelRequireCount(string id)
        {
            // 인벤토리 접근
            AccessoryInst accessoryItem = GetAccessory(id);
            if (accessoryItem == null)
                return 0;

            return accessoryItem.GetMaxLevelRequierCount();
        }

        public bool CanReforgeAccessory(string id)
        {
            // 인벤토리 접근
            AccessoryInst accessoryItem = GetAccessory(id);
            if (accessoryItem == null)
                return false;

            return CanReforgeAccessory(accessoryItem);
        }

        public bool CanReforgeAccessory(AccessoryInst accessoryItem)
        {
            if (accessoryItem.IsMaxLevel() == false)
                return false;

            if (accessoryItem.IsMaxReforge())
                return false;

            double requireStones = accessoryItem.GetReforgeRequireStone();
            if (IsEnoughReforgeStones(requireStones) == false)
                return false;

            return true;
        }


        // -1 실행 불가
        // 0 실패
        // 1 성공
        public int ReforgeAccessory(string id)
        {
            var inventory = GetAccessoryInventoryByAccessoryId(id);
            if (inventory == null)
                return -1;

            var accessoryData = DataUtil.GetAccessoryData(id);
            if (accessoryData == null)
                return -1;

            // 인벤토리에 있는 장비만 강화 가능
            AccessoryInst accessoryItem = GetAccessory(id);
            if (accessoryItem == null)
                return -1;

            if (accessoryItem.IsMaxLevel() == false)
                return -1;

            if (accessoryItem.IsMaxReforge())
                return -1;

            double requireStones = accessoryItem.GetReforgeRequireStone();
            if (IsEnoughReforgeStones(requireStones) == false)
                return -1;

            if (UseReforgeStones(requireStones))
            {
                accessoryItem.TryReforge();

                int reforgeStep = accessoryItem.GetReforgeStep();
                int failCount = accessoryItem.GetReforgeFailedCount();
                // 장비 재련시도
                float reforgeProb = DataUtil.GetAccessoryReforgeProb(accessoryData.grade, reforgeStep);
                float bonusProb = DataUtil.GetAccessoryReforgeFailBonusProb(accessoryData.grade, reforgeStep, failCount);
                if (Util.Dice(reforgeProb + bonusProb))
                {
                    accessoryItem.Reforge();

                    inventory.SetIsChangedData(true);

                    return 1;
                }
                else
                {
                    inventory.SetIsChangedData(true);

                    accessoryItem.StackFailReforge();

                    return 0;
                }
            }
            else
            {
                return -1;
            }
        }

        public void UpgradeAccessory(string id, int upgradeCount)
        {
            var inventory = GetAccessoryInventoryByAccessoryId(id);
            if (inventory == null)
                return;

            // 인벤토리에 있는 장비만 강화 가능
            AccessoryInst accessoryItem = GetAccessory(id);
            if (accessoryItem == null || accessoryItem.IsMaxLevel())
                return;

            if (accessoryItem.GetMaxLevel() < accessoryItem.GetLevel() + upgradeCount)
                return;

            double requireStones = accessoryItem.GetUpgradeRequireStones(upgradeCount);
            if (IsEnoughUpgradeStones(requireStones) == false)
                return;

            // 장비 레벨업
            accessoryItem.LevelUp(upgradeCount);

            // 강화석 사용
            UseUpgradeStones(requireStones);

            inventory.SetIsChangedData(true);
        }

        double GatherAccessoryInventoryStatValues(StatType statType)
        {
            double totalStatValue = 0;

            foreach (var inventory in AccessoryInventorys)
            {
                totalStatValue += inventory.GatherStatValues(statType);
            }

            return totalStatValue;
        }

        public double GetAccessoryStatValue(string id, StatType statType, bool ignoreEquipped = false)
        {
            var accessory = GetAccessory(id);
            if (accessory == null)
                return 0;

            double totalStatValue = 0;

            if (accessory.IsEquipped() || ignoreEquipped)
            {
                totalStatValue += accessory.GetEquippedStatValues(statType);
            }

            totalStatValue += accessory.GetOwnStatValues(statType);

            return totalStatValue;
        }
    }
}
