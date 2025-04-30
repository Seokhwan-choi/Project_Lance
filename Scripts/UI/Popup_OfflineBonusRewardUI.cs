using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using BackEnd;

namespace Lance
{
    class Popup_OfflineBonusRewardUI : PopupBase
    {
        int mOfflineTime;
        RewardResult mRewardResult;
        List<ItemSlotUI> mRewardSlotUIList;
        public void Init(RewardResult reward, int offlineTime, Action onReceiveReward)
        {
            SetUpCloseAction();

            SetTitleText(StringTableUtil.Get("Title_BonusReward"));

            mRewardSlotUIList = new List<ItemSlotUI>();

            mOfflineTime = offlineTime;

            var rewardListObj = gameObject.FindGameObject("RewardList");

            rewardListObj.AllChildObjectOff();

            float bonusValue = Lance.GameData.OfflineRewardCommonData.adBonusValue;

            mRewardResult = reward.BonusReward(bonusValue);

            var rectTm = rewardListObj.GetComponent<RectTransform>();

            mRewardSlotUIList = ItemSlotUIUtil.CreateItemSlotUIList(rectTm, mRewardResult);

            var buttonBonus = gameObject.FindComponent<Button>("Button_Bonus");
            buttonBonus.SetButtonAction(() =>
            {
                OnReceiveButton(onReceiveReward);
            });
        }

        public override void Close(bool immediate = false, bool hideMotion = true)
        {
            base.Close(immediate, hideMotion);

            if (mRewardSlotUIList != null)
            {
                foreach (var rewardSlot in mRewardSlotUIList)
                {
                    Lance.ObjectPool.ReleaseUI(rewardSlot.gameObject);
                }
            }

            mRewardSlotUIList.Clear();
            mRewardSlotUIList = null;
        }

        void OnReceiveButton(Action onReceiveReward)
        {
            Lance.GameManager.ShowRewardedAd(AdType.Offline_BonusReward, () =>
            {
                // ±¤°í ³¡³ª¸é
                Lance.GameManager.GiveReward(mRewardResult, ShowRewardType.Popup, ignoreUpdatePlayerStat: true);

                Close();

                Param param = new Param();
                param.Add("reward", mRewardResult);
                param.Add("offlineTime", mOfflineTime);
                param.Add("nowDateNum", TimeUtil.NowDateNum());

                Lance.BackEnd.InsertLog("ReceiveOfflineBonusReward", param, 7);

                onReceiveReward?.Invoke();
            });
        }
    }
}