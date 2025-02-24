using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Mosframe;
using UnityEngine.UI;
using TMPro;

namespace Lance
{
    public enum JoustingInfoTab
    {
        Desc,
        RankReward,
        TierTable,

        Count,
    }
    class JoustingInfoTabUIManager
    {
        JoustingInfoTab mCurTab;
        TabNavBarUIManager<JoustingInfoTab> mNavBarUI;

        GameObject mGameObject;
        List<JoustingInfoTabUI> mJoustingInfoTabUIList;
        public void Init(GameObject go)
        {
            mGameObject = go.FindGameObject("Contents");

            mNavBarUI = new TabNavBarUIManager<JoustingInfoTab>();
            mNavBarUI.Init(go.FindGameObject("NavBar"), OnChangeTabButton);
            mNavBarUI.RefreshActiveFrame(JoustingInfoTab.Desc);

            mJoustingInfoTabUIList = new List<JoustingInfoTabUI>();
            InitJoustingInfoTab<JoustingInfo_DescTabUI>(JoustingInfoTab.Desc);
            InitJoustingInfoTab<JoustingInfo_RankingRewardTabUI>(JoustingInfoTab.RankReward);
            InitJoustingInfoTab<JoustingInfo_TierTableTabUI>(JoustingInfoTab.TierTable);

            mCurTab = JoustingInfoTab.Desc;

            ShowTab(JoustingInfoTab.Desc);
        }

        public void InitJoustingInfoTab<T>(JoustingInfoTab tab) where T : JoustingInfoTabUI
        {
            GameObject tabObj = mGameObject.Find($"{tab}", true);

            Debug.Assert(tabObj != null, $"{tab}의 JoustingInfoTabUI가 없다.");

            T tabUI = tabObj.GetOrAddComponent<T>();

            tabUI.Init(this, tab);

            tabUI.SetVisible(false);

            mJoustingInfoTabUIList.Add(tabUI);
        }

        public T GetTab<T>() where T : JoustingInfoTabUI
        {
            return mJoustingInfoTabUIList.FirstOrDefault(x => x is T) as T;
        }

        int OnChangeTabButton(JoustingInfoTab tab)
        {
            ChangeTab(tab);

            return (int)mCurTab;
        }

        public bool ChangeTab(JoustingInfoTab tab)
        {
            if (mCurTab == tab)
                return false;

            // 갱신중일 때 탭 바꾸지 못하게하자
            JoustingInfoTabUI curTab = mJoustingInfoTabUIList[(int)mCurTab];
            if (curTab.IsUpdating())
                return false;

            HideTab(mCurTab);

            mCurTab = tab;

            ShowTab(mCurTab);

            return true;
        }

        void ShowTab(JoustingInfoTab tab)
        {
            JoustingInfoTabUI showTab = mJoustingInfoTabUIList[(int)tab];

            showTab.SetVisible(true);

            showTab.OnEnter();
        }

        void HideTab(JoustingInfoTab tab)
        {
            JoustingInfoTabUI hideTab = mJoustingInfoTabUIList[(int)tab];

            hideTab.OnLeave();

            hideTab.SetVisible(false);
        }
    }

    class JoustingInfoTabUI : MonoBehaviour
    {
        protected JoustingInfoTabUIManager mParent;
        protected JoustingInfoTab mTab;
        protected Canvas mCanvas;
        protected GraphicRaycaster mGraphicRaycaster;
        public JoustingInfoTab Tab => mTab;
        public virtual bool IsUpdating() { return false; }
        public virtual void Init(JoustingInfoTabUIManager parent, JoustingInfoTab tab)
        {
            mParent = parent;
            mTab = tab;
            mCanvas = GetComponent<Canvas>();
            mGraphicRaycaster = GetComponent<GraphicRaycaster>();
        }
        public virtual void OnEnter() { }
        public virtual void OnLeave() { }
        public virtual void Refresh() { }
        public void SetVisible(bool visible)
        {
            mCanvas.enabled = visible;
            mGraphicRaycaster.enabled = visible;
        }
    }
}