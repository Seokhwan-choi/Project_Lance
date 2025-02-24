using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using BackEnd;
using LitJson;
using CodeStage.AntiCheat.ObscuredTypes;

namespace Lance
{
    public class RankProfile : AccountBase
    {
        ObscuredString mCountryCode;    // 국가 코드
        ObscuredInt mLevel;             // 레벨
        ObscuredDouble mPowerLevel;     // 전투력
        ObscuredInt mLevelRank;         // 레벨 순위
        ObscuredInt mPowerLevelRank;    // 전투력 순위
        ObscuredInt mStageRank;         // 스테이지 순위
        List<RankProfile_EquipmentInfo> mEquipments = new();    // 장비
        List<RankProfile_AccessoryInfo> mAccessorys = new();    // 장신구
        RankProfile_PetInfo mPet;                               // 신수
        ObscuredString[] mCostumes;                             // 코스튬
        List<RankProfile_StatInfo> mStats = new();              // 스탯
        public override string GetTableName()
        {
            return "RankProfile";
        }

        protected override void InitializeData()
        {
            base.InitializeData();

            mCostumes = Enumerable.Repeat<ObscuredString>(string.Empty, (int)CostumeType.Count).ToArray();
        }

        public void ExternalSetServerDataToLocal(JsonData gameDataJson)
        {
            SetServerDataToLocal(gameDataJson);
        }

        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            if (gameDataJson.ContainsKey("CountryCode"))
                mCountryCode = gameDataJson["CountryCode"].ToString();

            double powerLevelTemp = 0;

            double.TryParse(gameDataJson["PowerLevel"].ToString(), out powerLevelTemp);

            mPowerLevel = powerLevelTemp;

            int levelTemp = 0;

            int.TryParse(gameDataJson["Level"].ToString(), out levelTemp);

            mLevel = levelTemp;

            int levelRankTemp = 0;

            int.TryParse(gameDataJson["LevelRank"].ToString(), out levelRankTemp);

            mLevelRank = levelRankTemp;

            int powerLevelRankTemp = 0;

            int.TryParse(gameDataJson["PowerLevelRank"].ToString(), out powerLevelRankTemp);

            mPowerLevelRank = powerLevelRankTemp;

            int stageRankTemp = 0;

            int.TryParse(gameDataJson["StageRank"].ToString(), out stageRankTemp);

            mStageRank = stageRankTemp;

            mCostumes = Enumerable.Repeat<ObscuredString>(string.Empty, (int)CostumeType.Count).ToArray();

            if (gameDataJson.ContainsKey("Equipments"))
            {
                var equipments = gameDataJson["Equipments"];

                for(int i = 0; i < equipments.Count; ++i)
                {
                    var equipment = equipments[i];

                    string id = equipment["Id"].ToString();

                    int equipmentLevelTemp = 1;

                    int.TryParse(equipment["Level"].ToString(), out equipmentLevelTemp);

                    int equipmentReforgeStepTemp = 0;

                    int.TryParse(equipment["ReforgeStep"].ToString(), out equipmentReforgeStepTemp);

                    var equipmenInfo = new RankProfile_EquipmentInfo();

                    equipmenInfo.Init(id, equipmentLevelTemp, equipmentReforgeStepTemp);

                    mEquipments.Add(equipmenInfo);
                }
            }

            if (gameDataJson.ContainsKey("Accessorys"))
            {
                var accessorys = gameDataJson["Accessorys"];

                for (int i = 0; i < accessorys.Count; ++i)
                {
                    var accessory = accessorys[i];

                    string id = accessory["Id"].ToString();

                    int accessoryLevelTemp = 1;

                    int.TryParse(accessory["Level"].ToString(), out accessoryLevelTemp);

                    int accessoryReforgeStepTemp = 0;

                    int.TryParse(accessory["ReforgeStep"].ToString(), out accessoryReforgeStepTemp);

                    var accessoryInfo = new RankProfile_AccessoryInfo();

                    accessoryInfo.Init(id, accessoryLevelTemp, accessoryReforgeStepTemp);

                    mAccessorys.Add(accessoryInfo);
                }
            }

            if (gameDataJson.ContainsKey("Pet"))
            {
                var pet = gameDataJson["Pet"];

                string id = pet["Id"].ToString();

                int petLevelTemp = 1;

                int.TryParse(pet["Level"].ToString(), out petLevelTemp);

                int petStepTemp = 0;

                int.TryParse(pet["Step"].ToString(), out petStepTemp);

                mPet = new RankProfile_PetInfo();

                mPet.Init(id, petLevelTemp, petStepTemp);
            }

            if (gameDataJson.ContainsKey("Stats"))
            {
                var stats = gameDataJson["Stats"];

                for (int i = 0; i < stats.Count; ++i)
                {
                    var stat = stats[i];

                    int statTypeTemp = 0;

                    int.TryParse(stat["StatType"].ToString(), out statTypeTemp);

                    double statValueTemp = 0;

                    double.TryParse(stat["StatValue"].ToString(), out statValueTemp);

                    var statInfo = new RankProfile_StatInfo();

                    statInfo.Init(statTypeTemp, statValueTemp);

                    mStats.Add(statInfo);
                }
            }

            if (gameDataJson.ContainsKey("Costumes"))
            {
                for (int i = 0; i < gameDataJson["Costumes"].Count; ++i)
                {
                    if (mCostumes.Length <= i)
                        break;

                    mCostumes[i] = gameDataJson["Costumes"][i].ToString();
                }
            }
        }

        public override Param GetParam()
        {
            Param param = new Param();

            if (mCountryCode.IsValid())
                param.Add("CountryCode", mCountryCode.ToString());
            param.Add("Level", (int)mLevel);
            param.Add("PowerLevel", (double)mPowerLevel);
            param.Add("LevelRank", (int)mLevelRank);
            param.Add("PowerLevelRank", (int)mPowerLevelRank);
            param.Add("StageRank", (int)mStageRank);

            foreach (var equipment in mEquipments)
            {
                equipment.ReadyToSave();
            }
            param.Add("Equipments", mEquipments);

            foreach (var accessory in mAccessorys)
            {
                accessory.ReadyToSave();
            }
            param.Add("Accessorys", mAccessorys);

            if (mPet != null)
            {
                mPet.ReadyToSave();
                param.Add("Pet", mPet);
            }

            foreach (var stat in mStats)
            {
                stat.ReadyToSave();
            }
            param.Add("Stats", mStats);

            param.Add("Costumes", mCostumes.Select(x => x.ToString()).ToArray());

            return param;
        }

        public override void RandomizeKey()
        {
            base.RandomizeKey();

            mCountryCode?.RandomizeCryptoKey();
            mLevel.RandomizeCryptoKey();
            mPowerLevel.RandomizeCryptoKey();
            mLevelRank.RandomizeCryptoKey();
            mPowerLevelRank.RandomizeCryptoKey();
            mStageRank.RandomizeCryptoKey();

            foreach(var equipment in mEquipments)
            {
                equipment.RandomizeKey();
            }
            
            foreach(var accessory in mAccessorys)
            {
                accessory.RandomizeKey();
            }

            mPet?.RandomizeKey();

            foreach (var stat in mStats)
            {
                stat.RandomizeKey();
            }

            foreach (var costume in mCostumes)
            {
                costume.RandomizeCryptoKey();
            }
        }

        public void SetCountryCode(string countryCode)
        {
            mCountryCode = countryCode;

            SetIsChangedData(true);
        }

        public string GetCountryCode()
        {
            return mCountryCode;
        }

        public void SetLevel(int level)
        {
            mLevel = level;

            SetIsChangedData(true);
        }

        public int GetLevel()
        {
            return mLevel;
        }

        public void SetPowerLevel(double powerLevel)
        {
            mPowerLevel = powerLevel;

            SetIsChangedData(true);
        }

        public double GetPowerLevel()
        {
            return mPowerLevel;
        }

        public void SetLevelRank(int levelRank)
        {
            mLevelRank = levelRank;

            SetIsChangedData(true);
        }

        public int GetLevelRank()
        {
            return mLevelRank;
        }

        public void SetPowerLevelRank(int powerLevelRank)
        {
            mPowerLevelRank = powerLevelRank;

            SetIsChangedData(true);
        }

        public int GetPowerLevelRank()
        {
            return mPowerLevelRank;
        }

        public void SetStageRank(int stageRank)
        {
            mStageRank = stageRank;

            SetIsChangedData(true);
        }

        public int GetStageRank()
        {
            return mStageRank;
        }

        public void SetEquipments(EquipmentInst[] equipments)
        {
            mEquipments.Clear();

            if (equipments != null)
            {
                foreach (var equipment in equipments)
                {
                    if (equipment != null)
                    {
                        var info = new RankProfile_EquipmentInfo();

                        info.Init(equipment.GetId(), equipment.GetLevel(), equipment.GetReforgeStep());

                        mEquipments.Add(info);
                    }
                }
            }

            SetIsChangedData(true);
        }

        public List<RankProfile_EquipmentInfo> GetEquipments()
        {
            return mEquipments;
        }

        public void SetAccessorys(AccessoryInst[] accessorys)
        {
            mAccessorys.Clear();

            if (accessorys != null)
            {
                foreach (var accessory in accessorys)
                {
                    if (accessory != null)
                    {
                        var info = new RankProfile_AccessoryInfo();

                        info.Init(accessory.GetId(), accessory.GetLevel(), accessory.GetReforgeStep());

                        mAccessorys.Add(info);
                    }
                }
            }

            SetIsChangedData(true);
        }

        public List<RankProfile_AccessoryInfo> GetAccessoryInfos()
        {
            return mAccessorys;
        }

        public void SetPet(PetInst pet)
        {
            if (pet != null)
            {
                mPet = new RankProfile_PetInfo();

                mPet.Init(pet.GetId(), pet.GetLevel(), pet.GetStep());

                SetIsChangedData(true);
            }
        }

        public RankProfile_PetInfo GetPetInfo()
        {
            return mPet;
        }

        public void SetStats(Dictionary<StatType, double> stats)
        {
            mStats.Clear();

            foreach(var stat in stats)
            {
                var statInfo = new RankProfile_StatInfo();

                statInfo.Init(stat.Key, stat.Value);

                mStats.Add(statInfo);
            }

            SetIsChangedData(true);
        }

        public List<RankProfile_StatInfo> GetStats()
        {
            return mStats;
        }

        public void SetCostumes(string[] costumes)
        {
            if (mCostumes == null)
                mCostumes = Enumerable.Repeat<ObscuredString>(string.Empty, (int)CostumeType.Count).ToArray();

            if (costumes.Length != mCostumes.Length)
                return;

            for (int i = 0; i < costumes.Length; ++i)
            {
                mCostumes[i] = costumes[i];
            }

            SetIsChangedData(true);
        }

        public ObscuredString GetCostume(CostumeType costumeType)
        {
            return mCostumes[(int)costumeType];
        }

        public string[] GetCostumes()
        {
            return mCostumes.Select(x => x.ToString()).ToArray();
        }
    }
}