using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using BackEnd;
using LitJson;
using CodeStage.AntiCheat.ObscuredTypes;

namespace Lance
{
    public class JoustBattleInfo : AccountBase
    {
        ObscuredString mNickName;   // 닉네임
        ObscuredDouble mPowerLevel; // 전투력
        ObscuredInt mLevel;         // 레벨
        ObscuredString[] mCostumes; // 착용중인 코스튬
        public override string GetTableName()
        {
            return "JoustBattleInfo";
        }

        protected override void InitializeData()
        {
            base.InitializeData();

            mNickName = Backend.UserNickName;
            mCostumes = Enumerable.Repeat<ObscuredString>(string.Empty, (int)CostumeType.Count).ToArray();
        }

        public void ExternalSetServerDataToLocal(JsonData gameDataJson)
        {
            SetServerDataToLocal(gameDataJson);
        }

        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            mNickName = gameDataJson["NickName"].ToString();

            double powerLevelTemp = 0;

            double.TryParse(gameDataJson["PowerLevel"].ToString(), out powerLevelTemp);

            mPowerLevel = powerLevelTemp;

            int levelTemp = 0;

            int.TryParse(gameDataJson["Level"].ToString(), out levelTemp);

            mLevel = levelTemp;

            mCostumes = Enumerable.Repeat<ObscuredString>(string.Empty, (int)CostumeType.Count).ToArray();

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

            param.Add("NickName", mNickName.ToString());
            param.Add("PowerLevel", (double)mPowerLevel);
            param.Add("Level", (int)mLevel);
            param.Add("Costumes", mCostumes.Select(x => x.ToString()).ToArray());

            return param;
        }

        public override void RandomizeKey()
        {
            base.RandomizeKey();

            mNickName.RandomizeCryptoKey();
            mPowerLevel.RandomizeCryptoKey();
            mLevel.RandomizeCryptoKey();

            foreach(var costume in mCostumes)
            {
                costume.RandomizeCryptoKey();
            }
        }

        public void SetNickName(string nickName)
        {
            mNickName = nickName;

            SetIsChangedData(true);
        }

        public string GetNickName()
        {
            return mNickName;
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

        public void SetLevel(int level)
        {
            mLevel = level;

            SetIsChangedData(true);
        }

        public int GetLevel()
        {
            return mLevel;
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