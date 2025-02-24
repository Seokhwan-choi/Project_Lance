using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BackEnd;

namespace Lance
{
    class Popup_JoustingSweepUI : PopupBase
    {
        Jousting_BattleTabUI mParent;
        int mSweepCount;
        TextMeshProUGUI mTextTicketCount;
        TextMeshProUGUI mTextSweepCount;
        public void Init(Jousting_BattleTabUI parent)
        {
            mParent = parent;

            SetUpCloseAction();
            SetTitleText(StringTableUtil.Get($"Title_SweepJousting"));

            // ¼ÒÅÁ È½¼ö ¹öÆ°
            var buttonMinus = gameObject.FindComponent<Button>("Button_Minus");
            buttonMinus.SetButtonAction(() => OnChangeSweepCount(minus: true));
            var buttonPlus = gameObject.FindComponent<Button>("Button_Plus");
            buttonPlus.SetButtonAction(() => OnChangeSweepCount(minus: false));

            var buttonMin = gameObject.FindComponent<Button>("Button_Min");
            buttonMin.SetButtonAction(() => SetSweepCount(1));

            var buttonMax = gameObject.FindComponent<Button>("Button_Max");
            buttonMax.SetButtonAction(() => SetSweepCount(Lance.Account.Currency.GetJoustingTicket()));

            // ¼ÒÅÁ ½ÇÇà ¹öÆ°
            var buttonSweep = gameObject.FindComponent<Button>("Button_Sweep");
            buttonSweep.SetButtonAction(OnSweepButton);

            // ¼ÒÅÁ È½¼ö
            mSweepCount = 1;
            mTextSweepCount = gameObject.FindComponent<TextMeshProUGUI>("Text_SweepCount");
            mTextSweepCount.text = $"{mSweepCount}";

            // ³»°¡ Áö´Ñ Æ¼ÄÏ
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
            mSweepCount = Mathf.Clamp(sweepCount, 1, Lance.Account.Currency.GetJoustingTicket());

            mTextSweepCount.text = $"{mSweepCount}";
        }

        void Refresh()
        {
            int myTicket = Lance.Account.Currency.GetJoustingTicket();
            int sweepRequireTicket = Lance.GameData.JoustingCommonData.sweepRequireTicket;
            mTextTicketCount.text = $"{myTicket}/{sweepRequireTicket}";
        }

        void OnSweepButton()
        {
            if (Lance.Account.Currency.IsEnoughJoustingTicket(mSweepCount) == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughJoustingTicket");

                return;
            }

            if (Lance.Account.Currency.UseJoustingTicket(mSweepCount))
            {
                // ÆË¾÷ ´Ý±â
                Close();

                // º¸»ó ÃÑ·®À» ±¸ÇÏ°í
                RewardResult reward = new RewardResult();

                for (int i = 0; i < mSweepCount; ++i)
                {
                    reward.joustCoin = reward.joustCoin + Lance.GameData.JoustingCommonData.resultCoinReward;
                    reward.gloryToken = reward.gloryToken + Lance.GameData.JoustingCommonData.resultGloryTokenReward;
                }

                Param param = new Param();
                param.Add("useTicket", mSweepCount);
                param.Add("remainTicket", Lance.Account.Currency.GetJoustingTicket());
                param.Add("reward", reward);
                param.Add("nowDateNum", TimeUtil.NowDateNum());

                Lance.BackEnd.InsertLog("SweepJousting", param, 7);

                // Áö±ÞÇÑ´Ù.
                Lance.GameManager.GiveReward(reward, ShowRewardType.Popup);

                Lance.Lobby.RefreshJoustingRedDot();

                mParent.Refresh();
                mParent.Parent.RefreshCurrency();
                mParent.Parent.RefreshRedDots();
            }
        }
    }
}