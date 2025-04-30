using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Lance
{
    enum ContentsLockType
    {
        None = 0,
        Spawn_EquipmentTab,
        Spawn_Equipment_WeaponItem,
        
        BossChallenge = 5,      // o
        BossAutoChallege = 6,   // o
        SpawnTab = 7,           // o
        InventoryTab = 8,       // o
        Inventory_WeaponTab,    // o
        Ability = 12,           // o
        Spawn_SkillTab = 13,
        SkillTab = 14,          // o
        AutoCast = 18,          // o
        Attendance = 19,        // o
        Quest = 20,             // o
        DungeonTab = 26,
        Spawn_Equipment_ArmorItem = 29,
        Inventory_ArmorTab = 30,// o
        Buff = 36,              // o
        SpeedMode = 37,
        StoneDungeon = 39,
        ReforgeDungeon = 41,
        Spawn_Equipment_GlovesItem = 42,
        Inventory_GlovesTab = 43,// o
        Spawn_Equipment_ShoesItem = 48,
        Inventory_ShoesTab = 49, // o
        GrowthDungeon = 68,
        LimitBreak = 80,        // o
        RaidDungeon = 81,
        Rank = 82,              // o
        PetDungeon = 84,
        BountyQuest = 110,

        Spawn_ArtifactTab = 271,
        Artifact = 272,         // o

        Jousting = 440,

        Essence = 924,

        DemonicRealm = 1724, // 3300 스테이지 클리어
    }

    static class ContentsLockUtil
    {
        public static bool IsLockContents(ContentsLockType contents)
        {
            return IsSatisfiedGuideStep((int)contents) == false;
        }

        public static string GetContentsLockMessage(ContentsLockType contents)
        {
            StringParam param = new StringParam("step", (int)contents);

            return StringTableUtil.GetSystemMessage("IsLockContents", param);
        }

        public static void ShowContentsLockMessage(ContentsLockType contents)
        {
            UIUtil.ShowSystemMessage(GetContentsLockMessage(contents));
        }

        static bool IsSatisfiedGuideStep(int step)
        {
            return Lance.Account.GuideQuest.GetCurrentStep() >= step;
        }
    }

    static class ContentsLockTypeUtil
    {
        public static ContentsLockType ChangeToContentsLockType(this LobbyTab tab)
        {
            switch (tab)
            {
                
                case LobbyTab.Skill:
                    return ContentsLockType.SkillTab;
                case LobbyTab.Inventory:
                    return ContentsLockType.InventoryTab;
                case LobbyTab.Adventure:
                    return ContentsLockType.DungeonTab;
                case LobbyTab.Spawn:
                    return ContentsLockType.SpawnTab;
                case LobbyTab.Pet:
                    return ContentsLockType.PetDungeon;
                case LobbyTab.Stature:
                case LobbyTab.Shop:
                default:
                    return ContentsLockType.None;
            }
        }

        public static ContentsLockType ChangeToContentsLockType(this StatureTab tab)
        {
            switch (tab)
            {
                case StatureTab.Ability:
                    return ContentsLockType.Ability;
                case StatureTab.Artifact:
                    return ContentsLockType.Artifact;
                case StatureTab.LimitBreak:
                    return ContentsLockType.LimitBreak;
                case StatureTab.GoldTrain:
                default:
                    return ContentsLockType.None;
            }
        }

        public static ContentsLockType ChangeToContentsLockType(this ArtifactTab tab)
        {
            switch (tab)
            {
                case ArtifactTab.Artifact:
                    return ContentsLockType.Artifact;
                case ArtifactTab.AncientArtifact:
                default:
                    return ContentsLockType.None;
            }
        }

        public static ContentsLockType ChangeToContentsLockType(this InventoryTab tab)
        {
            switch (tab)
            {
                case InventoryTab.Armor:
                    return ContentsLockType.Inventory_ArmorTab;
                case InventoryTab.Gloves:
                    return ContentsLockType.Inventory_GlovesTab;
                case InventoryTab.Shoes:
                    return ContentsLockType.Inventory_ShoesTab;
                case InventoryTab.Accessory:
                    return ContentsLockType.DemonicRealm;
                case InventoryTab.Weapon:
                default:
                    return ContentsLockType.None;
            }
        }

        public static ContentsLockType ChangeToContentsLockType(this SpawnTab tab)
        {
            switch (tab)
            {
                case SpawnTab.Equipment:
                    return ContentsLockType.Spawn_EquipmentTab;
                case SpawnTab.Skill:
                    return ContentsLockType.Spawn_SkillTab;
                case SpawnTab.Artifact:
                    return ContentsLockType.Spawn_ArtifactTab;
                case SpawnTab.Accessory:
                    return ContentsLockType.DemonicRealm;
                default:
                    return ContentsLockType.None;
            }
        }

        public static ContentsLockType ChangeToContentsLockType(this ItemType itemType)
        {
            if (itemType == ItemType.Armor)
                return ContentsLockType.Spawn_Equipment_ArmorItem;
            else if (itemType == ItemType.Gloves)
                return ContentsLockType.Spawn_Equipment_GlovesItem;
            else if (itemType == ItemType.Shoes)
                return ContentsLockType.Spawn_Equipment_ShoesItem;
            else if (itemType == ItemType.Artifact || itemType == ItemType.AncientArtifact)
                return ContentsLockType.Spawn_ArtifactTab;
            else if (itemType == ItemType.Necklace || itemType == ItemType.Earring || itemType == ItemType.Ring)
                return ContentsLockType.DemonicRealm;
            else
                return ContentsLockType.None;
        }

        public static ContentsLockType ChangeToContentsLockType(this StageType stageType)
        {
            switch (stageType)
            {
                case StageType.Stone:
                    return ContentsLockType.StoneDungeon;
                case StageType.Raid:
                    return ContentsLockType.RaidDungeon;
                case StageType.Reforge:
                    return ContentsLockType.ReforgeDungeon;
                case StageType.Growth:
                    return ContentsLockType.GrowthDungeon;
                case StageType.LimitBreak:
                    return ContentsLockType.LimitBreak;
                case StageType.Pet:
                    return ContentsLockType.PetDungeon;
                case StageType.Accessory:
                    return ContentsLockType.DemonicRealm;
                case StageType.Gold:
                case StageType.Normal:
                default:
                    return ContentsLockType.None;
            }
        }
    }
}