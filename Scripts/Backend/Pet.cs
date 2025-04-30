using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;
using System;
using LitJson;
using BackEnd;

namespace Lance
{
    public class PetInst
    {
        public string Id;
        public int Level;
        public int Step;
        public int Exp;
        public int StackedExp;
        public bool IsEquipped;
        public PetEvolutionStatInfo EvolutionStat;

        ObscuredString mId;
        ObscuredInt mLevel;
        ObscuredInt mStep;
        ObscuredInt mExp;
        ObscuredInt mStackedExp;
        ObscuredBool mIsEquipped;
        PetEvolutionStatInfo mEvolutionStat;

        public PetInst(string id, int level, int step, int exp, int stackedExp, bool isEquipped, PetEvolutionStatInfo evolutionStat)
        {
            mId = id;
            mLevel = level;
            mStep = step;
            mExp = exp;
            mStackedExp = stackedExp;
            mIsEquipped = isEquipped;
            mEvolutionStat = evolutionStat;
        }

        public void ReadyToSave()
        {
            Id = mId;
            Level = mLevel;
            Step = mStep;
            Exp = mExp;
            StackedExp = mStackedExp;
            IsEquipped = mIsEquipped;
            mEvolutionStat.ReadyToSave();
            EvolutionStat = mEvolutionStat;
        }

        public void RandomizeKey()
        {
            mId.RandomizeCryptoKey();
            mLevel.RandomizeCryptoKey();
            mStep.RandomizeCryptoKey();
            mExp.RandomizeCryptoKey();
            mStackedExp.RandomizeCryptoKey();
            mIsEquipped.RandomizeCryptoKey();
            mEvolutionStat.RandomizeKey();
        }

        public ElementalType GetElementalType()
        {
            var data = Lance.GameData.PetData.TryGet(mId);

            return data?.type ?? ElementalType.Normal;
        }

        public void SetEquip(bool equip)
        {
            mIsEquipped = equip;
        }

        public bool GetIsEquipped()
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

        public ObscuredInt GetStep()
        {
            return mStep;
        }

        public ObscuredInt GetExp()
        {
            return mExp;
        }

        public ObscuredInt GetMaxStep()
        {
            var data = Lance.GameData.PetData.TryGet(mId);

            return data?.maxStep ?? 0;
        }

        public ObscuredInt GetMaxLevel()
        {
            var data = Lance.GameData.PetStepData.TryGet(mStep);

            return data?.maxLevel ?? int.MaxValue;
        }

        public int GetMaxLevelRequirePetFood()
        {
            int curLevel = mLevel;
            int maxLevel = GetMaxLevel();
            int levelUpRequire = GetRequireLevelUpPetFood();

            int remainLevel = maxLevel - curLevel;
            int maxLeveltotalRequire = remainLevel * levelUpRequire;

            return maxLeveltotalRequire - mExp;
        }

        public bool IsMaxLevel()
        {
            return mLevel == GetMaxLevel();
        }

        public int GetRequireLevelUpPetFood()
        {
            var levelUpData = Lance.GameData.PetLevelUpData.TryGet(mStep);
            if (levelUpData == null)
                return int.MaxValue;

            return levelUpData.require;
        }

        public (bool, int) StackExp(int exp)
        {
            if (IsMaxLevel())
                return (false, 0);

            mStackedExp += exp;
            mExp += exp;

            int levelUpCount = 0;

            int requireLevelUpPetFood = GetRequireLevelUpPetFood();

            if (mExp >= requireLevelUpPetFood)
            {
                while (mExp >= requireLevelUpPetFood)
                {
                    mExp -= requireLevelUpPetFood;

                    LevelUp(1);

                    levelUpCount++;
                }

                return (true, levelUpCount);
            }
            else
            {
                return (false, levelUpCount);
            }
        }

        public void LevelUp(int levelUpCount)
        {
            mLevel += levelUpCount;

            mLevel = Math.Min(mLevel, GetMaxLevel());
        }

        public bool IsMaxStep()
        {
            return GetMaxStep() == mStep;
        }

        public int GetRequireEvolutionStone()
        {
            var evolutionData = Lance.GameData.PetEvolutionData.TryGet(mStep);
            if (evolutionData == null)
                return int.MaxValue;

            return evolutionData.require;
        }

        public int GetRequireEvolutionStatStone()
        {
            return mEvolutionStat.GetRequireEvolutionStatStone(mStep);
        }

        public bool Evolution()
        {
            // 최고 단계라면 진화 불가능
            if (IsMaxStep())
                return false;

            // 최고 레벨에서만 진화할 수 있다.
            if (IsMaxLevel() == false)
                return false;

            // 다음 단계로 넘어갓
            mStep = mStep + 1;

            return true;
        }

        public (StatType type, double value) GetEquippedStatValue()
        {
            PetEquipStatData data = Lance.GameData.PetEquipStatData.TryGet(mStep);
            if (data == null)
                return (StatType.Atk, 0);

            return (data.valueType, data.value);
        }

        public List<(StatType type, double value)> GetOwnStatValues()
        {
            List<(StatType, double)> statValues = new List<(StatType, double)>();

            PetData data = Lance.GameData.PetData.TryGet(mId);
            if (data == null)
                return null;

            for (int i = 0; i < data.ownStats.Length; ++i)
            {
                string ownStatId = data.ownStats[i];
                if (ownStatId.IsValid() == false)
                    continue;

                var ownStatData = Lance.GameData.PetOwnStatData.TryGet(ownStatId);
                if (ownStatData == null)
                    continue;

                double statValue = ownStatData.baseValue + (ownStatData.levelUpValue * (mLevel - 1));

                statValues.Add((ownStatData.valueType, statValue));
            }

            statValues.AddRange(mEvolutionStat.GatherStatValues(mStep));

            return statValues;
        }

        public PetEvolutionStat GetEvolutionStat(int slot)
        {
            return mEvolutionStat.GetStat(slot);
        }

        public bool IsSatisfied(StatType[] statTypes, PetEvolutionStatGrade statGrade)
        {
            return mEvolutionStat.IsSatisfied(statTypes, statGrade, mStep);
        }

        public int GetEvolutionStatLockCount()
        {
            return mEvolutionStat.GetEvolutionStatLockCount(mStep);
        }

        public bool IsAllEvolutionStatLocked()
        {
            return mEvolutionStat.IsAllEvolutionStatLocked(mStep);
        }

        public bool ChangeEvolutionStats()
        {
            return mEvolutionStat.ChangeStats(mStep);
        }

        public void ChangeEvolutionStatPreset(int preset)
        {
            mEvolutionStat.ChangePreset(preset);
        }

        public int GetCurrentPreset()
        {
            return mEvolutionStat.GetCurrentPreset();
        }

        public bool AnyCanChangeEvolutionStat()
        {
            return mEvolutionStat.AnyCanChangeStat(mStep);
        }

        public string GetActiveSkill()
        {
            var data = Lance.GameData.PetData.TryGet(mId);
            if (data == null)
                return string.Empty;

            return DataUtil.GetPetSkill(data.type, SkillType.Active, mStep);
        }

        public string GetPassiveSkill()
        {
            var data = Lance.GameData.PetData.TryGet(mId);
            if (data == null)
                return string.Empty;

            return DataUtil.GetPetSkill(data.type, SkillType.Passive, mStep);
        }
    }

    public class Pet : AccountBase
    {
        Dictionary<ObscuredString, PetInst> mPetInsts = new();

        protected override void InitializeData()
        {
            base.InitializeData();

            foreach(var data in Lance.GameData.PetData.Values)
            {
                var petEvolutionStatInfo = new PetEvolutionStatInfo();
                petEvolutionStatInfo.InitializeData();

                var petInst = new PetInst(data.id, 1, 0, 0, 0, false, petEvolutionStatInfo);

                mPetInsts.Add(data.id, petInst);
            }
        }

        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            var petsData = gameDataJson["Pets"];

            for (int i = 0; i < petsData.Count; i++)
            {
                var petJsonData = petsData[i];

                string id = petJsonData["Id"].ToString();

                int levelTemp = 0;
                int.TryParse(petJsonData["Level"].ToString(), out levelTemp);

                int stepTemp = 0;
                int.TryParse(petJsonData["Step"].ToString(), out stepTemp);

                int expTemp = 0;
                int.TryParse(petJsonData["Exp"].ToString(), out expTemp);

                int stackedExpTemp = 0;
                int.TryParse(petJsonData["StackedExp"].ToString(), out stackedExpTemp);

                bool isEquippedTemp = false;
                bool.TryParse(petJsonData["IsEquipped"].ToString(), out isEquippedTemp);

                PetEvolutionStatInfo evolutionStat = new PetEvolutionStatInfo();

                if (petJsonData.ContainsKey("EvolutionStat"))
                {
                    evolutionStat.SetServerDataToLocal(petJsonData["EvolutionStat"]);
                }
                else
                {
                    evolutionStat.InitializeData();
                }

                mPetInsts.Add(id, new PetInst(id, levelTemp, stepTemp, expTemp, stackedExpTemp, isEquippedTemp, evolutionStat));
            }
        }

        public override Param GetParam()
        {
            Dictionary<string, PetInst> savePets = new Dictionary<string, PetInst>();

            foreach (var item in mPetInsts)
            {
                item.Value.ReadyToSave();

                savePets.Add(item.Key, item.Value);
            }

            Param param = new Param();

            param.Add("Pets", savePets);

            return param;
        }

        public override void RandomizeKey()
        {
            base.RandomizeKey();

            if (mPetInsts != null && mPetInsts.Count > 0)
            {
                foreach (var item in mPetInsts)
                {
                    item.Key.RandomizeCryptoKey();
                    item.Value.RandomizeKey();
                }
            }
        }

        public override string GetTableName()
        {
            return "Pet";
        }

        public int GetLevel(string id)
        {
            var pet = GetPet(id);
            if (pet == null)
                return 0;

            return pet.GetLevel();
        }

        public int GetMaxLevel(string id)
        {
            var pet = GetPet(id);
            if (pet == null)
                return 0;

            return pet.GetMaxLevel();
        }

        public int GetMaxLevelRequirePetFood(string id)
        {
            var pet = GetPet(id);
            if (pet == null)
                return int.MaxValue;

            return pet.GetMaxLevelRequirePetFood();
        }

        public int GetStep(string id)
        {
            var pet = GetPet(id);
            if (pet == null)
                return 0;

            return pet.GetStep();
        }

        public int GetCurrentExp(string id)
        {
            var pet = GetPet(id);
            if (pet == null)
                return 0;

            return pet.GetExp();
        }

        public int GetRequirePetFood(string id)
        {
            var pet = GetPet(id);
            if (pet == null)
                return int.MaxValue;

            return pet.GetRequireLevelUpPetFood();
        }

        public int GetRequireElementalStone(string id)
        {
            var pet = GetPet(id);
            if (pet == null)
                return int.MaxValue;

            return pet.GetRequireEvolutionStone();
        }

        public int GetRequireEvolutionStatElementalStone(string id)
        {
            var pet = GetPet(id);
            if (pet == null)
                return int.MaxValue;

            return pet.GetRequireEvolutionStone();
        }


        // 현재 장착중인 펫의 속성가져오기
        public ElementalType GetCurrentElementalType()
        {
            foreach(var petInst in mPetInsts.Values)
            {
                if (petInst.GetIsEquipped())
                {
                    return petInst.GetElementalType();
                }
            }

            return ElementalType.Normal;
        }

        public IEnumerable<PetInst> GetPetInsts()
        {
            return mPetInsts.Values;
        }

        public PetInst GetEquippedInst()
        {
            foreach (var petInst in mPetInsts.Values)
            {
                if (petInst.GetIsEquipped())
                {
                    return petInst;
                }
            }

            return null;
        }

        public bool IsEquipped(string id)
        {
            var petInst = mPetInsts.TryGet(id);
            if (petInst == null)
                return false;

            return petInst.GetIsEquipped();
        }

        public PetInst FindEquippedPet()
        {
            foreach(var pet in mPetInsts.Values)
            {
                if (pet.GetIsEquipped())
                    return pet;
            }

            return null;
        }

        public PetInst GetPet(string id)
        {
            return mPetInsts.TryGet(id);
        }

        // 펫 장착
        public bool EquipPet(string id)
        {
            // 이미 착용중이라면 착용 X
            if (IsEquipped(id))
                return false;

            // 이미 착용중인 펫이 있다면 착용 해제
            var equippedPet = FindEquippedPet();
            equippedPet?.SetEquip(false);

            // 펫 착용
            PetInst equipPet = GetPet(id);
            equipPet.SetEquip(true);

            SetIsChangedData(true);

            return true;
        }

        // 펫 해제
        public void UnEquipItem(string id)
        {
            var pet = GetPet(id);
            if (pet == null)
                return;

            if (pet.GetIsEquipped() == false)
                return;

            pet.SetEquip(false);

            SetIsChangedData(true);
        }

        public bool IsMaxLevel(string id)
        {
            PetInst pet = GetPet(id);
            if (pet == null)
                return false;

            return pet.IsMaxLevel();
        }

        public bool IsMaxStep(string id)
        {
            PetInst pet = GetPet(id);
            if (pet == null)
                return false;

            return pet.IsMaxStep();
        }

        public (bool, int) StackExp(string id, int petFood)
        {
            var pet = GetPet(id);
            if (pet == null)
                return (false, 0);

            var result = pet.StackExp(petFood);
            if (result.Item1)
            {
                SetIsChangedData(true);
            }

            return result;
        }

        // 펫 진화
        public bool EvolutionPet(string id)
        {
            var pet = GetPet(id);
            if (pet == null)
                return false;

            if (pet.IsMaxStep())
                return false;

            if (pet.IsMaxLevel() == false)
                return false;

            pet.Evolution();

            SetIsChangedData(true);

            return true;
        }

        // 펫 능력치 읽어오기
        public double GatherStatValues(StatType type)
        {
            double totalStatValue = 0;

            foreach (PetInst pet in mPetInsts.Values)
            {
                if (pet.GetIsEquipped())
                {
                    var equipped = pet.GetEquippedStatValue();

                    if (equipped.type == type)
                        totalStatValue += equipped.value;
                }

                var owns = pet.GetOwnStatValues();

                foreach (var own in owns)
                {
                    if (own.type == type)
                    {
                        totalStatValue += own.value;
                    }
                }

                owns = null;
            }

            return totalStatValue;
        }

        public IEnumerable<ObscuredString> GetKeys()
        {
            return mPetInsts.Keys;
        }

        public PetEvolutionStat GetEvolutionStat(string id, int slot)
        {
            PetInst pet = GetPet(id);
            if (pet == null)
                return null;

            return pet.GetEvolutionStat(slot);
        }

        public bool IsSatisfied(string id, StatType[] statTypes, PetEvolutionStatGrade statGrade)
        {
            PetInst pet = GetPet(id);
            if (pet == null)
                return false;

            return pet.IsSatisfied(statTypes, statGrade);
        }

        public int GetEvolutionStatLockCount(string id)
        {
            PetInst pet = GetPet(id);
            if (pet == null)
                return int.MaxValue;

            return pet.GetEvolutionStatLockCount();
        }

        public bool IsAllEvolutionStatLocked(string id)
        {
            PetInst pet = GetPet(id);
            if (pet == null)
                return false;

            return pet.IsAllEvolutionStatLocked();
        }

        public bool ChangeEvolutionStats(string id)
        {
            PetInst pet = GetPet(id);
            if (pet == null)
                return false;

            return pet.ChangeEvolutionStats();
        }

        public void ChangeEvolutionStatPreset(string id, int preset)
        {
            PetInst pet = GetPet(id);
            if (pet == null)
                return;

            pet.ChangeEvolutionStatPreset(preset);

            SetIsChangedData(true);
        }

        public int GetCurrentPreset(string id)
        {
            PetInst pet = GetPet(id);
            if (pet == null)
                return int.MaxValue;

            return pet.GetCurrentPreset();
        }

        public string GetActiveSkill(string id)
        {
            PetInst pet = GetPet(id);
            if (pet == null)
                return string.Empty;

            return pet.GetActiveSkill();
        }

        public string GetPassiveSkill(string id)
        {
            PetInst pet = GetPet(id);
            if (pet == null)
                return string.Empty;

            return pet.GetPassiveSkill();
        }

        public int GetMaxLevel()
        {
            int maxLevel = 0;

            foreach(var pet in mPetInsts.Values)
            {
                var level = pet.GetLevel();
                if (level > maxLevel)
                {
                    maxLevel = level;
                }
            }

            return maxLevel;
        }

        public int GetMaxStep()
        {
            int maxStep = 0;

            foreach (var pet in mPetInsts.Values)
            {
                var step = pet.GetStep();
                if (step > maxStep)
                {
                    maxStep = step;
                }
            }

            return maxStep;
        }

        public bool AnyEquipped()
        {
            foreach(var pet in mPetInsts.Values)
            {
                if (pet.GetIsEquipped())
                    return true;
            }

            return false;
        }
    }
}