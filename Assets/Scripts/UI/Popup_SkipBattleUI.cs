using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BackEnd;
using TMPro;

namespace Lance
{
    class Popup_SkipBattleUI : PopupBase
    {
        Button mButtonSkipBattle;
        TextMeshProUGUI mTextRemainCount;
        TextMeshProUGUI mTextPrice;
        GameObject mPayObj;
        TextMeshProUGUI mTextFree;
        GameObject mRedDotObj;

        List<ItemSlotUI> mRewardSlotUIList;
        public void Init()
        {
            SetUpCloseAction();

            SetTitleText(StringTableUtil.Get("Title_SkipBattle"));

            StringParam param = new StringParam("stageName", StageRecordsUtil.GetCurStageInfoToString());

            int monsterKillCalcMinute = DataUtil.GetSkipBattleMonsterKillCount(Lance.Account.StageRecords.GetBestTotalStage());
            int timeForMinute = Lance.GameData.SkipBattleCommonData.maxTimeForMinute;
            int monsterKillCount = monsterKillCalcMinute * timeForMinute;

            param.AddParam("monsterKillCount", monsterKillCount);

            var textDesc = gameObject.FindComponent<TextMeshProUGUI>("Text_Desc");
            textDesc.text = StringTableUtil.GetDesc("SkipBattle", param);

            var rewardListObj = gameObject.FindGameObject("RewardList");

            rewardListObj.AllChildObjectOff();

            var rewardListRectTm = rewardListObj.GetComponent<RectTransform>();

            mRewardSlotUIList = ItemSlotUIUtil.CreateItemSlotUIList(rewardListRectTm, Lance.Account.GetSkipBattleRewards());

            mButtonSkipBattle = gameObject.FindComponent<Button>("Button_SkipBattle");
            mButtonSkipBattle.SetButtonAction(OnSkipBattleButton);
            mTextRemainCount = gameObject.FindComponent<TextMeshProUGUI>("Text_RemainCount");

            mPayObj = gameObject.FindGameObject("Pay");
            mTextPrice = mPayObj.FindComponent<TextMeshProUGUI>("Text_Price");
            mTextFree = gameObject.FindComponent<TextMeshProUGUI>("Text_Free");
            mRedDotObj = gameObject.FindGameObject("RedDot");

            Refresh();
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

        void OnSkipBattleButton()
        {
            if ( Lance.GameManager.SkipBattle() )
            {
                Refresh();
            }
        }

        void Refresh()
        {
            int maxCount = Lance.GameData.SkipBattleCommonData.dailyMaxCount;
            int remainCount = Lance.Account.StageRecords.GetSkipBattleRemainCount();
            bool isEnoughCount = remainCount > 0;

            StringParam param = new StringParam("remainCount", remainCount);
            param.AddParam("maxCount", maxCount);

            mTextRemainCount.text = StringTableUtil.Get("UIString_RemainDailyCount", param);

            int skipBattleStackedCount = Lance.Account.StageRecords.GetSkipBattleStackedCount();
            int price = DataUtil.GetSkipBattlePrice(skipBattleStackedCount);

            bool isEnougGem = Lance.Account.IsEnoughGem(price);
            bool isFree = price == 0;

            mButtonSkipBattle.SetActiveFrame(isEnougGem && isEnoughCount);
            mRedDotObj.SetActive(isFree);
            mPayObj.SetActive(!isFree);
            mTextFree.gameObject.SetActive(isFree);
            mTextPrice.text = $"{price}";
            mTextPrice.SetColor(isEnougGem ? Const.EnoughTextColor : Const.NotEnoughTextColor);
        }
    }
}