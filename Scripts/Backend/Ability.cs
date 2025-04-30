using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;
using LitJson;
using BackEnd;
using System.Linq;

namespace Lance
{
    public class Ability : AccountBase
    {
        Dictionary<string, AbilityInst> mAbilitys;

        public Ability()
        {
            mAbilitys = new Dictionary<string, AbilityInst>();
        }

        public bool LevelUp(string id)
        {
            if (IsMeetRequirements(id) == false)
                return false;

            if (IsMaxLevel(id))
                return false;

            var abilityInst = mAbilitys.TryGet(id);
            if (abilityInst == null)
                return false;

            abilityInst.LevelUp();

            SetIsChangedData(true);

            return true;
        }

        public bool IsMeetRequirements(string id)
        {
            var data = Lance.GameData.AbilityData.TryGet(id);
            if (data == null)
                return false;

            if (data.requireAbilitys.IsValid())
            {
                string[] requires = data.requireAbilitys.SplitByDelim();

                for(int i = 0; i < requires.Length; ++i)
                {
                    string require = requires[i];

                    var abilityInst = mAbilitys.TryGet(require);
                    if (abilityInst == null)
                        return false;

                    if (abilityInst.IsMaxLevel() == false)
                        return false;
                }
            }

            return true;
        }

        public IEnumerable<AbilityData> GetBestStepDatas()
        {
            var result = GetBestStepAbility();

            var datas = DataUtil.GetAbilityDatasByStep(result.step);

            return datas.Where(x => IsMeetRequirements(x.id) && IsMaxLevel(x.id) == false);
        }

        public (string id, int step) GetBestStepAbility()
        {
            int bestStep = 0;
            string id = string.Empty;

            foreach(var ability in mAbilitys)
            {
                if (ability.Value.IsMaxLevel())
                    continue;

                if (IsMeetRequirements(ability.Key) == false)
                    continue;

                var data = Lance.GameData.AbilityData.TryGet(ability.Key);
                if (data == null)
                    continue;

                if (bestStep <= data.step)
                {
                    if (id.IsValid() && bestStep == data.step)
                    {
                        var data2 = Lance.GameData.AbilityData.TryGet(id);
                        if (data2.statType > data.statType)
                        {
                            bestStep = data.step;
                            id = data.id;
                        }
                    }
                    else
                    {
                        bestStep = data.step;
                        id = data.id;
                    }
                }
            }
            
            return (id, bestStep);
        }

        public int GetAbilityLevel(string id)
        {
            return mAbilitys.TryGet(id)?.GetLevel() ?? 0;
        }

        public int GetTotalAbilityLevel()
        {
            int total = 0;

            foreach(var ability in mAbilitys.Values)
            {
                total += ability.GetLevel();
            }

            return total;
        }

        public double GatherStatValues(StatType statType)
        {
            double totalValue = 0;

            foreach(AbilityInst abilityInst in mAbilitys.Values)
            {
                if (abilityInst.GetStatType() == statType)
                    totalValue += abilityInst.GetStatValue();
            }

            return totalValue;
        }

        public bool IsMaxLevel(string id)
        {
            var data = Lance.GameData.AbilityData.TryGet(id);
            if (data == null)
                return false;

            int level = GetAbilityLevel(id);
            int maxLevel = data.maxLevel;

            return level == maxLevel;
        }

        public override void RandomizeKey()
        {
            base.RandomizeKey();

            foreach (var Ability in mAbilitys.Values)
            {
                Ability.RandomizeKey();
            }
        }

        public override string GetTableName()
        {
            return "Ability";
        }

        public override string GetColumnName()
        {
            return "Ability";
        }

        public override Param GetParam()
        {
            Param param = new Param();

            foreach(var inst in mAbilitys.Values)
            {
                inst.ReadyToSave();
            }

            param.Add(GetColumnName(), mAbilitys);

            return param;
        }

        protected override void InitializeData()
        {
            foreach(var data in Lance.GameData.AbilityData.Values)
            {
                string id = data.id;

                var abilityInst = new AbilityInst();

                abilityInst.Init(data.id, 0);

                mAbilitys.Add(id, abilityInst);
            }
        }

        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            for(int i = 0; i < gameDataJson.Count; ++i)
            {
                var abilityInstData = gameDataJson[i];

                string id = abilityInstData["Id"].ToString();

                var data = Lance.GameData.AbilityData.TryGet(id);
                if (data == null)
                    continue;

                int levelTemp = 0;

                int.TryParse(abilityInstData["Level"].ToString(), out levelTemp);

                var abilityInst = new AbilityInst();

                abilityInst.Init(id, levelTemp);

                mAbilitys.Add(id, abilityInst);
            }
        }
    }

    public class AbilityInst
    {
        public string Id;
        public int Level;

        ObscuredString mId;
        ObscuredInt mLevel;
        AbilityData mData;
        public void Init(string id, int level)
        {
            mId = id;
            mLevel = level;
            mData = Lance.GameData.AbilityData.TryGet(mId);
        }

        public int GetLevel()
        {
            return mLevel;
        }

        public bool IsMaxLevel()
        {
            return mData.maxLevel == mLevel;
        }

        public StatType GetStatType()
        {
            return mData.statType;
        }

        public double GetStatValue()
        {
            return AbilityUtil.CalcStatValue(mId, mLevel);
        }

        public void LevelUp()
        {
            if (IsMaxLevel())
                return;

            mLevel = Mathf.Min(mData.maxLevel, mLevel + 1);
        }

        public void RandomizeKey()
        {
            mId.RandomizeCryptoKey();
            mLevel.RandomizeCryptoKey();
        }

        public void ReadyToSave()
        {
            Id = mId;
            Level = mLevel;
        }
    }
}


