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
    class JoustingRankingItemUI : MonoBehaviour
    {
        bool mInit;
        protected Jousting_BattleTabUI mParent;
        protected Image mImageRanking;
        protected Image mImageRankingBack;
        protected Image mImageRankTier;
        protected TextMeshProUGUI mTextRanking;
        protected TextMeshProUGUI mTextNickName;
        protected TextMeshProUGUI mTextRankScore;

        public void Init()
        {
            if (mInit)
                return;

            mInit = true;

            mParent = gameObject.GetComponentInParent<Jousting_BattleTabUI>();

            mImageRanking = gameObject.FindComponent<Image>("Image_Rank");
            mImageRankingBack = gameObject.FindComponent<Image>("Image_Back");
            mImageRankTier = gameObject.FindComponent<Image>("Image_Tier");
            mTextRanking = gameObject.FindComponent<TextMeshProUGUI>("Text_Rank");
            mTextNickName = gameObject.FindComponent<TextMeshProUGUI>("Text_NickName");
            mTextRankScore = gameObject.FindComponent<TextMeshProUGUI>("Text_RankScore");
        }

        public void OnUpdate(UserLeaderboardItem rankItem, bool isMy)
        {
            mImageRankingBack.sprite = GetRankBack(isMy);

            if (rankItem != null)
            {
                int rank = rankItem.rank.ToIntSafe(-1);
                if (rank >= 1 && rank <= 3)
                {
                    mImageRanking.gameObject.SetActive(true);
                    mTextRanking.gameObject.SetActive(false);

                    mImageRanking.sprite = GetRankIcon(rank);
                }
                else if (rank > 3)
                {
                    mImageRanking.gameObject.SetActive(false);
                    mTextRanking.gameObject.SetActive(true);

                    StringParam param = new StringParam("rank", rankItem.rank);

                    mTextRanking.text = StringTableUtil.Get("UIString_Rank", param);
                }

                mTextNickName.text = rankItem.nickname;
                mTextRankScore.text = rankItem.score;
                mImageRankTier.gameObject.SetActive(true);

                JoustingTier tier = DataUtil.GetJoustingTier(rankItem.score.ToIntSafe());
                mImageRankTier.sprite = Lance.Atlas.GetUISprite($"Icon_Joust_Tier_{tier}");
            }
            else
            {
                mImageRanking.gameObject.SetActive(false);
                mTextRanking.gameObject.SetActive(true);
                
                mTextRanking.text = StringTableUtil.Get("UIString_NoneRank");
                mTextNickName.text = isMy ? Backend.UserNickName : "---";
                mTextRankScore.text = StringTableUtil.Get("UIString_NoneRankScore");

                mImageRankTier.sprite = Lance.Atlas.GetUISprite($"Icon_Joust_Tier_{JoustingTier.None}");
            }

            mTextNickName.SetColor(isMy ? "2A2220" : "FFFFFF");
            mTextRankScore.SetColor(!isMy ? "E9CFAA" : "FFFFFF");
        }

        Sprite GetRankBack(bool isMy)
        {
            string spriteName = isMy ?
                "Stature_List_Frame_Stroke_White_Back" : "Button_Brown";

            return Lance.Atlas.GetUISprite(spriteName);
        }

        Sprite GetRankIcon(int rank)
        {
            string rankSpriteName = $"Icon_Rank_{GetRankPrefix(rank)}";

            return Lance.Atlas.GetUISprite(rankSpriteName);

            string GetRankPrefix(int rank)
            {
                if (rank == 1)
                    return "1st";
                else if (rank == 2)
                    return "2nd";
                else
                    return "3rd";
            }
        }
    }
}