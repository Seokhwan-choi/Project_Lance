using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mosframe;
using TMPro;
using BackEnd.Leaderboard;

namespace Lance
{
       
    class Jousting_BattleTabUI : JoustingTabUI
    {
        DynamicVScrollView mScrollView;
        UserLeaderboardItem mMyRankItem;
        List<UserLeaderboardItem> mRankItemList;

        List<JoustingTopRankingItemUI> mTopRankItemUIList;
        MyJoustingRankingItemUI mMyRankingItemUI;
        bool mIsUpdating;
        GameObject mLoadingObj;
        TextMeshProUGUI mTextNoneRanker;
        
        TextMeshProUGUI mTextRequireTicketAmount;
        Image mImageAutoBattleCheck;
        Button mButtonSweepJousting;
        Button mButtonStartJousting;
        public override void Init(JoustingTabUIManager parent, JoustingTab tab)
        {
            base.Init(parent, tab);

            mLoadingObj = gameObject.FindGameObject("Loading");
            mScrollView = gameObject.FindComponent<DynamicVScrollView>("ScrollView");
            mRankItemList = new List<UserLeaderboardItem>();

            mMyRankingItemUI = gameObject.FindComponent<MyJoustingRankingItemUI>("MyRanking");
            mMyRankingItemUI.Init();

            mTopRankItemUIList = new List<JoustingTopRankingItemUI>();

            var topRankItemUIList = gameObject.FindGameObject("TopRankingItemUIList");
            for(int i = 0; i < 3; ++i)
            {
                int rank = i + 1;

                var topRankItemUIObj = topRankItemUIList.FindGameObject($"Rank_{rank}");
                var topRankItemUI = topRankItemUIObj.GetOrAddComponent<JoustingTopRankingItemUI>();
                topRankItemUI.Init(rank);

                mTopRankItemUIList.Add(topRankItemUI);
            }

            mTextNoneRanker = gameObject.FindComponent<TextMeshProUGUI>("Text_NoneRanker");
            
            mTextRequireTicketAmount = gameObject.FindComponent<TextMeshProUGUI>("Text_RequireTicketAmount");

            // 자동 도전 버튼 초기화
            var buttonAutoBattle = gameObject.FindComponent<Button>("Button_AutoBattle");
            buttonAutoBattle.SetButtonAction(OnAutoBattleButton);
            mImageAutoBattleCheck = gameObject.FindComponent<Image>("Image_Check");
            mImageAutoBattleCheck.gameObject.SetActive(SaveBitFlags.JoustingAutoBattle.IsOn());
            mButtonSweepJousting = gameObject.FindComponent<Button>("Button_SweepJousting");
            mButtonSweepJousting.SetButtonAction(() =>
            {
                string title = StringTableUtil.Get("Title_Confirm");
                string desc = StringTableUtil.Get("Desc_SweepJousting");

                UIUtil.ShowConfirmPopup(title, desc, () => 
                {
                    var popup = Lance.PopupManager.CreatePopup<Popup_JoustingSweepUI>();

                    popup.Init(this);
                }, null);
            });

            mButtonStartJousting = gameObject.FindComponent<Button>("Button_StartJousting");
            
            mButtonStartJousting.SetButtonAction(() =>
            {
                Lance.GameManager.StartJousting(EntranceType.Ticket);
            });
        }

        public override void OnEnter()
        {
            LeaderboardInfo rankInfo = Lance.Account.Leaderboard.GetLeaderboardInfo("JoustRankInfo");

            StartCoroutine(RefreshRankItems(rankInfo));
        }

        public override bool IsUpdating()
        {
            return mIsUpdating;
        }

        IEnumerator RefreshRankItems(LeaderboardInfo rankInfo)
        {
            mIsUpdating = true;

            mLoadingObj.SetActive(true);

            bool isFinishRefreshMyRank = false;

            rankInfo.GetMyRank((isSuccess, rankItem) => OnFinishGetMyRank(isSuccess, rankItem));

            void OnFinishGetMyRank(bool isSuccess, UserLeaderboardItem rankItem)
            {
                mMyRankingItemUI.gameObject.SetActive(true);

                if (isSuccess)
                {
                    mMyRankItem = rankItem;

                    mMyRankingItemUI.OnUpdate(rankItem, isMy: true);
                }
                else
                {
                    // 나의 랭킹이 없다는 뜻
                    mMyRankItem = null;

                    mMyRankingItemUI.OnUpdate(rankItem, isMy: true);
                }

                isFinishRefreshMyRank = true;
            }

            while (isFinishRefreshMyRank == false)
            {
                yield return null;
            }

            bool isFinishRefreshRankList = false;

            rankInfo.GetRankList((isSuccess, rankItemList) => OnFinishGetRankList(isSuccess, rankItemList));

            void OnFinishGetRankList(bool isSuccess, IReadOnlyList<UserLeaderboardItem> rankItemList)
            {
                if (isSuccess)
                {
                    mRankItemList.Clear();

                    foreach (var rankItem in rankItemList)
                    {
                        mRankItemList.Add(rankItem);
                    }

                    mScrollView.totalItemCount = mRankItemList.Count;

                    // TOP 3를 따로 또 보여주자
                    foreach (var topRankItemUI in mTopRankItemUIList)
                    {
                        var rankItem = GetRankItem(topRankItemUI.Rank - 1);

                        topRankItemUI.Refresh(rankItem);
                    }
                }
                else
                {
                    // 불러올 랭킹 목록이 없다는 뜻
                    mRankItemList.Clear();

                    mScrollView.totalItemCount = 0;
                }

                mTextNoneRanker.gameObject.SetActive(mScrollView.totalItemCount == 0);

                isFinishRefreshRankList = true;
            }

            while (isFinishRefreshRankList == false)
            {
                yield return null;
            }

            yield return null;

            mScrollView.refresh();

            if (mMyRankItem != null)
            {
                int myRank = mMyRankItem.rank.ToIntSafe(101);
                if (myRank <= 100)
                {
                    mScrollView.scrollByItemIndex(myRank - 1);
                }
                else
                {
                    mScrollView.scrollByItemIndex(0);
                }
            }
            else
            {
                mScrollView.scrollByItemIndex(0);
            }

            mLoadingObj.SetActive(false);

            mIsUpdating = false;

            Refresh();
        }

        public override void Refresh()
        {
            bool isEnoughTicket = Lance.Account.Currency.GetJoustingTicket() > 0;
            mButtonStartJousting.SetActiveFrame(isEnoughTicket);
            mButtonSweepJousting.SetActiveFrame(isEnoughTicket);

            mTextRequireTicketAmount.SetColor(isEnoughTicket ? Const.EnoughTextColor : Const.NotEnoughTextColor);
            mTextRequireTicketAmount.text = $"-{Lance.GameData.JoustingCommonData.entranceRequireTicket}";
        }

        void OnAutoBattleButton()
        {
            SaveBitFlags.JoustingAutoBattle.Toggle();

            mImageAutoBattleCheck.gameObject.SetActive(SaveBitFlags.JoustingAutoBattle.IsOn());
        }

        public UserLeaderboardItem GetRankItem(int index)
        {
            if (mRankItemList.Count > index && index < 0 == false)
                return mRankItemList[index];
            else
                return null;
        }

        public UserLeaderboardItem GetMyRankItem()
        {
            return mMyRankItem;
        }
    }
}