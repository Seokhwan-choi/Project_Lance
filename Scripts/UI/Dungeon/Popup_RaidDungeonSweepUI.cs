using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BackEnd;

namespace Lance
{
    class Popup_RaidDungeonSweepUI : PopupBase
    {
        Popup_RaidDungeonUI mParent;
        StageType mType;
        ElementalType mRaidBossType;
        double mBestScore;
        int mSweepCount;
        TextMeshProUGUI mTextTicketCount;
        TextMeshProUGUI mTextSweepCount;

        public void Init(Popup_RaidDungeonUI parent, StageType type, ElementalType raidBossType)
        {
            mParent = parent;

            SetUpCloseAction();
            SetTitleText(StringTableUtil.Get($"Title_{type}DungeonSweep"));

            mType = type;
            mRaidBossType = raidBossType;
            mBestScore = Lance.Account.GetRaidBossBestDamage(raidBossType);

            // 선택한 단계
            var textBestRecord = gameObject.FindComponent<TextMeshProUGUI>("Text_BestRecordValue");
            textBestRecord.text = mBestScore.ToAlphaString();

            // 소탕 횟수 버튼
            var buttonMinus = gameObject.FindComponent<Button>("Button_Minus");
            buttonMinus.SetButtonAction(() => OnChangeSweepCount(minus: true));
            var buttonPlus = gameObject.FindComponent<Button>("Button_Plus");
            buttonPlus.SetButtonAction(() => OnChangeSweepCount(minus: false));

            var buttonMin = gameObject.FindComponent<Button>("Button_Min");
            buttonMin.SetButtonAction(() => SetSweepCount(1));

            var buttonMax = gameObject.FindComponent<Button>("Button_Max");
            buttonMax.SetButtonAction(() => SetSweepCount(Lance.Account.GetDungeonTicket(mType)));

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
            mSweepCount = Mathf.Clamp(sweepCount, 1, Lance.Account.GetDungeonTicket(mType));

            mTextSweepCount.text = $"{mSweepCount}";
        }

        void Refresh()
        {
            int myTicket = Lance.Account.GetDungeonTicket(mType);
            int sweepRequireTicket = Lance.GameData.DungeonCommonData.sweepRequireTicket;
            mTextTicketCount.text = $"{myTicket}/{sweepRequireTicket}";
        }

        void OnSweepButton()
        {
            double raidStackedDamage = Lance.Account.GetRaidBossBestDamage(mRaidBossType);

            if (mBestScore <= 0)
            {
                string elementalName = StringTableUtil.GetName($"Elemental_{mRaidBossType}");

                StringParam param = new StringParam("elemental", elementalName);

                UIUtil.ShowSystemErrorMessage("NeedFirstCompleteRaidBoss", param: param);

                return;
            }

            if (Lance.Account.IsEnoughDungeonTicket(mType, mSweepCount) == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughDungeonSweepTicket");

                return;
            }

            if (Lance.Account.UseDungeonTicket(mType, mSweepCount))
            {
                // 보상 지급
                RaidRewardData raidBossRewardData = DataUtil.GetRaidRewardData(mRaidBossType, raidStackedDamage);
                if (raidBossRewardData != null && raidBossRewardData.reward.IsValid())
                {
                    Close();

                    var rewardData = Lance.GameData.RewardData.TryGet(raidBossRewardData.reward);
                    if (rewardData != null)
                    {
                        var reward = new RewardResult();

                        for (int i = 0; i < mSweepCount; ++i)
                        {
                            reward = reward.AddReward(rewardData);
                        }

                        Lance.GameManager.GiveReward(reward, ShowRewardType.Popup);

                        Lance.Lobby.RefreshTabRedDot(LobbyTab.Adventure);

                        mParent.Refresh();

                        mParent.Parent.Refresh();

                        Param param = new Param();
                        param.Add("type", $"{mType}");
                        param.Add("useTicket", mSweepCount);
                        param.Add("remainTicket", Lance.Account.Currency.GetDungeonTicket(mType));
                        param.Add("reward", reward);
                        param.Add("nowDateNum", TimeUtil.NowDateNum());

                        Lance.BackEnd.InsertLog("SweepDungeon", param, 7);
                    }
                }
            }
        }
    }
}