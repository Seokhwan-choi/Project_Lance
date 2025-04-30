using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;
using LitJson;
using System.Linq;
using System;

namespace Lance
{
    public class PetEvolutionStatInfo
    {
        public int CurPreset;
        public Dictionary<int, List<PetEvolutionStat>> PetStats;

        ObscuredInt mCurPreset;
        Dictionary<int, List<PetEvolutionStat>> mPetStats = new();
        public void InitializeData()
        {
            for(int i = 0; i < Lance.GameData.PetCommonData.evolutionStatMaxPreset; ++i)
            {
                int preset = i;

                var petStats = new List<PetEvolutionStat>();

                for (int j = 0; j < Lance.GameData.PetCommonData.evolutionStatMaxSlot; ++j)
                {
                    int slot = j;

                    petStats.Add(new PetEvolutionStat(slot));
                }

                mPetStats.Add(preset, petStats);
            }
        }

        public void SetServerDataToLocal(JsonData jsonData)
        {
            JsonData petStatDatas = jsonData["PetStats"];

            int curPresetTemp = 0;

            int.TryParse(jsonData["CurPreset"].ToString(), out curPresetTemp);

            mCurPreset = curPresetTemp;

            for (int i = 0; i < Lance.GameData.PetCommonData.evolutionStatMaxPreset; ++i)
            {
                int preset = i;

                var petStats = new List<PetEvolutionStat>();

                foreach (JsonData stat in petStatDatas[$"{preset}"])
                {
                    int slotTemp = 0;

                    int.TryParse(stat["Slot"].ToString(), out slotTemp);

                    int statTypeTemp = 0;

                    int.TryParse(stat["StatType"].ToString(), out statTypeTemp);

                    int gradeTemp = 0;

                    int.TryParse(stat["Grade"].ToString(), out gradeTemp);

                    bool isLockedTemp = false;

                    bool.TryParse(stat["IsLocked"].ToString(), out isLockedTemp);

                    petStats.Add(new PetEvolutionStat(slotTemp, statTypeTemp, gradeTemp, isLockedTemp));
                }

                if (petStats.Count != Lance.GameData.PetCommonData.evolutionStatMaxSlot)
                {
                    var petStatSlots = petStats.Select(x => x.GetSlot());

                    for (int j = 0; j < Lance.GameData.PetCommonData.evolutionStatMaxSlot; ++j)
                    {
                        int newSlot = j;
                        if (petStatSlots.Contains(newSlot) == false)
                        {
                            petStats.Add(new PetEvolutionStat(newSlot));

                            if (petStats.Count == Lance.GameData.PetCommonData.evolutionStatMaxSlot)
                                break;
                        }
                    }
                }

                mPetStats.Add(preset, petStats);
            }
        }

        public void ReadyToSave()
        {
            foreach(var petStatList in mPetStats.Values)
            {
                foreach(var petStat in petStatList)
                {
                    petStat.ReadyToSave();
                }
            }

            PetStats = mPetStats;
            CurPreset = mCurPreset;
        }

        public void RandomizeKey()
        {
            foreach (var petStatList in mPetStats.Values)
            {
                foreach (var petStat in petStatList)
                {
                    petStat.RandomizeKey();
                }
            }

            mCurPreset.RandomizeCryptoKey();
        }

        public int GetEvolutionStatLockCount(int petEvolutionStep)
        {
            return GetActiveStats(petEvolutionStep).Count(x => x.GetLocked());
        }

        public List<(StatType, double)> GatherStatValues(int petEvolutionStep)
        {
            List<(StatType, double)> statValues = new List<(StatType, double)>();

            foreach (var petStat in GetActiveStats(petEvolutionStep))
            {
                statValues.Add((petStat.GetStatType(), petStat.GetStatValue()));
            }

            return statValues;
        }

        public int GetCurrentPreset()
        {
            return mCurPreset;
        }

        public void ChangePreset(int preset)
        {
            mCurPreset = preset;

            mCurPreset = Math.Clamp(mCurPreset, 0, Lance.GameData.PetCommonData.evolutionStatMaxPreset - 1);
        }

        public double GatherStatValues(StatType statType, int petEvolutionStep)
        {
            double totalStatValue = 0;

            foreach(var petStat in GetActiveStats(petEvolutionStep))
            {
                if (petStat.GetStatType() != statType)
                    continue;

                totalStatValue += petStat.GetStatValue();
            }

            return totalStatValue;
        }

        public bool ChangeStats(int petEvolutionStep)
        {
            bool changeStat = false;

            foreach (var petStat in GetActiveStats(petEvolutionStep))
            {
                if (petStat.GetLocked())
                    continue;

                petStat.ChangeStat();

                changeStat = true;
            }

            return changeStat;
        }

        public bool AnyCanChangeStat(int petEvolutionStep)
        {
            bool any = false;

            foreach (var petStat in GetActiveStats(petEvolutionStep))
            {
                if (petStat.GetLocked() == false)
                {
                    any = true;
                    break;
                }
            }

            return any;
        }

        IEnumerable<PetEvolutionStat> GetActiveStats(int petEvolutionStep)
        {
            var petStatList = mPetStats.TryGet(mCurPreset);

            return petStatList.Where(x =>
            {
                int unlockStep = Lance.GameData.PetEvolutionStatUnlockData.unlockSlot[x.GetSlot()];

                return petEvolutionStep >= unlockStep;
            });
        }

        public PetEvolutionStat GetStat(int slot)
        {
            var petStatList = mPetStats.TryGet(mCurPreset);

            return petStatList.Where(x => x.GetSlot() == slot).FirstOrDefault();
        }

        public int GetRequireEvolutionStatStone(int petEvolutionStep)
        {
            int lockCount = GetEvolutionStatLockCount(petEvolutionStep);

            return Lance.GameData.PetEvolutionStatChangePrice.changePrice[lockCount];
        }

        public int GetEvolutionStatCount(int petEvolutionStep)
        {
            int count = 0;

            for (int i = 0; i < Lance.GameData.PetEvolutionStatUnlockData.unlockSlot.Length; ++i)
            {
                int unlockStep = Lance.GameData.PetEvolutionStatUnlockData.unlockSlot[i];

                if (petEvolutionStep >= unlockStep)
                    count++;
            }

            return count;
        }

        public bool IsAllEvolutionStatLocked(int petEvolutionStep)
        {
            return GetEvolutionStatCount(petEvolutionStep) == GetEvolutionStatLockCount(petEvolutionStep);
        }

        public bool IsSatisfied(StatType[] statTypes, PetEvolutionStatGrade statGrade, int step)
        {
            foreach (PetEvolutionStat optionStat in GetActiveStats(step))
            {
                if (optionStat.GetLocked())
                    continue;

                if (statTypes.Contains(optionStat.GetStatType()))
                {
                    if (optionStat.GetGrade() >= statGrade)
                        return true;
                }
            }

            return false;
        }
    }

    public class PetEvolutionStat
    {
        public int Slot;
        public int StatType;
        public int Grade;
        public bool IsLocked;

        ObscuredInt mSlot;
        ObscuredInt mStatType;
        ObscuredInt mGrade;
        ObscuredBool mIsLocked;

        public PetEvolutionStat(int slot)
        {
            mSlot = slot;
            mIsLocked = false;

            ChangeStat();
        }

        public PetEvolutionStat(int slot, int statType, int grade, bool isLocked)
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

        public PetEvolutionStatGrade GetGrade()
        {
            return (PetEvolutionStatGrade)(int)mGrade;
        }

        public StatType GetStatType()
        {
            return (StatType)(int)mStatType;
        }

        public double GetStatValue()
        {
            return DataUtil.GetPetEvolutionStatValue(GetStatType(), GetGrade());
        }

        public void ChangeStat()
        {
            if (mIsLocked)
                return;

            (int grade, StatType statType) newStat = DataUtil.ChangePetEvolutionStat();

            if (newStat.grade >= mGrade)
            {
                mGrade = newStat.grade;
            }

            mStatType = (int)newStat.statType;
        }
    }
}