using System.Linq;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using BackEnd;
using LitJson;
using System;

namespace Lance
{
    public class GuideQuest : AccountBase
    {
        ObscuredInt mStep;
        QuestInfo mQuest;
        List<ObscuredInt> mReceivedRewardList;      // stepÀ» ÀúÀå
        GuideData Data => Lance.GameData.GuideData.TryGet(mStep);
        protected override void InitializeData()
        {
            mStep = 1;

            mReceivedRewardList = new List<ObscuredInt>();

            CreateQuest();
        }
        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            int stepTemp = 0;

            int.TryParse(gameDataJson["Step"].ToString(), out stepTemp);

            mStep = stepTemp;

            var questJsonData = gameDataJson["Quest"];

            string id = questJsonData["Id"].ToString();

            var questData = DataUtil.GetQuestData(id);
            if (questData != null)
            {
                int requireCountTemp = 0;

                int.TryParse(questJsonData["RequireCount"].ToString(), out requireCountTemp);

                bool isReceivedTemp = true;

                bool.TryParse(questJsonData["IsReceived"].ToString(), out isReceivedTemp);

                mQuest = new QuestInfo();
                mQuest.Init(id, requireCountTemp, isReceivedTemp, false);

                if (mQuest.GetQuestCheckType() == QuestCheckType.Attain)
                {
                    mQuest.AttainRequireCount(Lance.Account.GetQuestTypeValue(mQuest.GetQuestType()));

                    SetIsChangedData(true);
                }
                else
                {
                    if (mQuest.GetQuestType() == QuestType.TryUpgradeArtifact)
                    {
                        if (Lance.Account.Artifact.IsAllArtifactMaxLevel())
                        {
                            mQuest.StackRequireCount(mQuest.GetMaxRequireCount());

                            SetIsChangedData(true);
                        }
                    }
                }
            }

            mReceivedRewardList = new List<ObscuredInt>();

            if (gameDataJson["ReceiveRewardList"].Count > 0)
            {
                for(int i = 0; i < gameDataJson["ReceiveRewardList"].Count; ++i)
                {
                    int receivedRewardTemp = 0;

                    int.TryParse(gameDataJson["ReceiveRewardList"][i].ToString(), out receivedRewardTemp);

                    mReceivedRewardList.Add(receivedRewardTemp);
                }
            }

            if (questData == null)
            {
                CreateQuest();
            }

            //InitializeData();
        }

        public override string GetTableName()
        {
            return "GuideQuest";
        }

        public override Param GetParam()
        {
            mQuest?.ReadyToSave();

            Param param = new Param();

            param.Add("Step", (int)mStep);
            if (mQuest != null)
            {
                param.Add("Quest", mQuest);
            }
            param.Add("ReceiveRewardList", mReceivedRewardList.Select(x => (int)x).ToArray());

            return param;
        }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        public void SkipGuideStep(int step)
        {
            mStep = step;

            mReceivedRewardList.Clear();

            CreateQuest();

            SetIsChangedData(true);
        }

        public void PrevGuideStep()
        {
            mStep = Math.Max(1, mStep - 1);

            mReceivedRewardList.Clear();

            CreateQuest();

            SetIsChangedData(true);
        }
#endif

        public override void RandomizeKey()
        {
            base.RandomizeKey();

            mStep.RandomizeCryptoKey();
            mQuest?.RandomizeKey();

            foreach(var receivedReward in mReceivedRewardList)
            {
                receivedReward.RandomizeCryptoKey();
            }
        }

        public int GetCurrentStep()
        {
            return mStep;
        }

        public string GetCurrentReward()
        {
            return mQuest?.GetReward() ?? string.Empty;
        }

        public QuestInfo GetCurrentQuest()
        {
            return mQuest;
        }

        public bool CanReceiveReward()
        {
            if (mReceivedRewardList.Contains(mStep))
                return false;

            return mQuest.CanReceiveReward();
        }

        public bool IsReceivedCurrentReward()
        {
            return mReceivedRewardList.Contains(mStep);
        }

        public (string, int) ReceiveReward()
        {
            if (mQuest.CanReceiveReward())
            {
                var result = mQuest.ReceiveReward();
                if (result.rewardId.IsValid())
                {
                    SetIsChangedData(true);

                    mReceivedRewardList.Add(Data.step);

                    NextStep();

                    return result;
                }
            }

            return (string.Empty, 0);
        }

        public void NextStep()
        {
            int nextStep = Data.step + 1;

            GuideData nextGuideData = DataUtil.GetGuideData(nextStep);

            mStep = nextGuideData != null ? nextStep : mStep;

            CreateQuest();
        }

        void CreateQuest()
        {
            if (Data != null)
            {
                QuestData questData = DataUtil.GetQuestData(Data.quest);

                if (mReceivedRewardList.Contains(Data.step) == false)
                {
                    mQuest = new QuestInfo();
                    mQuest.Init(questData.id, 0, false, false);

                    if (mQuest.GetQuestCheckType() == QuestCheckType.Attain)
                    {
                        mQuest.AttainRequireCount(Lance.Account.GetQuestTypeValue(mQuest.GetQuestType()));
                    }
                    else
                    {
                        if (mQuest.GetQuestType() == QuestType.TryUpgradeArtifact)
                        {
                            if (Lance.Account.Artifact.IsAllArtifactMaxLevel())
                            {
                                mQuest.StackRequireCount(mQuest.GetMaxRequireCount());
                            }
                        }
                    }

                    SetIsChangedData(true);
                }
            }
            else
            {
                mQuest = null;

                SetIsChangedData(true);
            }
        }
    }
}