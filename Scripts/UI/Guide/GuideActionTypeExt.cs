using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    static class GuideActionTypeExt
    {
        public static LobbyTab ChangeToLobbyTab(this GuideActionType type)
        {
            switch (type)
            {
                case GuideActionType.Move_StatureTab:
                case GuideActionType.Move_Stature_TrainTab:
                case GuideActionType.Move_Stature_AbilityTab:
                case GuideActionType.Move_Stature_ArtifactTab:
                case GuideActionType.Move_Stature_LimitBreakTab:
                    return LobbyTab.Stature;
                case GuideActionType.Move_PetTab:
                    return LobbyTab.Pet;
                case GuideActionType.Move_InventoryTab:
                case GuideActionType.Move_Inventory_WeaponTab:
                case GuideActionType.Move_Inventory_ArmorTab:
                case GuideActionType.Move_Inventory_GlovesTab:
                case GuideActionType.Move_Inventory_ShoesTab:
                    return LobbyTab.Inventory;
                case GuideActionType.Move_SkillTab:
                case GuideActionType.Move_Skill_ActiveTab:
                case GuideActionType.Move_Skill_PassiveTab:
                    return LobbyTab.Skill;
                case GuideActionType.Move_DungeonTab:
                    return LobbyTab.Adventure;
                case GuideActionType.Move_SpawnTab:
                case GuideActionType.Move_Spawn_SkillTab:
                case GuideActionType.Move_Spawn_EquipmentTab:
                case GuideActionType.Move_Spawn_ArtifactTab:
                    return LobbyTab.Spawn;
                case GuideActionType.Move_ShopTab:
                default:
                    return LobbyTab.Shop;
            }
        }

        public static StatureTab ChangeToStatureTab(this GuideActionType type)
        {
            if (type == GuideActionType.Move_Stature_AbilityTab)
                return StatureTab.Ability;
            else if (type == GuideActionType.Move_Stature_ArtifactTab)
                return StatureTab.Artifact;
            else if (type == GuideActionType.Move_Stature_TrainTab)
                return StatureTab.GoldTrain;
            else
                return StatureTab.LimitBreak;
        }

        public static InventoryTab ChangeToInventoryTab(this GuideActionType type)
        {
            if (type == GuideActionType.Move_Inventory_WeaponTab)
                return InventoryTab.Weapon;
            else if (type == GuideActionType.Move_Inventory_ArmorTab)
                return InventoryTab.Armor;
            else if (type == GuideActionType.Move_Inventory_GlovesTab)
                return InventoryTab.Gloves;
            else
                return InventoryTab.Shoes;
        }

        public static SkillInventoryTab ChangeToSkillTab(this GuideActionType type)
        {
            if (type == GuideActionType.Move_Skill_ActiveTab)
                return SkillInventoryTab.Active;
            else
                return SkillInventoryTab.Passive;
        }

        public static SpawnTab ChangeToSpawnTab(this GuideActionType type)
        {
            if (type == GuideActionType.Move_Spawn_EquipmentTab)
                return SpawnTab.Equipment;
            else if (type == GuideActionType.Move_Spawn_SkillTab)
                return SpawnTab.Skill;
            else
                return SpawnTab.Artifact;
        }

        public static bool IsEnsureVisibleType(this GuideActionType type)
        {
            return type == GuideActionType.Highlight_TrainAtkButton ||
                    type == GuideActionType.Highlight_TrainHpButton ||
                    type == GuideActionType.Highlight_TrainCriProbButton ||
                    type == GuideActionType.Highlight_TrainCriDmgButton ||
                    type == GuideActionType.Highlight_BestEquipment ||
                    type == GuideActionType.Highlight_BestSkill ||
                    type == GuideActionType.Highlight_CanEquipBestSkill ||
                    type == GuideActionType.Highlight_CanUpgradeSkill ||
                    type == GuideActionType.Highlight_SpawnWeaponButton ||
                    type == GuideActionType.Highlight_SpawnArmorButton ||
                    type == GuideActionType.Highlight_SpawnGlovesButton ||
                    type == GuideActionType.Highlight_SpawnShoesButton ||
                    type == GuideActionType.Highlight_CanUpgradeArtifact ||
                    type == GuideActionType.Highlight_GoldDungeon ||
                    type == GuideActionType.Highlight_StoneDungeon ||
                    type == GuideActionType.Highlight_RaidDungeon ||
                    type == GuideActionType.Highlight_ReforgeDungeon ||
                    type == GuideActionType.Highlight_PetDungeon ||
                    type == GuideActionType.Highlight_GrowthDungeon ||
                    type == GuideActionType.Highlight_AncientDungeon;
        }

        public static bool IsNeedParent(this GuideActionType type)
        {
            return type == GuideActionType.Highlight_TrainAtkButton ||
                    type == GuideActionType.Highlight_TrainHpButton ||
                    type == GuideActionType.Highlight_TrainCriProbButton ||
                    type == GuideActionType.Highlight_TrainCriDmgButton ||
                    type == GuideActionType.Highlight_SpawnWeaponButton ||
                    type == GuideActionType.Highlight_SpawnArmorButton ||
                    type == GuideActionType.Highlight_SpawnGlovesButton ||
                    type == GuideActionType.Highlight_SpawnShoesButton;
        }

        public static GuideActionType ChangeToParentType(this GuideActionType type)
        {
            if (type == GuideActionType.Highlight_TrainAtkButton)
                return GuideActionType.Highlight_TrainAtkButton_Parent;
            else if (type == GuideActionType.Highlight_TrainHpButton)
                return GuideActionType.Highlight_TrainHpButton_Parent;
            else if (type == GuideActionType.Highlight_TrainCriProbButton)
                return GuideActionType.Highlight_TrainCriProbButton_Parent;
            else if (type == GuideActionType.Highlight_TrainCriDmgButton)
                return GuideActionType.Highlight_TrainCriDmgButton_Parent;
            else if (type == GuideActionType.Highlight_SpawnWeaponButton)
                return GuideActionType.Highlight_SpawnWeaponButton_Parent;
            else if (type == GuideActionType.Highlight_SpawnArmorButton)
                return GuideActionType.Highlight_SpawnArmorButton_Parent;
            else if (type == GuideActionType.Highlight_SpawnGlovesButton)
                return GuideActionType.Highlight_SpawnGlovesButton_Parent;
            else
                return GuideActionType.Highlight_SpawnShoesButton_Parent;
        }
    }
}