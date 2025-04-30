using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using BackEnd;

namespace Lance
{
    class Popup_OfflineRewardUI : PopupBase
    {
        int mOfflineTime;
        RewardResult mReward;
        List<ItemSlotUI> mRewardSlotUIList;
        public void Init(int offlineTime)
        {
            HideCloseButton();

            SetTitleText(StringTableUtil.Get("Title_OfflineReward"));

            int maxTimeForHour = Lance.GameData.OfflineRewardCommonData.maxTimeForHour;
            int maxTimeForSeconds = maxTimeForHour * TimeUtil.SecondsInHour;
            mOfflineTime = Math.Min(maxTimeForSeconds, offlineTime);

            TextMeshProUGUI textOfflineMaxTime = gameObject.FindComponent<TextMeshProUGUI>("Text_OfflineMaxTime");
            StringParam param = new StringParam("hour", $"{maxTimeForHour}");
            textOfflineMaxTime.text = StringTableUtil.Get("UIString_MaxTimeForHour", param);

            TextMeshProUGUI textOfflineTime = gameObject.FindComponent<TextMeshProUGUI>("Text_OfflineTime");
            textOfflineTime.text = StringTableUtil.GetTimeStr(mOfflineTime);

            Slider sliderOfflineTime = gameObject.FindComponent<Slider>("Slider_OfflineTime");
            sliderOfflineTime.value = (float)mOfflineTime / (float)maxTimeForSeconds;

            var buttonBonus = gameObject.FindComponent<Button>("Button_Bonus");
            buttonBonus.SetButtonAction(OnBonusButton);

            bool isPurchasedRemoveAd = Lance.Account.PackageShop.IsPurchasedRemoveAD() || Lance.IAPManager.IsPurchasedRemoveAd();

            var buttonReceive = gameObject.FindComponent<Button>("Button_Receive");
            buttonReceive.SetButtonAction(OnReceiveButton);

            buttonReceive.gameObject.SetActive(!isPurchasedRemoveAd);

            mReward = Lance.Account.GetOfflineReward(mOfflineTime);

            var rewardListObj = gameObject.FindGameObject("RewardList");

            rewardListObj.AllChildObjectOff();

            var rewardListRectTm = rewardListObj.GetComponent<RectTransform>();

            mRewardSlotUIList = ItemSlotUIUtil.CreateItemSlotUIList(rewardListRectTm, mReward);
        }

        public override void OnBackButton(bool immediate = false, bool hideMotion = true)
        {
            
        }

        public override void Close(bool immediate = false, bool hideMotion = true)
        {
            base.Close(immediate, hideMotion);

            if (mRewardSlotUIList != null)
            {
                foreach(var rewardSlot in mRewardSlotUIList)
                {
                    Lance.ObjectPool.ReleaseUI(rewardSlot.gameObject);
                }

                mRewardSlotUIList.Clear();
                mRewardSlotUIList = null;
            }
        }

        void OnBonusButton()
        {
            var popup = Lance.PopupManager.CreatePopup<Popup_OfflineBonusRewardUI>();

            popup.Init(mReward, mOfflineTime, () => Close());
        }

        void OnReceiveButton()
        {
            Lance.GameManager.GiveReward(mReward, ShowRewardType.Popup, ignoreUpdatePlayerStat:true);

            Param param = new Param();
            param.Add("reward", mReward);
            param.Add("offlineTime", mOfflineTime);
            param.Add("nowDateNum", TimeUtil.NowDateNum());

            Lance.BackEnd.InsertLog("ReceiveOfflineReward", param, 7);

            Close();
        }
    }
}