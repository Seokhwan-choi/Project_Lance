using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;
using BackEnd;
using LitJson;
using System;
using System.Linq;

namespace Lance
{
    public class BeginnerRaidScore : AccountBase
    {
        ObscuredDouble mRaidBossBestScore;
        ObscuredDouble[] mRaidBossBestScores;

        public BeginnerRaidScore()
        {
            mRaidBossBestScores = Enumerable.Repeat<ObscuredDouble>(0, (int)ElementalType.Count).ToArray();
        }

        public void UpdateRaidBossDamage(ElementalType type, double damage)
        {
            if (mRaidBossBestScore < damage)
            {
                SetRaidBossDamage(damage);
            }

            int typeInx = (int)type;

            if (mRaidBossBestScores.Length > typeInx && typeInx >= 0)
            {
                if (mRaidBossBestScores[typeInx] < damage)
                {
                    mRaidBossBestScores[typeInx] = damage;

                    SetIsChangedData(true);
                }
            }
        }

        public void SetRaidBossDamage(double damage)
        {
            mRaidBossBestScore = damage;

            SetIsChangedData(true);
        }

        public double GetRaidBossDamage()
        {
            return mRaidBossBestScore;
        }

        public double GetRaidBossBestDamage(ElementalType type)
        {
            int typeInx = (int)type;

            if (mRaidBossBestScores.Length <= typeInx)
                return 0;

            return mRaidBossBestScores[typeInx];
        }

        public override bool CanUpdateRankScore()
        {
            return mRaidBossBestScore > 0 && mRaidBossBestScore <= Lance.GameData.RaidRankCommonData.rankStandardDamage;
        }

        public override void RandomizeKey()
        {
            base.RandomizeKey();

            mRaidBossBestScore.RandomizeCryptoKey();

            if (mRaidBossBestScores != null)
            {
                foreach(var score in mRaidBossBestScores)
                {
                    score.RandomizeCryptoKey();
                }
            }
        }

        public override string GetTableName()
        {
            return "BeginnerRaidScore";
        }

        public override Param GetParam()
        {
            Param param = new Param();

            param.Add("BestScore", (double)mRaidBossBestScore);
            param.Add("BestScores", mRaidBossBestScores.Select(x => (double)x).ToArray());

            return param;
        }

        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            double bestScoreTemp = 0;

            double.TryParse(gameDataJson["BestScore"].ToString(), out bestScoreTemp);

            mRaidBossBestScore = bestScoreTemp;

            if (gameDataJson.ContainsKey("BestScores"))
            {
                for (int i = 0; i < gameDataJson["BestScores"].Count; ++i)
                {
                    if (mRaidBossBestScores.Length <= i)
                        break;

                    double bestScoresTemp = 0;

                    double.TryParse(gameDataJson["BestScores"][i].ToString(), out bestScoresTemp);

                    mRaidBossBestScores[i] = bestScoresTemp;
                }
            }
        }
    }
}