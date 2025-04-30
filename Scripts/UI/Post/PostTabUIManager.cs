using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;
using BackEnd;

namespace Lance
{
    enum PostTab
    {
        Normal,
        Rank,

        Count,
    }
    class PostTabUIManager
    {
        PostTab mCurTab;
        TabNavBarUIManager<PostTab> mNavBarUI;
        Button mButtonAllReceive;
        GameObject mRedDotObj;
        GameObject mGameObject;
        List<PostTabUI> mPostTabUIList;
        public void Init(GameObject go)
        {
            mGameObject = go.FindGameObject("Contents");

            mNavBarUI = new TabNavBarUIManager<PostTab>();
            mNavBarUI.Init(go.FindGameObject("NavBar"), OnChangeTabButton);
            mNavBarUI.RefreshActiveFrame(PostTab.Normal);

            mPostTabUIList = new List<PostTabUI>();
            InitPostTab<Post_NormalTabUI>(PostTab.Normal);
            InitPostTab<Post_RankTabUI>(PostTab.Rank);

            mButtonAllReceive = go.FindComponent<Button>("Button_AllReceive");
            mButtonAllReceive.SetButtonAction(AllReceive);

            mRedDotObj = mButtonAllReceive.gameObject.FindGameObject("RedDot");

            ShowTab(PostTab.Normal);

            Refresh();
        }

        public void InitPostTab<T>(PostTab tab) where T : PostTabUI
        {
            GameObject tabObj = mGameObject.Find($"{tab}", true);

            Debug.Assert(tabObj != null, $"{tab}의 PostTabUI가 없다.");

            T tabUI = tabObj.GetOrAddComponent<T>();

            tabUI.Init(this, tab);

            tabObj.SetActive(false);

            mPostTabUIList.Add(tabUI);
        }

        public void AllReceive()
        {
            var curTab = mPostTabUIList[(int)mCurTab];

            curTab.AllReceive();
        }

        public T GetTab<T>() where T : PostTabUI
        {
            return mPostTabUIList.FirstOrDefault(x => x is T) as T;
        }

        public void Refresh()
        {
            bool canReceive = Lance.Account.Post.AnyCanReceiveReward(mCurTab.ChangeToPostType());
            mButtonAllReceive.SetActiveFrame(canReceive);

            mRedDotObj.SetActive(canReceive);

            RefreshRedDots();

            Lance.Lobby.RefreshPostRedDot();
        }

        int OnChangeTabButton(PostTab tab)
        {
            ChangeTab(tab);

            return (int)mCurTab;
        }

        public bool ChangeTab(PostTab tab)
        {
            if (mCurTab == tab)
                return false;

            HideTab(mCurTab);

            mCurTab = tab;

            ShowTab(mCurTab);

            Refresh();

            return true;
        }

        void ShowTab(PostTab tab)
        {
            PostTabUI showTab = mPostTabUIList[(int)tab];

            showTab.OnEnter();

            showTab.gameObject.SetActive(true);

            // To Do : 모션 어떻게 보여줄까?
            //showTab.PlayShowMotion();
        }

        void HideTab(PostTab tab)
        {
            PostTabUI hideTab = mPostTabUIList[(int)tab];

            hideTab.OnLeave();

            hideTab.gameObject.SetActive(false);

            // To Do : 모션 어떻게 보여줄까?
            //hidePopup.PlayHideMotion();
        }

        public void RefreshRedDots()
        {
            for(int i = 0; i < (int)PostTab.Count; ++i)
            {
                PostTab postTab = (PostTab)i;
                PostType postType = postTab.ChangeToPostType();

                mNavBarUI.SetActiveRedDot(postTab, Lance.Account.Post.AnyCanReceiveReward(postType));
            }
        }
    }

    class PostTabUI : MonoBehaviour
    {
        protected PostTab mTab;
        protected PostTabUIManager mParent;
        protected TextMeshProUGUI mTextIsEmpty;
        protected Dictionary<string, PostItemUI> mItemUIDics;
        public PostTab Tab => mTab;
        public virtual void Init(PostTabUIManager parent, PostTab tab)
        {
            mTab = tab;
            mParent = parent;
            mTextIsEmpty = gameObject.FindComponent<TextMeshProUGUI>("Text_IsEmptyPost");
            mItemUIDics = new Dictionary<string, PostItemUI>();
        }
        public virtual void OnEnter() 
        {
            Refresh();
        }
        public virtual void OnLeave() { }
        public virtual void Refresh() 
        {
            // 우편함에 우편이 없을 경우 텍스트 출력
            if (Lance.Account.Post.GetPostItemCount(mTab.ChangeToPostType()) <= 0)
                mTextIsEmpty.gameObject.SetActive(true);
            else
                mTextIsEmpty.gameObject.SetActive(false);
        }
        public virtual void AllReceive() 
        {
            PostType postType = mTab.ChangeToPostType();

            Lance.Account.Post.ReceiveAllPost(postType, OnSuccess);
        }
        protected void AddPostItemUIs(PostType postType, Transform parent)
        {
            foreach (PostItem item in Lance.Account.Post.GetPostItems(postType))
            {
                string idDate = item.inDate;

                // indate가 중복일 경우에는 패스
                if (mItemUIDics.ContainsKey(idDate))
                    continue;

                var postItemObj = Util.InstantiateUI("PostItemUI", parent);
                var postItemUI = postItemObj.GetOrAddComponent<PostItemUI>();
                postItemUI.Init(item, OnReceive);

                mItemUIDics.Add(idDate, postItemUI);

                void OnReceive()
                {
                    RemovePostItem(idDate);

                    Refresh();

                    mParent.Refresh();
                }
            }
        }

        protected void RemovePostItem(string inDate)
        {
            if (mItemUIDics.ContainsKey(inDate))
            {
                Destroy(mItemUIDics[inDate].gameObject);

                mItemUIDics.Remove(inDate);
            }
        }

        void OnSuccess(List<RewardResult> rewardResults)
        {
            var keys = mItemUIDics.Select(x => x.Key).ToArray();

            foreach (var key in keys)
            {
                RemovePostItem(key);
            }

            Refresh();

            RewardResult totalRewardResult = new RewardResult();

            foreach (var reward in rewardResults)
            {
                totalRewardResult = totalRewardResult.AddReward(reward);
            }

            Lance.GameManager.GiveReward(totalRewardResult, ShowRewardType.Popup);

            Lance.BackEnd.UpdateAllAccountInfos();

            mParent.Refresh();
        }
    }

    static class PossTabExt
    {
        public static PostType ChangeToPostType(this PostTab postTab)
        {
            if (postTab == PostTab.Normal)
                return PostType.Admin;
            else
                return PostType.Rank;
        }
    }
}