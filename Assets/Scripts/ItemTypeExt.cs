using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    static class ItemTypeExt
    {
        public static bool IsEquipment(this ItemType itemType)
        {
            return
                itemType == ItemType.Weapon ||
                itemType == ItemType.Armor ||
                itemType == ItemType.Gloves ||
                itemType == ItemType.Shoes;
        }

        public static bool IsAccessory(this ItemType itemType)
        {
            return itemType == ItemType.Necklace ||
                itemType == ItemType.Earring ||
                itemType == ItemType.Ring;
        }

        public static bool IsPetEquipment(this ItemType itemType)
        {
            return itemType == ItemType.PetEquipment;
        }

        public static bool IsDungeonTicket(this ItemType itemType)
        {
            return
                itemType == ItemType.GoldTicket ||
                itemType == ItemType.StoneTicket ||
                itemType == ItemType.RaidTicket ||
                itemType == ItemType.PetTicket ||
                itemType == ItemType.ReforgeTicket ||
                itemType == ItemType.GrowthTicket ||
                itemType == ItemType.AncientTicket ||
                itemType == ItemType.TicketBundle;
        }

        public static bool IsDemonicRealmStone(this ItemType itemType)
        {
            return itemType == ItemType.DemonicRealmStone_Accessory;
        }

        public static bool IsArtifact(this ItemType itemType)
        {
            return itemType == ItemType.Artifact;
        }

        public static bool IsRandomEquipment(this ItemType itemType)
        {
            return
                itemType == ItemType.Random_Weapon ||
                itemType == ItemType.Random_Armor ||
                itemType == ItemType.Random_Gloves ||
                itemType == ItemType.Random_Shoes ||
                itemType == ItemType.Random_Equipment;
        }

        public static bool IsRandomAccessory(this ItemType itemType)
        {
            return itemType == ItemType.Random_Necklace ||
                itemType == ItemType.Random_Earring ||
                itemType == ItemType.Random_Ring ||
                itemType == ItemType.Random_Accessory;
        }

        public static bool IsRandom(this ItemType itemType)
        {
            return
                itemType == ItemType.Random_Weapon ||
                itemType == ItemType.Random_Armor ||
                itemType == ItemType.Random_Gloves ||
                itemType == ItemType.Random_Shoes ||
                itemType == ItemType.Random_Equipment ||
                itemType == ItemType.Random_Accessory ||
                itemType == ItemType.Random_Skill;
        }

        public static ItemType RandomChangeToAccessoryType(this ItemType itemType)
        {
            switch (itemType)
            {
                case ItemType.Random_Necklace:
                    return ItemType.Necklace;
                case ItemType.Random_Earring:
                    return ItemType.Earring;
                case ItemType.Random_Ring:
                default:
                    return ItemType.Ring;
            }
        }

        public static ItemType RandomChangeToEquipmentType(this ItemType itemType)
        {
            switch (itemType)
            {
                case ItemType.Random_Weapon:
                    return ItemType.Weapon;
                case ItemType.Random_Armor:
                    return ItemType.Armor;
                case ItemType.Random_Gloves:
                    return ItemType.Gloves;
                case ItemType.Random_Shoes:
                    return ItemType.Shoes;
                case ItemType.Random_Skill:
                default:
                    return ItemType.Skill;
            }
        }

        public static bool IsLobbyVisibleCurrency(this ItemType itemType)
        {
            return itemType == ItemType.Gold ||
                itemType == ItemType.Gem ||
                itemType == ItemType.UpgradeStone;
        }

        public static bool IsCurrency(this ItemType itemType)
        {
            return itemType == ItemType.Gold ||
                itemType == ItemType.Gem ||
                itemType == ItemType.UpgradeStone ||
                itemType == ItemType.ReforgeStone ||
                itemType == ItemType.GoldTicket ||
                itemType == ItemType.StoneTicket ||
                itemType == ItemType.ReforgeTicket ||
                itemType == ItemType.PetTicket ||
                itemType == ItemType.GrowthTicket ||
                itemType == ItemType.RaidTicket ||
                itemType == ItemType.AncientTicket ||
                itemType == ItemType.DemonicRealmStone_Accessory ||
                itemType == ItemType.Mileage ||
                itemType == ItemType.Exp ||
                itemType == ItemType.TicketBundle ||
                itemType == ItemType.PetFood ||
                itemType == ItemType.ElementalStone ||
                itemType == ItemType.EventCurrency ||
                itemType == ItemType.SkillPiece ||
                itemType == ItemType.AncientEssence ||
                itemType == ItemType.Essence_Chapter1 ||
                itemType == ItemType.Essence_Chapter2 ||
                itemType == ItemType.Essence_Chapter3 ||
                itemType == ItemType.Essence_Chapter4 ||
                itemType == ItemType.Essence_Chapter5 ||
                itemType == ItemType.JoustingTicket ||
                itemType == ItemType.JoustingCoin ||
                itemType == ItemType.GloryToken ||
                itemType == ItemType.SoulStone ||
                itemType == ItemType.ManaEssence ||
                itemType == ItemType.CostumeUpgrade ||
                itemType == ItemType.Advertisement ||
                itemType == ItemType.SpeedMode ||
                itemType == ItemType.Buff;
        }

        public static bool IsSkill(this ItemType itemType)
        {
            return itemType == ItemType.Skill;
        }

        public static bool IsSpawn(this ItemType itemType)
        {
            return
                itemType == ItemType.Weapon ||
                itemType == ItemType.Armor ||
                itemType == ItemType.Gloves ||
                itemType == ItemType.Shoes ||
                itemType == ItemType.Skill ||
                itemType == ItemType.Artifact ||
                itemType == ItemType.AncientArtifact ||
                itemType == ItemType.Necklace ||
                itemType == ItemType.Earring ||
                itemType == ItemType.Ring;
        }

        public static DungeonType ChangeToDungeonType(this ItemType itemType)
        {
            if (itemType == ItemType.GoldTicket)
                return DungeonType.Gold;
            else if (itemType == ItemType.StoneTicket)
                return DungeonType.Stone;
            else if (itemType == ItemType.PetTicket)
                return DungeonType.Pet;
            else if (itemType == ItemType.ReforgeTicket)
                return DungeonType.Reforge;
            else if (itemType == ItemType.GrowthTicket)
                return DungeonType.Growth;
            else if (itemType == ItemType.AncientTicket)
                return DungeonType.Ancient;
            else
                return DungeonType.Raid;
        }

        public static DemonicRealmType ChangeToDemonicRealmType(this ItemType itemType)
        {
            if (itemType == ItemType.DemonicRealmStone_Accessory)
                return DemonicRealmType.Accessory;
            else
                return DemonicRealmType.Accessory;
        }

        public static EssenceType ChangeToEssenceType(this ItemType itemType)
        {
            if (itemType == ItemType.Essence_Chapter1)
                return EssenceType.Chapter1;
            else if(itemType == ItemType.Essence_Chapter2)
                return EssenceType.Chapter2;
            else if(itemType == ItemType.Essence_Chapter3)
                return EssenceType.Chapter3;
            else if (itemType == ItemType.Essence_Chapter4)
                return EssenceType.Chapter4;
            else
                return EssenceType.Chapter5;
        }

        public static GuideActionType ChangeToGuideActionHighlightSpawnButtonType(this ItemType itemType)
        {
            if (itemType == ItemType.Weapon)
            {
                return GuideActionType.Highlight_SpawnWeaponButton;
            }
            else if (itemType == ItemType.Armor)
            {
                return GuideActionType.Highlight_SpawnArmorButton;
            }
            else if (itemType == ItemType.Gloves)
            {
                return GuideActionType.Highlight_SpawnGlovesButton;
            }
            else if (itemType == ItemType.Shoes)
            {
                return GuideActionType.Highlight_SpawnShoesButton;
            }
            else if (itemType == ItemType.Skill)
            {
                return GuideActionType.Highlight_SpawnSkillButton;
            }
            else
            {
                return GuideActionType.Highlight_SpawnArtifactButton;
            }
        }

        public static AdType ChangeToAdType(this ItemType itemType)
        {
            if (itemType == ItemType.Weapon)
                return AdType.Daily_Spawn_Weapon;
            else if (itemType == ItemType.Armor)
                return AdType.Daily_Spawn_Armor;
            else if (itemType == ItemType.Gloves)
                return AdType.Daily_Spawn_Gloves;
            else if (itemType == ItemType.Shoes)
                return AdType.Daily_Spawn_Shoes;
            else if (itemType == ItemType.Skill)
                return AdType.Daily_Spawn_Skill;
            else if (itemType == ItemType.Gem)
                return AdType.DailyGem;
            else if (itemType == ItemType.Necklace)
                return AdType.Daily_Spawn_Necklace;
            else if (itemType == ItemType.Earring)
                return AdType.Daily_Spawn_Earring;
            else if (itemType == ItemType.Ring)
                return AdType.Daily_Spawn_Ring;
            else //if (itemType == ItemType.Artifact)
                return AdType.Daily_Spawn_Artifact;
        }

        public static InventoryTab ChangeToInventoryTab(this ItemType itemType)
        {
            switch (itemType)
            {
                case ItemType.Weapon:
                    return InventoryTab.Weapon;
                case ItemType.Armor:
                    return InventoryTab.Armor;
                case ItemType.Gloves:
                    return InventoryTab.Gloves;
                default:
                    return InventoryTab.Shoes;
            }
        }
    }

    static class DungoenTypeExt
    {
        public static ItemType ChangeToItemType(this DungeonType type)
        {
            if (type == DungeonType.Gold)
                return ItemType.GoldTicket;
            else if (type == DungeonType.Stone)
                return ItemType.StoneTicket;
            else if (type == DungeonType.Pet)
                return ItemType.PetTicket;
            else if (type == DungeonType.Reforge)
                return ItemType.ReforgeTicket;
            else if (type == DungeonType.Growth)
                return ItemType.GrowthTicket;
            else if (type == DungeonType.Ancient)
                return ItemType.AncientTicket;
            else
                return ItemType.RaidTicket;
        }
    }

    static class DemonicRealmTypeExt
    {
        public static ItemType ChangeToItemType(this DemonicRealmType type)
        {
            if (type == DemonicRealmType.Accessory)
                return ItemType.DemonicRealmStone_Accessory;
            else
                return ItemType.DemonicRealmStone_Accessory;
        }
    }

    static class EssenceTypeExt
    {
        public static ItemType ChangeToItemType(this EssenceType type)
        {
            if (type == EssenceType.Chapter1)
                return ItemType.Essence_Chapter1;
            else if (type == EssenceType.Chapter2)
                return ItemType.Essence_Chapter2;
            else if (type == EssenceType.Chapter3)
                return ItemType.Essence_Chapter3;
            else if (type == EssenceType.Chapter4)
                return ItemType.Essence_Chapter4;
            else
                return ItemType.Essence_Chapter5;
        }
    }

    static class SkillTypeExt
    {
        public static GuideActionType ChangeToGuideActionUpgradeSkillButtonType(this SkillType skillType)
        {
            if (skillType == SkillType.Active)
            {
                return GuideActionType.Highlight_UpgradeActiveSkillButton;
            }
            else
            {
                return GuideActionType.Highlight_UpgradePassiveSkillButton;
            }
        }

        public static GuideActionType ChangeToGuideActionEquipSkillButtonType(this SkillType skillType)
        {
            if (skillType == SkillType.Active)
            {
                return GuideActionType.Highlight_EquipActiveSkillButton;
            }
            else
            {
                return GuideActionType.Highlight_EquipPassiveSkillButton;
            }
        }
    }
}