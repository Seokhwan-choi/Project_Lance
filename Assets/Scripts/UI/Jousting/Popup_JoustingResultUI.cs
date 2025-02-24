using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Lance
{
    class Popup_JoustingResultUI : PopupBase
    {
        const float IntervalTime = 1f;

        TextMeshProUGUI mTextAutoStartRemainTime;
        float mIntervalTime;
        int mAutoStartRemainTime;
        public void Init(bool isWin, int prevRankScore, int curRankScore, JoustingAttackType myAtkType, JoustingAttackType opponentAtkType)
        {
            GameObject winObj = gameObject.FindGameObject("Win");
            GameObject loseObj = gameObject.FindGameObject("Lose");

            bool isKorean = Lance.LocalSave.LangCode == LangCode.KR;

            var imageWin = winObj.FindComponent<Image>("Image_Win");
            imageWin.sprite = Lance.Atlas.GetUISprite(isKorean ? "Image_Joust_Win" : "Image_Joust_Win_Eng");

            var imageLose = loseObj.FindComponent<Image>("Image_Lose");
            imageLose.sprite = Lance.Atlas.GetUISprite(isKorean ? "Image_Joust_Defeat" : "Image_Joust_Defeat_Eng");

            winObj.SetActive(isWin);
            loseObj.SetActive(!isWin);

            var textRankScoreValue = gameObject.FindComponent<TextMeshProUGUI>("Text_RankScoreValue");
            textRankScoreValue.text = isWin ? $"{Lance.GameData.JoustingCommonData.winRankingScore}" : $"{Lance.GameData.JoustingCommonData.loseRankingScore}";

            var textRankRewardCoinAmount = gameObject.FindComponent<TextMeshProUGUI>("Text_CoinAmount");
            textRankRewardCoinAmount.text = $"{Lance.GameData.JoustingCommonData.resultCoinReward}";

            var textRankRewardTokenAmount = gameObject.FindComponent<TextMeshProUGUI>("Text_TokenAmount");
            textRankRewardTokenAmount.text = $"{Lance.GameData.JoustingCommonData.resultGloryTokenReward}";

            var prevTier = DataUtil.GetJoustingTier(prevRankScore);
            var curTier = DataUtil.GetJoustingTier(curRankScore);

            var imageTier = gameObject.FindComponent<Image>("Image_Tier");
            imageTier.sprite = Lance.Atlas.GetUISprite($"Icon_Joust_Tier_{curTier}");

            var textTierUp = gameObject.FindComponent<TextMeshProUGUI>("Text_TierUp");
            textTierUp.gameObject.SetActive(prevTier != curTier);

            var textTierName = gameObject.FindComponent<TextMeshProUGUI>("Text_TierName");
            textTierName.text = StringTableUtil.GetName($"{curTier}");

            var textPrevRankScore = gameObject.FindComponent<TextMeshProUGUI>("Text_PrevRankScore");
            textPrevRankScore.text = $"{prevRankScore}";

            var textCurRankScore = gameObject.FindComponent<TextMeshProUGUI>("Text_CurRankScore");
            textCurRankScore.text = $"{curRankScore}";

            InitAtkResult(myAtkType, opponentAtkType);

            mTextAutoStartRemainTime = gameObject.FindComponent<TextMeshProUGUI>("Text_AutoStartRemainTime");
            mTextAutoStartRemainTime.gameObject.SetActive(SaveBitFlags.JoustingAutoBattle.IsOn());

            mAutoStartRemainTime = 4;

            var buttonExit = gameObject.FindComponent<Button>("Button_Giveup");
            buttonExit.SetButtonAction(() => 
            {
                Close();

                Lance.Lobby.RefreshJoustingRedDot();

                Lance.GameManager.StageManager.OnExitJoustingButton();
            });

            bool isEnoughTicket = Lance.Account.Currency.GetJoustingTicket() > 0;

            var textMyTicketAmount = gameObject.FindComponent<TextMeshProUGUI>("Text_TicketAmount");
            textMyTicketAmount.text = $"{Lance.Account.Currency.GetJoustingTicket()}";

            var buttonStartBattle = gameObject.FindComponent<Button>("Button_StartBattle");
            buttonStartBattle.SetButtonAction(() =>
            {
                var start = Lance.GameManager.StartJousting(EntranceType.Ticket);
                if (start)
                {
                    Close();
                }
            });
            buttonStartBattle.SetActiveFrame(isEnoughTicket);

            if (isWin)
                SoundPlayer.PlayJoustingWin();
            else
                SoundPlayer.PlayJoustingLose();
        }

        void InitAtkResult(JoustingAttackType myAtkType, JoustingAttackType opponentAtkType)
        {
            var imageMyAtkSelect = gameObject.FindComponent<Image>("Image_MyAtkSelect");
            imageMyAtkSelect.sprite = Lance.Atlas.GetUISprite($"Frame_Joust_Card_{myAtkType}");

            var textMyAtkSelect = gameObject.FindComponent<TextMeshProUGUI>("Text_MyAtkSelect");
            textMyAtkSelect.text = StringTableUtil.GetName($"{myAtkType}");

            var imageOpponentAtkSelect = gameObject.FindComponent<Image>("Image_OpponentAtkSelect");
            imageOpponentAtkSelect.sprite = Lance.Atlas.GetUISprite($"Frame_Joust_Card_{opponentAtkType}");

            var textOpponentAtkSelect = gameObject.FindComponent<TextMeshProUGUI>("Text_OpponentAtkSelect");
            textOpponentAtkSelect.text = StringTableUtil.GetName($"{opponentAtkType}");

            var data = Lance.GameData.JoustingCompatibilityData.TryGet(myAtkType);
            if (data != null)
            {
                var imageSignWin = gameObject.FindComponent<Image>("Image_Sign_Win");
                imageSignWin.gameObject.SetActive(data.strongType == opponentAtkType);

                var imageSignLose = gameObject.FindComponent<Image>("Image_Sign_Lose");
                imageSignLose.gameObject.SetActive(data.weakType == opponentAtkType);

                var imageSignDraw = gameObject.FindComponent<Image>("Image_Sign_Draw");
                imageSignDraw.gameObject.SetActive(myAtkType == opponentAtkType);

                var textAtkResult = gameObject.FindComponent<TextMeshProUGUI>("Text_AtkResult");
                textAtkResult.text = data.strongType == opponentAtkType ? StringTableUtil.Get("UIString_WinRateUp") :
                    data.weakType == opponentAtkType ? StringTableUtil.Get("UIString_WinRateDown") : StringTableUtil.Get("UIString_WinRateKeep");
            }
        }

        private void Update()
        {
            if (SaveBitFlags.JoustingAutoBattle.IsOn())
            {
                mIntervalTime -= Time.unscaledDeltaTime;
                if (mIntervalTime <= 0f)
                {
                    mIntervalTime = IntervalTime;
                    if (mAutoStartRemainTime > 0)
                    {
                        mAutoStartRemainTime -= Mathf.RoundToInt(IntervalTime);
                        if (mAutoStartRemainTime <= 0)
                        {
                            var start = Lance.GameManager.StartJousting(EntranceType.Ticket);
                            if (start)
                            {
                                Close();
                            }
                            else
                            {
                                SaveBitFlags.JoustingAutoBattle.Toggle();

                                mTextAutoStartRemainTime.gameObject.SetActive(SaveBitFlags.JoustingAutoBattle.IsOn());
                            }
                        }
                    }

                    mTextAutoStartRemainTime.text = $"({mAutoStartRemainTime}s)";
                }
            }
        }
    }
}