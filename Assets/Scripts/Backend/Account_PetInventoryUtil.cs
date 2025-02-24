using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Lance
{
    public partial class Account
    {
        public List<(string, int)> CombineAllPetEquipment(ElementalType type)
        {
            var inventory = GetPetInventoryByElementalType(type);
            if (inventory == null)
                return (null);

            return inventory.CombineAllItem();
        }

        public (string id, int combineCount) CombinePetEquipment(string id)
        {
            var inventory = GetPetInventoryByEquipmentId(id);
            if (inventory == null)
                return (id, 0);

            // 데이터가 없는 장비는 합성 불가
            var data = Lance.GameData.PetEquipmentData.TryGet(id);
            if (data == null)
                return (id, 0);

            // combineCount가 없다면 합성 진행이 불가능한 것
            if (data.combineCount == 0)
                return (string.Empty, 0);

            // 합성에 필요한 장비 갯수가 충분한지 확인
            PetEquipmentInst equipItem = inventory.GetEquipItem(id);
            if (equipItem.IsEnoughCount(data.combineCount) == false)
                return (string.Empty, 0);

            return inventory.CombineAndAddItem(id);
        }

        public bool AnyCanPetEquipmentCombine(ElementalType type)
        {
            PetInventory inventory = GetPetInventoryByElementalType(type);
            if (inventory == null)
                return false;

            foreach (var equipment in inventory.GetEquipItems())
            {
                if (CanPetEquipmentCombine(equipment))
                    return true;
            }

            return false;
        }

        public bool CanPetEquipmentCombine(string id)
        {
            var data = Lance.GameData.PetEquipmentData.TryGet(id);
            if (data == null)
                return false;

            if (data.combineCount == 0)
                return false;

            var equipItem = GetPetEquipment(id);
            if (equipItem == null)
                return false;

            if (equipItem.IsEnoughCount(data.combineCount) == false)
                return false;

            return true;
        }

        public bool CanPetEquipmentCombine(PetEquipmentInst equipItem)
        {
            var data = Lance.GameData.PetEquipmentData.TryGet(equipItem.GetId());
            if (data == null)
                return false;

            // combineCount가 없다면 합성 진행이 불가능한 것
            if (data.combineCount == 0)
                return false;

            if (equipItem.IsEnoughCount(data.combineCount) == false)
                return false;

            return true;
        }

        public bool IsEnoughPetEquipmentCount(string id, int count)
        {
            PetInventory inventory = GetPetInventoryByEquipmentId(id);
            if (inventory == null)
                return false;

            PetEquipmentInst equipItem = inventory.GetEquipItem(id);
            if (equipItem == null)
                return false;

            return equipItem.IsEnoughCount(count);
        }

        public int GetTotalStackedPetEquipmentsLevelUpCount()
        {
            int totalCount = 0;

            foreach (var inventory in PetInventorys)
            {
                totalCount += inventory.GetStackedEquipmentsLevelUpCount();
            }

            return totalCount;
        }

        public int GetTotalStackedPetEquipmentsCombineCount()
        {
            int totalCount = 0;

            foreach (var inventory in PetInventorys)
            {
                totalCount += inventory.GetStackedCombineCount();
            }

            return totalCount;
        }

        public bool EquipPetEquipment(string id)
        {
            PetInventory inventory = GetPetInventoryByEquipmentId(id);
            if (inventory == null)
                return false;

            return inventory.EquipItem(id);
        }

        public void UnEquipPetEquipment(string id)
        {
            var inventory = GetPetInventoryByEquipmentId(id);
            if (inventory == null)
                return;

            inventory.UnEquipItem(id);
        }

        // 추천 장착
        public List<string> EquipRecomandPetEquipments()
        {
            List<string> results = new List<string>();
            // 어떤 장비를 착용했는지 결과를 가져오자
            foreach (var inventory in PetInventorys)
            {
                var result = inventory.EquipRecomandItem();

                results.Add(result);
            }

            return results;
        }

        public void AddPetEquipment(string id, int count)
        {
            PetInventory inventory = GetPetInventoryByEquipmentId(id);
            if (inventory == null)
                return;

            inventory.AddItem(id, count);
        }

        public int GetPetEquipmentCount(string id)
        {
            PetInventory inventory = GetPetInventoryByEquipmentId(id);
            if (inventory == null)
                return 0;

            PetEquipmentInst equipment = inventory.GetEquipItem(id);
            if (equipment == null)
                return 0;

            return equipment.GetCount();
        }

        public PetEquipmentInst GetEquippedPetEquipment(ElementalType type)
        {
            PetInventory inventory = GetPetInventoryByElementalType(type);
            if (inventory == null)
                return null;

            return inventory?.GetEquippedItem();
        }

        public PetEquipmentInst GetPetEquipment(string id)
        {
            PetInventory inventory = GetPetInventoryByEquipmentId(id);
            if (inventory == null)
                return null;

            return inventory?.GetEquipItem(id);
        }

        public IEnumerable<PetEquipmentInst> GatherPetEquipments()
        {
            foreach (var inventory in PetInventorys)
                yield return inventory.GetEquippedItem();
        }

        public IEnumerable<PetEquipmentInst> GatherPetEquipments(ElementalType type)
        {
            PetInventory inventory = GetPetInventoryByElementalType(type);
            if (inventory == null)
                return null;

            return inventory?.GetEquipItems();
        }

        public int GetPetEquipmentLevel(string id)
        {
            PetInventory inventory = GetPetInventoryByEquipmentId(id);
            if (inventory == null)
                return 0;

            return inventory?.GetItemLevel(id) ?? 0;
        }

        public int GetPetEquipmentReforgeStep(string id)
        {
            PetInventory inventory = GetPetInventoryByEquipmentId(id);
            if (inventory == null)
                return 0;

            return inventory?.GetItemReforgeStep(id) ?? 0;
        }

        public int GetPetEquipmentReforgeFailCount(string id)
        {
            PetInventory inventory = GetPetInventoryByEquipmentId(id);
            if (inventory == null)
                return 0;

            return inventory?.GetItemReforgeFailedCount(id) ?? 0;
        }

        PetInventory GetPetInventoryByElementalType(ElementalType elementalType)
        {
            switch (elementalType)
            {
                case ElementalType.Fire:
                    return PetFireInventory;
                case ElementalType.Water:
                    return PetWaterInventory;
                case ElementalType.Grass:
                default:
                    return PetGrassInventory;
            }
        }

        PetInventory GetPetInventoryByEquipmentId(string id)
        {
            PetEquipmentData data = Lance.GameData.PetEquipmentData.TryGet(id);
            if (data == null)
                return null;

            return GetPetInventoryByElementalType(data.type);
        }

        public bool IsEquippedPetEquipment(string id)
        {
            var inventory = GetPetInventoryByEquipmentId(id);
            if (inventory == null)
                return false;

            return inventory.IsEquipped(id);
        }

        public bool IsMaxLevelPetEquipment(string id)
        {
            var inventory = GetPetInventoryByEquipmentId(id);
            if (inventory == null)
                return false;

            return inventory.IsMaxLevel(id);
        }

        public bool IsMaxReforgeStepPetEquipment(string id)
        {
            var inventory = GetPetInventoryByEquipmentId(id);
            if (inventory == null)
                return false;

            return inventory.IsMaxReforge(id);
        }

        public bool HavePetEquipment(string id)
        {
            foreach (var inventory in PetInventorys)
            {
                if (inventory.HaveItem(id))
                    return true;
            }

            return false;
        }

        public double GetPetEquipmentUpgradeRequireStones(string id, int upgradeCount)
        {
            // 인벤토리 접근
            PetInventory inventory = GetPetInventoryByEquipmentId(id);
            if (inventory == null)
                return double.MaxValue;

            return DataUtil.GetPetEquipmentUpgradeRequireStone(Lance.GameData.PetEquipmentData.TryGet(id), inventory.GetItemLevel(id), upgradeCount);
        }

        public double GetPetEquipmentReforgeRequireStones(string id)
        {
            // 인벤토리 접근
            PetInventory inventory = GetPetInventoryByEquipmentId(id);
            if (inventory == null)
                return double.MaxValue;

            var data = Lance.GameData.PetEquipmentData.TryGet(id);
            if (data == null)
                return double.MaxValue;

            return DataUtil.GetPetEquipmentReforgeRequireStone(data.grade, inventory.GetItemReforgeStep(id));
        }

        public int GetPetEquipmentReforgeRequireElementalStones(string id)
        {
            // 인벤토리 접근
            PetInventory inventory = GetPetInventoryByEquipmentId(id);
            if (inventory == null)
                return int.MaxValue;

            var data = Lance.GameData.PetEquipmentData.TryGet(id);
            if (data == null)
                return int.MaxValue;

            return DataUtil.GetPetEquipmentReforgeRequireElementalStone(data.grade, inventory.GetItemReforgeStep(id));
        }

        public bool CanUpgradePetEquipment(string id, int upgradeCount)
        {
            // 인벤토리 접근
            PetEquipmentInst equipItem = GetPetEquipment(id);
            if (equipItem == null)
                return false;

            return CanUpgradePetEquipment(equipItem, upgradeCount);
        }

        public bool CanUpgradePetEquipment(PetEquipmentInst equipItem, int upgradeCount)
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

        public int GetPetEquipmentMaxLevelRequireCount(string id)
        {
            // 인벤토리 접근
            PetEquipmentInst equipItem = GetPetEquipment(id);
            if (equipItem == null)
                return 0;

            return equipItem.GetMaxLevelRequierCount();
        }

        public bool CanReforgePetEquipment(string id)
        {
            // 인벤토리 접근
            PetEquipmentInst equipItem = GetPetEquipment(id);
            if (equipItem == null)
                return false;

            return CanReforgePetEquipment(equipItem);
        }

        public bool CanReforgePetEquipment(PetEquipmentInst equipItem)
        {
            if (equipItem.IsMaxLevel() == false)
                return false;

            if (equipItem.IsMaxReforge())
                return false;

            double requireStones = equipItem.GetReforgeRequireStone();
            if (IsEnoughReforgeStones(requireStones) == false)
                return false;

            int requireElementalStones = equipItem.GetReforgeRequireElementalStone();
            if (IsEnoughElementalStones(requireElementalStones) == false)
                return false;

            return true;
        }


        // -1 실행 불가
        // 0 실패
        // 1 성공
        public int ReforgePetEquipment(string id)
        {
            var inventory = GetPetInventoryByEquipmentId(id);
            if (inventory == null)
                return -1;

            var equipData = Lance.GameData.PetEquipmentData.TryGet(id);
            if (equipData == null)
                return -1;

            // 인벤토리에 있는 장비만 강화 가능
            PetEquipmentInst equipItem = GetPetEquipment(id);
            if (equipItem == null)
                return -1;

            if (equipItem.IsMaxLevel() == false)
                return -1;

            if (equipItem.IsMaxReforge())
                return -1;

            double requireStones = equipItem.GetReforgeRequireStone();
            if (IsEnoughReforgeStones(requireStones) == false)
                return -1;

            int requireElementalStones = equipItem.GetReforgeRequireElementalStone();
            if (IsEnoughElementalStones(requireElementalStones) == false)
                return -1;
            
            if (UseReforgeStones(requireStones) && UseElementalStones(requireElementalStones))
            {
                equipItem.TryReforge();

                int reforgeStep = equipItem.GetReforgeStep();
                int failCount = equipItem.GetReforgeFailedCount();
                // 장비 재련시도
                float reforgeProb = DataUtil.GetPetEquipmentReforgeProb(equipData.grade, reforgeStep);
                float bonusProb = DataUtil.GetPetEquipmentReforgeFailBonusProb(equipData.grade, reforgeStep, failCount);
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

        public void UpgradePetEquipment(string id, int upgradeCount)
        {
            var inventory = GetPetInventoryByEquipmentId(id);
            if (inventory == null)
                return;

            // 인벤토리에 있는 장비만 강화 가능
            PetEquipmentInst equipItem = GetPetEquipment(id);
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

        double GatherPetInventoryStatValues(StatType statType)
        {
            double totalStatValue = 0;

            foreach (var inventory in PetInventorys)
            {
                totalStatValue += inventory.GatherStatValues(statType);
            }

            return totalStatValue;
        }

        public double GetPetEquipmentStatValue(string id, StatType statType, bool ignoreEquipped = false)
        {
            var equipment = GetPetEquipment(id);
            if (equipment == null)
                return 0;

            double totalStatValue = 0;

            if (equipment.IsEquipped() || ignoreEquipped)
            {
                totalStatValue += equipment.GetEquippedStatValues(statType);
            }

            totalStatValue += equipment.GetOwnStatValues(statType);

            return totalStatValue;
        }

        public EquipmentOptionStat GetPetEquipmentOptionStat(string id, int slot)
        {
            var equipmentInst = GetPetEquipment(id);
            if (equipmentInst == null)
                return null;

            return equipmentInst.GetOptionStat(slot);
        }

        public int GetPetEquipmentOptionStatLockCount(string id)
        {
            var equipmentInst = GetPetEquipment(id);
            if (equipmentInst == null)
                return 0;

            return equipmentInst.GetOptionStatLockCount();
        }

        public bool IsActivePetEquipmentOptionStat(string id, int slot)
        {
            var equipmentInst = GetPetEquipment(id);
            if (equipmentInst == null)
                return false;

            return equipmentInst.IsActiveOptionStat(slot);
        }

        public double GetPetEquipmentOptionChangePrice(string id)
        {
            var equipmentInst = GetPetEquipment(id);
            if (equipmentInst == null)
                return 0;

            return equipmentInst.GetOptionChangePrice();
        }

        public bool IsAllPetEquipmentOptionStatLocked(string id)
        {
            var equipmentInst = GetPetEquipment(id);
            if (equipmentInst == null)
                return false;

            return equipmentInst.IsAllOptionStatLocked();
        }

        public bool ChangePetEquipmentOptionStats(string id)
        {
            var equipmentInst = GetPetEquipment(id);
            if (equipmentInst == null)
                return false;

            return equipmentInst.ChangeOptionStats();
        }

        public int GetPetEquipmentCurrentPreset(string id)
        {
            var equipmentInst = GetPetEquipment(id);
            if (equipmentInst == null)
                return int.MaxValue;

            return equipmentInst.GetCurrentPreset();
        }

        public void ChangePetEquipmentOptionPreset(string id, int preset)
        {
            PetInventory inventory = GetPetInventoryByEquipmentId(id);
            if (inventory == null)
                return;

            var equipmentInst = inventory.GetEquipItem(id);
            if (equipmentInst == null)
                return;

            equipmentInst.ChangeOptionStatPreset(preset);

            inventory.SetIsChangedData(true);
        }

        public bool ChangePetEquipmentOptionStat(string id)
        {
            PetInventory inventory = GetPetInventoryByEquipmentId(id);
            if (inventory == null)
                return false;

            var equipmentInst = inventory.GetEquipItem(id);
            if (equipmentInst == null)
                return false;

            int statLockCount = GetEquipmentOptionStatLockCount(id);
            if (Lance.GameData.PetEquipmentCommonData.optionStatMaxCount == statLockCount)
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

        public bool AnyCanChangePetEquipmentOptionStat(string id)
        {
            var equipment = GetPetEquipment(id);
            if (equipment == null)
                return false;

            return equipment.AnyCanChangeOptionStat();
        }
    }
}
