using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using BackEnd;

namespace Lance
{
    public partial class Account
    {
        public bool IsEnoughSpawnWatchAdCount(ItemType type)
        {
            var info = Spawn.GetInfo(type);

            return info.IsEnoughWathAdCount();
        }

        public int GetSpawnDailyWatchAdRemainCount(ItemType type)
        {
            var info = Spawn.GetInfo(type);

            return info.GetDailyWatchAdRemainCount();
        }

        public bool WatchSpawnAd(ItemType type, int count = 1)
        {
            var info = Spawn.GetInfo(type);

            if (info?.WatchAd(count) ?? false)
            {
                Spawn.SetIsChangedData(true);
            }

            return false;
        }

        public bool AnyCanReceiveReward(ItemType type)
        {
            var info = Spawn.GetInfo(type);

            return info?.AnyCanReceiveReward() ?? false;
        }

        public bool IsReceivedSpawnReward(ItemType itemType, int level)
        {
            var info = Spawn.GetInfo(itemType);

            return info.IsReceievedReward(level);
        }

        public RewardResult ReceiveSpawnReward(ItemType itemType, int level)
        {
            var info = Spawn.GetInfo(itemType);

            RewardResult result = info.ReceiveReward(level);
            if (result.IsEmpty() == false)
            {
                GiveReward(result);

                Spawn.SetIsChangedData(true);
            }

            return result;
        }

        public bool CanReceiveSpawnReward(ItemType itemType, int level)
        {
            var info = Spawn.GetInfo(itemType);

            return info.CanReceiveReward(level);
        }

        public bool AnyRangeCanReceiveSpawnReward(ItemType itemType, int level, bool isLeft)
        {
            var info = Spawn.GetInfo(itemType);

            return info.AnyRangeCanReceiveReward(level, isLeft);
        }

        public RewardResult SpawnItem(SpawnPriceData spawnPriceData, string probId, int spawnCount, bool isPurchasedRemoveAd)
        {
            // 보상은 미리 만들어두자
            RewardResult rewardResult = spawnPriceData.type.IsEquipment() ? SpawnEquipment(probId, spawnCount) :
                ( spawnPriceData.type.IsAccessory() ? SpawnAccessory(probId, spawnCount) :
                ( spawnPriceData.type.IsSkill() ? SpawnSkill(probId, spawnCount) : SpawnArtifact(spawnCount) ));

            if (Spawn.IsFreeSpawn(spawnPriceData.type, spawnPriceData.id))
            {
                Param param = new Param();
                param.Add("spawnType", spawnPriceData.spawnType);
                param.Add("spawnItemType", spawnPriceData.type);
                param.Add("spawnCount", spawnPriceData.spawnCount);

                Lance.BackEnd.InsertLog("FreeSpawn", param, 7);

                Spawn.AddFreeSpawned(spawnPriceData.type, spawnPriceData.id);

                return rewardResult;
            }
            else
            {
                if (spawnPriceData.spawnType == SpawnType.Gem) // 잼
                {
                    double gemPrice = spawnPriceData.price;

                    // 잼이 충분히 있는지 확인
                    if (IsEnoughGem(gemPrice) == false)
                        return rewardResult;

                    // 잼 소모
                    if (UseGem(gemPrice) == false)
                        return rewardResult;

                    return rewardResult;
                }
                else if (spawnPriceData.spawnType == SpawnType.AncientEssence)
                {
                    int essencePrice = (int)spawnPriceData.price;

                    // 고대 정수가 충분히 있는지 확인
                    if (IsEnoughAncientEssence(essencePrice) == false)
                        return rewardResult;

                    // 고대 정수 소모
                    if (UseAncientEssence(essencePrice) == false)
                        return rewardResult;

                    return rewardResult;
                }
                else // 광고
                {
                    int remainWatchAdCount = GetSpawnDailyWatchAdRemainCount(spawnPriceData.type);

                    int watchAdCount = isPurchasedRemoveAd ? remainWatchAdCount : 1;

                    // 광고 횟수가 충분히 있는지 확인
                    if (remainWatchAdCount <= 0)
                        return rewardResult;

                    // 광고 플레이 => 광고를 다 본 뒤 => 광고 횟수를 차감하고 보상을 지급
                    if (WatchSpawnAd(spawnPriceData.type, watchAdCount) == false)
                        return rewardResult;

                    int afterRemainWatchAdCount = GetSpawnDailyWatchAdRemainCount(spawnPriceData.type);

                    Param param = new Param();
                    param.Add("spawnType", spawnPriceData.spawnType);
                    param.Add("spawnItemType", spawnPriceData.type);
                    param.Add("remainAdCount", afterRemainWatchAdCount);
                    param.Add("spawnCount", spawnCount);

                    Lance.BackEnd.InsertLog("Spawn", param, 7);

                    return rewardResult;
                }
            }
            

            RewardResult SpawnEquipment(string probId, int spawnCount)
            {
                RewardResult rewardResult = new RewardResult();
                if (probId.IsValid() == false)
                    return rewardResult;

                StackedSpawnInfo spawnInfo = Spawn.GetInfo(spawnPriceData.type);
                if (spawnInfo == null)
                    return rewardResult;

                SpawnGradeProbData probData = DataUtil.GetSpawnGradeProbData(probId, spawnInfo.GetStackedSpawnCount());

                if (probData == null)
                    return rewardResult;

                var subGradeDatas = DataUtil.GetEquipmentSubgradeProbDatas(probData.level);
                if (subGradeDatas == null || subGradeDatas.Count() <= 0)
                    return rewardResult;

                List<MultiReward> rewards = new List<MultiReward>();

                for (int i = 0; i < spawnCount; ++i)
                {
                    Grade grade = (Grade)Util.RandomChoose(probData.prob);

                    EquipmentSubgradeProbData subGradeData = subGradeDatas.Where(x => x.grade == grade).First();

                    SubGrade subGrade = (SubGrade)Util.RandomChoose(subGradeData.prob);

                    EquipmentData equipData = DataUtil.GetEquipmentData(spawnPriceData.type, grade, subGrade);
                    if (equipData == null)
                        continue;

                    rewards.Add(new MultiReward()
                    {
                        ItemType = equipData.type,
                        Id = equipData.id,
                        Count = 1,
                    });
                }

                spawnInfo.StackSpawnCount(spawnCount);

                rewardResult.equipments = rewards.ToArray();

                Spawn.SetIsChangedData(true);

                rewards = null;

                return rewardResult;
            }

            RewardResult SpawnAccessory(string probId, int spawnCount)
            {
                RewardResult rewardResult = new RewardResult();
                if (probId.IsValid() == false)
                    return rewardResult;

                StackedSpawnInfo spawnInfo = Spawn.GetInfo(spawnPriceData.type);
                if (spawnInfo == null)
                    return rewardResult;

                SpawnGradeProbData probData = DataUtil.GetSpawnGradeProbData(probId, spawnInfo.GetStackedSpawnCount());
                if (probData == null)
                    return rewardResult;

                var subGradeDatas = DataUtil.GetAccessorySubgradeProbDatas(probData.level);
                if (subGradeDatas == null || subGradeDatas.Count() <= 0)
                    return rewardResult;

                List<MultiReward> rewards = new List<MultiReward>();

                for (int i = 0; i < spawnCount; ++i)
                {
                    Grade grade = (Grade)Util.RandomChoose(probData.prob);

                    AccessorySubgradeProbData subGradeData = subGradeDatas.Where(x => x.grade == grade).First();

                    SubGrade subGrade = (SubGrade)Util.RandomChoose(subGradeData.prob);

                    AccessoryData accessoryData = DataUtil.GetAccessoryData(spawnPriceData.type, grade, subGrade);
                    if (accessoryData == null)
                        continue;

                    rewards.Add(new MultiReward()
                    {
                        ItemType = accessoryData.type,
                        Id = accessoryData.id,
                        Count = 1,
                    });
                }

                spawnInfo.StackSpawnCount(spawnCount);

                rewardResult.accessorys = rewards.ToArray();

                Spawn.SetIsChangedData(true);

                rewards = null;

                return rewardResult;
            }

            RewardResult SpawnSkill(string probId, int spawnCount)
            {
                RewardResult rewardResult = new RewardResult();
                if (probId.IsValid() == false)
                    return rewardResult;

                StackedSpawnInfo spawnInfo = Spawn.GetInfo(spawnPriceData.type);
                if (spawnInfo == null)
                    return rewardResult;

                SkillTypeProbData typeProbData = Lance.GameData.SkillTypeProbData;
                SpawnGradeProbData probData = DataUtil.GetSpawnGradeProbData(probId, spawnInfo.GetStackedSpawnCount());

                if (typeProbData == null || probData == null)
                    return rewardResult;

                List<MultiReward> rewards = new List<MultiReward>();

                for (int i = 0; i < spawnCount; ++i)
                {
                    SkillType skillType = (SkillType)Util.RandomChoose(typeProbData.prob);
                    Grade grade = (Grade)Util.RandomChoose(probData.prob);

                    IEnumerable<SkillData> skillDatas = DataUtil.GetSkillDatas(skillType, grade);
                    if (skillDatas == null)
                        continue;

                    var randomData = Util.RandomSelect(skillDatas.ToArray());

                    rewards.Add(new MultiReward()
                    {
                        ItemType = ItemType.Skill,
                        Id = randomData.id,
                        Count = 1,
                    });
                }

                spawnInfo.StackSpawnCount(spawnCount);

                Lance.Lobby.RefreshGuideQuestUI();

                rewardResult.skills = rewards.ToArray();

                rewards = null;

                Spawn.SetIsChangedData(true);

                return rewardResult;
            }

            RewardResult SpawnArtifact(int spawnCount)
            {
                RewardResult rewardResult = new RewardResult();

                StackedSpawnInfo spawnInfo = Spawn.GetInfo(spawnPriceData.type);
                if (spawnInfo == null)
                    return rewardResult;

                bool isArtifact = spawnPriceData.type.IsArtifact();

                ArtifactProbData probData = isArtifact ? Lance.GameData.ArtifactProbData : Lance.GameData.AncientArtifactProbData;

                List<MultiReward> rewards = new List<MultiReward>();

                for (int i = 0; i < spawnCount; ++i)
                {
                    int index = Util.RandomChoose(probData.prob);

                    string artifactId = isArtifact ? $"Artifact_{index + 1}" : $"AncientArtifact_{index + 1}";

                    var artifactData = DataUtil.GetArtifactData(artifactId);
                    if (artifactData == null)
                        continue;

                    rewards.Add(new MultiReward()
                    {
                        ItemType = isArtifact ? ItemType.Artifact : ItemType.AncientArtifact,
                        Id = artifactData.id,
                        Count = 1,
                    });
                }

                spawnInfo.StackSpawnCount(spawnCount);

                rewardResult.artifacts = rewards.ToArray();

                rewards = null;

                Spawn.SetIsChangedData(true);

                return rewardResult;
            }
        }
    }
}