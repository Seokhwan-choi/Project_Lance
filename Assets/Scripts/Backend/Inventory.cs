using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using CodeStage.AntiCheat.ObscuredTypes;
using BackEnd;
using LitJson;

namespace Lance
{
    // ��� ����
    public class EquipmentInst
    {
        public string Id;           // ���� ���� ȣ���ؼ� ������� �� ��
        public int Level;
        public int Count;
        public bool SavedIsEquipped;

        public int ReforgeStep;
        public int FailedReforge;
        public int StackedFailedReforge;
        public int TryReporage;

        public EquipmentOptionStatInfo OptionStat;

        ObscuredString mId;       // ���̵�
        ObscuredInt mLevel;       // ����
        ObscuredInt mCount;       // ����
        ObscuredBool mIsEquipped; // ���� ����

        ObscuredInt mReforgeStep;           // ��� �ܰ�
        ObscuredInt mFailedReforge;         // ��� ���� Ƚ��
        ObscuredInt mStackedFailedReforge;  // ��� ���� ���� Ƚ��
        ObscuredInt mTryReporage;           // ��� �õ�

        EquipmentOptionStatInfo mOptionStat;

        EquipmentData mData;
        public EquipmentInst(string id, int level, int count, bool isEquipped, int reporge, int failedReforge, int stackedFailedReforge, int tryReforge, EquipmentOptionStatInfo optionStat) 
        {
            mId = id;
            mLevel = level;
            mCount = count;
            mIsEquipped = isEquipped;

            mReforgeStep = reporge;
            mFailedReforge = failedReforge;
            mStackedFailedReforge = stackedFailedReforge;
            mTryReporage = tryReforge;

            mOptionStat = optionStat;

            mData = DataUtil.GetEquipmentData(id);
        }

        public void ReadyToSave()
        {
            Id = mId;
            Level = mLevel;
            Count = mCount;
            SavedIsEquipped = mIsEquipped;

            ReforgeStep = mReforgeStep;
            FailedReforge = mFailedReforge;
            StackedFailedReforge = mStackedFailedReforge;
            TryReporage = mTryReporage;

            if (mOptionStat != null)
            {
                mOptionStat.ReadyToSave();
                OptionStat = mOptionStat;
            }
        }

        public void RandomizeKey()
        {
            mId.RandomizeCryptoKey();
            mLevel.RandomizeCryptoKey();
            mCount.RandomizeCryptoKey();
            mIsEquipped.RandomizeCryptoKey();

            mReforgeStep.RandomizeCryptoKey();
            mFailedReforge.RandomizeCryptoKey();
            mStackedFailedReforge.RandomizeCryptoKey();
            mTryReporage.RandomizeCryptoKey();
            mOptionStat?.RandomizeKey();
        }

        public void SetEquip(bool equip)
        {
            mIsEquipped = equip;
        }

        public bool IsEquipped()
        {
            return mIsEquipped;
        }

        public ObscuredString GetId()
        {
            return mId;
        }

        public ObscuredInt GetLevel()
        {
            return mLevel;
        }

        public ObscuredInt GetMaxLevel()
        {
            return mData.maxLevel + mReforgeStep * Lance.GameData.EquipmentCommonData.reforgeAddMaxLevel;
        }

        public Grade GetGrade()
        {
            return mData.grade;
        }

        public SubGrade GetSubGrade()
        {
            return mData.subGrade;
        }

        public bool IsMaxLevel()
        {
            return mLevel == GetMaxLevel();
        }

        public int GetMaxLevelRequierCount()
        {
            return GetMaxLevel() - mLevel;
        }

        public void LevelUp(int levelUpCount)
        {
            mLevel += levelUpCount;

            mLevel = Math.Min(mLevel, GetMaxLevel());
        }

        public ObscuredInt GetMaxReforge()
        {
            var reporgeData = Lance.GameData.ReforgeData.TryGet(mData.grade);
            if (reporgeData == null)
                return 0;

            return reporgeData.maxReforge;
        }

        public bool IsMaxReforge()
        {
            return mReforgeStep == GetMaxReforge();
        }

        public void TryReforge()
        {
            mTryReporage++;
        }

        public void StackFailReforge()
        {
            mFailedReforge++;
            mStackedFailedReforge++;
        }

        public void Reforge()
        {
            mReforgeStep += 1;

            mReforgeStep = Math.Min(mReforgeStep, GetMaxReforge());

            mFailedReforge = 0;
        }

        public int GetReforgeStep()
        {
            return mReforgeStep;
        }

#if UNITY_EDITOR
        public void SetReforgeStep(int step)
        {
            mReforgeStep = Math.Min(step, GetMaxReforge());
        }
#endif

        public int GetReforgeFailedCount()
        {
            return mFailedReforge;
        }

        public ObscuredInt GetCount()
        {
            return mCount;
        }

        public int GetCombineCount()
        {
            return mData.combineCount;
        }

        public void AddCount(int count)
        {
            mCount += count;
        }

        public bool UseCount(int count)
        {
            if (IsEnoughCount(count))
            {
                mCount -= count;

                return true;
            }

            return false;
        }

        public bool IsEnoughCount(int count)
        {
            return mCount >= count;
        }

        public ObscuredDouble GetUpgradeRequireStones(int upgradeCount)
        {
            return DataUtil.GetEquipmentUpgradeRequireStone(DataUtil.GetEquipmentData(mId), mLevel, upgradeCount);
        }

        public ObscuredDouble GetReforgeRequireStone()
        {
            return DataUtil.GetEquipmentReforgeRequireStone(mData.grade, mReforgeStep);
        }

        public double GetBaseStatValue()
        {
            return mData.baseValue + ((mData.baseValue * mData.levelUpValue) * mLevel);
        }

        public double GetStatValues(StatType statType, bool isEquipped)
        {
            double totalStatValues = 0;
            List<(StatType, double)> statValues = new List<(StatType, double)>();

            if (statType == mData.valueType && isEquipped)
                totalStatValues += mData.baseValue + ((mData.baseValue * mData.levelUpValue) * mLevel);

            totalStatValues += mOptionStat.GatherStatValues(statType, mData.grade, isEquipped);

            return totalStatValues;
        }

        public double GetOwnStatValues(StatType statType)
        {
            double totalStatValues = 0;

            for(int i = 0; i < mData.ownStats.Length; ++i)
            {
                string ownStatId = mData.ownStats[i];
                if (ownStatId.IsValid() == false)
                    continue;

                var ownStatData = Lance.GameData.OwnStatData.TryGet(ownStatId);
                if (ownStatData == null)
                    continue;

                if (statType != ownStatData.valueType)
                    continue;

                totalStatValues += ownStatData.baseValue + (ownStatData.levelUpValue * mLevel);
            }

            return totalStatValues;
        }

        public EquipmentOptionStat GetOptionStat(int slot)
        {
            return mOptionStat.GetStat(slot);
        }

        public bool IsActiveOptionStat(int slot)
        {
            return mOptionStat.IsActiveStat(mData.grade, slot);
        }

        public int GetOptionStatLockCount()
        {
            return mOptionStat.GetOptionStatLockCount(mData.grade);
        }

        public bool IsAllOptionStatLocked()
        {
            return mOptionStat.IsAllOptionStatLocked(mData.grade);
        }

        public bool ChangeOptionStats()
        {
            return mOptionStat.ChangeStats(mData.grade);
        }

        public bool IsSatisfied(StatType[] statTypes, EquipmentOptionStatGrade optionGrade)
        {
            return mOptionStat.IsSatisfied(statTypes, optionGrade, mData.grade);
        }

        public int GetCurrentPreset()
        {
            return mOptionStat.GetCurrentPreset();
        }

        public void ChangeOptionStatPreset(int preset)
        {
            mOptionStat.ChangePreset(preset);
        }

        public bool AnyCanChangeOptionStat()
        {
            return mOptionStat.AnyCanChangeStat(mData.grade);
        }

        public double GetOptionChangePrice()
        {
            return mOptionStat.GetOptionStatChangePrice(mData.grade);
        }

#if UNITY_EDITOR
        public void SetMaxLevel(string id)
        {

        }

        public void SetLevel(int level)
        {
            mLevel = level;
        }
#endif
    }

    // ��� ����
    public class Inventory : AccountBase
    {
        ObscuredInt mStackedCombineCount;
        Dictionary<ObscuredString, EquipmentInst> mEquipmentItems = new();
        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            var equipmentsData = gameDataJson["Equipments"];

            for (int i = 0; i < equipmentsData.Count; i++)
            {
                var equipmentJsonData = equipmentsData[i];

                string id = equipmentJsonData["Id"].ToString();

                int levelTemp = 0;
                int countTemp = 0;

                int.TryParse(equipmentJsonData["Level"].ToString(), out levelTemp);
                int.TryParse(equipmentJsonData["Count"].ToString(), out countTemp);

                bool isEquipped = false;

                if (equipmentJsonData.ContainsKey("SavedIsEquipped") )
                {
                    bool.TryParse(equipmentJsonData["SavedIsEquipped"].ToString(), out isEquipped);
                }

                int reforgeStepTemp = 0;

                if (equipmentJsonData.ContainsKey("ReforgeStep"))
                {
                    int.TryParse(equipmentJsonData["ReforgeStep"].ToString(), out reforgeStepTemp);
                }

                int failedReforgeTemp = 0;

                if (equipmentJsonData.ContainsKey("FailedReforge"))
                {
                    int.TryParse(equipmentJsonData["FailedReforge"].ToString(), out failedReforgeTemp);
                }

                int stackedFailedReforgeTemp = 0;

                if (equipmentJsonData.ContainsKey("StackedFailedReforge"))
                {
                    int.TryParse(equipmentJsonData["StackedFailedReforge"].ToString(), out stackedFailedReforgeTemp);
                }

                int tryReforgeTemp = 0;

                if (equipmentJsonData.ContainsKey("TryReporage"))
                {
                    int.TryParse(equipmentJsonData["TryReporage"].ToString(), out tryReforgeTemp);
                }

                if (levelTemp > 0)
                {
                    EquipmentOptionStatInfo optionStat = new EquipmentOptionStatInfo();

                    if (equipmentJsonData.ContainsKey("OptionStat"))
                    {
                        optionStat.SetServerDataToLocal(equipmentJsonData["OptionStat"]);
                    }
                    else
                    {
                        optionStat.InitializeData();
                    }

                    mEquipmentItems.Add(id, new EquipmentInst(id, levelTemp, countTemp, isEquipped,
                        reforgeStepTemp, failedReforgeTemp, stackedFailedReforgeTemp, tryReforgeTemp, optionStat));
                }
            }

            int stackedCombineCountTemp = 0;

            int.TryParse(gameDataJson["StackedCombineCount"].ToString(), out stackedCombineCountTemp);

            mStackedCombineCount = stackedCombineCountTemp;
        }

        public override Param GetParam()
        {
            Dictionary<string, EquipmentInst> saveEquipmentItems = new Dictionary<string, EquipmentInst>();

            foreach(var item in mEquipmentItems)
            {
                item.Value.ReadyToSave();

                saveEquipmentItems.Add(item.Key, item.Value);
            }

            Param param = new Param();

            param.Add("Equipments", saveEquipmentItems);
            param.Add("StackedCombineCount", (int)mStackedCombineCount);

            return param;
        }

        public override void RandomizeKey()
        {
            base.RandomizeKey();

            if (mEquipmentItems != null && mEquipmentItems.Count > 0)
            {
                foreach (var item in mEquipmentItems)
                {
                    item.Key.RandomizeCryptoKey();
                    item.Value.RandomizeKey();
                }
            }

            mStackedCombineCount.RandomizeCryptoKey();
        }

        public bool IsEquipped(string id)
        {
            if (HaveItem(id) == false)
                return false;

            var item = GetEquipItem(id);

            return item.IsEquipped();
        }

        public bool IsMaxLevel(string id)
        {
            if (HaveItem(id) == false)
                return false;

            var item = GetEquipItem(id);

            return item.IsMaxLevel();
        }

        public bool IsMaxReforge(string id)
        {
            if (HaveItem(id) == false)
                return false;

            var item = GetEquipItem(id);

            return item.IsMaxReforge();
        }

        public int GetStackedCombineCount()
        {
            return mStackedCombineCount;
        }

        public int GetStackedEquipmentsLevelUpCount()
        {
            int totalLevelUpCount = 0;

            foreach (var equipment in mEquipmentItems.Values)
            {
                totalLevelUpCount += equipment.GetLevel() - 1;
            }

            return totalLevelUpCount;
        }

        public bool HaveItem(string id)
        {
            return mEquipmentItems.ContainsKey(id);
        }

        public EquipmentInst GetEquipItem(string id)
        {
            return mEquipmentItems.TryGet(id);
        }

        public EquipmentInst GetEquipItem(Grade grade, SubGrade subGrade)
        {
            foreach(var inst in mEquipmentItems.Values)
            {
                if (inst.GetGrade() == grade && inst.GetSubGrade() == subGrade)
                    return inst;
            }

            return null;
        }

        public EquipmentInst GetEquippedItem()
        {
            foreach(var item in mEquipmentItems.Values)
            {
                if (item.IsEquipped())
                    return item;
            }

            return null;
        }

        public IEnumerable<EquipmentInst> GetEquipItems()
        {
            return mEquipmentItems.Values;
        }

        public int GetItemLevel(string id)
        {
            var item = GetEquipItem(id);

            return item?.GetLevel() ?? 0;
        }

        public int GetItemReforgeStep(string id)
        {
            var item = GetEquipItem(id);

            return item?.GetReforgeStep() ?? 0;
        }

        public int GetItemReforgeFailedCount(string id)
        {
            var item = GetEquipItem(id);

            return item?.GetReforgeFailedCount() ?? 0;
        }

        public void AddItem(string id, int count)
        {
            if (mEquipmentItems.ContainsKey(id))
            {
                EquipmentInst equipInst = mEquipmentItems.TryGet(id);
                if (equipInst != null)
                {
                    equipInst.AddCount(count);
                }
            }
            else
            {
                EquipmentOptionStatInfo optionStat = new EquipmentOptionStatInfo();
                optionStat.InitializeData();

                mEquipmentItems.Add(id, new EquipmentInst(id, 1, count, false, 0, 0, 0, 0, optionStat));
            }

            SetIsChangedData(true);
        }

        // ��� ��õ ����
        public string EquipRecomandItem()
        {
            EquipmentInst bestEquipment = FindBestEquipment();

            if (bestEquipment != null)
            {
                EquipItem(bestEquipment.GetId());

                SetIsChangedData(true);
            }

            // � ��� �����ߴ��� �˷�����
            return bestEquipment?.GetId() ?? string.Empty;

            EquipmentInst FindBestEquipment()
            {
                EquipmentInst bestEquipment = null;
                double bestPowerLevel = 0;

                foreach (EquipmentInst equipment in mEquipmentItems.Values)
                {
                    double powerLevel = PowerLevelCalculator.CalcEquipmentPowerLevel(equipment.GetId());

                    if (bestPowerLevel <= powerLevel)
                    {
                        bestPowerLevel = powerLevel;
                        bestEquipment = equipment;
                    }
                }

                return bestEquipment;
            }
        }

        EquipmentInst FindEquippedItem()
        {
            EquipmentInst equippedItem = null;

            foreach(var equipItem in mEquipmentItems.Values)
            {
                if (equipItem.IsEquipped())
                {
                    // ���� �ѹ��� �ϳ��� ������ �� �ִ�.
                    // �������� ��� ã������ break
                    equippedItem = equipItem;

                    break;
                }
            }

            return equippedItem;
        }

        // ��� ����
        public bool EquipItem(string id)
        {
            // ���� �����Ϸ��� ���
            // ���� ������ �ִ� ������� Ȯ��
            if (HaveItem(id) == false)
                return false;

            // �̹� �������̶�� ���� X
            if (IsEquipped(id))
                return false;

            // �̹� �������� �������� �ִٸ�
            var equippedItem = FindEquippedItem();

            // ���� ����
            equippedItem?.SetEquip(false);

            // ��� ����
            EquipmentInst equipItem = GetEquipItem(id);

            equipItem.SetEquip(true);

            SetIsChangedData(true);

            return true;
        }

        public void UnEquipItem(string id)
        {
            // ���� �����Ϸ��� ���
            // ���� ������ �ִ� ������� Ȯ��
            if (HaveItem(id) == false)
                return;

            var equipItem = GetEquipItem(id);
            if (equipItem == null)
                return;

            if (equipItem.IsEquipped() == false)
                return;

            equipItem.SetEquip(false);

            SetIsChangedData(true);
        }

        // ��� ��ȭ
        public void UpgradeItem(string id, int levelUpCount = 1)
        {
            EquipmentInst equipItem = GetEquipItem(id);
            if (equipItem == null)
                return;

            equipItem.LevelUp(levelUpCount);

            SetIsChangedData(true);
        }

        public (string id, int combineCount) CombineItem(string id)
        {
            // ���� ������ �ִ� ������� Ȯ��
            if (HaveItem(id) == false)
                return (string.Empty, 0);

            EquipmentData data = DataUtil.GetEquipmentData(id);
            if (data == null)
                return (string.Empty, 0);

            // combineCount�� ���ٸ� �ռ� ������ �Ұ����� ��
            if (data.combineCount == 0)
                return (string.Empty, 0);

            // �ռ��� �ʿ��� ��� ������ ������� Ȯ��
            EquipmentInst equipItem = GetEquipItem(id);
            if (equipItem.IsEnoughCount(data.combineCount) == false)
                return (string.Empty, 0);

            int combineCount = equipItem.GetCount() / data.combineCount;
            int useCount = data.combineCount * combineCount;

            // ���� ��� ��� �����͸� Ȯ��
            // �����Ͱ� ���ٸ� ���� �� ���� ��
            EquipmentData nextData = DataUtil.GetNextGradeEquipmentData(data.type, data.grade, data.subGrade);
            if (nextData == null)
                return (string.Empty, 0);

            // �ռ� Ƚ����ŭ ��� ���
            if (equipItem.UseCount(useCount) == false)
                return (string.Empty, 0);

            mStackedCombineCount += combineCount;

            SetIsChangedData(true);

            return (nextData.id, combineCount);
        }

        // ��� �ռ� �� �ٷ� ����
        public (string id, int combineCount) CombineAndAddItem(string id)
        {
            var result = CombineItem(id);
            if (result.id.IsValid())
            {
                // �ռ� ������� �κ��丮�� �߰�
                AddItem(result.id, result.combineCount);

                SetIsChangedData(true);

                return (result);
            }

            return (string.Empty, 0);
        }

        // ��� �ϰ� �ռ�
        public List<(string, int)> CombineAllItem()
        {
            List<(string, int)> resultList = new List<(string, int)>();

            for(int i = 0; i < (int)Grade.Count; ++i)
            {
                for(int j = 0; j < (int)SubGrade.Count; ++j)
                {
                    Grade grade = (Grade)i;
                    SubGrade subGrade = (SubGrade)j;

                    var inst = GetEquipItem(grade, subGrade);
                    if (inst != null)
                    {
                        var result = CombineItem(inst.GetId());
                        if (result.id.IsValid())
                        {
                            EquipmentInst nextInst = GetEquipItem(result.id);
                            if (nextInst != null)
                            {
                                int combineCount = nextInst.GetCombineCount();

                                int prevTotalCount = nextInst.GetCount();
                                int prevRemain = prevTotalCount % combineCount;

                                AddItem(result.id, result.combineCount);

                                int quotient = (prevRemain + result.combineCount) / combineCount;
                                if (quotient > 0)
                                {
                                    int remain = (prevRemain + result.combineCount) % combineCount;
                                    if (remain > 0)
                                        resultList.Add((result.id, remain));
                                }
                                else
                                {
                                    resultList.Add((result.id, result.combineCount));
                                }
                            }
                            else
                            {
                                AddItem(result.id, result.combineCount);
                                nextInst = GetEquipItem(result.id);

                                int combineCount = nextInst.GetCombineCount();

                                int quotient = result.combineCount / combineCount;
                                if (quotient > 0)
                                {
                                    int remain = result.combineCount % combineCount;
                                    if (remain > 0)
                                        resultList.Add((result.id, remain));
                                }
                                else
                                {
                                    resultList.Add((result.id, result.combineCount));
                                }
                            }
                        }
                    }
                }
            }

            return resultList;
        }

        public double GatherStatValues(StatType type)
        {
            double totalStatValue = 0;

            foreach(EquipmentInst item in mEquipmentItems.Values)
            {
                totalStatValue += item.GetStatValues(type, item.IsEquipped());
                totalStatValue += item.GetOwnStatValues(type);
            }

            return totalStatValue;
        }

        //// ���
        //public bool ReforgeItem(string id)
        //{
        //    // ���� ������ �ִ� ������� Ȯ��
        //    if (HaveItem(id) == false)
        //        return false;

        //    // �����Ͱ� ���� ���� ��� �Ұ�
        //    EquipmentData data = DataUtil.GetEquipmentData(id);
        //    if (data == null)
        //        return false;

        //    // ��� �����Ͱ� ���� ���� ��� �Ұ�
        //    ReforgeData reporgeData = Lance.GameData.ReforgeData.TryGet(data.grade);
        //    if (reporgeData == null)
        //        return false;

        //    var item = GetEquipItem(id);

        //    // �ְ����� �ƴϸ� ��� �Ұ�
        //    if (item.IsMaxLevel() == false)
        //        return false;

        //    // ��� �ְ� �ܰ�� ��� �Ұ�
        //    if ()
        //}

#if UNITY_EDITOR
        public void SetLevel(string id, int level)
        {
            var equipItem = GetEquipItem(id);

            equipItem?.SetLevel(level);
        }

        public void SetReforge(string id, int reforge)
        {
            var equipItem = GetEquipItem(id);

            equipItem?.SetReforgeStep(reforge);
        }
#endif
    }
}

