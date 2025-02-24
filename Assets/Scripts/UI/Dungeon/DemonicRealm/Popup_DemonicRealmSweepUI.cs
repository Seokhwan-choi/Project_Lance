using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BackEnd;

namespace Lance
{
    class Popup_DemonicRealmSweepUI : PopupBase
    {
        Popup_DemonicRealmUI mParent;
        StageType mType;
        int mSweepStep;
        int mSweepCount;
        TextMeshProUGUI mTextTicketCount;
        TextMeshProUGUI mTextSweepCount;
        public void Init(Popup_DemonicRealmUI parent, StageType type, int sweepStep)
        {
            mParent = parent;

            SetUpCloseAction();

            // 선택한 단계
            StringParam param = new StringParam("step", sweepStep);

            SetTitleText($"{StringTableUtil.Get("UIString_Step", param)} {StringTableUtil.Get($"Title_{type}DemonicRealmSweep")}");

            mType = type;
            mSweepStep = sweepStep;
            
            // 소탕 횟수 버튼
            var buttonMinus = gameObject.FindComponent<Button>("Button_Minus");
            buttonMinus.SetButtonAction(() => OnChangeSweepCount(minus:true));
            var buttonPlus = gameObject.FindComponent<Button>("Button_Plus");
            buttonPlus.SetButtonAction(() => OnChangeSweepCount(minus: false));

            var buttonMin = gameObject.FindComponent<Button>("Button_Min");
            buttonMin.SetButtonAction(() => SetSweepCount(1));

            var buttonMax = gameObject.FindComponent<Button>("Button_Max");
            buttonMax.SetButtonAction(() => SetSweepCount(Lance.Account.GetDemonicRealmStone(mType)));
            
            // 소탕 실행 버튼
            var buttonSweep = gameObject.FindComponent<Button>("Button_Sweep");
            buttonSweep.SetButtonAction(OnSweepButton);

            // 소탕 횟수
            mSweepCount = 1;
            mTextSweepCount = gameObject.FindComponent<TextMeshProUGUI>("Text_SweepCount");
            mTextSweepCount.text = $"{mSweepCount}";

            // 티켓 이미지
            var imageTicket = gameObject.FindComponent<Image>("Image_Ticket");
            imageTicket.sprite = Lance.Atlas.GetTicket(mType);

            // 내가 지닌 티켓
            mTextTicketCount = gameObject.FindComponent<TextMeshProUGUI>("Text_TicketCount");

            Refresh();
        }

        void OnChangeSweepCount(bool minus)
        {
            mSweepCount = mSweepCount + (minus ? -1 : 1);

            SetSweepCount(mSweepCount);
        }

        void SetSweepCount(int sweepCount)
        {
            mSweepCount = Mathf.Clamp(sweepCount, 1, Lance.Account.GetDemonicRealmStone(mType));

            mTextSweepCount.text = $"{mSweepCount}";
        }

        void Refresh()
        {
            int myTicket = Lance.Account.GetDemonicRealmStone(mType);
            int sweepRequireTicket = Lance.GameData.DemonicRealmCommonData.sweepRequireTicket;
            mTextTicketCount.text = $"{myTicket}/{sweepRequireTicket}";
        }

        void OnSweepButton()
        {
            int bestStep = Lance.Account.GetBestDemonicRealmStep(mType);
            if (bestStep == 1)
            {
                UIUtil.ShowSystemErrorMessage("HaveNotReachedDemonicRealmSelectedStep");

                return;
            }

            if (mType.HaveNextStepDemonicRealm())
            {
                if (mSweepStep >= bestStep)
                {
                    UIUtil.ShowSystemErrorMessage("HaveNotReachedDemonicRealmSelectedStep");

                    return;
                }
            }
            else
            {
                if (mSweepStep > bestStep)
                {
                    UIUtil.ShowSystemErrorMessage("HaveNotReachedDemonicRealmSelectedStep");

                    return;
                }
            }

            if (Lance.Account.IsEnoughDemonicRealmStone(mType, mSweepCount) == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughDemonicRealmSweepStone");

                return;
            }

            StageData stageData = mType.HaveNextStepDemonicRealm() ? 
                DataUtil.GetDemonicRealmStageData(mType, mSweepStep) :
                DataUtil.GetDemonicRealmStageData(mType, 1);
            if (stageData == null)
            {
                UIUtil.ShowSystemDefaultErrorMessage();

                return;
            }

            if (stageData.monsterDropReward.IsValid())
            {
                MonsterRewardData monsterRewardData = DataUtil.GetMonsterRewardData(stageData.type, stageData.monsterDropReward);
                if (monsterRewardData != null)
                {
                    if (Lance.Account.UseDemonicRealmStone(mType, mSweepCount))
                    {
                        // 팝업 닫기
                        Close();

                        // 보상 총량을 구하고
                        RewardResult reward = new RewardResult();

                        for (int i = 0; i < mSweepCount; ++i)
                        {
                            for (int j = 0; j < stageData.monsterLimitCount; ++j)
                            {
                                var monsterReward = DataUtil.GetMonsterReward(stageData.diff, stageData.type, stageData.chapter, stageData.stage, monsterRewardData);

                                reward = reward.AddReward(monsterReward);
                            }
                        }

                        reward.exp = reward.exp * (1 + Lance.Account.GatherStat(StatType.ExpAmount));
                        reward.gold = reward.gold * (1 + Lance.Account.GatherStat(StatType.GoldAmount));

                        // 지급한다.
                        Lance.GameManager.GiveReward(reward, ShowRewardType.Popup);

                        Lance.Lobby.RefreshTabRedDot(LobbyTab.Adventure);

                        mParent.Refresh();

                        Param param = new Param();
                        param.Add("type", $"{stageData.type}");
                        param.Add("useStone", mSweepCount);
                        param.Add("remainStone", Lance.Account.Currency.GetDemonicRealmStone(stageData.type));
                        param.Add("reward", reward);
                        param.Add("nowDateNum", TimeUtil.NowDateNum());

                        Lance.BackEnd.InsertLog("SweepDemonicRealm", param, 7);
                    }
                }
            }
        }
    }
}