using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Lance
{

    public partial class Account
    {
        public void CheckBountyQuest(MonsterType monsterType, string monsterId, int count = 1)
        {
            foreach(BountyQuestInfo questInfo in Bounty.GetQuestInfos())
            {
                if (monsterType != questInfo.GetMonsterType())
                    continue;

                if (questInfo.GetMonsterId() == monsterId)
                {
                    questInfo.StackRequireCount(count);

                    Bounty.SetIsChangedData(true);
                }
            }
        }

        public void CheckQuest(QuestType type, int count = 1)
        {
            int questTypeValue = GetQuestTypeValue(type);

            foreach (Quest quest in Quests)
            {
                var questInfos = quest.GetQuestInfoByType(type);
                if (questInfos != null && questInfos.Count() > 0)
                {
                    foreach (var questInfo in questInfos)
                    {
                        if (questInfo.GetQuestCheckType() == QuestCheckType.Stack)
                        {
                            questInfo.StackRequireCount(count);
                        }
                        else
                        {
                            questInfo.AttainRequireCount(questTypeValue);
                        }
                    }

                    quest.SetIsChangedData(true);
                }
            }

            var eventQuestInfos = Event.GetQuestInfoByType(type);
            if (eventQuestInfos != null && eventQuestInfos.Count() > 0)
            {
                foreach (var eventQuestInfo in eventQuestInfos)
                {
                    if (eventQuestInfo.GetQuestCheckType() == QuestCheckType.Stack)
                    {
                        eventQuestInfo.StackRequireCount(count);
                    }
                    else
                    {
                        eventQuestInfo.AttainRequireCount(questTypeValue);
                    }
                }

                Event.SetIsChangedData(true);
            }

            QuestInfo guideQuest = GuideQuest.GetCurrentQuest();
            if (guideQuest != null)
            {
                if (guideQuest.GetQuestType() == type)
                {
                    if (guideQuest.GetQuestCheckType() == QuestCheckType.Stack)
                    {
                        guideQuest.StackRequireCount(count);
                    }
                    else
                    {
                        guideQuest.AttainRequireCount(questTypeValue);
                    }

                    GuideQuest.SetIsChangedData(true);
                }
            }

            Achievement.CheckQuest(type, questTypeValue);
        }

        public int GetQuestTypeValue(QuestType questType)
        {
            switch (questType)
            {
                case QuestType.DailyquestClear:
                    return DailyQuest.GetQuestClearCount();
                case QuestType.WeeklyquestClear:
                    return WeeklyQuest.GetQuestClearCount();
                case QuestType.RepeatquestClear:
                    return RepeatQuest.GetQuestClearCount();
                case QuestType.WatchAd:
                    return UserInfo.GetStackedWatchAdCount();
                case QuestType.PlayTime:
                    return (int)UserInfo.GetPlayTime();
                case QuestType.Spawn:
                    return Spawn.GetTotalStackedSpawnCount();
                case QuestType.SpawnEquipment:
                    return Spawn.GetEquipmentTotalStackedSpawnCount();
                case QuestType.SpawnArtifact:
                    return Spawn.GetArtifactStackedSpawnCount();
                case QuestType.SpawnSkill:
                    return Spawn.GetSkillTotalStackedSpawnCount();
                case QuestType.SpawnWeapon:
                    return Spawn.GetEquipmentStackedSpawnCount(ItemType.Weapon);
                case QuestType.SpawnArmor:
                    return Spawn.GetEquipmentStackedSpawnCount(ItemType.Armor);
                case QuestType.SpawnGloves:
                    return Spawn.GetEquipmentStackedSpawnCount(ItemType.Gloves);
                case QuestType.SpawnShoes:
                    return Spawn.GetEquipmentStackedSpawnCount(ItemType.Shoes);
                //case QuestType.SpawnNecklace:
                //    return Spawn.Get
                case QuestType.KillMonster:
                    return (int)ExpLevel.GetStackedMonsterKillCount();
                case QuestType.KillBoss:
                    return (int)ExpLevel.GetStackedBossKillCount();
                case QuestType.ClearStage:
                    return StageRecords.GetBestTotalStage() - 1;
                case QuestType.ClearDungeon:
                    return Dungeon.GetTotalStackedClearCount();
                case QuestType.ClearGoldDungeon:
                    return Dungeon.GetBestStep(StageType.Gold) - 1;
                case QuestType.ClearStoneDungeon:
                    return Dungeon.GetBestStep(StageType.Stone) - 1;
                case QuestType.ClearPetDungeon:
                    return Dungeon.GetBestStep(StageType.Pet) - 1;
                case QuestType.ClearReforgeDungeon:
                    return Dungeon.GetBestStep(StageType.Reforge) - 1;
                case QuestType.ClearGrowthDungeon:
                    return Dungeon.GetBestStep(StageType.Growth) - 1;
                case QuestType.ClearAncientDungeon:
                    return Dungeon.GetBestStep(StageType.Ancient) - 1;
                case QuestType.UpgradeSkill:
                    return SkillInventory.GetStackedSkillsLevelUpCount();
                case QuestType.UpgradeActiveSkill:
                    return SkillInventory.GetStackedSkillsLevelUpCount(SkillType.Active);
                case QuestType.UpgradePassiveSkill:
                    return SkillInventory.GetStackedSkillsLevelUpCount(SkillType.Passive);
                case QuestType.UpgradeEquipment:
                    return GetTotalStackedEquipmentsLevelUpCount();
                case QuestType.CombineEquipment:
                    return GetTotalStackedEquipmentsCombineCount();
                case QuestType.CombineWeapon:
                    return WeaponInventory.GetStackedCombineCount();
                case QuestType.UpgradeWeapon:
                    return WeaponInventory.GetStackedEquipmentsLevelUpCount();
                case QuestType.TryUpgradeArtifact:
                    return Artifact.GetTryUpgradeCount();
                case QuestType.Train:
                    return GoldTrain.GetTotalTrainLevel();
                case QuestType.TrainAtk:
                    return GoldTrain.GetTrainLevel(StatType.Atk);
                case QuestType.TrainHp:
                    return GoldTrain.GetTrainLevel(StatType.Hp);
                case QuestType.TrainCriProb:
                    return GoldTrain.GetTrainLevel(StatType.CriProb);
                case QuestType.TrainCriDmg:
                    return GoldTrain.GetTrainLevel(StatType.CriDmg);
                case QuestType.TrainPowerAtk:
                    return GoldTrain.GetTrainLevel(StatType.PowerAtk);
                case QuestType.TrainPowerHp:
                    return GoldTrain.GetTrainLevel(StatType.PowerHp);
                case QuestType.TrainSuperCriProb:
                    return GoldTrain.GetTrainLevel(StatType.SuperCriProb);
                case QuestType.TrainSuperCriDmg:
                    return GoldTrain.GetTrainLevel(StatType.SuperCriDmg);
                case QuestType.LevelUpCharacter:
                    return ExpLevel.GetLevel();
                case QuestType.LevelUpAbility:
                    return Ability.GetTotalAbilityLevel();
                case QuestType.LevelUpPet:
                    return Pet.GetMaxLevel();
                case QuestType.EvolutionPet:
                    return Pet.GetMaxStep();
                case QuestType.EquipPet:
                    return Pet.AnyEquipped() ? 1 : 0;
                case QuestType.ActiveAutoChallenge:
                    return SaveBitFlags.BossBreakThrough.IsOn() ? 1 : 0;
                case QuestType.ActiveAutoCastSkill:
                    return SaveBitFlags.SkillAutoCast.IsOn() ? 1 : 0;
                case QuestType.ActiveBuff:
                    return Buff.AnyIsAlreadyActive() ? 1 : 0;
                case QuestType.LimitBreak:
                    return ExpLevel.GetLimitBreak();
                case QuestType.ClearBountyQuest:
                    return Bounty.GetQuestClearCount();
                case QuestType.LevelUpCentralEssence:
                    return Essence.GetEssence(EssenceType.Central)?.GetStep() ?? 0;
                case QuestType.Payments:
                    return UserInfo.GetStackedPayments();
                case QuestType.Login:
                    return UserInfo.GetLoginCount();
                case QuestType.PurchaseTicket:
                    return NormalShop.GetTotalPurchasedCount();
                case QuestType.UpgradeAllArtifactMaxLevel:
                    return Artifact.IsAllArtifactMaxLevel() ? 1 : 0;
                case QuestType.SendChatMessage:
                    return UserInfo.GetStackedChatCount();
                case QuestType.JoustingComboAtk:
                    return Lance.GameManager.GetJoustingComboAtkCount();
                case QuestType.TryRaidDungeon:
                case QuestType.EquipWeapon:
                case QuestType.EquipArmor:
                case QuestType.EquipGloves:
                case QuestType.EquipShoes:
                case QuestType.EquipActiveSkill:
                case QuestType.EquipPassiveSkill:
                case QuestType.ConfirmAttendance:
                case QuestType.None:
                case QuestType.Count:
                default:
                    return 0;
            }
        }

        Quest GetQuestByQuestId(string id)
        {
            var questData = DataUtil.GetQuestData(id);
            if (questData != null)
            {
                return GetQuestByUpdateType(questData.updateType);
            }

            return null;
        }

        Quest GetQuestByUpdateType(QuestUpdateType updateType)
        {
            switch (updateType)
            {
                case QuestUpdateType.Daily:
                    return DailyQuest;
                case QuestUpdateType.Weekly:
                    return WeeklyQuest;
                case QuestUpdateType.Repeat:
                default:
                    return RepeatQuest;
            }
        }

        public (List<(string questId, string rewardId, int count)>, RewardResult totalReward, int totalCount) ReceiveAllQuestReward(QuestUpdateType updateType)
        {
            int totalRewardCount = 0;
            RewardResult totalReward = new RewardResult();

            Quest quest = GetQuestByUpdateType(updateType);

            List<(string questId, string rewardId, int count)> results = quest.AllReceiveReward();
            if (results.Count > 0)
            {
                totalRewardCount += results.Count;
                foreach (var result in results)
                {
                    var rewardData = Lance.GameData.RewardData.TryGet(result.rewardId);
                    if (rewardData != null)
                    {
                        for (int i = 0; i < result.count; ++i)
                        {
                            totalReward = totalReward.AddReward(rewardData);
                        }
                    }
                }

                GiveReward(totalReward);
            }

            return (results, totalReward, totalRewardCount);
        }

        public QuestInfo GetQuestInfo(string id)
        {
            Quest quest = GetQuestByQuestId(id);

            return quest.GetQuestInfo(id);
        }
        
        public BountyQuestInfo GetBountyQuestInfo(string id)
        {
            return Bounty.GetQuestInfo(id);
        }

        public IEnumerable<BountyQuestInfo> GetBountyQuestInfos()
        {
            return Bounty.GetQuestInfos();
        }

        public IEnumerable<QuestInfo> GetQuestInfos(QuestUpdateType updateType)
        {
            Quest quest = GetQuestByUpdateType(updateType);

            return quest.GetQuestInfos();
        }

        public int GetQuestClearCount(QuestUpdateType updateType)
        {
            Quest quest = GetQuestByUpdateType(updateType);

            return quest.GetQuestClearCount();
        }

        public bool AnyQuestCanReceiveReward()
        {
            foreach(var quest in Quests)
            {
                if (quest.AnyCanReceiveReward())
                    return true;
            }

            return false;
        }

        public bool AnyQuestCanReceiveReward(QuestUpdateType updateType)
        {
            var quest = GetQuestByUpdateType(updateType);

            return quest?.AnyCanReceiveReward() ?? false;
        }

        public RewardResult ReceiveBountyQuestReward(string id)
        {
            RewardResult reward = new RewardResult();

            (string rewardId, int count) result = Bounty.ReceiveReward(id);
            if (result.rewardId.IsValid())
            {
                var rewardData = Lance.GameData.RewardData.TryGet(result.rewardId);
                if (rewardData != null)
                {
                    for (int i = 0; i < result.count; ++i)
                    {
                        reward = reward.AddReward(rewardData);
                    }
                }
            }

            GiveReward(reward);

            return reward;
        }

        public RewardResult ReceiveQuestReward(string id)
        {
            RewardResult reward = new RewardResult();

            Quest quest = GetQuestByQuestId(id);
            if ( quest != null )
            {
                (string rewardId, int count) result = quest.ReceiveReward(id);
                if (result.rewardId.IsValid())
                {
                    var rewardData = Lance.GameData.RewardData.TryGet(result.rewardId);
                    if (rewardData != null)
                    {
                        for (int i = 0; i < result.count; ++i)
                        {
                            reward = reward.AddReward(rewardData);
                        }
                    }
                }
            }

            GiveReward(reward);

            return reward;
        }

        public RewardResult ReceiveQuestBonusReward(string id)
        {
            RewardResult reward = new RewardResult();

            Quest quest = GetQuestByQuestId(id);
            if (quest != null)
            {
                (string rewardId, int count) result = quest.ReceiveBonusReward(id);
                if (result.rewardId.IsValid())
                {
                    var rewardData = Lance.GameData.RewardData.TryGet(result.rewardId);
                    if (rewardData != null)
                    {
                        for (int i = 0; i < result.count; ++i)
                        {
                            reward = reward.AddReward(rewardData);
                        }
                    }
                }
            }

            GiveReward(reward);

            return reward;
        }

        IEnumerable<QuestInfo> GatherQuestInfos()
        {
            foreach (var quest in Quests)
                foreach (var info in quest.GetQuestInfos())
                    yield return info;
        }
    }
}