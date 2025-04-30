using BackEnd;
using System.Linq;
using LitJson;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;


namespace Lance
{
    public class Preset : AccountBase
    {
        Dictionary<ObscuredInt, PresetInfo> mPresetInfos = new();

        public override string GetTableName()
        {
            return "Preset";
        }

        protected override void InitializeData()
        {
            base.InitializeData();

            foreach (PresetData data in Lance.GameData.PresetData.Values)
            {
                var presetInfo = new PresetInfo();

                presetInfo.Init(data.preset, data.unlockPrice <= 0 && data.requireUnlockPreset == 0);

                mPresetInfos.Add(data.preset, presetInfo);
            }
        }

        public void ExternalSetServerDataToLocal(JsonData gameDataJson)
        {
            SetServerDataToLocal(gameDataJson);
        }

        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            var presetInfoDatas = gameDataJson["PresetInfos"];

            for(int i = 0; i < presetInfoDatas.Count; ++i)
            {
                var presetInfoData = presetInfoDatas[i];

                int presetNum = i + 1;

                var presetInfo = mPresetInfos.TryGet(presetNum);
                if (presetInfo != null)
                {
                    presetInfo.SetServerDataToLocal(presetInfoData);
                }
                else
                {
                    presetInfo = new PresetInfo();

                    presetInfo.SetServerDataToLocal(presetInfoData);

                    mPresetInfos.Add(presetNum, presetInfo);
                }
            }
        }

        public override Param GetParam()
        {
            Param param = new Param();

            Dictionary<int, PresetInfo> savePresetInfos = new Dictionary<int, PresetInfo>();

            foreach (var item in mPresetInfos)
            {
                item.Value.ReadyToSave();

                savePresetInfos.Add(item.Key, item.Value);
            }

            param.Add("PresetInfos", savePresetInfos);

            return param;
        }

        public override void RandomizeKey()
        {
            base.RandomizeKey();

            foreach(var presetInfo in mPresetInfos)
            {
                presetInfo.Key.RandomizeCryptoKey();
                presetInfo.Value.RandomizeKey();
            }
        }

        public bool IsUnlockedPreset(int preset)
        {
            var presetInfo = mPresetInfos.TryGet(preset);
            if (presetInfo == null)
                return false;

            return presetInfo.IsUnlocked();
        }

        public IEnumerable<PresetInfo> GetPresetInfos()
        {
            return mPresetInfos.Values;
        }

        public PresetInfo GetPresetInfo(int preset)
        {
            return mPresetInfos.TryGet(preset);
        }
    }

    public class PresetInfo
    {
        public int PresetNum;
        public string PresetName;
        public bool Unlocked;
        public string[] Costumes;
        public List<PresetInfo_EquipmentInfo> EquipmentInfos = new();
        public List<PresetInfo_AccessoryInfo> EquippedAccessorys = new();
        public List<PresetInfo_PetInfo> PetInfos = new();
        public List<PresetInfo_PetEquipmentInfo> EquippedPetEquipments = new();
        public List<PresetInfo_SkillInfo> EquippedActiveSkills = new();
        public List<PresetInfo_SkillInfo> EquippedPassiveSkills = new();
        public List<PresetInfo_StatInfo> Stats = new();

        ObscuredInt mPresetNum;                                           // 프리셋 번호
        ObscuredString mPresetName;                                       // 프리셋 이름
        ObscuredBool mUnlocked;                                           // 프리셋 언락여부
        string[] mCostumes;                                               // 착용한 코스튬
        List<PresetInfo_EquipmentInfo> mEquipmentInfos = new();           // 모든 장비 정보
        List<PresetInfo_AccessoryInfo> mEquippedAccessorys = new();       // 착용중인 장신구
        List<PresetInfo_PetInfo> mPetInfos = new();                        // 모든 신수 정보
        List<PresetInfo_PetEquipmentInfo> mEquippedPetEquipments = new(); // 착용한 신물
        List<PresetInfo_SkillInfo> mEquippedActiveSkills = new();         // 착용중인 액티브 스킬
        List<PresetInfo_SkillInfo> mEquippedPassiveSkills = new();        // 착용중인 패시브 스킬
        List<PresetInfo_StatInfo> mStats = new();                         // 스탯

        public void Init(int presetNum, bool isUnlocked = false)
        {
            mPresetNum = presetNum;
            mPresetName = $"Preset_{presetNum}";
            mCostumes = Enumerable.Repeat(string.Empty, (int)CostumeType.Count).ToArray();
            mUnlocked = isUnlocked;
        }

        public void SetServerDataToLocal(JsonData gameDataJson)
        {
            int presetNumTemp = 1;

            int.TryParse(gameDataJson["PresetNum"].ToString(), out presetNumTemp);

            mPresetNum = presetNumTemp;

            mPresetName = gameDataJson["PresetName"].ToString();

            bool unlockedTemp = false;

            bool.TryParse(gameDataJson["Unlocked"].ToString(), out unlockedTemp);

            mUnlocked = unlockedTemp;

            // 장비 정보
            if (gameDataJson.ContainsKey("EquipmentInfos"))
            {
                var equipmentInfsoDatas = gameDataJson["EquipmentInfos"];

                for (int i = 0; i < equipmentInfsoDatas.Count; i++)
                {
                    var equipmentInfoData = equipmentInfsoDatas[i];

                    string id = equipmentInfoData["Id"].ToString();

                    int optionPresetTemp = 0;

                    int.TryParse(equipmentInfoData["PresetNum"].ToString(), out optionPresetTemp);

                    bool isEquippedTemp = false;

                    bool.TryParse(equipmentInfoData["IsEquipped"].ToString(), out isEquippedTemp);

                    var equipmentInfo = new PresetInfo_EquipmentInfo();

                    equipmentInfo.Init(id, optionPresetTemp, isEquippedTemp);

                    mEquipmentInfos.Add(equipmentInfo);
                }
            }

            // 장신구 정보
            if (gameDataJson.ContainsKey("EquippedAccessorys"))
            {
                var accessoryInfsoDatas = gameDataJson["EquippedAccessorys"];

                for (int i = 0; i < accessoryInfsoDatas.Count; i++)
                {
                    var accessoryInfoData = accessoryInfsoDatas[i];

                    string id = accessoryInfoData["Id"].ToString();

                    bool isEquippedTemp = false;

                    bool.TryParse(accessoryInfoData["IsEquipped"].ToString(), out isEquippedTemp);

                    var accessoryInfo = new PresetInfo_AccessoryInfo();

                    accessoryInfo.Init(id, isEquippedTemp);

                    mEquippedAccessorys.Add(accessoryInfo);
                }
            }

            mCostumes = Enumerable.Repeat(string.Empty, (int)CostumeType.Count).ToArray();

            // 코스튬 정보
            if (gameDataJson.ContainsKey("Costumes"))
            {
                for (int i = 0; i < gameDataJson["Costumes"].Count; ++i)
                {
                    if (mCostumes.Length <= i)
                        break;

                    mCostumes[i] = gameDataJson["Costumes"][i].ToString();
                }
            }

            // 신수 정보
            if (gameDataJson.ContainsKey("PetInfos"))
            {
                var petInfosDatas = gameDataJson["PetInfos"];

                for (int i = 0; i < petInfosDatas.Count; i++)
                {
                    var petInfoData = petInfosDatas[i];

                    string id = petInfoData["Id"].ToString();

                    int presetTemp = 0;

                    int.TryParse(petInfoData["PresetNum"].ToString(), out presetTemp);

                    bool isEquippedTemp = false;

                    bool.TryParse(petInfoData["IsEquipped"].ToString(), out isEquippedTemp);

                    var petInfo = new PresetInfo_PetInfo();

                    petInfo.Init(id, presetTemp, isEquippedTemp);

                    mPetInfos.Add(petInfo);
                }
            }

            // 신물 정보
            if (gameDataJson.ContainsKey("EquippedPetEquipments"))
            {
                var petEquipmentInfosDatas = gameDataJson["EquippedPetEquipments"];

                for (int i = 0; i < petEquipmentInfosDatas.Count; i++)
                {
                    var petEquipmentInfoData = petEquipmentInfosDatas[i];

                    string id = petEquipmentInfoData["Id"].ToString();

                    bool isEquippedTemp = false;

                    bool.TryParse(petEquipmentInfoData["IsEquipped"].ToString(), out isEquippedTemp);

                    var petEquipmentInfo = new PresetInfo_PetEquipmentInfo();

                    petEquipmentInfo.Init(id, isEquippedTemp);

                    mEquippedPetEquipments.Add(petEquipmentInfo);
                }
            }

            // 액티브 스킬
            if (gameDataJson.ContainsKey("EquippedActiveSkills"))
            {
                var activeSkillDatas = gameDataJson["EquippedActiveSkills"];

                for (int i = 0; i < activeSkillDatas.Count; i++)
                {
                    var activeSkillData = activeSkillDatas[i];

                    string id = activeSkillData["Id"].ToString();

                    var activeSkillInfo = new PresetInfo_SkillInfo();

                    activeSkillInfo.Init(id);

                    mEquippedActiveSkills.Add(activeSkillInfo);
                }
            }

            // 패시브 스킬
            if (gameDataJson.ContainsKey("EquippedPassiveSkills"))
            {
                var passiveSkillDatas = gameDataJson["EquippedPassiveSkills"];

                for (int i = 0; i < passiveSkillDatas.Count; i++)
                {
                    var passiveSkillData = passiveSkillDatas[i];

                    string id = passiveSkillData["Id"].ToString();

                    var passiveSkillInfo = new PresetInfo_SkillInfo();

                    passiveSkillInfo.Init(id);

                    mEquippedPassiveSkills.Add(passiveSkillInfo);
                }
            }

            // 스탯 정보
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

                    var statInfo = new PresetInfo_StatInfo();

                    statInfo.Init(statTypeTemp, statValueTemp);

                    mStats.Add(statInfo);
                }
            }
        }

        public void RandomizeKey()
        {
            mPresetNum.RandomizeCryptoKey();
            mPresetName?.RandomizeCryptoKey();
            mUnlocked.RandomizeCryptoKey();
        }

        public void ReadyToSave()
        {
            PresetNum = mPresetNum;
            PresetName = mPresetName;
            Unlocked = mUnlocked;
            Costumes = mCostumes;

            foreach (var equipmentInfo in mEquipmentInfos)
                equipmentInfo.ReadyToSave();
            EquipmentInfos = mEquipmentInfos;

            foreach (var accessoryInfo in mEquippedAccessorys)
                accessoryInfo.ReadyToSave();
            EquippedAccessorys = mEquippedAccessorys;

            foreach (var petInfo in mPetInfos)
                petInfo.ReadyToSave();
            PetInfos = mPetInfos;

            foreach (var petEquipmentInfo in mEquippedPetEquipments)
                petEquipmentInfo.ReadyToSave();
            EquippedPetEquipments = mEquippedPetEquipments;

            foreach (var activeSkillInfo in mEquippedActiveSkills)
                activeSkillInfo.ReadyToSave();
            EquippedActiveSkills = mEquippedActiveSkills;

            foreach (var passiveSkillInfo in mEquippedPassiveSkills)
                passiveSkillInfo.ReadyToSave();
            EquippedPassiveSkills = mEquippedPassiveSkills;

            foreach (var statInfo in mStats)
                statInfo.ReadyToSave();
            Stats = mStats;
        }

        public string GetPresetName()
        {
            return mPresetName;
        }

        public void SetPresetName(string name)
        {
            mPresetName = name;
        }

        public bool IsUnlocked()
        {
            return mUnlocked;
        }

        public void Unlock()
        {
            if (mUnlocked == false)
                mUnlocked = true;
        }

        public void SetEquipmentInfos(IEnumerable<EquipmentInst> equipments)
        {
            mEquipmentInfos.Clear();

            foreach (var equipment in equipments)
            {
                if (equipment != null)
                {
                    var id = equipment.GetId();
                    int preset = equipment.GetCurrentPreset();
                    bool isEquipped = equipment.IsEquipped();

                    var info = new PresetInfo_EquipmentInfo();

                    info.Init(id, preset, isEquipped);

                    mEquipmentInfos.Add(info);
                }
            }
        }

        public List<PresetInfo_EquipmentInfo> GetEquipmentInfos()
        {
            return mEquipmentInfos;
        }

        public void SetEquippedAccessorys(List<AccessoryInst> accessorys)
        {
            mEquippedAccessorys.Clear();

            if (accessorys != null)
            {
                foreach (var accessory in accessorys)
                {
                    if (accessory != null)
                    {
                        var id = accessory.GetId();
                        bool isEquipped = accessory.IsEquipped();

                        var info = new PresetInfo_AccessoryInfo();

                        info.Init(id, isEquipped);

                        mEquippedAccessorys.Add(info);
                    }
                }
            }
        }

        public List<PresetInfo_AccessoryInfo> GetEquippedAccessorys()
        {
            return mEquippedAccessorys;
        }

        public void SetEquippedCostumes(string[] costumes)
        {
            if (mCostumes == null)
                mCostumes = Enumerable.Repeat(string.Empty, (int)CostumeType.Count).ToArray();

            if (costumes.Length != mCostumes.Length)
                return;

            for (int i = 0; i < costumes.Length; ++i)
            {
                mCostumes[i] = costumes[i];
            }
        }

        public string[] GetEquippedCostumes()
        {
            return mCostumes;
        }

        public void SetPetInfos(IEnumerable<PetInst> pets)
        {
            mPetInfos.Clear();

            if (pets != null)
            {
                foreach (var pet in pets)
                {
                    if (pet != null)
                    {
                        var id = pet.GetId();
                        int preset = pet.GetCurrentPreset();
                        bool isEquipped = pet.GetIsEquipped();

                        var info = new PresetInfo_PetInfo();

                        info.Init(id, preset, isEquipped);

                        mPetInfos.Add(info);
                    }
                }
            }
            
        }

        public List<PresetInfo_PetInfo> GetPetInfos()
        {
            return mPetInfos;
        }

        public PresetInfo_PetInfo GetEquippedPetInfo()
        {
            if (mPetInfos != null)
            {
                foreach (var petInfo in mPetInfos)
                {
                    if (petInfo != null && petInfo.GetId().IsValid() && petInfo.GetIsEquipped())
                        return petInfo;
                }
            }

            return null;
        }

        public void SetEquippedPetEquipments(IEnumerable<PetEquipmentInst> equipments)
        {
            mEquippedPetEquipments.Clear();

            if (equipments != null)
            {
                foreach (var equipment in equipments)
                {
                    if (equipment != null)
                    {
                        var id = equipment.GetId();
                        bool isEquipped = equipment.IsEquipped();

                        var info = new PresetInfo_PetEquipmentInfo();

                        info.Init(id, isEquipped);

                        mEquippedPetEquipments.Add(info);
                    }
                }
            }
        }

        public List<PresetInfo_PetEquipmentInfo> GetEquippedPetEquipments()
        {
            return mEquippedPetEquipments;
        }

        public void SetEquippedActiveSkills(string[] activeSkills)
        {
            mEquippedActiveSkills.Clear();

            if (activeSkills != null)
            {
                foreach (var activeSkill in activeSkills)
                {
                    if (activeSkill.IsValid())
                    {
                        var info = new PresetInfo_SkillInfo();

                        info.Init(activeSkill);

                        mEquippedActiveSkills.Add(info);
                    }
                }
            }
            
        }

        public List<PresetInfo_SkillInfo> GetEquippedActiveSkills()
        {
            return mEquippedActiveSkills;
        }

        public void SetEquippedPassiveSkills(string[] passiveSkills)
        {
            mEquippedPassiveSkills.Clear();

            if (passiveSkills != null)
            {
                foreach (var passiveSkill in passiveSkills)
                {
                    if (passiveSkill.IsValid())
                    {
                        var info = new PresetInfo_SkillInfo();

                        info.Init(passiveSkill);

                        mEquippedPassiveSkills.Add(info);
                    }
                }
            }
        }

        public List<PresetInfo_SkillInfo> GetEquippedPassiveSkills()
        {
            return mEquippedPassiveSkills;
        }

        public void SetStats(Dictionary<StatType, double> stats)
        {
            mStats.Clear();

            foreach (var stat in stats)
            {
                int statTypeInt = (int)stat.Key;
                double statValue = stat.Value;

                var info = new PresetInfo_StatInfo();

                info.Init(statTypeInt, statValue);

                mStats.Add(info);
            }
        }

        public List<PresetInfo_StatInfo> GetStats()
        {
            return mStats;
        }
        public bool IsEmpty()
        {
            return mEquipmentInfos.Count == 0 &&
                mEquippedAccessorys.Count == 0 &&
                mStats.Count == 0 &&
                mEquippedActiveSkills.Count == 0 &&
                mEquippedPassiveSkills.Count == 0;
        }
    }

    public class PresetInfo_EquipmentInfo
    {
        public string Id;
        public int PresetNum;
        public bool IsEquipped;

        string mId;
        int mPresetNum;
        bool mIsEquipped;

        public void Init(string id, int presetNum, bool isEquipped)
        {
            mId = id;
            mPresetNum = presetNum;
            mIsEquipped = isEquipped;
        }

        public void ReadyToSave()
        {
            Id = mId;
            PresetNum = mPresetNum;
            IsEquipped = mIsEquipped;
        }

        public string GetId()
        {
            return mId;
        }

        public int GetPresetNum()
        {
            return mPresetNum;
        }

        public bool GetIsEquipped()
        {
            return mIsEquipped;
        }
    }

    public class PresetInfo_AccessoryInfo
    {
        public string Id;
        public bool IsEquipped;

        string mId;
        bool mIsEquipped;

        public void Init(string id, bool isEquipped)
        {
            mId = id;
            mIsEquipped = isEquipped;
        }

        public void ReadyToSave()
        {
            Id = mId;
            IsEquipped = mIsEquipped;
        }

        public string GetId()
        {
            return mId;
        }

        public bool GetIsEquipped()
        {
            return mIsEquipped;
        }
    }

    public class PresetInfo_PetInfo
    {
        public string Id;
        public int PresetNum;
        public bool IsEquipped;

        string mId;
        int mPresetNum;
        bool mIsEquipped;

        public void Init(string id, int presetNum, bool isEquipped)
        {
            mId = id;
            mPresetNum = presetNum;
            mIsEquipped = isEquipped;
        }

        public void ReadyToSave()
        {
            Id = mId;
            PresetNum = mPresetNum;
            IsEquipped = mIsEquipped;
        }

        public string GetId()
        {
            return mId;
        }

        public int GetPresetNum()
        {
            return mPresetNum;
        }

        public bool GetIsEquipped()
        {
            return mIsEquipped;
        }
    }

    public class PresetInfo_PetEquipmentInfo
    {
        public string Id;
        public bool IsEquipped;

        string mId;
        bool mIsEquipped;

        public void Init(string id, bool isEquipped)
        {
            mId = id;
            mIsEquipped = isEquipped;
        }

        public void ReadyToSave()
        {
            Id = mId;
            IsEquipped = mIsEquipped;
        }

        public string GetId()
        {
            return mId;
        }

        public bool GetIsEquipped()
        {
            return mIsEquipped;
        }
    }

    public class PresetInfo_SkillInfo
    {
        public string Id;

        public string mId;

        public void Init(string id)
        {
            mId = id;
        }

        public void ReadyToSave()
        {
            Id = mId;
        }

        public string GetId()
        {
            return mId;
        }
    }

    public class PresetInfo_StatInfo
    {
        public int StatType;
        public double StatValue;

        int mStatType;
        double mStatValue;

        public void Init(int statType, double statValue)
        {
            mStatType = statType;
            mStatValue = statValue;
        }

        public void Init(StatType statType, double statValue)
        {
            mStatType = (int)statType;
            mStatValue = statValue;
        }

        public void ReadyToSave()
        {
            StatType = mStatType;
            StatValue = mStatValue;
        }

        public StatType GetStatType()
        {
            return (StatType)((int)mStatType);
        }

        public double GetStatValue()
        {
            return mStatValue;
        }
    }
}