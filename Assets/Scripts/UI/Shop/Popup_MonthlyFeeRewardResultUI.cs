using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;
using System.Linq;

namespace Lance
{
    class Popup_MonthlyFeeRewardResultUI : PopupBase
    {
        int mCurIdIndex;

        List<string> mMonthlyFeePackageIdList;
        Dictionary<string, MonthlyFeeRewardResultTabUI> mMonthlyFeeRewardResultTabs;
        public void Init()
        {
            var contents = gameObject.FindGameObject("Contents");
            mMonthlyFeePackageIdList = new List<string>();
            mMonthlyFeeRewardResultTabs = new Dictionary<string, MonthlyFeeRewardResultTabUI>();

            // 우선순위를 기준으로 정렬한다.
            foreach (var data in Lance.GameData.PackageShopData)
            {
                if (data.type != PackageType.MonthlyFee)
                    continue;

                if (Lance.Account.PackageShop.CanReceiveMonthlyFeeDailyReward(data.id) == false)
                    continue;

                GameObject monthlyFeeObj = gameObject.Find($"{data.id}", true);

                MonthlyFeeRewardResultTabUI tabUI = monthlyFeeObj.GetOrAddComponent<MonthlyFeeRewardResultTabUI>();

                tabUI.Init(data.id, () => Close());

                tabUI.SetVisible(false);

                mMonthlyFeePackageIdList.Add(data.id);
                mMonthlyFeeRewardResultTabs.Add(data.id, tabUI);
            }

            ShowTab(GetCurId());
        }

        public override void Close(bool immediate = false, bool hideMotion = true)
        {
            ShowNextMonthlyFeeReward();

            string curId = GetCurId();

            if (curId.IsValid() == false)
                base.Close(immediate, hideMotion);
        }

        public override void OnBackButton(bool immediate = false, bool hideMotion = true)
        {
            
        }

        void ShowNextMonthlyFeeReward()
        {
            string curId = GetCurId();
            if (curId.IsValid())
            {
                HideTab(curId);

                mCurIdIndex++;

                string nextId = GetCurId();

                if (nextId.IsValid())
                {
                    ShowTab(nextId);
                }
            }
        }

        string GetCurId()
        {
            if (mMonthlyFeePackageIdList.Count <= mCurIdIndex)
                return string.Empty;

            return mMonthlyFeePackageIdList[mCurIdIndex];
        }

        void ShowTab(string id)
        {
            MonthlyFeeRewardResultTabUI showTab = mMonthlyFeeRewardResultTabs.TryGet(id);

            showTab?.SetVisible(true);
        }

        void HideTab(string id)
        {
            MonthlyFeeRewardResultTabUI hideTab = mMonthlyFeeRewardResultTabs.TryGet(id);

            hideTab?.SetVisible(false);
        }
    }
}