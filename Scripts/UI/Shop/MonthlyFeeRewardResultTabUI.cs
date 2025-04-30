using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using DG.Tweening;
using TMPro;

namespace Lance
{
    class MonthlyFeeRewardResultTabUI : MonoBehaviour
    {
        string mId;
        Canvas mCanvas;
        GraphicRaycaster mGraphicRaycaster;
        public virtual void Init(string id, Action onReceive)
        {
            mId = id;

            mCanvas = GetComponent<Canvas>();
            mGraphicRaycaster = GetComponent<GraphicRaycaster>();

            var textName = gameObject.FindComponent<TextMeshProUGUI>("Text_Name");
            textName.text = StringTableUtil.GetName(id);

            var textEndDateNum = gameObject.FindComponent<TextMeshProUGUI>("Text_EndDateNum");

            StringParam endDateParam = new StringParam("endDateNum", TimeUtil.GetTimeStr(Lance.Account.PackageShop.GetMonthlyFeeEndDateNum(id)));

            textEndDateNum.text = StringTableUtil.Get("UIString_EndDateNum", endDateParam);

            // 일일 보상을 보여주자
            var packageShopData = DataUtil.GetPackageShopData(id, 1);
            if (packageShopData != null && packageShopData.monthlyFeeDailyReward.IsValid())
            {
                var dailyReward = Lance.GameData.RewardData.TryGet(packageShopData.monthlyFeeDailyReward);
                if (dailyReward != null)
                {
                    var rewards = ItemInfoUtil.CreateItemInfos(dailyReward);

                    var rewardsObj = gameObject.FindGameObject("Rewards");

                    rewardsObj.AllChildObjectOff();

                    for (int i = 0; i < rewards.Count; ++i)
                    {
                        var rewardSlotObj = Util.InstantiateUI("MonthlyFeeDailyRewardSlotUI", rewardsObj.transform);

                        var rewardSlotUI = rewardSlotObj.GetOrAddComponent<ItemSlotUI>();

                        rewardSlotUI.Init(rewards[i]);
                    }
                }
            }

            var buttonReceiveReward = gameObject.FindComponent<Button>("Button_ReceiveReward");
            buttonReceiveReward.SetButtonAction(() => { Lance.GameManager.ReceiveMonthlyFeeDailyReward(id, onReceive); });
        }
        public void SetVisible(bool visible)
        {
            mCanvas.enabled = visible;
            mGraphicRaycaster.enabled = visible;

            if (visible)
                SoundPlayer.PlayShowReward();
        }
    }
}