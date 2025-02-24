using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;
using System.Linq;
using LitJson;
using BackEnd;

namespace Lance
{
    public class Pass : AccountBase
    {
        protected Dictionary<ObscuredString, PassInfo> mPassInfos;

        public Pass()
        {
            mPassInfos = new Dictionary<ObscuredString, PassInfo>();

            foreach (var id in Lance.GameData.PassData.Keys)
            {
                var passInfo = new PassInfo();

                passInfo.Init(id);

                mPassInfos.Add(id, passInfo);
            }
        }

        public virtual int CalcTotalPayments()
        {
            int totalPayments = 0;

            foreach (PassInfo passInfo in mPassInfos.Values)
            {
                if (passInfo.GetIsPurchased())
                {
                    var passData = Lance.GameData.PassData.TryGet(passInfo.GetId());
                    if (passData == null)
                        continue;

                    totalPayments += passData.price;
                }
            }

            return totalPayments;
        }

        public override string GetTableName() 
        { 
            return "Pass"; 
        }
        public override string GetColumnName() 
        { 
            return "Pass"; 
        } 
        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            for (int i = 0; i < gameDataJson.Count; i++)
            {
                var jsonData = gameDataJson[i];

                string id = jsonData["Id"].ToString();

                PassInfo passInfo = mPassInfos.TryGet(id);
                if (passInfo != null)
                {
                    bool isPurchased = false;
                    bool.TryParse(jsonData["IsPurchased"].ToString(), out isPurchased);

                    double rewardValue = 0;
                    double.TryParse(jsonData["RewardValue"].ToString(), out rewardValue);

                    var freeRewardReceived = new List<ObscuredInt>();
 
                    if (jsonData["FreeRewardReceived"].Count > 0)
                    {
                        foreach (JsonData data in jsonData["FreeRewardReceived"])
                        {
                            int rewardTemp = 0;

                            int.TryParse(data.ToString(), out rewardTemp);

                            freeRewardReceived.Add(rewardTemp);
                        }
                    }

                    var payRewardReceived = new List<ObscuredInt>();

                    if (jsonData["PayRewardReceived"].Count > 0)
                    {
                        foreach (JsonData data in jsonData["PayRewardReceived"])
                        {
                            int rewardTemp = 0;

                            int.TryParse(data.ToString(), out rewardTemp);

                            payRewardReceived.Add(rewardTemp);
                        }
                    }

                    bool retroactivePayRewards = false;

                    if (jsonData.ContainsKey("RetroactivePayRewards"))
                    {
                        bool.TryParse(jsonData["RetroactivePayRewards"].ToString(), out retroactivePayRewards);
                    }

                    if (retroactivePayRewards == false)
                    {
                        retroactivePayRewards = true;

                        if (isPurchased)
                        {
                            payRewardReceived.Clear();

                            Param param = new Param();

                            param.Add("id", id);
                            param.Add("nowDateNum", TimeUtil.NowDateNum());

                            Lance.BackEnd.InsertLog("RetroactivePassRewards", param, 7);
                        }
                    }

                    passInfo.Init(id, rewardValue, isPurchased, freeRewardReceived, payRewardReceived, retroactivePayRewards);
                }
            }
        }

        public bool IsPurchased(string id)
        {
            var passInfo = mPassInfos.TryGet(id);

            return passInfo?.GetIsPurchased() ?? false;
        }

        public bool IsSatisfiedValue(string id, int step)
        {
            PassInfo passInfo = mPassInfos.TryGet(id);

            return passInfo.IsSatisfiedValue(step);
        }

        public bool IsAlreadyReceived(string id, int step, bool isFree)
        {
            PassInfo passInfo = mPassInfos.TryGet(id);

            return passInfo?.IsAlreadyReceiveReward(step, isFree) ?? true;
        }

        public override Param GetParam()
        {
            Dictionary<string, PassInfo> savePassInfos = new Dictionary<string, PassInfo>();

            foreach (var passInfo in mPassInfos)
            {
                passInfo.Value.ReadyToSave();

                savePassInfos.Add(passInfo.Key, passInfo.Value);
            }

            Param param = new Param();

            param.Add(GetColumnName(), savePassInfos);

            return param;
        }

        public override void RandomizeKey()
        {
            base.RandomizeKey();

            if (mPassInfos != null && mPassInfos.Count > 0)
            {
                foreach (var item in mPassInfos)
                {
                    item.Key.RandomizeCryptoKey();
                    item.Value.RandomizeKey();
                }
            }
        }

        public double GetRewardValue(string id)
        {
            var passInfo = mPassInfos.TryGet(id);

            return passInfo?.GetRewardValue() ?? 0;
        }

        public bool ReceiveReward(string id, int step, bool isFree)
        {
            var info = mPassInfos.TryGet(id);
            if (info != null)
            {
                if (info.ReceiveReward(step, isFree))
                {
                    SetIsChangedData(true);

                    return true;
                }
            }

            return false;
        }

        public bool Purchase(string id)
        {
            var passInfo = mPassInfos.TryGet(id);
            if (passInfo.GetIsPurchased())
                return false;

            passInfo.SetIsPurchased(true);

            SetIsChangedData(true);

            return true;
        }

        public bool IsAlreadyPurchased(string id)
        {
            var passInfo = mPassInfos.TryGet(id);
            if (passInfo == null)
                return false;

            return passInfo.GetIsPurchased();
        }

        public (List<int> free, List<int> pay) ReceiveAllReward(string id)
        {
            var info = mPassInfos.TryGet(id);
            if (info == null)
                return (null, null);

            var result = info.ReceiveAllReward();

            SetIsChangedData(true);

            return result;
        }

        public void UpdateRewardValues(PassType type, double value)
        {
            foreach(var passInfo in GatherPassInfosByType(type))
            {
                passInfo.UpdateRewardValue(value);
            }

            SetIsChangedData(true);
        }

        IEnumerable<PassInfo> GatherPassInfosByType(PassType type)
        {
            foreach(PassInfo passInfo in mPassInfos.Values)
            {
                if (passInfo.GetPassType() == type)
                    yield return passInfo;
            }
        }

        public bool AnyCanReceiveReward()
        {
            foreach(var passInfo in mPassInfos.Values)
            {
                if (passInfo.AnyCanReceiveReward())
                    return true;
            }

            return false;
        }

        public bool CanReceiveReward(string id, int step, bool isFree)
        {
            var passInfo = mPassInfos.TryGet(id);
            if (passInfo == null)
                return false;

            return passInfo.CanReceiveReward(step, isFree);
        }

        public bool AnyCanReceiveReward(string id)
        {
            var passInfo = mPassInfos.TryGet(id);
            if (passInfo == null)
                return false;

            return passInfo.AnyCanReceiveReward();
        }

        public bool AnyCanReceiveReward(PassType passType)
        {
            var passInfos = mPassInfos.Values.Where(x => x.GetPassType() == passType);
            foreach(var passInfo in passInfos)
            {
                if (passInfo.AnyCanReceiveReward())
                    return true;
            }

            return false;
        }
    }

    public class PassInfo
    {
        public string Id;
        public double RewardValue;
        public bool IsPurchased;
        public List<int> FreeRewardReceived;
        public List<int> PayRewardReceived;
        public bool RetroactivePayRewards;

        ObscuredString mId;
        ObscuredDouble mRewardValue;
        ObscuredBool mIsPurchased;
        List<ObscuredInt> mFreeRewardReceived;
        List<ObscuredInt> mPayRewardReceived;
        ObscuredBool mRetroactivePayRewards;
        public void Init(string id)
        {
            mId = id;
            mIsPurchased = false;
            mFreeRewardReceived = new List<ObscuredInt>();
            mPayRewardReceived = new List<ObscuredInt>();
            mRetroactivePayRewards = true;
        }

        public void Init(string id, double rewardValue, bool isPurchased,
            List<ObscuredInt> freeRewardReceived,
            List<ObscuredInt> payRewardReceived, bool retroactivePayRewards)
        {
            mId = id;
            mRewardValue = rewardValue;
            mIsPurchased = isPurchased;
            mFreeRewardReceived = freeRewardReceived;
            mPayRewardReceived = payRewardReceived;
            mRetroactivePayRewards = retroactivePayRewards;
        }

        public void RandomizeKey()
        {
            mId.RandomizeCryptoKey();
            mRewardValue.RandomizeCryptoKey();
            mIsPurchased.RandomizeCryptoKey();
            mRetroactivePayRewards.RandomizeCryptoKey();

            foreach (var freeReward in mFreeRewardReceived)
            {
                freeReward.RandomizeCryptoKey();
            }

            foreach (var payReward in mPayRewardReceived)
            {
                payReward.RandomizeCryptoKey();
            }
        }

        public void ReadyToSave()
        {
            Id = mId;
            RewardValue = mRewardValue;
            IsPurchased = mIsPurchased;
            RetroactivePayRewards = mRetroactivePayRewards;
            FreeRewardReceived = mFreeRewardReceived.Select(x => (int)x).ToList();
            PayRewardReceived = mPayRewardReceived.Select(x => (int)x).ToList();
        }

        public bool GetIsPurchased()
        {
            return mIsPurchased;
        }

        public string GetId()
        {
            return mId;
        }

        public double GetRewardValue()
        {
            return mRewardValue;
        }

        public PassType GetPassType()
        {
            var passData = Lance.GameData.PassData.TryGet(mId);

            return passData.type;
        }

        //public bool IsPurchased()
        //{
        //    return mIsPurchased;
        //}

        public void UpdateRewardValue(double value)
        {
            var passData = DataUtil.GetPassData(mId);
            if (passData != null)
            {
                if (passData.checkType == QuestCheckType.Attain)
                {
                    mRewardValue = value;
                }
                else
                {
                    mRewardValue += value;
                }
            }
        }

        public (List<int> free, List<int> step) ReceiveAllReward()
        {
            List<int> freeSteps = new List<int>();
            List<int> paySteps = new List<int>();

            foreach(PassStepData passStepData in DataUtil.GetPassStepDatas(mId))
            {
                InternalReceiveReward(passStepData.step, true);
                InternalReceiveReward(passStepData.step, false);
            }

            return (freeSteps, paySteps);

            void InternalReceiveReward(int step, bool isFree)
            {
                if (IsAlreadyReceiveReward(step, isFree) == false)
                {
                    if (ReceiveReward(step, isFree))
                    {
                        if (isFree)
                        {
                            freeSteps.Add(step);
                        }
                        else
                        {
                            paySteps.Add(step);
                        }
                    }
                }
            }
        }

        public bool IsAlreadyReceiveReward(int step, bool isFree)
        {
            if (isFree)
            {
                return mFreeRewardReceived.Contains(step);
            }
            else
            {
                return mPayRewardReceived.Contains(step);
            }
        }

        public bool IsSatisfiedValue(int step)
        {
            var stepData = DataUtil.GetPassStepData(mId, step);
            if (stepData == null)
                return false;

            return mRewardValue >= stepData.requireValue;
        }

        public bool CanReceiveReward(int step, bool isFree)
        {
            if (IsSatisfiedValue(step) == false)
                return false;

            // 보상을 이미 받았는지 확인
            if (IsAlreadyReceiveReward(step, isFree))
                return false;

            if (isFree == false && mIsPurchased == false)
                return false;

            return true;
        }

        // 보상은 밖에서 주자
        public bool ReceiveReward(int step, bool isFree)
        {
            if (CanReceiveReward(step, isFree) == false)
                return false;

            if (isFree)
            {
                mFreeRewardReceived.Add(step);
            }
            else
            {
                mPayRewardReceived.Add(step);
            }

            return true;
        }

        public void SetIsPurchased(bool isPurchased)
        {
            mIsPurchased = isPurchased;
        }

        public bool AnyCanReceiveReward()
        {
            foreach(PassStepData stepData in DataUtil.GetPassStepDatas(mId))
            {
                if (CanReceiveReward(stepData.step, isFree: true) ||
                    CanReceiveReward(stepData.step, isFree: false))
                    return true;
            }

            return false;
        }
    }
}