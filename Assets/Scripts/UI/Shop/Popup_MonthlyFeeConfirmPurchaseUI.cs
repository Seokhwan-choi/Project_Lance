using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;


namespace Lance
{
    class Popup_MonthlyFeeConfirmPurchaseUI : PopupBase
    {
        public void Init(string name, string desc, ItemInfo[] immediatleyReward, ItemInfo[] dailyRewards, Action onConfirm)
        {
            SetUpCloseAction();
            SetTitleText(StringTableUtil.Get("Title_Purchase"));

            var textPackageName = gameObject.FindComponent<TextMeshProUGUI>("Text_PackageName");
            textPackageName.text = name;

            var textPackageDesc = gameObject.FindComponent<TextMeshProUGUI>("Text_PackageDesc");
            textPackageDesc.text = desc;

            var buttonCancle = gameObject.FindComponent<Button>("Button_Cancle");
            buttonCancle.SetButtonAction(() => OnClose());

            var buttonConfirm = gameObject.FindComponent<Button>("Button_Confirm");
            buttonConfirm.SetButtonAction(() =>
            {
                onConfirm?.Invoke();

                Close();
            });

            if (immediatleyReward != null)
            {
                var immediateltyRewardObj = gameObject.FindGameObject("ImmediatelyRewards");
                var rewardsObj = immediateltyRewardObj.FindGameObject("Rewards");

                rewardsObj.AllChildObjectOff();

                for (int i = 0; i < immediatleyReward.Length; ++i)
                {
                    var rewardSlotObj = Util.InstantiateUI("PackagePopupRewardSlotUI", rewardsObj.transform);

                    var rewardSlotUI = rewardSlotObj.GetOrAddComponent<ItemSlotUI>();

                    rewardSlotUI.Init(immediatleyReward[i]);
                }
            }

            if (dailyRewards != null)
            {
                var dailyRewardObj = gameObject.FindGameObject("DailyRewards");
                var rewardsObj = dailyRewardObj.FindGameObject("Rewards");

                rewardsObj.AllChildObjectOff();

                for (int i = 0; i < dailyRewards.Length; ++i)
                {
                    var rewardSlotObj = Util.InstantiateUI("PackagePopupRewardSlotUI", rewardsObj.transform);

                    var rewardSlotUI = rewardSlotObj.GetOrAddComponent<ItemSlotUI>();

                    rewardSlotUI.Init(dailyRewards[i]);
                }
            }
        }
    }
}