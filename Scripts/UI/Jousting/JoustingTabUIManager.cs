using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Mosframe;
using UnityEngine.UI;
using TMPro;

namespace Lance
{
    public enum JoustingTab
    {
        Battle,
        GloryOrb,
        Shop,

        Count,
    }
    class JoustingTabUIManager
    {
        TextMeshProUGUI mTextMyTicketAmount;
        TextMeshProUGUI mTextMyCoinAmount;
        TextMeshProUGUI mTextMyTokenAmount;
        TextMeshProUGUI mTextWeeklyUpdateRemainTime;

        JoustingTab mCurTab;
        TabNavBarUIManager<JoustingTab> mNavBarUI;

        GameObject mGameObject;
        List<JoustingTabUI> mJoustingTabUIList;
        public void Init(GameObject go)
        {
            mTextMyTicketAmount = go.FindComponent<TextMeshProUGUI>("Text_TicketAmount");
            mTextMyCoinAmount = go.FindComponent<TextMeshProUGUI>("Text_CoinAmount");
            mTextMyTokenAmount = go.FindComponent<TextMeshProUGUI>("Text_TokenAmount");
            mTextWeeklyUpdateRemainTime = go.FindComponent<TextMeshProUGUI>("Text_WeeklyUpdateRemainTime");

            mGameObject = go.FindGameObject("Contents");

            mNavBarUI = new TabNavBarUIManager<JoustingTab>();
            mNavBarUI.Init(go.FindGameObject("NavBar"), OnChangeTabButton);
            mNavBarUI.RefreshActiveFrame(JoustingTab.Battle);

            mJoustingTabUIList = new List<JoustingTabUI>();
            InitJoustingTab<Jousting_BattleTabUI>(JoustingTab.Battle);
            InitJoustingTab<Jousting_GloryOrbTabUI>(JoustingTab.GloryOrb);
            InitJoustingTab<Jousting_ShopTabUI>(JoustingTab.Shop);

            mCurTab = JoustingTab.Battle;

            ShowTab(JoustingTab.Battle);

            RefreshMyTicketAmount();
            RefreshMyCoinAmount();
            RefreshMyTokenAmount();
            RefreshWeeklyUpdteRemainTime();

            var buttonInfo = go.FindComponent<Button>("Button_Info");
            buttonInfo.SetButtonAction(() =>
            {
                var popup = Lance.PopupManager.CreatePopup<Popup_JoustingInfoUI>();
                popup.Init();
            });

            RefreshRedDots();
        }

        public void InitJoustingTab<T>(JoustingTab tab) where T : JoustingTabUI
        {
            GameObject tabObj = mGameObject.Find($"{tab}", true);

            Debug.Assert(tabObj != null, $"{tab}의 JoustingTabUI가 없다.");

            T tabUI = tabObj.GetOrAddComponent<T>();

            tabUI.Init(this, tab);

            tabUI.SetVisible(false);

            mJoustingTabUIList.Add(tabUI);
        }

        public T GetTab<T>() where T : JoustingTabUI
        {
            return mJoustingTabUIList.FirstOrDefault(x => x is T) as T;
        }

        public void OnClose()
        {
            // 갱신중일 때 탭 바꾸지 못하게하자
            JoustingTabUI curTab = mJoustingTabUIList[(int)mCurTab];

            curTab.OnClose();
        }

        int OnChangeTabButton(JoustingTab tab)
        {
            ChangeTab(tab);

            return (int)mCurTab;
        }

        public bool ChangeTab(JoustingTab tab)
        {
            if (mCurTab == tab)
                return false;

            // 갱신중일 때 탭 바꾸지 못하게하자
            JoustingTabUI curTab = mJoustingTabUIList[(int)mCurTab];
            if (curTab.IsUpdating())
                return false;

            HideTab(mCurTab);

            mCurTab = tab;

            ShowTab(mCurTab);

            return true;
        }

        void ShowTab(JoustingTab tab)
        {
            JoustingTabUI showTab = mJoustingTabUIList[(int)tab];

            showTab.SetVisible(true);

            showTab.OnEnter();

            mTextWeeklyUpdateRemainTime.gameObject.SetActive(tab != JoustingTab.GloryOrb);
        }

        void HideTab(JoustingTab tab)
        {
            JoustingTabUI hideTab = mJoustingTabUIList[(int)tab];

            hideTab.OnLeave();

            hideTab.SetVisible(false);
        }

        public void RefreshWeeklyUpdteRemainTime()
        {
            var remainSeconds = TimeUtil.GetWeeklyUpdateRemainTime();

            var splitResult = TimeUtil.SplitTime2(remainSeconds);

            StringParam timeParam = new StringParam("day", splitResult.day);
            timeParam.AddParam("hour", $"{splitResult.hour:00}");
            timeParam.AddParam("min", $"{splitResult.min:00}");

            mTextWeeklyUpdateRemainTime.text = StringTableUtil.Get("UIString_WeeklyUpdateRemainTime", timeParam);
        }

        public void RefreshCurrency()
        {
            RefreshMyTicketAmount();
            RefreshMyCoinAmount();
            RefreshMyTokenAmount();
        }

        public void RefreshMyTicketAmount()
        {
            mTextMyTicketAmount.text = $"{Lance.Account.Currency.GetJoustingTicket()}";
        }

        public void RefreshMyCoinAmount()
        {
            mTextMyCoinAmount.text = $"{Lance.Account.Currency.GetJoustingCoin()}";
        }

        public void RefreshMyTokenAmount()
        {
            mTextMyTokenAmount.text = $"{Lance.Account.Currency.GetGloryToken()}";
        }

        public void RefreshRedDots()
        {
            for(int i = 0; i < (int)JoustingTab.Count; ++i)
            {
                JoustingTab tab = (JoustingTab)i;

                if (tab == JoustingTab.GloryOrb)
                    mNavBarUI.SetActiveRedDot(tab, Lance.Account.CanUpgradeJoustGloryOrb());
                else
                    mNavBarUI.SetActiveRedDot(tab, false);
            }
        }
    }

    class JoustingTabUI : MonoBehaviour
    {
        protected JoustingTabUIManager mParent;
        protected JoustingTab mTab;
        protected Canvas mCanvas;
        protected GraphicRaycaster mGraphicRaycaster;
        public JoustingTab Tab => mTab;
        public JoustingTabUIManager Parent => mParent;
        public virtual bool IsUpdating() { return false; }
        public virtual void Init(JoustingTabUIManager parent, JoustingTab tab)
        {
            mParent = parent;
            mTab = tab;
            mCanvas = GetComponent<Canvas>();
            mGraphicRaycaster = GetComponent<GraphicRaycaster>();
        }
        public virtual void OnEnter() { }
        public virtual void OnLeave() { }
        public virtual void Refresh() { }
        public virtual void OnClose() { }
        public void SetVisible(bool visible)
        {
            mCanvas.enabled = visible;
            mGraphicRaycaster.enabled = visible;
        }
    }
}