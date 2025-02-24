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
    public class AccessoryInst
    {
        public string Id;           // ���� ���� ȣ���ؼ� ������� �� ��
        public int Level;
        public int Count;
        public bool SavedIsEquipped;

        public int ReforgeStep;
        public int FailedReforge;
        public int StackedFailedReforge;
        public int TryReporage;

        //public AccessoryOptionStatInfo OptionStat;

        ObscuredString mId;       // ���̵�
        ObscuredInt mLevel;       // ����
        ObscuredInt mCount;       // ����
        ObscuredBool mIsEquipped; // ���� ����

        ObscuredInt mReforgeStep;           // ��� �ܰ�
        ObscuredInt mFailedReforge;         // ��� ���� Ƚ��
        ObscuredInt mStackedFailedReforge;  // ��� ���� ���� Ƚ��
        ObscuredInt mTryReporage;           // ��� �õ�

        //AccessoryOptionStatInfo mOptionStat;

        AccessoryData mData;
        public AccessoryInst(string id, int level, int count, bool IsEquipped, int reporge, int failedReforge, int stackedFailedReforge, int tryReforge)
        {
            mId = id;
            mLevel = level;
            mCount = count;
            mIsEquipped = IsEquipped;

            mReforgeStep = reporge;
            mFailedReforge = failedReforge;
            mStackedFailedReforge = stackedFailedReforge;
            mTryReporage = tryReforge;

            mData = DataUtil.GetAccessoryData(id);
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

        public Grade GetGrade()
        {
            return mData.grade;
        }

        public SubGrade GetSubGrade()
        {
            return mData.subGrade;
        }

        public ObscuredInt GetMaxLevel()
        {
            return mData.maxLevel + mReforgeStep * Lance.GameData.AccessoryCommonData.reforgeAddMaxLevel;
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
            var reporgeData = Lance.GameData.AccessoryReforgeData.TryGet(mData.grade);
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
            return DataUtil.GetAccessoryUpgradeRequireStone(mData, mLevel, upgradeCount);
        }

        public ObscuredDouble GetReforgeRequireStone()
        {
            return DataUtil.GetAccessoryReforgeRequireStone(mData.grade, mReforgeStep);
        }

        public double GetEquippedStatValues(StatType statType)
        {
            double totalStatValues = 0;
            
            if (mData.valueType == statType)
                totalStatValues += mData.baseValue + (mData.levelUpValue * mLevel);

            return totalStatValues;
        }

        public double GetOwnStatValues(StatType statType)
        {
            double totalStatValues = 0;

            for (int i = 0; i < mData.ownStats.Length; ++i)
            {
                string ownStatId = mData.ownStats[i];
                if (ownStatId.IsValid() == false)
                    continue;

                var ownStatData = Lance.GameData.AccessoryOwnStatData.TryGet(ownStatId);
                if (ownStatData == null)
                    continue;

                if (ownStatData.valueType == statType)
                    totalStatValues += ownStatData.baseValue + (ownStatData.levelUpValue * mLevel);
            }

            return totalStatValues;
        }

    }

    // �Ǽ��縮 ����
    public class AccessoryInventory : AccountBase
    {
        ObscuredInt mStackedCombineCount;
        ObscuredBool mAllAccessoryUnEquip;
        List<ObscuredString> mEquippedItems = new();
        Dictionary<ObscuredString, AccessoryInst> mAccessoryItems = new();
        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            var equippedAccessorysDatas = gameDataJson["EquippedAccessorys"];

            for(int i = 0; i < equippedAccessorysDatas.Count; ++i)
            {
                string id = equippedAccessorysDatas[i].ToString();

                mEquippedItems.Add(id);
            }

            var accessorysDatas = gameDataJson["Accessorys"];

            for (int i = 0; i < accessorysDatas.Count; i++)
            {
                var accessoryJsonData = accessorysDatas[i];

                string id = accessoryJsonData["Id"].ToString();

                int levelTemp = 0;
                int countTemp = 0;

                int.TryParse(accessoryJsonData["Level"].ToString(), out levelTemp);
                int.TryParse(accessoryJsonData["Count"].ToString(), out countTemp);

                bool IsEquipped = false;

                if (accessoryJsonData.ContainsKey("SavedIsEquipped"))
                {
                    bool.TryParse(accessoryJsonData["SavedIsEquipped"].ToString(), out IsEquipped);
                }

                int reforgeStepTemp = 0;

                if (accessoryJsonData.ContainsKey("ReforgeStep"))
                {
                    int.TryParse(accessoryJsonData["ReforgeStep"].ToString(), out reforgeStepTemp);
                }

                int failedReforgeTemp = 0;

                if (accessoryJsonData.ContainsKey("FailedReforge"))
                {
                    int.TryParse(accessoryJsonData["FailedReforge"].ToString(), out failedReforgeTemp);
                }

                int stackedFailedReforgeTemp = 0;

                if (accessoryJsonData.ContainsKey("StackedFailedReforge"))
                {
                    int.TryParse(accessoryJsonData["StackedFailedReforge"].ToString(), out stackedFailedReforgeTemp);
                }

                int tryReforgeTemp = 0;

                if (accessoryJsonData.ContainsKey("TryReporage"))
                {
                    int.TryParse(accessoryJsonData["TryReporage"].ToString(), out tryReforgeTemp);
                }

                if (levelTemp > 0)
                {
                    mAccessoryItems.Add(id, new AccessoryInst(id, levelTemp, countTemp, IsEquipped,
                        reforgeStepTemp, failedReforgeTemp, stackedFailedReforgeTemp, tryReforgeTemp));
                }
            }

            int stackedCombineCountTemp = 0;

            int.TryParse(gameDataJson["StackedCombineCount"].ToString(), out stackedCombineCountTemp);

            mStackedCombineCount = stackedCombineCountTemp;

            mAllAccessoryUnEquip = false;

            if (gameDataJson.ContainsKey("AllAccessoryUnEquip"))
            {
                bool allAccessoryUnEquipTemp = false;

                bool.TryParse(gameDataJson["AllAccessoryUnEquip"].ToString(), out allAccessoryUnEquipTemp);

                mAllAccessoryUnEquip = allAccessoryUnEquipTemp;
            }

            if(mAllAccessoryUnEquip == false)
            {
                mAllAccessoryUnEquip = true;

                mEquippedItems.Clear();

                foreach(var inst in mAccessoryItems.Values)
                {
                    inst.SetEquip(false);
                }

                SetIsChangedData(true);
            }
        }

        public override Param GetParam()
        {
            Dictionary<string, AccessoryInst> saveAccessoryItems = new Dictionary<string, AccessoryInst>();

            foreach (var item in mAccessoryItems)
            {
                item.Value.ReadyToSave();

                saveAccessoryItems.Add(item.Key, item.Value);
            }

            Param param = new Param();

            param.Add("EquippedAccessorys", mEquippedItems.Select(x => x.ToString()));
            param.Add("Accessorys", saveAccessoryItems);
            param.Add("StackedCombineCount", (int)mStackedCombineCount);
            param.Add("AllAccessoryUnEquip", (bool)mAllAccessoryUnEquip);

            return param;
        }

        public override void RandomizeKey()
        {
            base.RandomizeKey();

            if (mEquippedItems != null && mEquippedItems.Count > 0)
            {
                foreach (var equippedItem in mEquippedItems)
                {
                    equippedItem.RandomizeCryptoKey();
                }
            }

            if (mAccessoryItems != null && mAccessoryItems.Count > 0)
            {
                foreach (var item in mAccessoryItems)
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

            var item = GetAccessoryItem(id);

            return item.IsEquipped();
        }

        public bool IsMaxLevel(string id)
        {
            if (HaveItem(id) == false)
                return false;

            var item = GetAccessoryItem(id);

            return item.IsMaxLevel();
        }

        public bool IsMaxReforge(string id)
        {
            if (HaveItem(id) == false)
                return false;

            var item = GetAccessoryItem(id);

            return item.IsMaxReforge();
        }

        public int GetStackedCombineCount()
        {
            return mStackedCombineCount;
        }

        public int GetStackedAccessorysLevelUpCount()
        {
            int totalLevelUpCount = 0;

            foreach (var Accessory in mAccessoryItems.Values)
            {
                totalLevelUpCount += Accessory.GetLevel() - 1;
            }

            return totalLevelUpCount;
        }

        public bool HaveItem(string id)
        {
            return mAccessoryItems.ContainsKey(id);
        }

        public AccessoryInst GetAccessoryItem(string id)
        {
            return mAccessoryItems.TryGet(id);
        }

        public AccessoryInst GetAccessoryItem(Grade grade, SubGrade subGrade)
        {
            foreach (var inst in mAccessoryItems.Values)
            {
                if (inst.GetGrade() == grade && inst.GetSubGrade() == subGrade)
                    return inst;
            }

            return null;
        }

        public IEnumerable<AccessoryInst> GetEquippedItems()
        {
            foreach (var equipped in mEquippedItems)
                yield return GetAccessoryItem(equipped);
        }

        public string GetEquippedItemId(int slot)
        {
            if (mEquippedItems.Count <= slot)
                return null;

            return mEquippedItems[slot];
        }

        public IEnumerable<AccessoryInst> GetAccessoryItems()
        {
            return mAccessoryItems.Values;
        }

        public int GetItemLevel(string id)
        {
            var item = GetAccessoryItem(id);

            return item?.GetLevel() ?? 0;
        }

        public int GetItemReforgeStep(string id)
        {
            var item = GetAccessoryItem(id);

            return item?.GetReforgeStep() ?? 0;
        }

        public int GetItemReforgeFailedCount(string id)
        {
            var item = GetAccessoryItem(id);

            return item?.GetReforgeFailedCount() ?? 0;
        }

        public void AddItem(string id, int count)
        {
            if (mAccessoryItems.ContainsKey(id))
            {
                AccessoryInst AccessoryInst = mAccessoryItems.TryGet(id);
                if (AccessoryInst != null)
                {
                    AccessoryInst.AddCount(count);
                }
            }
            else
            {
                mAccessoryItems.Add(id, new AccessoryInst(id, 1, count, false, 0, 0, 0, 0));
            }

            SetIsChangedData(true);
        }

        public void AllUnEquip()
        {
            var equippeds = mEquippedItems.ToArray();

            foreach (var equipped in equippeds)
            {
                UnEquipItem(equipped);
            }
        }

        // �Ǽ��縮 ��õ ����
        public List<string> EquipRecomandItem()
        {
            AllUnEquip();

            List<string> equipResult = new List<string>();

            var result = FindBestAccessorys();

            if (result.best.IsValid())
            {
                EquipItem(result.best);

                equipResult.Add(result.best);

                SetIsChangedData(true);
            }

            if (result.no2.IsValid())
            {
                var data = DataUtil.GetAccessoryData(result.no2);
                if (data != null && data.type != ItemType.Necklace)
                {
                    EquipItem(result.no2);

                    equipResult.Add(result.no2);

                    SetIsChangedData(true);
                }
            }

            // � �Ǽ��縮�� �����ߴ��� �˷�����
            return equipResult;

            (string best, string no2) FindBestAccessorys()
            {
                if (mAccessoryItems.Count > 0)
                {
                    var orderInstArrays = mAccessoryItems.Values.Select(x => x.GetId()).OrderByDescending(x =>
                    {
                        return PowerLevelCalculator.CalcAccessoryPowerLevel(x);
                    }).ToArray();

                    if (orderInstArrays.Length >= 2)
                    {
                        return (orderInstArrays[0], orderInstArrays[1]);
                    }
                    else
                    {
                        return (orderInstArrays[0], null);
                    }
                }
                else
                {
                    return (null, null);
                }
            }
        }

        // �Ǽ��縮 ����
        public bool EquipItem(string id)
        {
            // ���� �����Ϸ��� ���
            // ���� ������ �ִ� ������� Ȯ��
            if (HaveItem(id) == false)
                return false;

            // �̹� �������̶�� ���� X
            if (IsEquipped(id))
                return false;

            var data = DataUtil.GetAccessoryData(id);
            if (data == null)
                return false;

            // �� ������ �ִ��� Ȯ��
            // �� ������ ���ٸ� ���� ������ �� ���� ����
            if (mEquippedItems.Count >= DataUtil.GetAccessoryMaxEquipCount(data.type))
            {
                var firstItem = mEquippedItems[0];

                var firstAccessory = GetAccessoryItem(firstItem);

                firstAccessory.SetEquip(false);

                mEquippedItems.RemoveAt(0);
            }

            // �Ǽ��縮 ����
            AccessoryInst accessoryItem = GetAccessoryItem(id);

            accessoryItem.SetEquip(true);

            SetIsChangedData(true);

            mEquippedItems.Add(id);

            return true;
        }

        public void UnEquipItem(string id)
        {
            // ���� �����Ϸ��� ���
            // ���� ������ �ִ� ������� Ȯ��
            if (HaveItem(id) == false)
                return;

            var accessoryItem = GetAccessoryItem(id);
            if (accessoryItem == null)
                return;

            if (accessoryItem.IsEquipped() == false)
                return;

            mEquippedItems.Remove(id);

            accessoryItem.SetEquip(false);

            SetIsChangedData(true);
        }

        // �Ǽ��縮 ��ȭ
        public void UpgradeItem(string id, int levelUpCount = 1)
        {
            AccessoryInst accessoryItem = GetAccessoryItem(id);
            if (accessoryItem == null)
                return;

            accessoryItem.LevelUp(levelUpCount);

            SetIsChangedData(true);
        }

        public (string id, int combineCount) CombineItem(string id)
        {
            // ���� ������ �ִ� ������� Ȯ��
            if (HaveItem(id) == false)
                return (string.Empty, 0);

            // �����Ͱ� ���� ���� �ռ� �Ұ�
            AccessoryData data = DataUtil.GetAccessoryData(id);
            if (data == null)
                return (string.Empty, 0);

            // combineCount�� ���ٸ� �ռ� ������ �Ұ����� ��
            if (data.combineCount == 0)
                return (string.Empty, 0);

            // �ռ��� �ʿ��� ��� ������ ������� Ȯ��
            AccessoryInst accessoryItem = GetAccessoryItem(id);
            if (accessoryItem.IsEnoughCount(data.combineCount) == false)
                return (string.Empty, 0);

            int combineCount = accessoryItem.GetCount() / data.combineCount;
            int useCount = data.combineCount * combineCount;

            // ���� ��� ��� �����͸� Ȯ��
            // �����Ͱ� ���ٸ� ���� �� ���� ��
            AccessoryData nextData = DataUtil.GetNextGradeAccessoryData(data.type, data.grade, data.subGrade);
            if (nextData == null)
                return (string.Empty, 0);

            // �ռ� Ƚ����ŭ ��� ���
            if (accessoryItem.UseCount(useCount) == false)
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

            for (int i = 0; i < (int)Grade.Count; ++i)
            {
                for (int j = 0; j < (int)SubGrade.Four_Star; ++j)
                {
                    Grade grade = (Grade)i;
                    SubGrade subGrade = (SubGrade)j;

                    var inst = GetAccessoryItem(grade, subGrade);
                    if (inst != null)
                    {
                        var result = CombineItem(inst.GetId());
                        if (result.id.IsValid())
                        {
                            AccessoryInst nextInst = GetAccessoryItem(result.id);
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
                                nextInst = GetAccessoryItem(result.id);

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
                            //var nextInst = GetAccessoryItem(result.id);
                            //if (nextInst != null)
                            //{
                            //    int combineCount = nextInst.GetCombineCount();

                            //    int prevTotalCount = nextInst.GetCount();
                            //    int prevRemain = prevTotalCount % combineCount;

                            //    int quotient = (prevRemain + result.combineCount) / combineCount;
                            //    if (quotient > 0)
                            //    {
                            //        int remain = (prevRemain + result.combineCount) % combineCount;
                            //        if (remain > 0)
                            //            resultList.Add((result.id, remain));
                            //    }
                            //    else
                            //    {
                            //        resultList.Add((result.id, result.combineCount));
                            //    }
                            //}
                        }
                    }
                }
            }

            return resultList;
        }

        public double GatherStatValues(StatType type)
        {
            double totalStatValue = 0;

            foreach (AccessoryInst item in mAccessoryItems.Values)
            {
                if (item.IsEquipped())
                {
                    totalStatValue += item.GetEquippedStatValues(type);
                }

                totalStatValue += item.GetOwnStatValues(type);
            }

            return totalStatValue;
        }
    }
}