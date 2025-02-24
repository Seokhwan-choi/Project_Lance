using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Lance
{
    public partial class Account
    {
        public List<(string, int)> CombineAllEquipment(ItemType type)
        {
            var inventory = GetInventoryByItemType(type);
            if (inventory == null)
                return (null);

            return inventory.CombineAllItem();
        }

        public (string id, int combineCount) CombineEquipment(string id)
        {
            Inventory inventory = GetInventoryByEquipmentId(id);
            if (inventory == null)
                return (id, 0);

            // 데이터가 없는 장비는 합성 불가
            var data = DataUtil.GetEquipmentData(id);
            if (data == null)
                return (id, 0);

            // combineCount가 없다면 합성 진행이 불가능한 것
            if (data.combineCount == 0)
                return (string.Empty, 0);

            // 합성에 필요한 장비 갯수가 충분한지 확인
            EquipmentInst equipItem = inventory.GetEquipItem(id);
            if (equipItem.IsEnoughCount(data.combineCount) == false)
                return (string.Empty, 0);

            return inventory.CombineAndAddItem(id);
        }

        public bool AnyCanCombineEquipment(ItemType type)
        {
            Inventory inventory = GetInventoryByItemType(type);
            if (inventory == null)
                return false;

            foreach(var equipment in inventory.GetEquipItems())
            {
                if (CanCombine(equipment))
                    return true;
            }

            return false;
        }

        public bool CanCombine(string id)
        {
            var data = DataUtil.GetEquipmentData(id);
            if (data == null)
                return false;

            if (data.combineCount == 0)
                return false;

            var equipItem = GetEquipment(id);
            if (equipItem == null)
                return false;

            if (equipItem.IsEnoughCount(data.combineCount) == false)
                return false;

            return true;
        }

        public bool CanCombine(EquipmentInst equipItem)
        {
            var data = DataUtil.GetEquipmentData(equipItem.GetId());
            if (data == null)
                return false;

            // combineCount가 없다면 합성 진행이 불가능한 것
            if (data.combineCount == 0)
                return false;

            if (equipItem.IsEnoughCount(data.combineCount) == false)
                return false;

            return true;
        }

        public bool IsEnoughCount(string id, int count)
        {
            Inventory inventory = GetInventoryByEquipmentId(id);
            if (inventory == null)
                return false;

            EquipmentInst equipItem = inventory.GetEquipItem(id);
            if (equipItem == null)
                return false;

            return equipItem.IsEnoughCount(count);
        }

        public int GetTotalStackedEquipmentsLevelUpCount()
        {
            int totalCount = 0;

            foreach (var inventory in Inventorys)
            {
                totalCount += inventory.GetStackedEquipmentsLevelUpCount();
            }

            return totalCount;
        }

        public int GetTotalStackedEquipmentsCombineCount()
        {
            int totalCount = 0;

            foreach (var inventory in Inventorys)
            {
                totalCount += inventory.GetStackedCombineCount();
            }

            return totalCount;
        }

        public bool EquipEquipment(string id)
        {
            Inventory inventory = GetInventoryByEquipmentId(id);
            if (inventory == null)
                return false;

            return inventory.EquipItem(id);
        }

        public void UnEquipEquipment(string id)
        {
            var inventory = GetInventoryByEquipmentId(id);
            if (inventory == null)
                return;

            inventory.UnEquipItem(id);
        }

        // 추천 장착
        public List<string> EquipRecomandEquipments()
        {
            List<string> results = new List<string>();
            // 어떤 장비를 착용했는지 결과를 가져오자
            foreach(var inventory in Inventorys)
            {
                var result = inventory.EquipRecomandItem();

                results.Add(result);
            }

            return results;
        }

        public void AddEquipment(string id, int count)
        {
            Inventory inventory = GetInventoryByEquipmentId(id);
            if (inventory == null)
                return;

            inventory.AddItem(id, count);
        }

        public int GetEquipmentCount(string id)
        {
            Inventory inventory = GetInventoryByEquipmentId(id);
            if (inventory == null)
                return 0;

            EquipmentInst equipment = inventory.GetEquipItem(id);
            if (equipment == null)
                return 0;

            return equipment.GetCount();
        }

        public IEnumerable<EquipmentInst> GetEquippedEquipments()
        {
            foreach(var inventory in Inventorys)
                yield return inventory.GetEquippedItem();
        }

        public EquipmentInst GetEquippedEquipment(ItemType itemType)
        {
            Inventory inventory = GetInventoryByItemType(itemType);
            if (inventory == null)
                return null;

            return inventory?.GetEquippedItem();
        }

        public EquipmentInst GetEquipment(string id)
        {
            Inventory inventory = GetInventoryByEquipmentId(id);
            if (inventory == null)
                return null;

            return inventory?.GetEquipItem(id);
        }

        public IEnumerable<EquipmentInst> GatherEquipments()
        {
            foreach (var inventory in Inventorys)
                foreach (var equipItem in inventory.GetEquipItems())
                    yield return equipItem;
        }

        public IEnumerable<EquipmentInst> GatherEquipments(ItemType type)
        {
            Inventory inventory = GetInventoryByItemType(type);
            if (inventory == null)
                return null;

            return inventory?.GetEquipItems();
        }

        public int GetEquipmentLevel(string id)
        {
            Inventory inventory = GetInventoryByEquipmentId(id);
            if (inventory == null)
                return 0;

            return inventory?.GetItemLevel(id) ?? 0;
        }

        public int GetEquipmentReforgeStep(string id)
        {
            Inventory inventory = GetInventoryByEquipmentId(id);
            if (inventory == null)
                return 0;

            return inventory?.GetItemReforgeStep(id) ?? 0;
        }

        public int GetEquipmentReforgeFailCount(string id)
        {
            Inventory inventory = GetInventoryByEquipmentId(id);
            if (inventory == null)
                return 0;

            return inventory?.GetItemReforgeFailedCount(id) ?? 0;
        }

        Inventory GetInventoryByItemType(ItemType type)
        {
            switch (type)
            {
                case ItemType.Weapon:
                    return WeaponInventory;
                case ItemType.Armor:
                    return ArmorInventory;
                case ItemType.Gloves:
                    return GlovesInventory;
                case ItemType.Shoes:
                default:
                    return ShoesInventory;
            }
        }

        Inventory GetInventoryByEquipmentId(string id)
        {
            EquipmentData data = DataUtil.GetEquipmentData(id);
            if (data == null)
                return null;

            return GetInventoryByItemType(data.type);
        }

        public bool IsEquippedEquipment(string id)
        {
            var inventory = GetInventoryByEquipmentId(id);
            if (inventory == null)
                return false;

            return inventory.IsEquipped(id);
        }

        public bool IsMaxLevelEquipment(string id)
        {
            var inventory = GetInventoryByEquipmentId(id);
            if (inventory == null)
                return false;

            return inventory.IsMaxLevel(id);
        }

        public bool IsMaxReforgeStepEquipment(string id)
        {
            var inventory = GetInventoryByEquipmentId(id);
            if (inventory == null)
                return false;

            return inventory.IsMaxReforge(id);
        }

        public bool HaveEquipment(string id)
        {
            foreach (var inventory in Inventorys)
            {
                if (inventory.HaveItem(id))
                    return true;
            }

            return false;
        }

        public double GetEquipmentUpgradeRequireStones(string id, int upgradeCount)
        {
            // 인벤토리 접근
            Inventory inventory = GetInventoryByEquipmentId(id);
            if (inventory == null)
                return double.MaxValue;

            return DataUtil.GetEquipmentUpgradeRequireStone(DataUtil.GetEquipmentData(id), inventory.GetItemLevel(id), upgradeCount);
        }

        public double GetEquipmentReforgeRequireStones(string id)
        {
            // 인벤토리 접근
            Inventory inventory = GetInventoryByEquipmentId(id);
            if (inventory == null)
                return double.MaxValue;

            var data = DataUtil.GetEquipmentData(id);
            if (data == null)
                return double.MaxValue;

            return DataUtil.GetEquipmentReforgeRequireStone(data.grade, inventory.GetItemReforgeStep(id));
        }

        //public bool AnyCanUpgradeEquipment(ItemType itemType)
        //{
        //    var inventory = GetInventoryByItemType(itemType);
        //    if (inventory == null)
        //        return false;

        //    foreach(var equipItem in inventory.GetEquipItems())
        //    {
        //        if (CanUpgradeEquipment(equipItem))
        //            return true;
        //    }

        //    return false;
        //}

        public bool CanUpgradeEquipment(string id, int upgradeCount)
        {   
            // 인벤토리 접근
            EquipmentInst equipItem = GetEquipment(id);
            if (equipItem == null)
                return false;

            return CanUpgradeEquipment(equipItem, upgradeCount);
        }

        public bool CanUpgradeEquipment(EquipmentInst equipItem, int upgradeCount)
        {
            if (upgradeCount <= 0)
                return false;

            if (equipItem.GetLevel() + upgradeCount > equipItem.GetMaxLevel())
                return false;

            double requireStones = equipItem.GetUpgradeRequireStones(upgradeCount);
            if (IsEnoughUpgradeStones(requireStones) == false)
                return false;

            return true;
        }

        public int GetEquipmentMaxLevelRequireCount(string id)
        {
            // 인벤토리 접근
            EquipmentInst equipItem = GetEquipment(id);
            if (equipItem == null)
                return 0;

            return equipItem.GetMaxLevelRequierCount();
        }

        public bool CanReforgeEquipment(string id)
        {
            // 인벤토리 접근
            EquipmentInst equipItem = GetEquipment(id);
            if (equipItem == null)
                return false;

            return CanReforgeEquipment(equipItem);
        }

        public bool CanReforgeEquipment(EquipmentInst equipItem)
        {
            if (equipItem.IsMaxLevel() == false)
                return false;

            if (equipItem.IsMaxReforge())
                return false;

            double requireStones = equipItem.GetReforgeRequireStone();
            if (IsEnoughReforgeStones(requireStones) == false)
                return false;

            return true;
        }


        // -1 실행 불가
        // 0 실패
        // 1 성공
        public int ReforgeEquipment(string id)
        {
            var inventory = GetInventoryByEquipmentId(id);
            if (inventory == null)
                return -1;

            var equipData = DataUtil.GetEquipmentData(id);
            if (equipData == null)
                return -1;

            // 인벤토리에 있는 장비만 강화 가능
            EquipmentInst equipItem = GetEquipment(id);
            if (equipItem == null)
                return -1;

            if (equipItem.IsMaxLevel() == false)
                return -1;

            if (equipItem.IsMaxReforge())
                return -1;

            double requireStones = equipItem.GetReforgeRequireStone();
            if (UseReforgeStones(requireStones))
            {
                equipItem.TryReforge();

                int reforgeStep = equipItem.GetReforgeStep();
                int failCount = equipItem.GetReforgeFailedCount();
                // 장비 재련시도
                float reforgeProb = DataUtil.GetEquipmentReforgeProb(equipData.grade, reforgeStep);
                float bonusProb = DataUtil.GetEquipmentReforgeFailBonusProb(equipData.grade, reforgeStep, failCount);
                if (Util.Dice(reforgeProb + bonusProb))
                {
                    equipItem.Reforge();

                    inventory.SetIsChangedData(true);

                    return 1;
                }
                else
                {
                    inventory.SetIsChangedData(true);

                    equipItem.StackFailReforge();

                    return 0;
                }
            }
            else
            {
                return -1;
            }
        }

        public void UpgradeEquipment(string id, int upgradeCount)
        {
            var inventory = GetInventoryByEquipmentId(id);
            if (inventory == null)
                return;

            // 인벤토리에 있는 장비만 강화 가능
            EquipmentInst equipItem = GetEquipment(id);
            if (equipItem == null || equipItem.IsMaxLevel())
                return;

            if (equipItem.GetMaxLevel() < equipItem.GetLevel() + upgradeCount)
                return;

            double requireStones = equipItem.GetUpgradeRequireStones(upgradeCount);
            if (IsEnoughUpgradeStones(requireStones) == false)
                return;

            // 장비 레벨업
            equipItem.LevelUp(upgradeCount);

            // 강화석 사용
            UseUpgradeStones(requireStones);

            inventory.SetIsChangedData(true);
        }

        double GatherInventoryStatValues(StatType statType)
        {
            double totalStatValue = 0;

            foreach (var inventory in Inventorys)
            {
                totalStatValue += inventory.GatherStatValues(statType);
            }

            return totalStatValue;
        }

        public double GetEquipmentStatValue(string id, StatType statType, bool ignoreEquipped = false)
        {
            var equipment = GetEquipment(id);
            if (equipment == null)
                return 0;

            double totalStatValue = 0;

            totalStatValue += equipment.GetStatValues(statType, equipment.IsEquipped() || ignoreEquipped);
            totalStatValue += equipment.GetOwnStatValues(statType);

            return totalStatValue;
        }

        public EquipmentOptionStat GetEquipmentOptionStat(string id, int slot)
        {
            var equipmentInst = GetEquipment(id);
            if (equipmentInst == null)
                return null;

            return equipmentInst.GetOptionStat(slot);
        }

        public int GetEquipmentOptionStatLockCount(string id)
        {
            var equipmentInst = GetEquipment(id);
            if (equipmentInst == null)
                return 0;

            return equipmentInst.GetOptionStatLockCount();
        }

        public bool IsActiveEquipmentOptionStat(string id, int slot)
        {
            var equipmentInst = GetEquipment(id);
            if (equipmentInst == null)
                return false;

            return equipmentInst.IsActiveOptionStat(slot);
        }

        public double GetEquipmentOptionChangePrice(string id)
        {
            var equipmentInst = GetEquipment(id);
            if (equipmentInst == null)
                return 0;

            return equipmentInst.GetOptionChangePrice();
        }

        public bool IsAllEquipmentOptionStatLocked(string id)
        {
            var equipmentInst = GetEquipment(id);
            if (equipmentInst == null)
                return false;

            return equipmentInst.IsAllOptionStatLocked();
        }

        public bool ChangeEquipmentOptionStats(string id)
        {
            var equipmentInst = GetEquipment(id);
            if (equipmentInst == null)
                return false;

            return equipmentInst.ChangeOptionStats();
        }

        public int GetEquipmentCurrentPreset(string id)
        {
            var equipmentInst = GetEquipment(id);
            if (equipmentInst == null)
                return int.MaxValue;

            return equipmentInst.GetCurrentPreset();
        }

        public void ChangeEquipmentOptionPreset(string id, int preset)
        {
            Inventory inventory = GetInventoryByEquipmentId(id);
            if (inventory == null)
                return ;

            var equipmentInst = inventory.GetEquipItem(id);
            if (equipmentInst == null)
                return ;

            equipmentInst.ChangeOptionStatPreset(preset);

            inventory.SetIsChangedData(true);
        }

        public bool ChangeEquipmentOptionStat(string id)
        {
            Inventory inventory = GetInventoryByEquipmentId(id);
            if (inventory == null)
                return false;

            var equipmentInst = inventory.GetEquipItem(id);
            if (equipmentInst == null)
                return false;

            int statLockCount = GetEquipmentOptionStatLockCount(id);
            if (Lance.GameData.EquipmentCommonData.optionStatMaxCount == statLockCount)
                return false;

            double changePrice = equipmentInst.GetOptionChangePrice();
            if (Currency.UseReforgeStone(changePrice))
            {
                if (equipmentInst.ChangeOptionStats())
                {
                    inventory.SetIsChangedData(true);

                    return true;
                }
            }

            return false;
        }

        public bool AnyCanChangeOptionStat(string id)
        {
            var equipment = GetEquipment(id);
            if (equipment == null)
                return false;

            return equipment.AnyCanChangeOptionStat();
        }
    }
}
