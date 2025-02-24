using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Lance
{
    public enum LobbyTab
    {
        Stature,
        Pet,
        Skill,
        Inventory,
        Adventure,
        Spawn,
        Shop,

        Count,
    }

    class LobbyTabUIManager
    {
        LobbyTab mCurTab;
        LobbyTabNavBarUI mLobbyTabNavBarUI;

        GameObject mGameObject;
        List<LobbyTabUI> mLobbyTabUIList;
        public LobbyTab CurTab => mCurTab;

        public IEnumerator AsyncInit(GameObject go)
        {
            mGameObject = go;

            mLobbyTabNavBarUI = new LobbyTabNavBarUI();
            mLobbyTabNavBarUI.Init(this);

            mLobbyTabUIList = new List<LobbyTabUI>();
            InitLobbyTab<Lobby_StatureUI>(LobbyTab.Stature);
            InitLobbyTab<Lobby_PetUI>(LobbyTab.Pet);
            InitLobbyTab<Lobby_SkillInventoryUI>(LobbyTab.Skill);
            InitLobbyTab<Lobby_InventoryUI>(LobbyTab.Inventory);
            InitLobbyTab<Lobby_AdventureUI>(LobbyTab.Adventure);
            InitLobbyTab<Lobby_SpawnUI>(LobbyTab.Spawn);
            yield return null;
            InitLobbyTab<Lobby_ShopUI>(LobbyTab.Shop);
            yield return null;

            foreach(var tab in mLobbyTabUIList)
            {
                if (tab.Tab != LobbyTab.Stature)
                    tab.InitFinished();
            }

            ShowTab(LobbyTab.Stature);
        }

        public void RefreshContentsLockUI()
        {
            mLobbyTabNavBarUI.RefreshContentsLockUI();
             
            foreach(var tab in mLobbyTabUIList)
            {
                tab.RefreshContentsLockUI();
            }
        }

        public void RefreshRedDot(LobbyTab tab)
        {
            var tabUI = mLobbyTabUIList[(int)tab];

            tabUI?.RefreshRedDots();

            mLobbyTabNavBarUI.RefreshRedDots();
        }

        public void RefreshRedDots()
        {
            foreach(var tab in mLobbyTabUIList)
            {
                tab.RefreshRedDots();
            }

            mLobbyTabNavBarUI.RefreshRedDots();
        }

        public void RefreshNavBarRedDot()
        {
            mLobbyTabNavBarUI.RefreshRedDots();
        }

        public void OnUpdate()
        {
            // 현재 탭만 
            foreach (LobbyTabUI tab in mLobbyTabUIList)
            {
                if (mCurTab == tab.Tab)
                {
                    tab.OnUpdate();
                }
            }
        }

        public void InitLobbyTab<T>(LobbyTab tab) where T : LobbyTabUI
        {
            GameObject tabObj = mGameObject.Find($"{tab}", true);

            Debug.Assert(tabObj != null, $"{tab}의 LobbyTabUI가 없다.");

            T tabUI = tabObj.GetOrAddComponent<T>();

            tabUI.Init(tab);

            mLobbyTabUIList.Add(tabUI);
        }

        public T GetTab<T>() where T : LobbyTabUI
        {
            return mLobbyTabUIList.FirstOrDefault(x => x is T) as T;
        }

        public void Refresh()
        {
            var curTab = mLobbyTabUIList[(int)mCurTab];

            curTab.Refresh();
        }

        public void RefreshAllTab(LobbyTab tab)
        {
            var tabUI = mLobbyTabUIList[(int)tab];

            tabUI.RefreshAll();
        }

        public void Localize()
        {
            mLobbyTabNavBarUI.Localize();

            foreach (var tab in mLobbyTabUIList)
            {
                tab.Localize();
            }
        }

        public bool ChangeTab(LobbyTab tab)
        {
            if (mCurTab == tab)
                return false;

            HideTab(mCurTab);

            mCurTab = tab;

            ShowTab(mCurTab);

            return true;
        }

        void ShowTab(LobbyTab tab)
        {
            LobbyTabUI showTab = mLobbyTabUIList[(int)tab];

            showTab.SetVisible(true);

            showTab.OnEnter();
        }

        public void RefreshTabFrame()
        {
            mLobbyTabNavBarUI.RefreshTab();
        }

        void HideTab(LobbyTab tab)
        {
            LobbyTabUI hideTab = mLobbyTabUIList[(int)tab];

            hideTab.OnLeave();

            hideTab.SetVisible(false);
        }
    }

    class LobbyTabUI : MonoBehaviour
    {
        protected Canvas mCanvas;
        protected GraphicRaycaster mGraphicRaycaster;
        protected LobbyTab mTab;
        public LobbyTab Tab => mTab;
        public virtual void Init(LobbyTab tab) 
        {
            mTab = tab;
            mCanvas = GetComponent<Canvas>();
            mGraphicRaycaster = GetComponent<GraphicRaycaster>();
        }

        public virtual void InitFinished() { SetVisible(false); }
        public virtual void OnEnter() { }
        public virtual void OnLeave() { }
        public virtual void Refresh() { }
        public virtual void Localize() { }
        public virtual void RefreshAll() { }
        public virtual void RefreshContentsLockUI() { }
        public virtual void OnUpdate() { }
        public virtual void RefreshRedDots() { }

        public void SetVisible(bool visible)
        {
            mCanvas.enabled = visible;
            mGraphicRaycaster.enabled = visible;
        }
    }
}