using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BackEnd;
using BackEnd.Leaderboard;


namespace Lance
{
    // 스코어 색 : E9CFAA
    // 4등 ~ 색 : 9B8961
    // 내 이름 : 2A2220
    // 나머지 : 화이트
    class RankingItemUI : MonoBehaviour
    {
        bool mInit;
        protected RankingTabUI mParent;
        protected Image mImageRanking;
        protected Image mImageRankingBack;
        protected TextMeshProUGUI mTextRanking;
        protected TextMeshProUGUI mTextNickName;
        protected TextMeshProUGUI mTextRankScore;

        public void Init()
        {
            if (mInit)
                return;

            mInit = true;

            mParent = gameObject.GetComponentInParent<RankingTabUI>();

            mImageRanking = gameObject.FindComponent<Image>("Image_Rank");
            mImageRankingBack = gameObject.FindComponent<Image>("Image_Back");
            mTextRanking = gameObject.FindComponent<TextMeshProUGUI>("Text_Rank");
            mTextNickName = gameObject.FindComponent<TextMeshProUGUI>("Text_NickName");
            mTextRankScore = gameObject.FindComponent<TextMeshProUGUI>("Text_RankScore");
        }

        public void OnUpdate(UserLeaderboardItem userLeaderboardItem, bool isMy)
        {
            mImageRankingBack.sprite = GetRankBack(isMy);

            if(userLeaderboardItem != null)
            {
                int rank = userLeaderboardItem.rank.ToIntSafe(-1);
                if (rank >= 1 && rank <= 3)
                {
                    mImageRanking.gameObject.SetActive(true);
                    mTextRanking.gameObject.SetActive(false);

                    mImageRanking.sprite = Lance.Atlas.GetRankIcon(rank);
                }
                else if (rank > 3)
                {
                    mImageRanking.gameObject.SetActive(false);
                    mTextRanking.gameObject.SetActive(true);

                    StringParam param = new StringParam("rank", userLeaderboardItem.rank);

                    mTextRanking.text = StringTableUtil.Get("UIString_Rank", param);
                }

                mTextNickName.text = userLeaderboardItem.nickname;
                mTextRankScore.text = GetRankScoreString(userLeaderboardItem);
            }
            else
            {
                mImageRanking.gameObject.SetActive(false);
                mTextRanking.gameObject.SetActive(true);

                mTextRanking.text = StringTableUtil.Get("UIString_NoneRank");
                mTextNickName.text = isMy ? Backend.UserNickName : "---";
                mTextRankScore.text = StringTableUtil.Get("UIString_NoneRankScore");
            }

            mTextNickName.SetColor(isMy ? "2A2220" : "FFFFFF");
            mTextRankScore.SetColor(!isMy ? "E9CFAA" : "FFFFFF");
        }

        string GetRankScoreString(UserLeaderboardItem userLeaderboardItem)
        {
            if (mParent == null)
                mParent = gameObject.GetComponentInParent<RankingTabUI>();

            if (mParent?.Tab == RankingTab.Stage)
            {
                int totalBestStage = userLeaderboardItem.score.ToIntSafe(1);

                var result = StageRecordsUtil.SplitTotalStage(totalBestStage);

                return StageRecordsUtil.ChangeStageInfoToString(result, includeChapName: false);
            }
            else if (mParent?.Tab == RankingTab.PowerLevel)
            {
                double bestPowerLevel = userLeaderboardItem.score.ToDoubleSafe();

                return bestPowerLevel.ToAlphaString();
            }
            else if (mParent?.Tab == RankingTab.AdvancedRaid || mParent?.Tab == RankingTab.BeginnerRaid)
            {
                double bestScore = userLeaderboardItem.score.ToDoubleSafe();

                return bestScore.ToAlphaString();
            }
            else
            {
                return string.Empty;
            }
        }

        Sprite GetRankBack(bool isMy)
        {
            string spriteName = isMy ?
                "Stature_List_Frame_Stroke_White_Back" : "Button_Brown";

            return Lance.Atlas.GetUISprite(spriteName);
        }
    }
}