using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    static class RedDotUtil
    {
        public static bool IsAcitveRedDotByLobbyTab(LobbyTab tab)
        {
            if (tab == LobbyTab.Stature)
            {
                return
                    IsAcitveRedDotByStatureTab(StatureTab.Ability) ||
                    IsAcitveRedDotByStatureTab(StatureTab.Artifact) ||
                    IsAcitveRedDotByStatureTab(StatureTab.LimitBreak) ||
                    IsAcitveRedDotByStatureTab(StatureTab.Costume);
            }
            else if (tab == LobbyTab.Skill)
            {
                return 
                    IsAcitveRedDotBySkillTab(SkillInventoryTab.Active) ||
                    IsAcitveRedDotBySkillTab(SkillInventoryTab.Passive);
            }
            else if (tab == LobbyTab.Inventory)
            {
                return
                    IsAcitveRedDotByInventoryTab(InventoryTab.Weapon) ||
                    IsAcitveRedDotByInventoryTab(InventoryTab.Armor) ||
                    IsAcitveRedDotByInventoryTab(InventoryTab.Gloves) ||
                    IsAcitveRedDotByInventoryTab(InventoryTab.Shoes) ||
                    IsAcitveRedDotByInventoryTab(InventoryTab.Accessory);
            }
            else if (tab == LobbyTab.Adventure)
            {
                return false;
            }
            else if (tab == LobbyTab.Spawn)
            {
                return
                    IsActiveRedDotBySpawnTab(SpawnTab.Equipment) ||
                    IsActiveRedDotBySpawnTab(SpawnTab.Skill) ||
                    IsActiveRedDotBySpawnTab(SpawnTab.Artifact) ||
                    IsActiveRedDotBySpawnTab(SpawnTab.Accessory);
            }
            else if (tab == LobbyTab.Shop)
            {
                return IsAcitveRedDotByShopTab(ShopTab.Currency) || IsAcitveRedDotByShopTab(ShopTab.Package);
            }
            else if (tab == LobbyTab.Pet)
            {
                return IsAcitveRedDotByPetTab(PetTab.Pet) || IsAcitveRedDotByPetTab(PetTab.PetInventory);
            }
            else
            {
                return false;
            }
        }

        public static bool IsAcitveRedDotByStatureTab(StatureTab tab)
        {
            if (tab == StatureTab.GoldTrain)
            {
                return false;
            }
            else if (tab == StatureTab.Ability)
            {
                return Lance.Account.AnyAbilityCanLevelUp();
            }
            else if (tab == StatureTab.Artifact)
            {
                return
                    Lance.Account.Artifact.AnyCanTryLevelUp() ||
                    Lance.Account.Artifact.AnyCanDismantle() ||
                    Lance.Account.AncientArtifact.AnyCanTryLevelUp() ||
                    Lance.Account.AncientArtifact.AnyCanDismantle() ||
                    Lance.Account.CanUpgradeExcalibur() ||
                    Lance.Account.AnyCanUpgradeExcaliburForce();
            }
            else if (tab == StatureTab.LimitBreak)
            {
                return Lance.Account.ExpLevel.CanTryLimitBreak() ||
                    Lance.Account.ExpLevel.CanTryUltimateLimitBreak();
            }
            else // 새로 획득한 코스튬
            {
                return IsActiveRedDotByCostumeTab(CostumeTab.Body) ||
                    IsActiveRedDotByCostumeTab(CostumeTab.Weapon) ||
                    IsActiveRedDotByCostumeTab(CostumeTab.Etc);
            }
        }

        public static bool IsAcitveRedDotByArtifactTab(ArtifactTab tab)
        {
            if (tab == ArtifactTab.Artifact)
            {
                return Lance.Account.Artifact.AnyCanTryLevelUp() || 
                    Lance.Account.Artifact.AnyCanDismantle();
            }
            else if (tab == ArtifactTab.AncientArtifact)
            {
                return Lance.Account.AncientArtifact.AnyCanTryLevelUp() ||
                    Lance.Account.AncientArtifact.AnyCanDismantle();
            }
            else
            {
                return Lance.Account.CanUpgradeExcalibur() || 
                    Lance.Account.AnyCanUpgradeExcaliburForce();
            }
        }

        public static bool IsAcitveRedDotBySkillTab(SkillInventoryTab tab)
        {
            SkillType skillType = tab.ChangeToSkillType();

            foreach (var skill in Lance.Account.SkillInventory.GatherSkills(skillType))
            {
                if (IsActiveSkillRedDot(skill.GetSkillId()))
                    return true;
            }

            return Lance.Account.AnyCurSkillSlotEmpty(skillType);
        }

        public static bool IsAcitveRedDotByInventoryTab(InventoryTab tab)
        {
            ItemType itemType = tab.ChangeToItemType();

            if (tab != InventoryTab.Accessory)
            {
                // 새로 획득한 장비or 합성 or 강화 가능한지
                foreach (var equipment in Lance.Account.GatherEquipments(itemType))
                {
                    if (IsActiveEquipmentRedDot(equipment.GetId()))
                        return true;
                }
            }
            else
            {
                return IsAcitveRedDotByAccessoryTab(AccessoryInventoryTab.Necklace) ||
                    IsAcitveRedDotByAccessoryTab(AccessoryInventoryTab.Earring) ||
                    IsAcitveRedDotByAccessoryTab(AccessoryInventoryTab.Ring);
            }
            
            
            return false;
        }

        public static bool IsAcitveRedDotByAccessoryTab(AccessoryInventoryTab tab)
        {
            ItemType itemType = tab.ChangeToItemType();

            // 새로 획득한 장비or 합성 or 강화 가능한지
            foreach (var accessory in Lance.Account.GatherAccessorys(itemType))
            {
                if (IsActiveAccessoryRedDot(accessory.GetId()))
                    return true;
            }

            return false;
        }

        public static bool IsAcitveRedDotByPetInventoryTab(PetInventoryTab tab)
        {
            ElementalType type = tab.ChangeToElementalType();

            // 새로 획득한 장비or 합성 or 강화 가능한지
            foreach (var equipment in Lance.Account.GatherPetEquipments(type))
            {
                if (IsActivePetEquipmentRedDot(equipment.GetId()))
                    return true;
            }

            return false;
        }

        public static bool IsActiveRedDotByCostumeTab(CostumeTab tab)
        {
            CostumeType costumeType = tab.ChangeToCostumeType();

            foreach (var id in DataUtil.GetCostumeIds(costumeType))
            {
                if (IsActiveCostumeRedDot(id))
                    return true;
            }

            return false;
        }

        public static bool IsActiveSkillRedDot(string id)
        {
            var data = DataUtil.GetSkillData(id);
            if (data == null)
                return false;

            return IsNewSkill(id) || Lance.Account.CanUpgradeSkill(data.type, id, 1);
        }

        static bool IsNewSkill(string id)
        {
            var data = DataUtil.GetSkillData(id);
            if (data == null)
                return false;

            return Lance.LocalSave.IsNewSkill(id) &&
                    Lance.Account.HaveSkill(data.type, id);
        }

        public static bool IsActiveEquipmentRedDot(string id)
        {
            return IsNewEquipment(id) || Lance.Account.CanCombine(id);
        }

        public static bool IsActiveAccessoryRedDot(string id)
        {
            return IsNewAccessory(id) || Lance.Account.CanAccessoryCombine(id);
        }

        public static bool IsActivePetEquipmentRedDot(string id)
        {
            return IsNewPetEquipment(id) || Lance.Account.CanPetEquipmentCombine(id);
        }

        static bool IsNewEquipment(string id)
        {
            return Lance.LocalSave.IsNewEquipment(id) &&
                Lance.Account.HaveEquipment(id);
        }

        static bool IsNewAccessory(string id)
        {
            return Lance.LocalSave.IsNewAccessory(id) &&
                Lance.Account.HaveAccessory(id);
        }

        static bool IsNewPetEquipment(string id)
        {
            return Lance.LocalSave.IsNewPetEquipment(id) &&
                Lance.Account.HavePetEquipment(id);
        }

        public static bool IsActiveCostumeRedDot(string id)
        {
            return Lance.LocalSave.IsNewCostume(id) &&
                Lance.Account.Costume.HaveCostume(id);
        }

        public static bool IsActiveRedDotBySpawnTab(SpawnTab tab)
        {
            if (tab == SpawnTab.Equipment)
            {
                return 
                    Lance.Account.IsEnoughSpawnWatchAdCount(ItemType.Weapon) ||
                    Lance.Account.AnyCanReceiveReward(ItemType.Weapon) ||
                    Lance.Account.IsEnoughSpawnWatchAdCount(ItemType.Armor) ||
                    Lance.Account.AnyCanReceiveReward(ItemType.Armor) ||
                    Lance.Account.IsEnoughSpawnWatchAdCount(ItemType.Gloves) ||
                    Lance.Account.AnyCanReceiveReward(ItemType.Gloves) ||
                    Lance.Account.IsEnoughSpawnWatchAdCount(ItemType.Shoes) ||
                    Lance.Account.AnyCanReceiveReward(ItemType.Shoes);
            }
            else if (tab == SpawnTab.Skill)
            {
                return
                    Lance.Account.IsEnoughSpawnWatchAdCount(ItemType.Skill) ||
                    Lance.Account.AnyCanReceiveReward(ItemType.Skill);
            }
            else if (tab == SpawnTab.Artifact)
            {
                return
                    Lance.Account.IsEnoughSpawnWatchAdCount(ItemType.Artifact) ||
                    Lance.Account.IsEnoughSpawnWatchAdCount(ItemType.AncientArtifact);
            }
            else
            {
                return
                    Lance.Account.IsEnoughSpawnWatchAdCount(ItemType.Necklace) ||
                    Lance.Account.AnyCanReceiveReward(ItemType.Necklace) ||
                    Lance.Account.IsEnoughSpawnWatchAdCount(ItemType.Earring) ||
                    Lance.Account.AnyCanReceiveReward(ItemType.Earring) ||
                    Lance.Account.IsEnoughSpawnWatchAdCount(ItemType.Ring) ||
                    Lance.Account.AnyCanReceiveReward(ItemType.Ring);
            }
        }

        public static bool IsAcitveRedDotByPetTab(PetTab tab)
        {
            if (tab == PetTab.Pet)
            {
                return Lance.Account.AnyCanEvolutionPet();
            }
            else
            {
                return 
                    IsAcitveRedDotByPetInventoryTab(PetInventoryTab.Fire) ||
                    IsAcitveRedDotByPetInventoryTab(PetInventoryTab.Water) ||
                    IsAcitveRedDotByPetInventoryTab(PetInventoryTab.Grass);
            }
        }

        public static bool IsAcitveRedDotByShopTab(ShopTab tab)
        {
            if (tab == ShopTab.Currency)
            {
                var data = Lance.GameData.GemShopData.Values.Where(x => x.watchAdDailyCount > 0).FirstOrDefault();
                if (data != null)
                    return Lance.Account.Shop.IsEnoughFreeGemWatchAdCount(data.watchAdDailyCount);
                else
                    return false;
            }
            else if (tab == ShopTab.Package)
            {
                return Lance.Account.PackageShop.AnyCanPurchaseFreePackage();
            }
            else
            {
                return false;
            }
        }
    }
}



