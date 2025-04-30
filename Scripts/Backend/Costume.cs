using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CodeStage.AntiCheat.ObscuredTypes;
using LitJson;
using BackEnd;
using System;


namespace Lance
{
    // 장비 정보
    public class CostumeInst
    {
        public string Id;           // 절대 직접 호출해서 사용하지 말 것
        public bool SavedIsEquipped;
        public int Level;

        CostumeData mData;
        ObscuredString mId;         // 아이디
        ObscuredInt mLevel;
        ObscuredBool mIsEquipped;   // 장착 여부
        public CostumeInst(string id, int level, bool isEquipped)
        {
            mData = DataUtil.GetCostumeData(id);
            mId = id;
            mLevel = level;
            mIsEquipped = isEquipped;
        }

        public void ReadyToSave()
        {
            Id = mId;
            Level = mLevel;
            SavedIsEquipped = mIsEquipped;
        }

        public void RandomizeKey()
        {
            mId.RandomizeCryptoKey();
            mLevel.RandomizeCryptoKey();
            mIsEquipped.RandomizeCryptoKey();
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

        public int GetLevel()
        {
            return mLevel;
        }

        public bool IsMaxLevel()
        {
            return mLevel >= mData.maxLevel;
        }

        public Grade GetGrade()
        {
            return mData.grade;
        }

        public double GetUpgradeRequireStone()
        {
            return DataUtil.GetCostumeUpgradeRequireStone(mData.grade, mLevel);
        }

        public double GetUpgradeRequire()
        {
            return DataUtil.GetCostumeUpgradeRequire(mData.grade, mLevel);
        }

        public CostumeType GetCostumeType()
        {
            return mData.type;
        }

        public List<(StatType type, double value)> GetOwnStatValues()
        {
            string id = mId;

            List<(StatType, double)> statValues = new List<(StatType, double)>();

            if (id.IsValid() == false)
                return null;

            if (mData == null)
                return null;

            for (int i = 0; i < mData.ownStats.Length; ++i)
            {
                string ownStatId = mData.ownStats[i];
                if (ownStatId.IsValid() == false)
                    continue;

                var ownStatData = Lance.GameData.CostumeOwnStatData.TryGet(ownStatId);
                if (ownStatData == null)
                    continue;

                int ownLevel = Math.Max(0, mLevel - 1);

                var totalValue = ownStatData.baseValue + (ownLevel * ownStatData.levelUpValue);

                statValues.Add((ownStatData.valueType, totalValue));
            }

            return statValues;
        }

        public void LevelUp()
        {
            mLevel += 1;

            mLevel = Math.Min(mLevel, mData.maxLevel);
        }
    }
    public class Costume : AccountBase
    {
        Dictionary<ObscuredString, CostumeInst> mCostumeInsts = new();

        public override string GetTableName()
        {
            return "Costume";
        }

        protected override void InitializeData()
        {
            foreach (var costumeData in Lance.GameData.BodyCostumeData.Values)
            {
                if (costumeData.defaultCostume)
                {
                    if (HaveCostume(costumeData.id) == false)
                    {
                        AddCostume(costumeData.id);
                        EquipCostume(costumeData.id);
                    }
                }
            }

            foreach (var costumeData in Lance.GameData.WeaponCostumeData.Values)
            {
                if (costumeData.defaultCostume)
                {
                    if (HaveCostume(costumeData.id) == false)
                    {
                        AddCostume(costumeData.id);
                        EquipCostume(costumeData.id);
                    }
                }
            }

            foreach (var costumeData in Lance.GameData.EtcCostumeData.Values)
            {
                if (costumeData.defaultCostume)
                {
                    if (HaveCostume(costumeData.id) == false)
                    {
                        AddCostume(costumeData.id);
                        EquipCostume(costumeData.id);
                    }
                }
            }
        }
        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            var costumesData = gameDataJson["Constumes"];

            for (int i = 0; i < costumesData.Count; i++)
            {
                var costumeJsonData = costumesData[i];

                string id = costumeJsonData["Id"].ToString();

                bool isEquipped = false;

                if (costumeJsonData.ContainsKey("SavedIsEquipped"))
                {
                    bool.TryParse(costumeJsonData["SavedIsEquipped"].ToString(), out isEquipped);
                }

                int levelTemp = 1;

                if (costumeJsonData.ContainsKey("Level"))
                {
                    int.TryParse(costumeJsonData["Level"].ToString(), out levelTemp);
                }

                mCostumeInsts.Add(id, new CostumeInst(id, levelTemp, isEquipped));
            }

            foreach (var costumeData in Lance.GameData.BodyCostumeData.Values)
            {
                if (costumeData.defaultCostume)
                {
                    if (HaveCostume(costumeData.id) == false)
                        AddCostume(costumeData.id);
                }
            }

            foreach (var costumeData in Lance.GameData.WeaponCostumeData.Values)
            {
                if (costumeData.defaultCostume)
                {
                    if (HaveCostume(costumeData.id) == false)
                        AddCostume(costumeData.id);
                }
            }

            foreach (var costumeData in Lance.GameData.EtcCostumeData.Values)
            {
                if (costumeData.defaultCostume)
                {
                    if (HaveCostume(costumeData.id) == false)
                    {
                        AddCostume(costumeData.id);
                        EquipCostume(costumeData.id);
                    }
                }
            }
        }

        public override Param GetParam()
        {
            Dictionary<string, CostumeInst> saveCostumes = new Dictionary<string, CostumeInst>();

            foreach (var item in mCostumeInsts)
            {
                item.Value.ReadyToSave();

                saveCostumes.Add(item.Key, item.Value);
            }

            Param param = new Param();

            param.Add("Constumes", saveCostumes);

            return param;
        }

        public override void RandomizeKey()
        {
            base.RandomizeKey();

            if (mCostumeInsts != null && mCostumeInsts.Count > 0)
            {
                foreach (var item in mCostumeInsts)
                {
                    item.Key.RandomizeCryptoKey();
                    item.Value.RandomizeKey();
                }
            }
        }

        public bool IsEquipped(string id)
        {
            if (HaveCostume(id) == false)
                return false;

            var item = GetCostume(id);

            return item.IsEquipped();
        }

        public bool HaveCostume(string id)
        {
            return mCostumeInsts.ContainsKey(id);
        }

        public CostumeInst GetCostume(string id)
        {
            return mCostumeInsts.TryGet(id);
        }

        public IEnumerable<string> GetCostumeIds(CostumeType costumeType)
        {
            foreach(var costume in mCostumeInsts.Values)
            {
                string costumeId = costume.GetId();
                var data = DataUtil.GetCostumeData(costumeId);
                if (data != null && data.type == costumeType)
                    yield return costumeId;
            }
        }

        public void AddCostume(string id)
        {
            if (mCostumeInsts.ContainsKey(id) == false)
            {
                mCostumeInsts.Add(id, new CostumeInst(id, 1, false));

                SetIsChangedData(true);
            }
        }

        CostumeInst FindEquippedCostume(CostumeType type)
        {
            CostumeInst equippedCostume = null;

            foreach (var costume in  mCostumeInsts.Values)
            {
                if (costume.GetCostumeType() == type &&
                    costume.IsEquipped())
                {
                    // 장비는 한번에 하나만 착용할 수 있다.
                    // 착용중인 장비를 찾았으면 break
                    equippedCostume = costume;

                    break;
                }
            }

            return equippedCostume;
        }

        // 장비 장착
        public bool EquipCostume(string id)
        {
            // 새로 착용하려는 코스튬이 내가 가지고 있는지 확인
            if (HaveCostume(id) == false)
                return false;

            // 이미 착용중이라면 착용 X
            if (IsEquipped(id))
                return false;

            CostumeInst costume = GetCostume(id);

            // 이미 착용중인 코스튬이 있다면
            var equippedItem = FindEquippedCostume(costume.GetCostumeType());

            // 착용 해제
            equippedItem?.SetEquip(false);

            // 코스튬 착용
            costume.SetEquip(true);

            SetIsChangedData(true);

            return true;
        }

        public double GatherStatValues(StatType type)
        {
            double totalStatValue = 0;

            foreach (CostumeInst item in mCostumeInsts.Values)
            {
                var owns = item.GetOwnStatValues();

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

        public string GetEquippedCostumeId(CostumeType costumeType)
        {
            foreach(var costume in mCostumeInsts.Values)
            {
                if (costume.GetCostumeType() == costumeType)
                {
                    if (costume.IsEquipped())
                        return costume.GetId();
                }
            }

            return string.Empty;
        }

        public string[] GetEquippedCostumeIds()
        {
            string[] costumes = Enumerable.Repeat(string.Empty, (int)CostumeType.Count).ToArray();

            for(int i = 0; i < (int)CostumeType.Count; ++i)
            {
                CostumeType costumeType = (CostumeType)i;

                foreach (var costume in mCostumeInsts.Values)
                {
                    if (costume.GetCostumeType() == costumeType)
                    {
                        if (costume.IsEquipped())
                            costumes[i] = costume.GetId();
                    }
                }
            }

            return costumes;
        }

        public int CalcTotalPayments()
        {
            int totalPayments = 0;
            
            foreach (CostumeInst item in mCostumeInsts.Values)
            {
                var data = DataUtil.GetCostumeData(item.GetId());
                if (data == null)
                    continue;

                if (data.productId.IsValid())
                {
                    totalPayments += data.price;
                }
            }

            return totalPayments;
        }
    }
}