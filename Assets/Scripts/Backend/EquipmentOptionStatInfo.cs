using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;
using LitJson;
using System.Linq;
using System;

namespace Lance
{
    public class EquipmentOptionStatInfo
    {
        public int CurPreset;
        public Dictionary<int, List<EquipmentOptionStat>> OptionStats;

        ObscuredInt mCurPreset;
        Dictionary<int, List<EquipmentOptionStat>> mOptionStats = new();
        public void InitializeData()
        {
            for (int i = 0; i < Lance.GameData.EquipmentCommonData.optionStatMaxPreset; ++i)
            {
                int preset = i;

                var optionStats = new List<EquipmentOptionStat>();

                for (int j = 0; j < Lance.GameData.EquipmentCommonData.optionStatMaxCount; ++j)
                {
                    int slot = j;

                    optionStats.Add(new EquipmentOptionStat(slot));
                }

                mOptionStats.Add(preset, optionStats);
            }
        }

        public void SetServerDataToLocal(JsonData jsonData)
        {
            JsonData optionStatDatas = jsonData["OptionStats"];

            int curPresetTemp = 0;

            int.TryParse(jsonData["CurPreset"].ToString(), out curPresetTemp);

            mCurPreset = curPresetTemp;

            for (int i = 0; i < Lance.GameData.EquipmentCommonData.optionStatMaxPreset; ++i)
            {
                int preset = i;

                var optionStats = new List<EquipmentOptionStat>();

                if (optionStatDatas.ContainsKey($"{preset}"))
                {
                    foreach (JsonData stat in optionStatDatas[$"{preset}"])
                    {
                        int slotTemp = 0;

                        int.TryParse(stat["Slot"].ToString(), out slotTemp);

                        int statTypeTemp = 0;

                        int.TryParse(stat["StatType"].ToString(), out statTypeTemp);

                        int gradeTemp = 0;

                        int.TryParse(stat["Grade"].ToString(), out gradeTemp);

                        bool isLockedTemp = false;

                        bool.TryParse(stat["IsLocked"].ToString(), out isLockedTemp);

                        optionStats.Add(new EquipmentOptionStat(slotTemp, statTypeTemp, gradeTemp, isLockedTemp));
                    }

                    mOptionStats.Add(preset, optionStats);
                }
            }

            // »Æ¿Œ
            for (int i = 0; i < Lance.GameData.EquipmentCommonData.optionStatMaxPreset; ++i)
            {
                int newPreset = i;

                if (mOptionStats.ContainsKey(newPreset) == false)
                {
                    var newOptionStats = new List<EquipmentOptionStat>();

                    for (int j = 0; j < Lance.GameData.EquipmentCommonData.optionStatMaxCount; ++j)
                    {
                        int slot = j;

                        newOptionStats.Add(new EquipmentOptionStat(slot));
                    }

                    mOptionStats.Add(newPreset, newOptionStats);
                }
            }
        }

        public void ReadyToSave()
        {
            foreach (var optionStatList in mOptionStats.Values)
            {
                foreach (var optionStat in optionStatList)
                {
                    optionStat.ReadyToSave();
                }
            }

            OptionStats = mOptionStats;
            CurPreset = mCurPreset;
        }

        public void RandomizeKey()
        {
            foreach (var optionStatList in mOptionStats.Values)
            {
                foreach (var optionStat in optionStatList)
                {
                    optionStat.RandomizeKey();
                }
            }

            mCurPreset.RandomizeCryptoKey();
        }

        public int GetOptionStatLockCount(Grade grade)
        {
            return GetActiveStats(grade).Count(x => x.GetLocked());
        }

        public double GatherStatValues(StatType statType, Grade grade, bool isEquipped)
        {
            double totalStatValues = 0;

            foreach (var optionStat in GetActiveStats(grade))
            {
                if (optionStat.GetStatType() == statType)
                    totalStatValues += optionStat.GetStatValue(grade, isEquipped);
            }

            return totalStatValues;
        }

        public int GetCurrentPreset()
        {
            return mCurPreset;
        }

        public void ChangePreset(int preset)
        {
            mCurPreset = preset;

            mCurPreset = Math.Clamp(mCurPreset, 0, Lance.GameData.EquipmentCommonData.optionStatMaxPreset - 1);
        }

        public bool ChangeStats(Grade grade)
        {
            bool changeStat = false;

            foreach (var optionStat in GetActiveStats(grade))
            {
                if (optionStat.GetLocked())
                    continue;

                optionStat.ChangeStat();

                changeStat = true;
            }

            return changeStat;
        }

        public bool IsSatisfied(StatType[] statTypes, EquipmentOptionStatGrade optionGrade, Grade grade)
        {
            foreach (EquipmentOptionStat optionStat in GetActiveStats(grade))
            {
                if (optionStat.GetLocked())
                    continue;

                if (statTypes.Contains(optionStat.GetStatType()))
                {
                    if (optionStat.GetGrade() >= optionGrade)
                        return true;
                }
            }

            return false;
        }

        public bool AnyCanChangeStat(Grade grade)
        {
            bool any = false;

            foreach (var optionStat in GetActiveStats(grade))
            {
                if (optionStat.GetLocked() == false)
                {
                    any = true;
                    break;
                }
            }

            return any;
        }

        IEnumerable<EquipmentOptionStat> GetActiveStats(Grade grade)
        {
            var optionStatList = mOptionStats.TryGet(mCurPreset);

            var data = Lance.GameData.EquipmentOptionStatUnlockData.TryGet(grade);

            return optionStatList.Where(x =>
            {
                return data.unlockSlot[x.GetSlot()];
            });
        }

        public bool IsActiveStat(Grade grade, int slot)
        {
            var data = Lance.GameData.EquipmentOptionStatUnlockData.TryGet(grade);

            return data.unlockSlot[slot];
        }

        public EquipmentOptionStat GetStat(int slot)
        {
            var optionStatList = mOptionStats.TryGet(mCurPreset);

            return optionStatList.Where(x => x.GetSlot() == slot).FirstOrDefault();
        }

        public double GetOptionStatChangePrice(Grade grade)
        {
            var data = Lance.GameData.EquipmentOptionStatChangePrice.TryGet(grade);
            if (data == null)
                return double.MaxValue;

            return data.changePrice[GetOptionStatLockCount(grade)];
        }

        public int GetOptionStatCount(Grade grade)
        {
            int count = 0;

            var data = Lance.GameData.EquipmentOptionStatUnlockData.TryGet(grade);

            for (int i = 0; i < data.unlockSlot.Length; ++i)
            {
                if (data.unlockSlot[i])
                    count++;
            }

            return count;
        }

        public bool IsAllOptionStatLocked(Grade grade)
        {
            return GetOptionStatCount(grade) == GetOptionStatLockCount(grade);
        }
    }

    public class EquipmentOptionStat
    {
        public int Slot;
        public int StatType;
        public int Grade;
        public bool IsLocked;

        ObscuredInt mSlot;
        ObscuredInt mStatType;
        ObscuredInt mGrade;
        ObscuredBool mIsLocked;

        public EquipmentOptionStat(int slot)
        {
            mSlot = slot;
            mIsLocked = false;

            ChangeStat();
        }

        public EquipmentOptionStat(int slot, int statType, int grade, bool isLocked)
        {
            mSlot = slot;
            mStatType = statType;
            mGrade = grade;
            mIsLocked = isLocked;
        }

        public void ReadyToSave()
        {
            Slot = mSlot;
            StatType = mStatType;
            Grade = mGrade;
            IsLocked = mIsLocked;
        }

        public void RandomizeKey()
        {
            mSlot.RandomizeCryptoKey();
            mStatType.RandomizeCryptoKey();
            mGrade.RandomizeCryptoKey();
            mIsLocked.RandomizeCryptoKey();
        }

        public void ToggleChangeLock()
        {
            mIsLocked = !mIsLocked;
        }

        public void SetLock(bool isLock)
        {
            mIsLocked = isLock;
        }

        public bool GetLocked()
        {
            return mIsLocked;
        }

        public int GetSlot()
        {
            return mSlot;
        }

        public EquipmentOptionStatGrade GetGrade()
        {
            return (EquipmentOptionStatGrade)(int)mGrade;
        }

        public StatType GetStatType()
        {
            return (StatType)(int)mStatType;
        }

        public double GetStatValue(Grade equipmentGrade, bool isEquipped)
        {
            double statValue = DataUtil.GetEquipmentOptionStatValue(GetStatType(), GetGrade());

            if (isEquipped == false)
            {
                var data = Lance.GameData.EquipmentOptionStatToOwnStatData.TryGet(equipmentGrade);
                if (data != null)
                {
                    statValue *= data.changeValue;
                }
            }

            return statValue;
        }

        public void ChangeStat()
        {
            if (mIsLocked)
                return;

            (int grade, StatType statType) newStat = DataUtil.ChangeEquipmentOptionStat();

            if (newStat.grade >= mGrade)
            {
                mGrade = newStat.grade;
            }

            mStatType = (int)newStat.statType;
        }
    }
}