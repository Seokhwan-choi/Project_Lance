using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Mosframe;
using UnityEngine.UI;
using TMPro;
using BackEnd.Leaderboard;

namespace Lance
{
    public enum RankingTab
    {
        Stage,
        PowerLevel,
        BeginnerRaid,
        AdvancedRaid,

        Count,
    }
    class RankingTabUIManager
    {
        RankingTab mCurTab;
        TabNavBarUIManager<RankingTab> mNavBarUI;
        Button mButtonRankingReward;
        GameObject mGameObject;
        List<RankingTabUI> mRankingTabUIList;
        public void Init(GameObject go, RankingTab firstShowTab)
        {
            mGameObject = go.FindGameObject("Contents");

            mButtonRankingReward = go.FindComponent<Button>("Button_RankingReward");
            mButtonRankingReward.SetButtonAction(OnRankingRewardButton);

            mNavBarUI = new TabNavBarUIManager<RankingTab>();
            mNavBarUI.Init(go.FindGameObject("NavBar"), OnChangeTabButton);

            Lance.GameManager.CheckQuest(QuestType.ConfirmRaidRank, 1);

            mRankingTabUIList = new List<RankingTabUI>();
            InitRankingTab<Ranking_StageTabUI>(RankingTab.Stage);
            InitRankingTab<Ranking_PowerLevelTabUI>(RankingTab.PowerLevel);
            InitRankingTab<Ranking_BeginnerRaidTabUI>(RankingTab.BeginnerRaid);
            InitRankingTab<Ranking_AdvancedRaidTabUI>(RankingTab.AdvancedRaid);

            mCurTab = firstShowTab;

            ShowTab(firstShowTab);

            mNavBarUI.RefreshActiveFrame(firstShowTab);
        }


        public void InitRankingTab<T>(RankingTab tab) where T : RankingTabUI
        {
            GameObject tabObj = mGameObject.Find($"{tab}", true);

            Debug.Assert(tabObj != null, $"{tab}의 RankingTabUI가 없다.");

            T tabUI = tabObj.GetOrAddComponent<T>();

            tabUI.Init(tab);

            tabObj.SetActive(false);

            mRankingTabUIList.Add(tabUI);
        }

        public T GetTab<T>() where T : RankingTabUI
        {
            return mRankingTabUIList.FirstOrDefault(x => x is T) as T;
        }

        void OnRankingRewardButton()
        {
            if (mCurTab != RankingTab.AdvancedRaid && mCurTab != RankingTab.BeginnerRaid)
            {
                UIUtil.ShowSystemErrorMessage("HaveNotRankingReward");

                return;
            }

            var popup = Lance.PopupManager.CreatePopup<Popup_RankingRewardUI>();

            popup.Init(mCurTab);
        }

        int OnChangeTabButton(RankingTab tab)
        {
            ChangeTab(tab);

            return (int)mCurTab;
        }

        public bool ChangeTab(RankingTab tab)
        {
            if (mCurTab == tab)
                return false;

            // 갱신중일 때 탭 바꾸지 못하게하자
            RankingTabUI curTab = mRankingTabUIList[(int)mCurTab];
            if (curTab.IsUpdating)
                return false;

            HideTab(mCurTab);

            mCurTab = tab;

            ShowTab(mCurTab);

            return true;
        }

        void ShowTab(RankingTab tab)
        {
            RankingTabUI showTab = mRankingTabUIList[(int)tab];

            showTab.gameObject.SetActive(true);

            showTab.OnEnter();

            mButtonRankingReward.SetActiveFrame(tab == RankingTab.AdvancedRaid || tab == RankingTab.BeginnerRaid, btnActive: "Button_Brown", textActive: "FFFFFF", textInactive: "FFFFFF");
        }

        void HideTab(RankingTab tab)
        {
            RankingTabUI hideTab = mRankingTabUIList[(int)tab];

            hideTab.OnLeave();

            hideTab.gameObject.SetActive(false);

            // To Do : 모션 어떻게 보여줄까?
            //hidePopup.PlayHideMotion();
        }
    }

    class RankingTabUI : MonoBehaviour
    {
        protected RankingTab mTab;
        protected DynamicVScrollView mScrollView;
        protected UserLeaderboardItem mMyUserLeaderboardItem;
        protected List<UserLeaderboardItem> mUserLeaderboardItemList;
        protected MyRankingItemUI mMyRankingItemUI;

        bool mIsUpdating;
        GameObject mLoadingObj;
        TextMeshProUGUI mTextNoneRanker;
        public bool IsUpdating => mIsUpdating;
        public RankingTab Tab => mTab;
        public virtual void Init(RankingTab tab)
        {
            mTab = tab;
            mLoadingObj = gameObject.FindGameObject("Loading");
            mScrollView = gameObject.FindComponent<DynamicVScrollView>("ScrollView");
            mUserLeaderboardItemList = new List<UserLeaderboardItem>();
            mMyRankingItemUI = gameObject.FindComponent<MyRankingItemUI>("MyRanking");
            mMyRankingItemUI.Init();

            mTextNoneRanker = gameObject.FindComponent<TextMeshProUGUI>("Text_NoneRanker");
        }
        public virtual void OnEnter()
        {
            var leaderboardInfo = Lance.Account.Leaderboard.GetLeaderboardInfo(mTab.ChangeToTableName());

            if (leaderboardInfo != null)
            {
                StartCoroutine(RefreshUserLeaderboardItems(leaderboardInfo));
            }
        }

        IEnumerator RefreshUserLeaderboardItems(LeaderboardInfo leaderboardInfo)
        {
            mIsUpdating = true;
            mLoadingObj.SetActive(true);

            bool isFinishRefreshMyRank = false;

            leaderboardInfo.GetMyRank((isSuccess, userLeaderboardItem) => OnFinishGetMyRank(isSuccess, userLeaderboardItem));

            void OnFinishGetMyRank(bool isSuccess, UserLeaderboardItem userLeaderboardItem)
            {
                mMyRankingItemUI.gameObject.SetActive(true);

                if (isSuccess)
                {
                    mMyUserLeaderboardItem = userLeaderboardItem;

                    mMyRankingItemUI.OnUpdate(userLeaderboardItem, isMy: true);
                }
                else
                {
                    // 나의 랭킹이 없다는 뜻
                    mMyUserLeaderboardItem = null;

                    mMyRankingItemUI.OnUpdate(userLeaderboardItem, isMy: true);
                }

                isFinishRefreshMyRank = true;
            }

            while(isFinishRefreshMyRank == false)
            {
                yield return null;
            }

            bool isFinishRefreshRankList = false;

            leaderboardInfo.GetRankList((isSuccess, userLeaderboardItem) => OnFinishGetRankList(isSuccess, userLeaderboardItem));

            void OnFinishGetRankList(bool isSuccess, IReadOnlyList<UserLeaderboardItem> userLeaderboardItem)
            {
                if (isSuccess)
                {
                    mUserLeaderboardItemList.Clear();

                    foreach (var UserLeaderboardItem in userLeaderboardItem)
                    {
                        mUserLeaderboardItemList.Add(UserLeaderboardItem);
                    }

                    mScrollView.totalItemCount = mUserLeaderboardItemList.Count;
                }
                else
                {
                    // 불러올 랭킹 목록이 없다는 뜻
                    mUserLeaderboardItemList.Clear();

                    mScrollView.totalItemCount = 0;
                }

                mTextNoneRanker.gameObject.SetActive(mScrollView.totalItemCount == 0);

                isFinishRefreshRankList = true;
            }

            while(isFinishRefreshRankList == false)
            {
                yield return null;
            }

            yield return null;

            mScrollView.refresh();

            if (mMyUserLeaderboardItem != null)
            {
                int myRank = mMyUserLeaderboardItem.rank.ToIntSafe(101);
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
        }

        public UserLeaderboardItem GetUserLeaderboardItem(int index)
        {
            if (mUserLeaderboardItemList.Count > index && index < 0 == false)
                return mUserLeaderboardItemList[index];
            else
                return null;
        }

        public UserLeaderboardItem GetMyUserLeaderboardItem()
        {
            return mMyUserLeaderboardItem;
        }

        public virtual void OnLeave() { }
        public virtual void Refresh() { }
    }

    static class RankingTabExt
    {
        public static string ChangeToTableName(this RankingTab tab)
        {
            if (tab == RankingTab.Stage)
                return Lance.Account.StageRecords.GetTableName();
            else if (tab == RankingTab.PowerLevel)
                return Lance.Account.UserInfo.GetTableName();
            else if (tab == RankingTab.AdvancedRaid)
                return Lance.Account.Dungeon.GetTableName();
            else if (tab == RankingTab.BeginnerRaid)
                return Lance.Account.NewBeginnerRaidScore.GetTableName();
            else
                return string.Empty;
        }
    }
}