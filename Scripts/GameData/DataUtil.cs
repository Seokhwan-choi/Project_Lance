using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using CodeStage.AntiCheat.ObscuredTypes;

namespace Lance
{
    static class DataUtil
    {
        static ItemType[] EquipmentTypes = new ItemType[] { ItemType.Weapon, ItemType.Armor, ItemType.Gloves, ItemType.Shoes };
        static ItemType[] AccessoryTypes = new ItemType[] { ItemType.Necklace, ItemType.Earring, ItemType.Ring };
        static SubGrade[] EquipmentSubGrades = new SubGrade[] { SubGrade.One_Star, SubGrade.Two_Star, SubGrade.Three_Star, SubGrade.Four_Star, SubGrade.Five_Star };
        static SubGrade[] AccessorySubGrades = new SubGrade[] { SubGrade.One_Star, SubGrade.Two_Star, SubGrade.Three_Star };
        static SkillType[] SkillTypes = new SkillType[] { SkillType.Active, SkillType.Passive };
        public static StatType[] PetEvolutionStatTypes = new StatType[] { StatType.AtkRatio, StatType.HpRatio, StatType.CriProb, StatType.CriDmg, StatType.AtkSpeedRatio, StatType.MoveSpeedRatio, StatType.AddDmg, StatType.BossDmg, StatType.MonsterDmg, StatType.ExpAmount, StatType.GoldAmount, StatType.SkillDmg };
        public static StatType[] EquipmentOptionStatTypes = new StatType[] { StatType.AtkRatio, StatType.HpRatio, StatType.CriProb, StatType.CriDmg, StatType.AtkSpeedRatio, StatType.MoveSpeedRatio, StatType.AddDmg, StatType.BossDmg, StatType.MonsterDmg, StatType.ExpAmount, StatType.GoldAmount, StatType.SkillDmg };

        public static (bool canTrain, double totalRequireGold) GetGoldTrainTotalRequireGold(StatType type, int curLevel, int trainCount)
        {
            bool canTrain = true;
            double totalRequireGold = 0;

            for (int count = 1; count <= trainCount; ++count)
            {
                int nextLevel = curLevel + count;

                double requireGold = GetGoldTrainRequireGold(type, nextLevel);
                if (requireGold > 0)
                {
                    totalRequireGold += requireGold;
                }
                else
                {
                    canTrain = false;

                    break;
                }
            }

            return (canTrain, totalRequireGold);
        }

        public static int GetEquipmentReforgeMaxStep(Grade grade)
        {
            var reforgeData = Lance.GameData.ReforgeData.TryGet(grade);

            return reforgeData?.maxReforge ?? int.MaxValue;
        }

        public static int GetPetEquipmentReforgeMaxStep(Grade grade)
        {
            var reforgeData = Lance.GameData.PetEquipmentReforgeData.TryGet(grade);

            return reforgeData?.maxReforge ?? int.MaxValue;
        }

        public static int GetAccessoryReforgeMaxStep(Grade grade)
        {
            var reforgeData = Lance.GameData.AccessoryReforgeData.TryGet(grade);

            return reforgeData?.maxReforge ?? int.MaxValue;
        }

        public static EventData GetEventData(EventType eventType)
        {
            return Lance.GameData.EventData.Values.Where(x => x.eventType == eventType).FirstOrDefault();
        }

        public static int GetGoldTrainMaxLevel(StatType type)
        {
            switch (type)
            {
                case StatType.Hp:
                case StatType.Atk:
                    return Lance.GameData.GoldTrainAtkNHpData.Count();
                case StatType.CriProb:
                case StatType.CriDmg:
                    return Lance.GameData.GoldTrainCriData.Count();
                case StatType.SuperCriProb:
                case StatType.SuperCriDmg:
                    return Lance.GameData.GoldTrainSuperCriData.Count();
                case StatType.GoldAmount:
                case StatType.ExpAmount:
                    return Lance.GameData.GoldTrainGoldNExpData.Count();
                case StatType.AmplifyAtk:
                case StatType.AmplifyHp:
                    return Lance.GameData.GoldTrainAmplifyData.Count();
                case StatType.FireAddDmg:
                case StatType.WaterAddDmg:
                case StatType.GrassAddDmg:
                    return Lance.GameData.GoldTrainElementalData.Count();
                case StatType.PowerAtk:
                case StatType.PowerHp:
                default:
                    return Lance.GameData.GoldTrainPowerAtkNHpData.Count();

            }
        }

        public static double GetGoldTrainRequireGold(StatType type, int level)
        {
            if (level <= 0)
                return 0;

            int levelIndex = level - 1;

            switch (type)
            {
                case StatType.Atk:
                case StatType.Hp:
                    {
                        if (Lance.GameData.GoldTrainAtkNHpData.Count <= levelIndex)
                            return 0;
                        else
                            return Lance.GameData.GoldTrainAtkNHpData[levelIndex].requireGold;
                    }
                case StatType.PowerAtk:
                case StatType.PowerHp:
                    {
                        if (Lance.GameData.GoldTrainPowerAtkNHpData.Count <= levelIndex)
                            return 0;
                        else
                            return Lance.GameData.GoldTrainPowerAtkNHpData[levelIndex].requireGold;
                    }
                case StatType.CriProb:
                case StatType.CriDmg:
                    {
                        if (Lance.GameData.GoldTrainCriData.Count <= levelIndex)
                            return 0;
                        else
                            return Lance.GameData.GoldTrainCriData[levelIndex].requireGold;
                    }
                case StatType.SuperCriProb:
                case StatType.SuperCriDmg:
                    {
                        if (Lance.GameData.GoldTrainSuperCriData.Count <= levelIndex)
                            return 0;
                        else
                            return Lance.GameData.GoldTrainSuperCriData[levelIndex].requireGold;
                    }
                case StatType.GoldAmount:
                case StatType.ExpAmount:
                    {
                        if (Lance.GameData.GoldTrainGoldNExpData.Count <= levelIndex)
                            return 0;
                        else
                            return Lance.GameData.GoldTrainGoldNExpData[levelIndex].requireGold;
                    }
                case StatType.AmplifyAtk:
                case StatType.AmplifyHp:
                    {
                        if (Lance.GameData.GoldTrainAmplifyData.Count <= levelIndex)
                            return 0;
                        else
                            return Lance.GameData.GoldTrainAmplifyData[levelIndex].requireGold;
                    }
                case StatType.FireAddDmg:
                case StatType.WaterAddDmg:
                case StatType.GrassAddDmg:
                    {
                        if (Lance.GameData.GoldTrainElementalData.Count <= levelIndex)
                            return 0;
                        else
                            return Lance.GameData.GoldTrainElementalData[levelIndex].requireGold;
                    }
                default:
                    return 0;
            }
        }

        public static AchievementData GetAchievementDataByKey(string achievementKey)
        {
            return Lance.GameData.AchievementData.Values.Where(x => x.key == achievementKey).FirstOrDefault();
        }

        public static double GetGoldTrainStatValue(StatType type, int level)
        {
            if (level <= 0)
                return 0;

            int levelIndex = level - 1;

            switch (type)
            {
                case StatType.Atk:
                case StatType.Hp:
                    {
                        if (Lance.GameData.GoldTrainAtkNHpData.Count <= levelIndex)
                            return 0;

                        var data = Lance.GameData.GoldTrainAtkNHpData[levelIndex];

                        if (type == StatType.Atk)
                            return data.atkValue;
                        else
                            return data.hpValue;
                    }
                case StatType.PowerAtk:
                case StatType.PowerHp:
                    {
                        if (Lance.GameData.GoldTrainPowerAtkNHpData.Count <= levelIndex)
                            return 0;

                        var data = Lance.GameData.GoldTrainPowerAtkNHpData[levelIndex];

                        if (type == StatType.PowerAtk)
                            return data.atkValue;
                        else
                            return data.hpValue;
                    }
                case StatType.CriProb:
                case StatType.CriDmg:
                    {
                        if (Lance.GameData.GoldTrainCriData.Count <= levelIndex)
                            return 0;

                        var data = Lance.GameData.GoldTrainCriData[levelIndex];
                        if (type == StatType.CriProb)
                            return data.criProbValue;
                        else
                            return data.criDmgValue;
                    }
                case StatType.SuperCriProb:
                case StatType.SuperCriDmg:
                    {
                        if (Lance.GameData.GoldTrainSuperCriData.Count <= levelIndex)
                            return 0;

                        var data = Lance.GameData.GoldTrainSuperCriData[levelIndex];
                        if (type == StatType.SuperCriProb)
                            return data.criProbValue;
                        else
                            return data.criDmgValue;
                    }
                case StatType.GoldAmount:
                case StatType.ExpAmount:
                    {
                        if (Lance.GameData.GoldTrainGoldNExpData.Count <= levelIndex)
                            return 0;

                        var data = Lance.GameData.GoldTrainGoldNExpData[levelIndex];
                        if (type == StatType.GoldAmount)
                            return data.goldAmountValue;
                        else
                            return data.expAmountValue;
                    }
                case StatType.AmplifyAtk:
                case StatType.AmplifyHp:
                    {
                        if (Lance.GameData.GoldTrainAmplifyData.Count <= levelIndex)
                            return 0;

                        var data = Lance.GameData.GoldTrainAmplifyData[levelIndex];
                        if (type == StatType.AmplifyAtk)
                            return data.atkAmplifyValue;
                        else
                            return data.hpAmplifyValue;
                    }
                case StatType.FireAddDmg:
                case StatType.WaterAddDmg:
                case StatType.GrassAddDmg:
                    {
                        if (Lance.GameData.GoldTrainElementalData.Count <= levelIndex)
                            return 0;

                        var data = Lance.GameData.GoldTrainElementalData[levelIndex];
                        if (type == StatType.FireAddDmg)
                            return data.fireAddDmgValue;
                        else if (type == StatType.WaterAddDmg)
                            return data.waterAddDmgValue;
                        else
                            return data.grassAddDmgValue;
                    }
                default:
                    return 0;
            }
        }

        public static int GetGoldTrainStatMaxLevel(StatType type)
        {
            switch (type)
            {
                case StatType.Atk:
                case StatType.Hp:
                    return Lance.GameData.GoldTrainAtkNHpData.Count();
                case StatType.PowerAtk:
                case StatType.PowerHp:
                    return Lance.GameData.GoldTrainPowerAtkNHpData.Count();
                case StatType.CriProb:
                case StatType.CriDmg:
                    return Lance.GameData.GoldTrainCriData.Count();
                case StatType.SuperCriProb:
                case StatType.SuperCriDmg:
                    return Lance.GameData.GoldTrainSuperCriData.Count();
                case StatType.GoldAmount:
                case StatType.ExpAmount:
                    return Lance.GameData.GoldTrainGoldNExpData.Count();
                case StatType.AmplifyAtk:
                case StatType.AmplifyHp:
                    return Lance.GameData.GoldTrainAmplifyData.Count();
                case StatType.FireAddDmg:
                case StatType.WaterAddDmg:
                case StatType.GrassAddDmg:
                    return Lance.GameData.GoldTrainElementalData.Count();
                default:
                    return 0;
            }
        }

        public static IEnumerable<AbilityData> GetAbilityDatasByStep(int step)
        {
            return Lance.GameData.AbilityData.Values.Where(x => x.step == step);
        }

        public static IEnumerable<PassData> GetPassDatas(PassType passType)
        {
            return Lance.GameData.PassData.Values.Where(x => x.type == passType);
        }

        public static IEnumerable<StageData> GetStageDatas(StageDifficulty diff, int chap)
        {
            return Lance.GameData.StageData.Values.Where(x => x.diff == diff && x.chapter == chap);
        }

        public static StageData GetStageData(StageDifficulty diff, int chapter, int stage)
        {
            string key = $"{diff}_{chapter}_{stage}";

            return Lance.GameData.StageData.TryGet(key);
        }

        public static MonsterStatData GetMonsterStatData(MonsterData data, int level)
        {
            if (data.type == MonsterType.monster)
                return Lance.GameData.MonsterStatData.TryGet(level);
            else
                return Lance.GameData.BossStatData.TryGet(level);
        }

        public static MonsterData GetBossData(StageData stageData)
        {
            var stageMonsterData = GetStageMonsterData(stageData.chapter, stageData.stage, stageData.type);

            MonsterData bossData = null;

            string[] boss = stageMonsterData.boss.SplitByDelim();

            var bossId = Util.RandomSelect(boss);
            if (bossId.IsValid())
            {
                bossData = Lance.GameData.BossData.TryGet(bossId);
                if (bossData == null)
                    bossData = Lance.GameData.RaidBossData.TryGet(bossId);
                if (bossData == null)
                    bossData = Lance.GameData.AncientBossData.TryGet(bossId);
                if (bossData == null)
                    bossData = Lance.GameData.PetBossData.TryGet(bossId);
            }

            return bossData;
        }

        public static MonsterData GetRandomStageMonster(StageData stageData)
        {
            var stageMonsterData = GetStageMonsterData(stageData.chapter, stageData.stage, stageData.type);
            if (stageMonsterData != null && stageMonsterData.monsters.IsValid())
            {
                string[] monsters = stageMonsterData.monsters.SplitByDelim();

                var monsterId = Util.RandomSelect(monsters);
                if (monsterId.IsValid())
                {
                    return Lance.GameData.MonsterData.TryGet(monsterId);
                }
            }

            return null;
        }

        private static StageMonsterData GetStageMonsterData(int chapter, int stage, StageType type)
        {
            if (type == StageType.Normal)
            {
                return Lance.GameData.StageMonsterData.Where(x => x.chapter == chapter && x.stage == stage).FirstOrDefault();
            }
            else
            {
                if (type == StageType.Raid)
                    return Lance.GameData.StageMonsterData.Where(x => x.chapter == chapter && x.type == type).FirstOrDefault();
                else if (type == StageType.LimitBreak || type == StageType.UltimateLimitBreak)
                    return Lance.GameData.StageMonsterData.Where(x => x.chapter == chapter && x.stage == stage && x.type == type).FirstOrDefault();
                else
                    return Lance.GameData.StageMonsterData.Where(x => x.type == type).FirstOrDefault();
            }
        }

        public static ArtifactLevelUpData GetArtifactLevelUpData(string id, int level)
        {
            return GetArtifactLevelUpData(IsAncientArtifact(id), level);
        }

        public static ArtifactLevelUpData GetArtifactLevelUpData(bool isAncient, int level)
        {
            if (isAncient)
                return Lance.GameData.AncientArtifactLevelUpData.TryGet(level);
            else
                return Lance.GameData.ArtifactLevelUpData.TryGet(level);
        }

        public static int GetAbilityMaxLevel(string id)
        {
            var data = Lance.GameData.AbilityData.TryGet(id);

            return data?.maxLevel ?? int.MaxValue;
        }

        public static float GetArtifactUpgradeProb(string id, int level)
        {
            return GetArtifactUpgradeProb(IsAncientArtifact(id), level);
        }

        public static float GetArtifactUpgradeProb(bool isAncient, int level)
        {
            if (isAncient)
            {
                var ancientData = Lance.GameData.AncientArtifactLevelUpData.TryGet(level);

                return ancientData?.prob ?? 0f;
            }
            else
            {
                var data = Lance.GameData.ArtifactLevelUpData.TryGet(level);

                return data?.prob ?? 0f;
            }
        }

        public static int GetArtifactMaxLevel(string id)
        {
            return GetArtifactMaxLevel(IsAncientArtifact(id));
        }

        public static int GetArtifactMaxLevel(bool isAncient)
        {
            if (isAncient)
                return Lance.GameData.AncientArtifactLevelUpData.Values.Max(x => x.level) + 1;
            else
                return Lance.GameData.ArtifactLevelUpData.Values.Max(x => x.level) + 1;
        }

        public static bool IsAncientArtifact(string id)
        {
            return Lance.GameData.AncientArtifactData.ContainsKey(id);
        }

        public static ArtifactData GetArtifactData(string id)
        {
            ArtifactData artifactData = Lance.GameData.ArtifactData.TryGet(id);
            if (artifactData == null)
                artifactData = Lance.GameData.AncientArtifactData.TryGet(id);

            return artifactData;
        }

        public static int GetAbilityMaxStep()
        {
            return Lance.GameData.AbilityData.Values.Max(x => x.step);
        }

        public static bool IsMaxStepAbility(string id)
        {
            int step = Lance.GameData.AbilityData.TryGet(id)?.step ?? 0;
            int maxStep = GetAbilityMaxStep();

            return step == maxStep;
        }

        public static PlayerLevelUpData GetPlayerLevelUpData(int level)
        {
            return Lance.GameData.PlayerLevelUpData.TryGet(level);
        }

        public static FriendShipLevelUpData GetFriendShipLevelUpData(int level)
        {
            return Lance.GameData.FriendShipLevelUpData.TryGet(level);
        }

        public static int GetFriendShipVisitExp(int level)
        {
            var data = Lance.GameData.FriendShipVisitExpData.TryGet(level);

            return data?.exp ?? 0;
        }

        public static double GetCostumeUpgradeRequireStone(Grade costumeGrade, int level)
        {
            CostumeUpgradeData upgradeData = Lance.GameData.CostumeUpgradeRequireStoneData.TryGet(costumeGrade);

            int levelIndex = level - 1;

            if (levelIndex < 0 || upgradeData.require.Length <= levelIndex)
                return double.MaxValue;

            return upgradeData.baseRequire + upgradeData.require[levelIndex];
        }

        public static double GetCostumeUpgradeRequire(Grade costumeGrade, int level)
        {
            CostumeUpgradeData upgradeData = Lance.GameData.CostumeUpgradeRequireData.TryGet(costumeGrade);

            int levelIndex = level - 1;

            if (levelIndex < 0 || upgradeData.require.Length <= levelIndex)
                return double.MaxValue;

            return upgradeData.baseRequire + upgradeData.require[levelIndex];
        }

        public static double GetEquipmentUpgradeRequireStone(EquipmentData equipData, int level, int upgradeCount)
        {
            if (equipData == null)
                return double.MaxValue;

            UpgradeData upgradeData = GetUpgradeData(equipData.grade, equipData.subGrade);

            double totalRequireStone = 0;

            for(int j = 0; j < upgradeCount; ++j)
            {
                int reforge = 1;
                int reforgeMaxLevel = 0;
                double requireStone = upgradeData.baseRequire;

                for (int i = 0; i < level + j; ++i) // ex) 110
                {
                    if (i < equipData.maxLevel)
                    {
                        requireStone += upgradeData.reforge[0];
                    }
                    else
                    {
                        if (upgradeData.reforge.Length <= reforge)
                            break;

                        requireStone += upgradeData.reforge[reforge];

                        reforgeMaxLevel++;
                        if (reforgeMaxLevel >= Lance.GameData.EquipmentCommonData.reforgeAddMaxLevel)
                        {
                            reforgeMaxLevel = 0;

                            reforge++;
                        }
                    }
                }

                totalRequireStone += requireStone;
            }

            return totalRequireStone;
        }

        public static double GetPetEquipmentUpgradeRequireStone(PetEquipmentData equipData, int level, int upgradeCount)
        {
            if (equipData == null)
                return double.MaxValue;

            PetEquipmentUpgradeData upgradeData = GetPetEquipmentUpgradeData(equipData.grade, equipData.subGrade);

            double totalRequireStone = 0;

            for (int j = 0; j < upgradeCount; ++j)
            {
                int reforge = 1;
                int reforgeMaxLevel = 0;
                double requireStone = upgradeData.baseRequire;

                for (int i = 0; i < level + j; ++i) // ex) 110
                {
                    if (i < equipData.maxLevel)
                    {
                        requireStone += upgradeData.reforge[0];
                    }
                    else
                    {
                        if (upgradeData.reforge.Length <= reforge)
                            break;

                        requireStone += upgradeData.reforge[reforge];

                        reforgeMaxLevel++;
                        if (reforgeMaxLevel >= Lance.GameData.PetEquipmentCommonData.reforgeAddMaxLevel)
                        {
                            reforgeMaxLevel = 0;

                            reforge++;
                        }
                    }
                }

                totalRequireStone += requireStone;
            }

            return totalRequireStone;
        }

        public static double GetAccessoryUpgradeRequireStone(AccessoryData accessoryData, int level, int upgradeCount)
        {
            if (accessoryData == null)
                return double.MaxValue;

            UpgradeData upgradeData = GetAccessoryUpgradeData(accessoryData.grade, accessoryData.subGrade);

            double totalRequireStone = 0;

            for (int j = 0; j < upgradeCount; ++j)
            {
                int reforge = 1;
                int reforgeMaxLevel = 0;
                double requireStone = upgradeData.baseRequire;

                for (int i = 0; i < level + j; ++i) // ex) 110
                {
                    if (i < accessoryData.maxLevel)
                    {
                        requireStone += upgradeData.reforge[0];
                    }
                    else
                    {
                        if (upgradeData.reforge.Length <= reforge)
                            break;

                        requireStone += upgradeData.reforge[reforge];

                        reforgeMaxLevel++;
                        if (reforgeMaxLevel >= Lance.GameData.AccessoryCommonData.reforgeAddMaxLevel)
                        {
                            reforgeMaxLevel = 0;

                            reforge++;
                        }
                    }
                }

                totalRequireStone += requireStone;
            }

            return totalRequireStone;
        }

        public static double GetEquipmentReforgeRequireStone(Grade grade, int reforge)
        {
            var reporgeRequireData = Lance.GameData.ReforgeRequireStoneData.TryGet(grade);
            if (reporgeRequireData == null)
                return double.MaxValue;

            if (reporgeRequireData.require.Length <= reforge || reforge < 0)
                return double.MaxValue;

            return reporgeRequireData.require[reforge];
        }

        public static double GetPetEquipmentReforgeRequireStone(Grade grade, int reforge)
        {
            var reporgeRequireData = Lance.GameData.PetEquipmentReforgeRequireStoneData.TryGet(grade);
            if (reporgeRequireData == null)
                return double.MaxValue;

            if (reporgeRequireData.require.Length <= reforge || reforge < 0)
                return double.MaxValue;

            return reporgeRequireData.require[reforge];
        }

        public static double GetAccessoryReforgeRequireStone(Grade grade, int reforge)
        {
            var reporgeRequireData = Lance.GameData.AccessoryReforgeRequireStoneData.TryGet(grade);
            if (reporgeRequireData == null)
                return double.MaxValue;

            if (reporgeRequireData.require.Length <= reforge || reforge < 0)
                return double.MaxValue;

            return reporgeRequireData.require[reforge];
        }

        public static int GetPetEquipmentReforgeRequireElementalStone(Grade grade, int reforge)
        {
            var reporgeRequireData = Lance.GameData.PetEquipmentReforgeRequireElementalStoneData.TryGet(grade);
            if (reporgeRequireData == null)
                return int.MaxValue;

            if (reporgeRequireData.require.Length <= reforge || reforge < 0)
                return int.MaxValue;

            return reporgeRequireData.require[reforge];
        }

        public static float GetEquipmentReforgeProb(Grade grade, int reporge)
        {
            var reporgeProbData = Lance.GameData.ReforgeProbData.TryGet(grade);
            if (reporgeProbData == null)
                return 0;

            if (reporgeProbData.probs.Length <= reporge || reporge < 0)
                return 0;

            return reporgeProbData.probs[reporge];
        }

        public static float GetPetEquipmentReforgeProb(Grade grade, int reporge)
        {
            var reporgeProbData = Lance.GameData.PetEquipmentReforgeProbData.TryGet(grade);
            if (reporgeProbData == null)
                return 0;

            if (reporgeProbData.probs.Length <= reporge || reporge < 0)
                return 0;

            return reporgeProbData.probs[reporge];
        }

        public static float GetAccessoryReforgeProb(Grade grade, int reporge)
        {
            var reporgeProbData = Lance.GameData.AccessoryReforgeProbData.TryGet(grade);
            if (reporgeProbData == null)
                return 0;

            if (reporgeProbData.probs.Length <= reporge || reporge < 0)
                return 0;

            return reporgeProbData.probs[reporge];
        }

        public static float GetEquipmentReforgeFailBonusProb(Grade grade, int reporge, int failCount)
        {
            var reporgeProbData = Lance.GameData.ReforgeProbData.TryGet(grade);
            if (reporgeProbData == null)
                return 0;

            if (reporgeProbData.probs.Length <= reporge || reporge < 0)
                return 0;

            float orgProb = reporgeProbData.probs[reporge];
            float bonusProb = orgProb * Lance.GameData.EquipmentCommonData.reforgeFailedBonusProbValue * failCount;

            return bonusProb;
        }

        public static float GetPetEquipmentReforgeFailBonusProb(Grade grade, int reporge, int failCount)
        {
            var reporgeProbData = Lance.GameData.PetEquipmentReforgeProbData.TryGet(grade);
            if (reporgeProbData == null)
                return 0;

            if (reporgeProbData.probs.Length <= reporge || reporge < 0)
                return 0;

            float orgProb = reporgeProbData.probs[reporge];
            float bonusProb = orgProb * Lance.GameData.PetEquipmentCommonData.reforgeFailedBonusProbValue * failCount;

            return bonusProb;
        }

        public static float GetAccessoryReforgeFailBonusProb(Grade grade, int reporge, int failCount)
        {
            var reporgeProbData = Lance.GameData.AccessoryReforgeProbData.TryGet(grade);
            if (reporgeProbData == null)
                return 0;

            if (reporgeProbData.probs.Length <= reporge || reporge < 0)
                return 0;

            float orgProb = reporgeProbData.probs[reporge];
            float bonusProb = orgProb * Lance.GameData.AccessoryCommonData.reforgeFailedBonusProbValue * failCount;

            return bonusProb;
        }

        public static UpgradeData GetUpgradeData(Grade grade, SubGrade subGrade)
        {
            return Lance.GameData.UpgradeData.
                Where(x => x.grade == grade && x.subGrade == subGrade).
                FirstOrDefault();
        }

        public static PetEquipmentUpgradeData GetPetEquipmentUpgradeData(Grade grade, SubGrade subGrade)
        {
            return Lance.GameData.PetEquipmentUpgradeData.
                Where(x => x.grade == grade && x.subGrade == subGrade).
                FirstOrDefault();
        }

        public static UpgradeData GetAccessoryUpgradeData(Grade grade, SubGrade subGrade)
        {
            return Lance.GameData.AccessoryUpgradeData.
                Where(x => x.grade == grade && x.subGrade == subGrade).
                FirstOrDefault();
        }

        public static EquipmentData GetEquipmentData(ItemType type, Grade grade, SubGrade subGrade)
        {
            switch (type)
            {
                case ItemType.Weapon:
                    return Lance.GameData.WeaponData.Values.Where(x => x.grade == grade && x.subGrade == subGrade).FirstOrDefault();
                case ItemType.Armor:
                    return Lance.GameData.ArmorData.Values.Where(x => x.grade == grade && x.subGrade == subGrade).FirstOrDefault();
                case ItemType.Gloves:
                    return Lance.GameData.GlovesData.Values.Where(x => x.grade == grade && x.subGrade == subGrade).FirstOrDefault();
                case ItemType.Shoes:
                default:
                    return Lance.GameData.ShoesData.Values.Where(x => x.grade == grade && x.subGrade == subGrade).FirstOrDefault();
            }
        }

        public static AccessoryData GetAccessoryData(ItemType type, Grade grade, SubGrade subGrade)
        {
            switch (type)
            {
                case ItemType.Necklace:
                    return Lance.GameData.NecklaceData.Values.Where(x => x.grade == grade && x.subGrade == subGrade).FirstOrDefault();
                case ItemType.Earring:
                    return Lance.GameData.EarringData.Values.Where(x => x.grade == grade && x.subGrade == subGrade).FirstOrDefault();
                default:
                case ItemType.Ring:
                    return Lance.GameData.RingData.Values.Where(x => x.grade == grade && x.subGrade == subGrade).FirstOrDefault();
            }
        }

        public static PetEquipmentData GetPetEquipmentData(ElementalType type, Grade grade, SubGrade subGrade)
        {
            return Lance.GameData.PetEquipmentData.Values.Where(x => x.type == type && x.grade == grade && x.subGrade == subGrade).FirstOrDefault();
        }

        public static int GetPackageShopDataMaxStep(string id)
        {
            return Lance.GameData.PackageShopData.Where(x => x.id == id).Max(x => x.step);
        }

        public static EquipmentData GetRandomEquipmentData(string randomRewardDataId)
        {
            if (randomRewardDataId.IsValid())
            {
                var data = Lance.GameData.RandomEquipmentRewardData.TryGet(randomRewardDataId);
                if (data != null)
                {
                    ItemType randomType = Util.RandomSelect(EquipmentTypes);
                    SubGrade subGrade = data.subGrade;
                    if (data.subGrade == SubGrade.Any)
                    {
                        subGrade = Util.RandomSelect(EquipmentSubGrades);
                    }

                    return GetEquipmentData(randomType, data.grade, subGrade);
                }
            }

            return null;
        }

        public static AccessoryData GetRandomAccessoryData(string randomRewardDataId)
        {
            if (randomRewardDataId.IsValid())
            {
                var data = Lance.GameData.RandomAccessoryRewardData.TryGet(randomRewardDataId);
                if (data != null)
                {
                    ItemType randomType = Util.RandomSelect(AccessoryTypes);
                    SubGrade subGrade = data.subGrade;
                    if (data.subGrade == SubGrade.Any)
                    {
                        subGrade = Util.RandomSelect(AccessorySubGrades);
                    }

                    return GetAccessoryData(randomType, data.grade, subGrade);
                }
            }

            return null;
        }

        public static SkillData GetRandomSkillData(string randomRewardDataId)
        {
            if (randomRewardDataId.IsValid())
            {
                var data = Lance.GameData.RandomSkillRewardData.TryGet(randomRewardDataId);
                if (data != null)
                {
                    SkillType randomType = Util.RandomSelect(SkillTypes);

                    var skillDatas = GetSkillDatas(randomType, data.grade);

                    return Util.Shuffle(skillDatas).FirstOrDefault();
                }
            }

            return null;
        }

        public static EquipmentData GetEquipmentData(string id)
        {
            EquipmentData data = Lance.GameData.WeaponData.TryGet(id);
            if (data == null)
            {
                data = Lance.GameData.ArmorData.TryGet(id);
                if (data == null)
                {
                    data = Lance.GameData.GlovesData.TryGet(id);
                    if (data == null)
                    {
                        data = Lance.GameData.ShoesData.TryGet(id);
                    }
                }
            }

            return data;
        }

        public static AccessoryData GetAccessoryData(string id)
        {
            AccessoryData data = Lance.GameData.NecklaceData.TryGet(id);
            if (data == null)
            {
                data = Lance.GameData.EarringData.TryGet(id);
                if (data == null)
                {
                    data = Lance.GameData.RingData.TryGet(id);
                }
            }

            return data;
        }

        public static MonsterRewardData GetMonsterRewardData(StageType stageType, string reward)
        {
            switch (stageType)
            {
                
                case StageType.Gold:
                    return Lance.GameData.GoldDungeonMonsterRewardData.TryGet(reward);
                case StageType.Stone:
                    return Lance.GameData.StoneDungeonMonsterRewardData.TryGet(reward);
                case StageType.Pet:
                    return Lance.GameData.PetDungeonMonsterRewardData.TryGet(reward);
                case StageType.Reforge:
                    return Lance.GameData.ReforgeDungeonMonsterRewardData.TryGet(reward);
                case StageType.Growth:
                    return Lance.GameData.GrowthDungeonMonsterRewardData.TryGet(reward);
                case StageType.Ancient:
                    return Lance.GameData.AncientDungeonMonsterRewardData.TryGet(reward);
                case StageType.Accessory:
                    return Lance.GameData.DemonicRealmMonsterRewardData.TryGet(reward);
                case StageType.Normal:
                default:
                    return Lance.GameData.MonsterRewardData.TryGet(reward);
            };
        }

        public static RewardResult GetMonsterReward(StageDifficulty diff, StageType type, int chapter, int stage, MonsterRewardData rewardData, float rewardBonusValue = 1f)
        {
            // 골드, 강화석, 펫 먹이, 장비 순으로 확률
            float[] dropProbs = new float[] { rewardData.goldProb, rewardData.stonesProb, rewardData.reforgeStoneProb, rewardData.petFoodProb, rewardData.ancientEssenceProb, rewardData.equipmentProb, rewardData.accessoryProb };

            int index = Util.RandomChoose(dropProbs);

            RewardResult reward = new RewardResult();

            if (index == 0)
            {
                reward.gold = rewardData.gold;
            }
            else if (index == 1)
            {
                reward.stones = rewardData.stones;
            }
            else if (index == 2)
            {
                reward.reforgeStones = rewardData.reforgeStone;
            }
            else if (index == 3)
            {
                reward.petFood = rewardData.petFood;
            }
            else if (index == 4)
            {
                reward.ancientEssence = rewardData.ancientEssence;
            }
            else if (index == 5)
            {
                if (rewardData.randomEquipment.IsValid())
                {
                    // 장비를 랜덤으로 획득한다.
                    EquipmentData equipmentData = GetRandomEquipmentData(rewardData.randomEquipment);
                    if (equipmentData != null)
                    {
                        reward.equipments = new MultiReward[]
                        {
                            new MultiReward()
                            {
                                ItemType = equipmentData.type,
                                Id = equipmentData.id,
                                Count = 1,
                            }
                        };
                    }
                }
            }
            else
            {
                if (rewardData.randomAccessory.IsValid())
                {
                    // 장신구를 랜덤으로 획득한다.
                    AccessoryData accessoryData = GetRandomAccessoryData(rewardData.randomAccessory);
                    if (accessoryData != null)
                    {
                        reward.accessorys = new MultiReward[]
                        {
                            new MultiReward()
                            {
                                ItemType = accessoryData.type,
                                Id = accessoryData.id,
                                Count = 1,
                            }
                        };
                    }
                }
            }

            reward.exp = rewardData.exp;

            // 활성화되어 있는 이벤트를 모두 확인하여 드랍 가능한지 체크
            var eventCurrencys = new List<MultiReward>();
            foreach (var data in Lance.GameData.EventData.Values)
            {
                if (data.active)
                {
                    if (data.startDate > 0 && data.endDate > 0)
                    {
                        if (TimeUtil.IsActiveDateNum(data.startDate, data.endDate))
                        {
                            if (Util.Dice(data.currencyDropProb))
                            {
                                eventCurrencys.Add(new MultiReward(ItemType.EventCurrency, data.id, data.currencyDropAmount));
                            }
                        }
                    }
                }
            }

            if (type.IsNormal())
            {
                if (diff >= Lance.GameData.StageCommonData.essenceDropDiff)
                {
                    if (Util.Dice(Lance.GameData.StageCommonData.essenceDropProb))
                    {
                        int minAmount = Lance.GameData.StageCommonData.essenceMinDropAmount;
                        int maxAmount = Lance.GameData.StageCommonData.essenceMaxDropAmount;

                        int randAmount = minAmount == maxAmount ? minAmount : UnityEngine.Random.Range(minAmount, maxAmount + 1);

                        reward.essences = reward.essences.AddEssence(ChangeToEssenceType(chapter), randAmount);
                    }
                }

                if (diff >= Lance.GameData.StageCommonData.threadDropDiff)
                {
                    if (Util.Dice(Lance.GameData.StageCommonData.threadDropProb))
                    {
                        reward.costumeUpgrade = reward.costumeUpgrade + Lance.GameData.StageCommonData.threadDropAmount;
                    }
                }
            }

            if (eventCurrencys.Count() > 0)
                reward.eventCurrencys = eventCurrencys.ToArray();

            float weekendgoldBonusValue = WeekendDoubleUtil.GetValue(ItemType.Gold);
            float weekendexpBonusValue = WeekendDoubleUtil.GetValue(ItemType.Exp);
            float weekendpetFoodBonusValue = WeekendDoubleUtil.GetValue(ItemType.PetFood);
            float weekendstoneBonusValue = WeekendDoubleUtil.GetValue(ItemType.UpgradeStone);
            float weekendreforgeStoneBonusValue = WeekendDoubleUtil.GetValue(ItemType.ReforgeStone);
            float weekendthreadBonusValue = WeekendDoubleUtil.GetValue(ItemType.CostumeUpgrade);

            reward.gold = reward.gold * weekendgoldBonusValue;
            reward.exp = reward.exp * weekendexpBonusValue;
            reward.petFood = Mathf.RoundToInt(reward.petFood * weekendpetFoodBonusValue);
            reward.stones = reward.stones * weekendstoneBonusValue;
            reward.reforgeStones = reward.reforgeStones * weekendreforgeStoneBonusValue;
            reward.costumeUpgrade = reward.costumeUpgrade * weekendthreadBonusValue;

            var demoniceRealmSoulStoneRewardData = GetDemonicRealmSoulStoneRewardData(type, stage);
            if (demoniceRealmSoulStoneRewardData != null)
            {
                if (Util.Dice(demoniceRealmSoulStoneRewardData.dropProb))
                {
                    reward.soulStone = reward.soulStone + demoniceRealmSoulStoneRewardData.dropAmount;
                }
            }

            var demonicRealmManaEssenceRewardData = GetDemonicRealmManaEssenceRewardData(type, stage);
            if (demonicRealmManaEssenceRewardData != null)
            {
                if (Util.Dice(demonicRealmManaEssenceRewardData.dropProb))
                {
                    reward.manaEssence = reward.manaEssence + demonicRealmManaEssenceRewardData.dropAmount;
                }
            }

            dropProbs = null;

            if (rewardBonusValue > 1f)
                reward = reward.BonusReward(rewardBonusValue);

            return reward;
        }

        public static IEnumerable<SpawnPriceData> GetSpawnPriceDatas(ItemType itemType)
        {
            return Lance.GameData.SpawnPriceData.Where(x => x.type == itemType);
        }

        //public static double GetSpawnPriceOfOne(ItemType itemType)
        //{
        //    foreach(var data in GetSpawnPriceDatas(itemType))
        //    {
        //        if (data.spawnType == SpawnType.Gem &&
        //            data.spawnCount == 1)
        //        {
        //            return data.price;
        //        }
        //    }

        //    return 0;
        //}

        public static SpawnPriceData GetSpawnPriceData(string id)
        {
            return Lance.GameData.SpawnPriceData.Where(x => x.id == id).FirstOrDefault();
        }

        public static SpawnPriceData GetSpawnPriceData(ItemType itemType, SpawnType spawnType, bool autoSpawn)
        {
            SpawnPriceData data = Lance.GameData.SpawnPriceData.Where(x =>
                x.type == itemType &&
                x.spawnType == spawnType &&
                x.autoSpawn == autoSpawn).FirstOrDefault();

            return data;
        }

        public static SpawnPriceData GetSpawnPriceData(ItemType itemType, SpawnType spawnType)
        {
            SpawnPriceData data = Lance.GameData.SpawnPriceData.Where(x =>
                x.type == itemType &&
                x.spawnType == spawnType).FirstOrDefault();

            return data;
        }

        public static SpawnPriceData GetSpawnPriceData(ItemType type, SpawnType spawnType, int spawnCount)
        {
            SpawnPriceData data = Lance.GameData.SpawnPriceData.Where(x =>
                x.type == type &&
                x.spawnType == spawnType &&
                x.spawnCount == spawnCount).FirstOrDefault();

            return data;
        }

        public static IEnumerable<SpawnGradeProbData> GetSpawnGradeProbDatas(ItemType itemType)
        {
            var spawnData = Lance.GameData.SpawnData.TryGet(itemType);

            return Lance.GameData.SpawnGradeProbData.Where(x => x.id == spawnData.probId);
        }

        public static SpawnGradeProbData GetNextLevelSpawnGradeProbdata(string id, int level)
        {
            return Lance.GameData.SpawnGradeProbData.Where(x => x.id == id && x.level == level + 1).FirstOrDefault();
        }

        public static SpawnGradeProbData GetSpawnGradeProbDataByLevel(string id, int level)
        {
            return Lance.GameData.SpawnGradeProbData
                .Where(x => x.id == id && x.level == level)
                .FirstOrDefault();
        }

        public static int GetSpawnTotalRequireCount(string id, int level)
        {
            int totalRequire = 0;

            foreach (SpawnGradeProbData data in Lance.GameData.SpawnGradeProbData.Where(x => x.id == id))
            {
                totalRequire += data.requireStack;

                if (data.level == level)
                    break;
            }

            return totalRequire;
        }

        public static EquipmentSubgradeProbData GetEquipmentSubgradeProbData(Grade grade, int level)
        {
            return Lance.GameData.EquipmentSubgradeProbData.Where(x => x.grade == grade && x.level == level).FirstOrDefault();
        }

        public static float[] GetEquipmentSubgradeProbs(Grade grade, int level)
        {
            var data = GetEquipmentSubgradeProbData(grade, level);

            return data?.prob ?? Enumerable.Repeat(0f, (int)Grade.Count).ToArray();
        }

        public static IEnumerable<EquipmentSubgradeProbData> GetEquipmentSubgradeProbDatas(int level)
        {
            return Lance.GameData.EquipmentSubgradeProbData.Where(x => x.level == level);
        }

        public static float GetEquipmentSpawnProb(string id, int stackCount, Grade grade, SubGrade subGrade, int level)
        {
            var gradeProbData = GetSpawnGradeProbData(id, stackCount);
            var subGradeProbData = GetEquipmentSubgradeProbData(grade, level);

            float gradeProb = gradeProbData?.prob[(int)grade] ?? 0f;
            float subGradeProb = subGradeProbData?.prob[(int)subGrade] ?? 0f;

            return gradeProb * subGradeProb;
        }

        public static AccessorySubgradeProbData GetAccessorySubgradeProbData(Grade grade, int level)
        {
            return Lance.GameData.AccessorySubgradeProbData.Where(x => x.grade == grade && x.level == level).FirstOrDefault();
        }

        public static float[] GetAccessorySubgradeProbs(Grade grade, int level)
        {
            var data = GetAccessorySubgradeProbData(grade, level);

            return data?.prob ?? Enumerable.Repeat(0f, (int)Grade.Count).ToArray();
        }

        public static IEnumerable<AccessorySubgradeProbData> GetAccessorySubgradeProbDatas(int level)
        {
            return Lance.GameData.AccessorySubgradeProbData.Where(x => x.level == level);
        }

        public static float GetAccessorySpawnProb(string id, int stackCount, Grade grade, SubGrade subGrade, int level)
        {
            var gradeProbData = GetSpawnGradeProbData(id, stackCount);
            var subGradeProbData = GetAccessorySubgradeProbData(grade, level);

            float gradeProb = gradeProbData?.prob[(int)grade] ?? 0f;
            float subGradeProb = subGradeProbData?.prob[(int)subGrade] ?? 0f;

            return gradeProb * subGradeProb;
        }

        public static SpawnGradeProbData GetSpawnGradeProbData(string id, int stackCount)
        {
            SpawnGradeProbData resultData = null;

            int totalRequire = 0;

            foreach (SpawnGradeProbData data in Lance.GameData.SpawnGradeProbData.Where(x => x.id == id))
            {
                totalRequire += data.requireStack;

                if (totalRequire <= stackCount)
                    resultData = data;
                else
                    break;
            }

            return resultData;
        }

        public static SpawnRewardData GetSpawnRewardData(ItemType itemType, int level)
        {
            return Lance.GameData.SpawnRewardData
                .Where(x => x.type == itemType && x.level == level)
                .FirstOrDefault();
        }

        public static int GetSkillMaxLevel(string id)
        {
            var skillData = GetSkillData(id);
            if (skillData == null)
                return 0;

            return skillData.maxLevel;
        }

        public static double GetSkillValue(string id, int level)
        {
            SkillData data = GetSkillData(id);
            if (data == null)
                return 0;

            return data.skillValue + (data.levelUpValue * level);
        }

        public static SkillData GetSkillData(SkillType skillType, string id)
        {
            if (skillType == SkillType.Active)
            {
                return Lance.GameData.ActiveSkillData.TryGet(id);
            }
            else
            {
                return Lance.GameData.PassiveSkillData.TryGet(id);
            }
        }

        public static SkillData GetSkillData(string id)
        {
            SkillData skillData = Lance.GameData.ActiveSkillData.TryGet(id);
            if (skillData == null)
                skillData = Lance.GameData.PassiveSkillData.TryGet(id);

            return skillData;
        }

        public static SkillData GetFirstSkillData(SkillType skillType)
        {
            var datas = GetSkillDatas(skillType);

            return datas.FirstOrDefault();
        }

        public static IEnumerable<SkillData> GetSkillDatas(SkillType skillType)
        {
            if (skillType == SkillType.Active)
            {
                return Lance.GameData.ActiveSkillData.Values;
            }
            else
            {
                return Lance.GameData.PassiveSkillData.Values;
            }
        }

        public static IEnumerable<SkillData> GetSkillDatas(SkillType skillType, Grade grade)
        {
            return GetSkillDatas(skillType).Where(x => x.grade == grade);
        }

        public static EquipmentData GetNextGradeEquipmentData(ItemType type, Grade grade, SubGrade subGrade)
        {
            (Grade grade, SubGrade subGrade) next = NextGrade(grade, subGrade);

            return GetEquipmentData(type, next.grade, next.subGrade);
        }

        public static PetEquipmentData GetNextGradePetEquipmentData(ElementalType type, Grade grade, SubGrade subGrade)
        {
            (Grade grade, SubGrade subGrade) next = NextPetEquipmentGrade(grade, subGrade);

            return GetPetEquipmentData(type, next.grade, next.subGrade);
        }

        public static AccessoryData GetNextGradeAccessoryData(ItemType type, Grade grade, SubGrade subGrade)
        {
            (Grade grade, SubGrade subGrade) next = NextAccessoryGrade(grade, subGrade);

            return GetAccessoryData(type, next.grade, next.subGrade);
        }

        public static int GetAccessoryMaxEquipCount(ItemType type)
        {
            switch(type)
            {
                case ItemType.Ring:
                    return Lance.GameData.AccessoryCommonData.ringMaxEquipCount;
                case ItemType.Earring:
                    return Lance.GameData.AccessoryCommonData.earringMaxEquipCount;
                case ItemType.Necklace:
                default:
                    return Lance.GameData.AccessoryCommonData.necklaceMaxEquipCount;
            }
        }

        public static (Grade grade, SubGrade subGrade) NextGrade(Grade grade, SubGrade subGrade)
        {
            SubGrade nextSubGrade = (SubGrade)(((int)subGrade + 1) % (int)SubGrade.Count);

            Grade nextGrade = nextSubGrade == SubGrade.One_Star ?
                (Grade)((int)grade + 1) : grade;

            nextGrade = (Grade)Math.Min((int)Grade.SSR, (int)nextGrade);

            return (nextGrade, nextSubGrade);
        }

        public static (Grade grade, SubGrade subGrade) NextPetEquipmentGrade(Grade grade, SubGrade subGrade)
        {
            SubGrade nextSubGrade = (SubGrade)(((int)subGrade + 1) % (int)SubGrade.Four_Star);

            Grade nextGrade = nextSubGrade == SubGrade.One_Star ?
                (Grade)((int)grade + 1) : grade;

            nextGrade = (Grade)Math.Min((int)Grade.SSR, (int)nextGrade);

            return (nextGrade, nextSubGrade);
        }

        public static (Grade grade, SubGrade subGrade) NextAccessoryGrade(Grade grade, SubGrade subGrade)
        {
            SubGrade nextSubGrade = (SubGrade)(((int)subGrade + 1) % (int)SubGrade.Four_Star);

            Grade nextGrade = nextSubGrade == SubGrade.One_Star ?
                (Grade)((int)grade + 1) : grade;

            nextGrade = (Grade)Math.Min((int)Grade.SSR, (int)nextGrade);

            return (nextGrade, nextSubGrade);
        }

        public static int GetSkillLevelUpRequireCount(string id, int level, int count)
        {
            var skillData = GetSkillData(id);
            if (skillData == null)
                return int.MaxValue;

            SkillUpgradeData levelUpData;
            if (skillData.levelUpMaterial == SkillLevelUpMaterial.Skill)
                levelUpData = Lance.GameData.SkillUpgradeData.TryGet(skillData.grade);
            else
                levelUpData = Lance.GameData.SkillUpgradeData2.TryGet(skillData.grade);

            if (levelUpData == null)
                return int.MaxValue;

            int totalCount = 0;

            int endLevel = Mathf.Min(GetSkillMaxLevel(id), level + count);

            for(int i = level; i < endLevel; ++i)
            {
                int index = i - 1;

                if (levelUpData.requireCount.Length <= index)
                    continue;

                totalCount += levelUpData.requireCount[index];
            }

            return totalCount;
        }

        public static int GetSkillSlotUnlockLevel(int slot)
        {
            if (Lance.GameData.SkillSlotUnlockData.unlockLevel.Length <= slot)
                return int.MaxValue;

            return Lance.GameData.SkillSlotUnlockData.unlockLevel[slot];
        }

        public static IEnumerable<PassStepData> GetPassStepDatas(string id)
        {
            var datas = Lance.GameData.PassStepData.Where(x => x.id == id);
            if (datas == null || datas.Count() <= 0)
                return Lance.GameData.EventPassStepData.Where(x => x.id == id);
            else
                return datas;
        }

        public static int GetPassStepDatasCount(string id)
        {
            int count = Lance.GameData.PassStepData.Count(x => x.id == id);
            if (count == 0)
                count = Lance.GameData.EventPassStepData.Count(x => x.id == id);

            return count;
        }

        public static PassStepData GetPassStepData(string id, int step)
        {
            var data = Lance.GameData.PassStepData.Where(x => x.id == id && x.step == step).FirstOrDefault();
            if (data == null)
                data = Lance.GameData.EventPassStepData.Where(x => x.id == id && x.step == step).FirstOrDefault();

            return data;
        }

        public static AttendanceDayData GetAttendanceDayData(string id, int day)
        {
            return Lance.GameData.AttendanceDayData.Where(x => x.id == id && x.day == day).FirstOrDefault();
        }

        public static int GetAttendanceMaxDay(string id)
        {
            return Lance.GameData.AttendanceDayData.Where(x => x.id == id).Max(x => x.day);
        }

        public static QuestData GetQuestData(string id)
        {
            QuestData data = Lance.GameData.DailyQuestData.TryGet(id);
            if (data == null)
                data = Lance.GameData.WeeklyQuestData.TryGet(id);
            if (data == null)
                data = Lance.GameData.RepeatQuestData.TryGet(id);
            if (data == null)
                data = Lance.GameData.GuideQuestData.TryGet(id);
            if (data == null)
                data = Lance.GameData.AchievementQuestData.TryGet(id);
            if (data == null)
                data = Lance.GameData.EventQuestData.TryGet(id);

            return data;
        }

        public static IEnumerable<QuestData> GetQuestDatas(QuestUpdateType updateType)
        {
            if (updateType == QuestUpdateType.Daily)
            {
                return Lance.GameData.DailyQuestData.Values;
            }
            else if (updateType == QuestUpdateType.Weekly)
            {
                return Lance.GameData.WeeklyQuestData.Values;
            }
            else
            {
                return Lance.GameData.RepeatQuestData.Values;
            }
        }

        public static IEnumerable<QuestData> GetEventQuestDatas(string eventId)
        {
            foreach (var data in Lance.GameData.EventQuestData.Values)
            {
                if (data.eventId == eventId)
                    yield return data;
            }
        }

        public static IEnumerable<QuestData> GetQuestDatas(QuestUpdateType updateType, QuestType type)
        {
            return GetQuestDatas(updateType).Where(x => x.type == type);
        }

        public static int GetNowRaidBossElementalTypeIndex()
        {
            // 한국 시간으로 새벽 5시에 레이드 보스를 바꿔줘야 한다.
            // 한국 시간은 UTC + 9 새벽 5시를 하루 단위로 계산하기 위해서
            // UTC + 9 - 5 => UTC + 4
            int totalDayCount = TimeUtil.GetRankTotalDayCount(4);
            int raidCount = (int)ElementalType.Count;

            int raidIndex = totalDayCount % raidCount;

#if UNITY_EDITOR
            raidIndex = Lance.GameManager?.TestIndex ?? 0;
#endif
            return raidIndex;
        }

        public static StageData GetDungeonStageData(StageType type, int step)
        {
            if (type == StageType.Raid)
            {
                int raidIndex = GetNowRaidBossElementalTypeIndex();

                return Lance.GameData.DungeonStageData.TryGet($"RaidDungeon_{raidIndex + 1}");
            }
            else
            {
                return Lance.GameData.DungeonStageData.Values
                .Where(x => x.type == type && x.stage == step)
                .FirstOrDefault();
            }
        }

        public static StageData GetDemonicRealmStageData(StageType type, int step)
        {
            return Lance.GameData.DemonicRealmStageData.Values
                .Where(x => x.type == type && x.stage == step)
                .FirstOrDefault();
        }

        public static int GetDungeonBestStage(StageType type)
        {
            return Lance.GameData.DungeonData.TryGet(type).maxStep;
        }

        public static int GetDemonicRealmBestStage(StageType type)
        {
            return Lance.GameData.DemonicRealmData.TryGet(type).maxStep;
        }

        public static SkillProficiencyData GetSatisfiedSkillProficiencyData(int levelUpCount)
        {
            SkillProficiencyData satisfiedData = null;
            int stackRequireCount = 0;

            foreach (SkillProficiencyData data in Lance.GameData.SkillProficiencyData.Values)
            {
                stackRequireCount += data.requireCount;

                if (stackRequireCount <= levelUpCount)
                {
                    satisfiedData = data;
                }
            }

            return satisfiedData;
        }

        public static int CalcSkillProficiencyNextStepRemainCount(int levelUpCount)
        {
            int stackRequireCount = 0;

            foreach (SkillProficiencyData data in Lance.GameData.SkillProficiencyData.Values)
            {
                if (stackRequireCount + data.requireCount > levelUpCount)
                {
                    return levelUpCount - stackRequireCount;
                }

                stackRequireCount += data.requireCount;
            }

            return 0;
        }

        public static SkillProficiencyData CalcSkillProficiencyNextStepData(int levelUpCount)
        {
            SkillProficiencyData nextData;
            int stackRequireCount = 0;

            foreach (SkillProficiencyData data in Lance.GameData.SkillProficiencyData.Values)
            {
                nextData = data;

                if (stackRequireCount + data.requireCount > levelUpCount)
                {
                    return nextData;
                }

                stackRequireCount += data.requireCount;
            }

            return Lance.GameData.SkillProficiencyData.Values.Last();
        }

        public static GuideData GetGuideData(int step)
        {
            return Lance.GameData.GuideData.Values.Where(x => x.step == step).FirstOrDefault();
        }

        public static PackageShopData GetPackageShopData(string id, int step)
        {
            return Lance.GameData.PackageShopData.Where(x => x.id == id && x.step == step).FirstOrDefault();
        }

        public static LimitBreakData GetLimitBreakDataByStep(int step)
        {
            return Lance.GameData.LimitBreakData.Where(x => x.step == step).FirstOrDefault();
        }

        public static double GetLimitBreakStatValue(StatType statType, int step)
        {
            var limitBreakData = GetLimitBreakDataByStep(step);
            if (limitBreakData == null)
                return 0;

            if (statType == StatType.AtkRatio)
            {
                return limitBreakData.atkRatio;
            }
            else if (statType == StatType.HpRatio)
            {
                return limitBreakData.hpRatio;
            }
            else if (statType == StatType.AtkSpeedRatio)
            {
                return limitBreakData.atkSpeed;
            }
            else if (statType == StatType.MoveSpeedRatio)
            {
                return limitBreakData.moveSpeed;
            }
            else
            {
                return 0;
            }
        }

        public static LimitBreakData GetUltimateLimitBreakDataByStep(int step)
        {
            return Lance.GameData.UltimateLimitBreakData.Where(x => x.step == step).FirstOrDefault();
        }

        public static double GetUltimateLimitBreakStatValue(StatType statType, int step)
        {
            var limitBreakData = GetUltimateLimitBreakDataByStep(step);
            if (limitBreakData == null)
                return 0;

            if (statType == StatType.AtkRatio)
            {
                return limitBreakData.atkRatio;
            }
            else if (statType == StatType.HpRatio)
            {
                return limitBreakData.hpRatio;
            }
            else if (statType == StatType.AtkSpeedRatio)
            {
                return limitBreakData.atkSpeed;
            }
            else if (statType == StatType.MoveSpeedRatio)
            {
                return limitBreakData.moveSpeed;
            }
            else
            {
                return 0;
            }
        }

        public static int[] GetLimitBreakActiveLines(int step)
        {
            var limitBreakUIData = Lance.GameData.LimitBreakUIData.Where(x => x.step == step).FirstOrDefault();
            if (limitBreakUIData == null)
                return null;

            if (limitBreakUIData.activeLine.IsValid() == false)
                return null;

            var lines = limitBreakUIData.activeLine.SplitByDelim().Select(x => x.ToIntSafe(1));

            return lines.ToArray();
        }

        public static StageData GetLimitBreakStageData(int step)
        {
            return Lance.GameData.LimitBreakStageData.TryGet($"LimitBreak_{step}");
        }

        public static StageData GetUltimateLimitBreakStageData(int step)
        {
            return Lance.GameData.UltimateLimitBreakStageData.TryGet($"UltimateLimitBreak_{step}");
        }

        public static string GetRemoveAdProductId()
        {
            return Lance.GameData.PackageShopData.Where(x => x.type == PackageType.RemoveAD).FirstOrDefault().productId;
        }

        public static bool IsUnlockSkillSlotLevel(int level)
        {
            return GetUnlockSkillSlotByRequireLevel(level) != -1;
        }

        public static int GetUnlockSkillSlotByRequireLevel(int level)
        {
            int[] unlockLevels = Lance.GameData.SkillSlotUnlockData.unlockLevel;

            for (int i = 0; i < Lance.GameData.SkillSlotUnlockData.unlockLevel.Length; ++i)
            {
                if (unlockLevels[i] == level)
                {
                    return i;
                }
            }

            return -1;
        }

        public static bool CanTryLimitBreakLevel(int level)
        {
            foreach (var data in Lance.GameData.LimitBreakData)
            {
                if (data.requireLevel == level)
                    return true;
            }

            return false;
        }

        public static LimitBreakData GetLimitBreakDataByRequireLevel(int requireLevel)
        {
            return Lance.GameData.LimitBreakData.Where(x => x.requireLevel == requireLevel).FirstOrDefault();
        }

        public static RaidRewardData GetRaidRewardData(ElementalType type, double damage)
        {
            RaidRewardData raidRewardData = null;

            foreach (var data in GetRaidRewardDatas(type))
            {
                if (data.minValue < damage)
                {
                    raidRewardData = data;
                }
            }

            return raidRewardData;
        }

        public static PassData GetEventPassData(string eventId)
        {
            var datas = Lance.GameData.EventPassData.Values.Where(x => x.eventId == eventId);

            return datas?.FirstOrDefault();
        }

        public static PassData GetPassData(string id)
        {
            PassData passData = Lance.GameData.PassData.TryGet(id);
            if (passData == null)
                passData = Lance.GameData.EventPassData.TryGet(id);

            return passData;
        }

        public static CostumeData GetCostumeData(string id)
        {
            CostumeData data = Lance.GameData.BodyCostumeData.TryGet(id);
            if (data == null)
                data = Lance.GameData.WeaponCostumeData.TryGet(id);
            if (data == null)
                data = Lance.GameData.EtcCostumeData.TryGet(id);

            return data;
        }

        public static IEnumerable<CostumeData> GetCostumeDatas(CostumeType costumeType)
        {
            if (costumeType == CostumeType.Body)
            {
                return Lance.GameData.BodyCostumeData.Values;
            }
            else if (costumeType == CostumeType.Etc)
            {
                return Lance.GameData.EtcCostumeData.Values;
            }
            else
            {
                return Lance.GameData.WeaponCostumeData.Values;
            }
        }

        public static IEnumerable<string> GetCostumeIds(CostumeType costumeType)
        {
            if (costumeType == CostumeType.Body)
            {
                return Lance.GameData.BodyCostumeData.Keys;
            }
            else if (costumeType == CostumeType.Etc)
            {
                return Lance.GameData.EtcCostumeData.Keys;
            }
            else
            {
                return Lance.GameData.WeaponCostumeData.Keys;
            }
        }

        public static int GetSkillPieceAmount(string id, int count)
        {
            var data = GetSkillData(id);
            if (data == null)
                return 0;

            return GetSkillPieceAmount(data.grade, count);
        }

        public static int GetSkillPieceAmount(Grade grade, int count)
        {
            var dismantleData = Lance.GameData.SkillDismantleData.TryGet(grade);
            if (dismantleData == null)
                return 0;

            return dismantleData.amount * count;
        }

        public static string GetPetSkill(ElementalType type, SkillType skillType, int step)
        {
            var skillUnlockData = Lance.GameData.PetSkillUnlockData.Where(x => x.type == type && x.step == step).FirstOrDefault();

            return skillType == SkillType.Active ? skillUnlockData?.active ?? string.Empty : skillUnlockData?.passive ?? string.Empty;
        }

        public static SkillData GetPetSkillData(string id)
        {
            SkillData skillData = null;
            skillData = Lance.GameData.PetActiveSkillData.TryGet(id);
            if (skillData == null)
                skillData = Lance.GameData.PetPassiveSkillData.TryGet(id);

            return skillData;
        }

        public static PetSkillUnlockData GetFirstPetSkillUnlockData(ElementalType type, SkillType skillType)
        {
            int minStep = int.MaxValue;
            PetSkillUnlockData skillUnlockData = null;

            foreach (var data in Lance.GameData.PetSkillUnlockData)
            {
                if (data.type == type)
                {
                    if (skillType == SkillType.Active)
                    {
                        if (data.active.IsValid() && minStep > data.step)
                        {
                            minStep = data.step;

                            skillUnlockData = data;
                        }
                    }
                    else
                    {
                        if (data.passive.IsValid() && minStep > data.step)
                        {
                            minStep = data.step;

                            skillUnlockData = data;
                        }
                    }
                }
            }

            return skillUnlockData;
        }

        public static (int grade, StatType statType) ChangePetEvolutionStat()
        {
            int newGrade = Util.RandomChoose(Lance.GameData.PetEvolutionStatGradeProbData.probs);
            StatType newStatType = Util.RandomSelect(PetEvolutionStatTypes);

            return (newGrade, newStatType);
        }

        public static double GetPetEvolutionStatValue(StatType statType, PetEvolutionStatGrade grade)
        {
            var data = Lance.GameData.PetEvolutionStatValueData.TryGet(statType);
            if (data != null)
            {
                if ( data.statValue.Length > (int)grade )
                {
                    return data.statValue[(int)grade];
                }
            }

            return 0;
        }

        public static int GetPetEvolutionChangePrice(int lockCount)
        {
            if (Lance.GameData.PetEvolutionStatChangePrice.changePrice.Length <= lockCount)
                return int.MaxValue;

            return Lance.GameData.PetEvolutionStatChangePrice.changePrice[lockCount];
        }

        public static (double min, double max) GetPetEvolutionStatMinMaxValue(StatType statType)
        {
            var data = Lance.GameData.PetEvolutionStatValueData.TryGet(statType);
            if (data == null)
                return (0, 0);

            double min = data.statValue[(int)PetEvolutionStatGrade.C];
            double max = data.statValue[(int)PetEvolutionStatGrade.SR];

            return (min, max);
        }

        public static bool IsActivePetEvolutionStat(int petStep, int slot)
        {
            int unlockStep = Lance.GameData.PetEvolutionStatUnlockData.unlockSlot[slot];

            return petStep >= unlockStep;
        }

        public static string GetBountyQuestReward()
        {
            var datas = Lance.GameData.BountyQuestRewardProbData.Values;
            var ids = datas.Select(x => x.id).ToArray();
            var probs = datas.Select(x => x.prob).ToArray();

            var ranIndex = Util.RandomChoose(probs);

            return ids[ranIndex];
        }

        public static EssenceType ChangeToEssenceType(int chapter)
        {
            switch(chapter)
            {
                case 1:
                    return EssenceType.Chapter1;
                case 2:
                    return EssenceType.Chapter2;
                case 3:
                    return EssenceType.Chapter3;
                case 4:
                    return EssenceType.Chapter4;
                case 5:
                    return EssenceType.Chapter5;
                default:
                    return EssenceType.Count;
            }
        }

        public static EssenceStepData GetEssenceStepData(EssenceType essenceType, int step)
        {
            return Lance.GameData.EssenceStepData.Where(x => x.type == essenceType && x.step == step).FirstOrDefault();
        }

        public static int GetEssenceMaxStep(EssenceType essenceType)
        {
            if (essenceType == EssenceType.Central)
            {
                return Lance.GameData.CentralEssenceStepData.Values.Max(x => x.step);
            }
            else
            {
                return Lance.GameData.EssenceStepData.Where(x => x.type == essenceType).Max(x => x.step);
            }
        }

        public static double GetEssenceStatValue(EssenceType essenceType, int step, int level)
        {
            if (essenceType == EssenceType.Central)
            {
                var essenceStatValueData = Lance.GameData.CentralEssenceStatValueData.TryGet(step);

                return essenceStatValueData?.value ?? 0;
            }
            else
            {
                var essenceStatValueData = Lance.GameData.EssenceStatValueData.TryGet(essenceType);

                if (essenceStatValueData.value.Length <= level || level < 0)
                    return 0;
                else
                {
                    double statValue = essenceStatValueData.value[level];

                    return statValue;
                }
            }
        }

        public static ExcaliburForceStepData GetExcaliburForceStepData(ExcaliburForceType forceType, int step)
        {
            return Lance.GameData.ExcaliburForceStepData.Where(x => x.type == forceType && x.step == step).FirstOrDefault();
        }

        public static int GetExcaliburMaxStep()
        {
            return Lance.GameData.ExcaliburStepData.Values.Max(x => x.step);
        }

        public static int GetExcaliburForceMaxStep(ExcaliburForceType forceType)
        {
            return Lance.GameData.ExcaliburForceStepData.Where(x => x.type == forceType).Max(x => x.step);
        }

        public static double GetExcaliburForceStatValue(ExcaliburForceType forceType, int step, int level)
        {
            var data = Lance.GameData.ExcaliburData.TryGet(forceType);
            if (data == null)
                return 0;
            else
                return data.levelUpValue * level;
        }

        public static string GetEventId(EventType type)
        {
            var data = Lance.GameData.EventData.Values.Where(x => x.eventType == type).FirstOrDefault();

            return data?.id ?? string.Empty;
        }

        public static (int startRange, int endRange) SplitTrainSupportRange(TrainSupportTab tab)
        {
            if (TrainSupportTab.Beginner == tab)
            {
                return (Const.TrainSupportBeginnerStart, Const.TrainSupportBeginnerEnd);
            }
            else if (TrainSupportTab.Intermediate == tab)
            {
                return (Const.TrainSupportIntermediateStart, Const.TrainSupportIntermediateEnd);
            }
            else
            {
                return (Const.TrainSupportAdvancedStart, Const.TrainSupportAdvancedEnd);
            }
        }

        public static TrainSupportTab CalcTrainSupportTab(int requireCount)
        {
            if (Const.TrainSupportBeginnerStart <= requireCount &&
                Const.TrainSupportIntermediateStart > requireCount)
            {
                return TrainSupportTab.Beginner;
            }
            else if (Const.TrainSupportIntermediateStart <= requireCount &&
                Const.TrainSupportAdvancedStart > requireCount)
            {
                return TrainSupportTab.Intermediate;
            }
            else
            {
                return TrainSupportTab.Advanced;
            }
        }

        public static int GetSkipBattlePrice(int currentCount)
        {
            if (Lance.GameData.SkipBattlePriceData.price.Length <= currentCount)
                return Lance.GameData.SkipBattlePriceData.price.Last();

            return Lance.GameData.SkipBattlePriceData.price[currentCount];
        }

        public static int GetSkipBattleMonsterKillCount(int bestStage)
        {
            foreach(var data in Lance.GameData.SkipBattleMonsterKillData)
            {
                if (data.stageMin <= bestStage && data.stageMax >= bestStage)
                {
                    return data.monsterKillCalcMinute;
                }
            }

            return Lance.GameData.SkipBattleMonsterKillData.FirstOrDefault()?.monsterKillCalcMinute ?? 0;
        }

        public static (int grade, StatType statType) ChangeEquipmentOptionStat()
        {
            int newGrade = Util.RandomChoose(Lance.GameData.EquipmentOptionStatGradeProbData.probs);
            StatType newStatType = Util.RandomSelect(EquipmentOptionStatTypes);

            return (newGrade, newStatType);
        }

        public static double GetEquipmentOptionStatValue(StatType statType, EquipmentOptionStatGrade grade)
        {
            var data = Lance.GameData.EquipmentOptionStatValueData.TryGet(statType);
            if (data != null)
            {
                if (data.statValue.Length > (int)grade)
                {
                    return data.statValue[(int)grade];
                }
            }

            return 0;
        }

        public static IEnumerable<Grade> EquipmentOptionStatGradeToGrade()
        {
            foreach(var grade in Enum.GetValues(typeof(EquipmentOptionStatGrade)))
            {
                if ((int)grade == (int)EquipmentOptionStatGrade.Count)
                    continue;

                yield return (Grade)((int)grade + 1);
            }
        }

        public static IEnumerable<Grade> PetEvolutionStatGradeToGrade()
        {
            foreach (var grade in Enum.GetValues(typeof(PetEvolutionStatGrade)))
            {
                if ((int)grade == (int)PetEvolutionStatGrade.Count)
                    continue;

                yield return (Grade)((int)grade + 1);
            }
        }

        public static double GetEquipmentOptionStatChangePrice(Grade grade, int lockCount)
        {
            var data = Lance.GameData.EquipmentOptionStatChangePrice.TryGet(grade);
            if (data == null)
                return int.MaxValue;

            if (data.changePrice.Length <= lockCount)
                return int.MaxValue;

            return data.changePrice[lockCount];
        }

        
        public static (double min, double max) GetEquipmentOptionStatMinMaxValue(StatType statType)
        {
            var data = Lance.GameData.EquipmentOptionStatValueData.TryGet(statType);
            if (data == null)
                return (0, 0);

            double min = data.statValue[(int)PetEvolutionStatGrade.C];
            double max = data.statValue[(int)PetEvolutionStatGrade.SR];

            return (min, max);
        }

        public static bool IsActiveEquipmentOptonStat(Grade grade, int slot)
        {
            var data = Lance.GameData.EquipmentOptionStatUnlockData.TryGet(grade);
            if (data == null)
                return false;

            return data.unlockSlot[slot];
        }

        public static bool HaveGoldTrainRequireType(StatType statType)
        {
            return Lance.GameData.GoldTrainData.ContainsKey(statType);
        }

        public static int GetJoustingRankingRewardIndex(int rank)
        {
            bool isMyReward = false;
            int index = 0;

            for(int i = 0; i < Lance.GameData.JoustingRankingRewardData.Count; ++i)
            {
                index = i;
                var data = Lance.GameData.JoustingRankingRewardData[i];

                if (data.rankMin > 0 && data.rankMax > 0)
                {
                    isMyReward = data.rankMin > rank && data.rankMax <= rank;
                }
                else if (data.rankMax > 0)
                {
                    isMyReward = data.rankMax == rank;
                }
                else
                {
                    isMyReward = true;
                }

                if (isMyReward)
                    break;
            }

            return index;
        }

        public static JoustingTier GetJoustingTier(int rankScore)
        {
            JoustingTier tier = JoustingTier.None;

            foreach(var data in Lance.GameData.JoustingTierData)
            {
                if (data.rankScore <= rankScore)
                    tier = data.tier;
                else
                    break;
            }

            return tier;
        }

        public static int CalcTotalUsedSkillPiece(string id, int level)
        {
            int totalUsedSkillPiece = 0;
            var skillData = GetSkillData(id);
            var skillUpgradeData = Lance.GameData.SkillUpgradeData2.TryGet(skillData.grade);

            for(int i = 0; i < level -1 ; ++i)
            {
                int levelIndex = i;

                totalUsedSkillPiece += skillUpgradeData.requireCount[levelIndex];
            }

            return totalUsedSkillPiece;
        }

        public static IEnumerable<RaidRewardData> GetRaidRewardDatas(ElementalType type)
        {
            return Lance.GameData.RaidRewardData.Where(x => x.type == type);
        }

        public static int GetTotalUsedAncientEssence(ExcaliburForceType type, int step, int level)
        {
            int totalUsed = 0;

            for(int i = 0; i <= step; ++i)
            {
                int tempStep = i;
                int tempPrevStep = tempStep - 1;

                var stepData = GetExcaliburForceStepData(type, tempStep);
                if (stepData == null)
                    continue;

                var prevStepData = GetExcaliburForceStepData(type, tempPrevStep);

                int maxLevel = stepData.maxLevel - (prevStepData?.maxLevel ?? 0);

                var upgradeData = Lance.GameData.ExcaliburUpgradeData.TryGet(type);

                if (maxLevel <= level)
                {
                    totalUsed += (maxLevel * upgradeData.require[tempStep]);

                    level -= maxLevel;
                }
                else
                {
                    totalUsed += (level * upgradeData.require[tempStep]);
                }
            }

            return totalUsed;
        }

        public static int GetJoustGloryOrbMaxLevel(JoustGloryOrbType type)
        {
            var data = Lance.GameData.JoustingGloryOrbUpgradeData.TryGet(type);

            return data.require.Length;
        }

        public static double GetJoustGloryOrbStatValue(JoustGloryOrbType type, int level)
        {
            var data = Lance.GameData.JoustingGloryOrbStatData.TryGet(type);
            if (data == null)
                return 0;

            if (data.statValue.Length <= level)
                return 0;

            return data.statValue[level];
        }

        public static int GetJoustGloryOrbMaxStep()
        {
            return Lance.GameData.JoustingGloryOrbStepData.Values.Max(x => x.step);
        }

        public static DemonicRealmSoulStoneRewardData GetDemonicRealmSoulStoneRewardData(StageType type, int stage)
        {
            return Lance.GameData.DemonicRealmSoulStoneRewardData.Where(x => x.type == type && x.stage == stage).FirstOrDefault();
        }

        public static DemonicRealmManaEssenceRewardData GetDemonicRealmManaEssenceRewardData(StageType type, int stage)
        {
            return Lance.GameData.DemonicRealmManaEssenceRewardData.Where(x => x.type == type && x.stage == stage).FirstOrDefault();
        }

        public static IEnumerable<CostumeShopData> GetCostumeShopDatas(CostumeType type)
        {
            return Lance.GameData.CostumeShopData.Values.Where(x => x.type == type);
        }

        public static int GetManaHeartMaxLevel(ManaHeartType type)
        {
            var data = Lance.GameData.ManaHeartUpgradeData.TryGet(type);

            return data.require.Length;
        }

        public static double GetManaHeartStatValue(ManaHeartType type, int level)
        {
            var data = Lance.GameData.ManaHeartStatData.TryGet(type);
            if (data == null)
                return 0;

            if (data.statValue.Length <= level)
                return 0;

            return data.statValue[level];
        }

        public static int GetManaHeartMaxUpgradeStep()
        {
            return Lance.GameData.ManaHeartUpgradeStepData.Values.Max(x => x.step);
        }

        public static int GetManaHeartMaxStep()
        {
            return Lance.GameData.ManaHeartStepData.Values.Max(x => x.step);
        }
    }
}