using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;
using System.Linq;
using LitJson;
using BackEnd;

namespace Lance
{
    public class NewEventPass
    {
        public PassInfo PassInfo;
        PassInfo mPassInfo;

        public void InitializeData(string eventId)
        {
            PassData passData = DataUtil.GetEventPassData(eventId);
            if (passData != null)
            {
                mPassInfo = new PassInfo();

                mPassInfo.Init(passData.id);
            }
        }

        public void SetServerDataToLocal(JsonData gameDataJson)
        {
            var passInfoJsonData = gameDataJson["PassInfo"];

            string id = passInfoJsonData["Id"].ToString();

            var passData = Lance.GameData.EventPassData.TryGet(id);
            if (passData != null)
            {
                mPassInfo = new PassInfo();

                bool isPurchased = false;
                bool.TryParse(passInfoJsonData["IsPurchased"].ToString(), out isPurchased);

                double rewardValue = 0;
                double.TryParse(passInfoJsonData["RewardValue"].ToString(), out rewardValue);

                var freeRewardReceived = new List<ObscuredInt>();

                if (passInfoJsonData["FreeRewardReceived"].Count > 0)
                {
                    foreach (JsonData data in passInfoJsonData["FreeRewardReceived"])
                    {
                        int rewardTemp = 0;

                        int.TryParse(data.ToString(), out rewardTemp);

                        freeRewardReceived.Add(rewardTemp);
                    }
                }

                var payRewardReceived = new List<ObscuredInt>();

                if (passInfoJsonData["PayRewardReceived"].Count > 0)
                {
                    foreach (JsonData data in passInfoJsonData["PayRewardReceived"])
                    {
                        int rewardTemp = 0;

                        int.TryParse(data.ToString(), out rewardTemp);

                        payRewardReceived.Add(rewardTemp);
                    }
                }

                bool retroactivePayRewards = false;

                if (passInfoJsonData.ContainsKey("RetroactivePayRewards"))
                {
                    bool.TryParse(passInfoJsonData["RetroactivePayRewards"].ToString(), out retroactivePayRewards);
                }

                if (retroactivePayRewards == false)
                {
                    retroactivePayRewards = true;

                    if (isPurchased)
                    {
                        payRewardReceived.Clear();
                    }
                }

                mPassInfo.Init(id, rewardValue, isPurchased, freeRewardReceived, payRewardReceived, retroactivePayRewards);
            }
        }

        public void RandomizeKey()
        {
            mPassInfo?.RandomizeKey();
        }

        public void ReadyToSave()
        {
            mPassInfo?.ReadyToSave();

            PassInfo = mPassInfo;
        }

        public void UpdateRewardValue(double value)
        {
            mPassInfo?.UpdateRewardValue(value);
        }

        public bool IsPurchased()
        {
            return mPassInfo?.GetIsPurchased() ?? false;
        }

        public bool IsSatisfiedValue(int step)
        {
            return mPassInfo?.IsSatisfiedValue(step) ?? false;
        }

        public bool IsAlreadyReceived(int step, bool isFree)
        {
            return mPassInfo?.IsAlreadyReceiveReward(step, isFree) ?? true;
        }

        public double GetRewardValue()
        {
            return mPassInfo?.GetRewardValue() ?? 0;
        }

        public bool ReceiveReward(int step, bool isFree)
        {
            return mPassInfo?.ReceiveReward(step, isFree) ?? false;
        }

        public bool Purchase()
        {
            if (mPassInfo?.GetIsPurchased() ?? false)
                return false;

            mPassInfo?.SetIsPurchased(true);

            return true;
        }

        public bool IsAlreadyPurchased()
        {
            return mPassInfo?.GetIsPurchased() ?? false;
        }

        public (List<int> free, List<int> pay) ReceiveAllReward()
        {
            var result = mPassInfo?.ReceiveAllReward() ?? (null, null);

            return result;
        }

        public bool AnyCanReceiveReward()
        {
            return mPassInfo?.AnyCanReceiveReward() ?? false;
        }

        public bool CanReceiveReward(int step, bool isFree)
        {
            return mPassInfo?.CanReceiveReward(step, isFree) ?? false;
        }

        public bool AnyCanReceiveReward(string id)
        {
            return mPassInfo?.AnyCanReceiveReward() ?? false;
        }

        public int GetTotalPayments()
        {
            if (mPassInfo?.GetIsPurchased() ?? false)
            {
                var passData = Lance.GameData.EventPassData.TryGet(mPassInfo.GetId());
                if (passData != null)
                {
                    return passData.price;
                }
            }

            return 0;
        }
    }
}