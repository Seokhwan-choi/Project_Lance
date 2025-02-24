using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace Lance
{
    enum StatisticsTab
    {
        StageReward,
        StackedDamage,

        Count,
    }

    class StatisticsTabUIManager
    {
        StatisticsTab mCurTab;
        TabNavBarUIManager<StatisticsTab> mNavBarUI;

        GameObject mGameObject;
        List<StatisticsTabUI> mStatisticsTabUIList;
        public StatisticsTab CurTab => mCurTab;
        public void Init(GameObject go)
        {
            mGameObject = go.FindGameObject("Contents");

            mNavBarUI = new TabNavBarUIManager<StatisticsTab>();
            mNavBarUI.Init(go.FindGameObject("Statistics_NavBar"), OnChangeTabButton);

            mStatisticsTabUIList = new List<StatisticsTabUI>();
            InitStatisticsTab<Statistics_StageRewardTabUI>(StatisticsTab.StageReward);
            InitStatisticsTab<Statistics_StackedDamageTabUI>(StatisticsTab.StackedDamage);

            ShowTab(StatisticsTab.StageReward);
        }

        public void ResetInfo()
        {
            foreach (StatisticsTabUI tab in mStatisticsTabUIList)
            {
                tab.ResetInfo();
            }
        }

        public void Localize()
        {
            mNavBarUI.Localize();

            foreach (StatisticsTabUI tab in mStatisticsTabUIList)
            {
                tab.Localize();
            }
        }

        public void OnUpdate(float dt)
        {
            foreach (StatisticsTabUI tab in mStatisticsTabUIList)
            {
                tab.OnUpdate(dt);
            }
        }

        public void StackReward(RewardResult reward)
        {
            foreach (StatisticsTabUI tab in mStatisticsTabUIList)
            {
                tab.StackReward(reward);
            }
        }

        public void StackDamage(string id, double damage)
        {
            foreach (StatisticsTabUI tab in mStatisticsTabUIList)
            {
                tab.StackDamage(id, damage);
            }
        }

        public void InitStatisticsTab<T>(StatisticsTab tab) where T : StatisticsTabUI
        {
            GameObject tabObj = mGameObject.Find($"{tab}", true);

            Debug.Assert(tabObj != null, $"{tab}의 StatisticsTabUI가 없다.");

            T tabUI = tabObj.GetOrAddComponent<T>();

            tabUI.Init(tab);

            mStatisticsTabUIList.Add(tabUI);
        }

        int OnChangeTabButton(StatisticsTab tab)
        {
            ChangeTab(tab);

            return (int)mCurTab;
        }

        public bool ChangeTab(StatisticsTab tab)
        {
            if (mCurTab == tab)
                return false;

            HideTab(mCurTab);

            mCurTab = tab;

            ShowTab(mCurTab);

            return true;
        }

        void ShowTab(StatisticsTab tab)
        {
            StatisticsTabUI showTab = mStatisticsTabUIList[(int)tab];

            showTab.SetVisible(true);

            showTab.OnEnter();
        }

        void HideTab(StatisticsTab tab)
        {
            StatisticsTabUI hideTab = mStatisticsTabUIList[(int)tab];

            hideTab.OnLeave();

            hideTab.SetVisible(false);
        }
    }

    class StatisticsTabUI : MonoBehaviour
    {
        const float StackedSecondsInvertal = 1f;

        protected float mStackedSecondsInterval;
        protected int mStackedSeconds;
        protected TextMeshProUGUI mTextStackedTime;
        protected Canvas mCanvas;
        protected GraphicRaycaster mGraphicRaycaster;
        protected StatisticsTab mTab;
        public StatisticsTab Tab => mTab;
        public virtual void Init(StatisticsTab tab)
        {
            mStackedSecondsInterval = 0;
            mTab = tab;
            mCanvas = GetComponent<Canvas>();
            mGraphicRaycaster = GetComponent<GraphicRaycaster>();
            mTextStackedTime = gameObject.FindComponent<TextMeshProUGUI>("Text_StackedTime");

            var buttonReset = gameObject.FindComponent<Button>("Button_Reset");
            buttonReset.SetButtonAction(() =>
            {
                ResetInfo();
            });
        }

        public virtual void OnEnter() { }
        public virtual void OnLeave() { }
        public virtual void Localize() { }
        public virtual void Refresh() { }
        public virtual void OnUpdate(float dt) 
        {
            if (mStackedSecondsInterval > 0)
                mStackedSecondsInterval -= dt;

            if (mStackedSecondsInterval <= 0f)
            {
                mStackedSecondsInterval = StackedSecondsInvertal;

                mStackedSeconds += 1;

                RefreshTimeText();
            }
        }

        public virtual void ResetInfo() 
        {
            mStackedSecondsInterval = StackedSecondsInvertal;

            mStackedSeconds = 0;

            RefreshTimeText();
        }
        public virtual void StackReward(RewardResult reward) { }
        public virtual void StackDamage(string id, double damage) { }
        public void SetVisible(bool visible)
        {
            mCanvas.enabled = visible;
            mGraphicRaycaster.enabled = visible;
        }

        void RefreshTimeText()
        {
            if (mStackedSeconds >= 0)
            {
                var result = TimeUtil.SplitTime(mStackedSeconds);

                StringParam timeParam = new StringParam("hour", $"{result.hour:00}");
                timeParam.AddParam("minute", $"{result.min:00}");
                timeParam.AddParam("second", $"{result.sec:00}");

                mTextStackedTime.text = StringTableUtil.Get("UIString_Time", timeParam);
            }
        }
    }
}