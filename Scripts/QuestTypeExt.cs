using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    static class QuestTypeExt
    {
        public static bool IsQuestClearType(this QuestType type)
        {
            return type == QuestType.DailyquestClear ||
                   type == QuestType.WeeklyquestClear ||
                   type == QuestType.EventquestClear ||
                   type == QuestType.EventquestClearByOpenDay;
        }

        public static bool IsImmediatelyGuideQuestCheck(this QuestType type)
        {
            switch (type)
            {
                case QuestType.TryUpgradeArtifact:
                case QuestType.UpgradeAllArtifactMaxLevel:
                case QuestType.LevelUpAbility:
                case QuestType.ActiveAutoChallenge:
                case QuestType.ActiveAutoCastSkill:
                case QuestType.CastSkill:
                case QuestType.TrainAtk:
                case QuestType.TrainHp:
                case QuestType.TrainCriProb:
                case QuestType.TrainCriDmg:
                case QuestType.TrainPowerAtk:
                case QuestType.TrainPowerHp:
                case QuestType.TrainSuperCriProb:
                case QuestType.TrainSuperCriDmg:
                case QuestType.KillMonster:
                case QuestType.KillBoss:
                case QuestType.LevelUpCharacter:
                case QuestType.LimitBreak:          // ÇÑ°è µ¹ÆÄ
                    return true;

                // Æ¯Á¤ ÆË¾÷ ´ÝÈù ´ÙÀ½ È®ÀÎ
                case QuestType.ClearStage:
                case QuestType.ClearDungeon:
                case QuestType.ClearGoldDungeon:
                case QuestType.ClearStoneDungeon:
                case QuestType.ClearPetDungeon:
                case QuestType.ClearReforgeDungeon:
                case QuestType.ClearGrowthDungeon:
                case QuestType.ClearAncientDungeon:
                case QuestType.TryRaidDungeon:
                
                case QuestType.SpawnEquipment:
                case QuestType.SpawnWeapon:
                case QuestType.SpawnArmor:
                case QuestType.SpawnGloves:
                case QuestType.SpawnShoes:
                case QuestType.SpawnSkill:
                case QuestType.SpawnArtifact:

                case QuestType.CombineEquipment:
                case QuestType.CombineWeapon:
                case QuestType.EquipWeapon:
                case QuestType.EquipArmor:
                case QuestType.EquipGloves:
                case QuestType.EquipShoes:
                case QuestType.UpgradeWeapon:
                case QuestType.UpgradeEquipment:

                case QuestType.EquipActiveSkill:
                case QuestType.EquipPassiveSkill:
                case QuestType.UpgradeSkill:
                case QuestType.UpgradeActiveSkill:
                case QuestType.UpgradePassiveSkill:

                case QuestType.ConfirmRaidRank:     // ·©Å· ÆË¾÷
                case QuestType.ConfirmAttendance:   
                case QuestType.ActiveBuff:

                case QuestType.DailyquestClear:
                case QuestType.WeeklyquestClear:
                case QuestType.RepeatquestClear:

                case QuestType.EquipPet:
                case QuestType.LevelUpPet:
                case QuestType.EvolutionPet:
                case QuestType.ConfirmEssence:
                case QuestType.ClearBountyQuest:

                // È®ÀÎÇÒ ÇÊ¿ä ¾øÀ½
                case QuestType.WatchAd:
                case QuestType.PlayTime:
                case QuestType.Spawn:
                case QuestType.Count:
                default:
                    return false;
            }
        }
    }
}