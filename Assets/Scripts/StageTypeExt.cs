using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    public static class StageTypeExt
    {
        public static bool IsOnlyBoss(this StageType type)
        {
            return type == StageType.Raid ||
                type == StageType.Ancient ||
                type == StageType.Pet ||
                type == StageType.LimitBreak ||
                type == StageType.UltimateLimitBreak;
        }

        public static bool IsLimitBreak(this StageType type)
        {
            return type == StageType.LimitBreak;
        }

        public static bool IsDungeon(this StageType type)
        {
            return type == StageType.Gold ||
                type == StageType.Stone ||
                type == StageType.Pet ||
                type == StageType.Reforge ||
                type == StageType.Growth ||
                type == StageType.Raid ||
                type == StageType.Ancient;
        }

        public static bool IsDemonicRealm(this StageType type)
        {
            return type == StageType.Accessory;
        }

        public static bool IsJousting(this StageType type)
        {
            return type == StageType.Jousting;
        }

        public static bool HaveNextStepDungeon(this StageType type)
        {
            return type == StageType.Gold ||
                type == StageType.Stone ||
                type == StageType.Reforge ||
                type == StageType.Growth;
        }

        public static bool HaveNextStepDemonicRealm(this StageType type)
        {
            return type == StageType.Accessory;
        }

        public static bool IsNormal(this StageType type)
        {
            return type == StageType.Normal;
        }

        public static QuestType StageTypeChangeToQuestType(this StageType type)
        {
            switch (type)
            {
                case StageType.Gold:
                    return QuestType.ClearGoldDungeon;
                case StageType.Stone:
                    return QuestType.ClearStoneDungeon;
                case StageType.Pet:
                    return QuestType.ClearPetDungeon;
                case StageType.Growth:
                    return QuestType.ClearGrowthDungeon;
                case StageType.Ancient:
                    return QuestType.ClearAncientDungeon;
                default:
                    return QuestType.ClearReforgeDungeon;
            }
        }

        public static DungeonType ChangeToDungeonType(this StageType type)
        {
            switch(type)
            {
                case StageType.Gold:
                    return DungeonType.Gold;
                case StageType.Stone:
                    return DungeonType.Stone;
                case StageType.Reforge:
                    return DungeonType.Reforge;
                case StageType.Pet:
                    return DungeonType.Pet;
                case StageType.Growth:
                    return DungeonType.Growth;
                case StageType.Ancient:
                    return DungeonType.Ancient;
                case StageType.Raid:
                default:
                    return DungeonType.Raid;
            }
        }

        public static DemonicRealmType ChangeToDemonicRealmType(this StageType type)
        {
            switch (type)
            {
                case StageType.Accessory:
                default:
                    return DemonicRealmType.Accessory;
            }
        }

        public static GuideActionType ChangeToGuideActionType(this StageType type)
        {
            switch (type)
            {
                case StageType.Gold:
                    return GuideActionType.Highlight_GoldDungeon;
                case StageType.Stone:
                    return GuideActionType.Highlight_StoneDungeon;
                case StageType.Pet:
                    return GuideActionType.Highlight_PetDungeon;
                case StageType.Reforge:
                    return GuideActionType.Highlight_ReforgeDungeon;
                case StageType.Growth:
                    return GuideActionType.Highlight_GrowthDungeon;
                case StageType.Ancient:
                    return GuideActionType.Highlight_AncientDungeon;
                default:
                    return GuideActionType.Highlight_RaidDungeon;
            }
        }

        public static AdType ChangeToAdType(this StageType stageType)
        {
            switch (stageType)
            {
                case StageType.Gold:
                    return AdType.Daily_DungeonTicket_Gold;
                case StageType.Stone:
                    return AdType.Daily_DungeonTicket_Stone;
                case StageType.Pet:
                    return AdType.Daily_DungeonTicket_Pet;
                case StageType.Reforge:
                    return AdType.Daily_DungeonTicket_Reforge;
                case StageType.Growth:
                    return AdType.Daily_DungeonTicket_Growth;
                case StageType.Ancient:
                    return AdType.Daily_DungeonTicket_Ancient;
                case StageType.Jousting:
                    return AdType.Daily_JoustingTicket;
                case StageType.Accessory:
                    return AdType.Daily_DemonicRealm_Accessory;
                default:
                    return AdType.Daily_DungeonTicket_Raid;
            }
        }
    }

}
