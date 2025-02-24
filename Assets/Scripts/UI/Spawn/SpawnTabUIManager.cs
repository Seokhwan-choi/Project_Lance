using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

namespace Lance
{
    public enum SpawnTab
    {
        Equipment,
        Skill,
        Artifact,
        Accessory,

        Count,
    }

    class SpawnTabUIManager
    {
        SpawnTab mCurTab;
        TabNavBarUIManager<SpawnTab> mNavBarUI;

        GameObject mGameObject;
        List<SpawnTabUI> mSpawnTabUIList;
        public void Init(GameObject go)
        {
            mGameObject = go.FindGameObject("TabUIList");

            mNavBarUI = new TabNavBarUIManager<SpawnTab>();
            mNavBarUI.Init(go.FindGameObject("Spawn_NavBar"), OnChangeTabButton);
            mNavBarUI.RefreshActiveFrame(SpawnTab.Equipment);
            
            mSpawnTabUIList = new List<SpawnTabUI>();
            InitSpawnTab<Spawn_EquipmentTabUI>(SpawnTab.Equipment);
            InitSpawnTab<Spawn_SkillTabUI>(SpawnTab.Skill);
            InitSpawnTab<Spawn_ArtifactTabUI>(SpawnTab.Artifact);
            InitSpawnTab<Spawn_AccessoryTabUI>(SpawnTab.Accessory);

            ShowTab(SpawnTab.Equipment);
        }

        public void RefreshContentsLockUI()
        {
            foreach (var tabButtonUI in mNavBarUI.GetTabButtonUIList())
            {
                SpawnTab tab = (SpawnTab)tabButtonUI.Tab;

                tabButtonUI.SetLockButton(ContentsLockUtil.IsLockContents(tab.ChangeToContentsLockType()));
            }

            foreach (var spawnTab in mSpawnTabUIList)
            {
                spawnTab.RefreshContentsLockUI();
            }

        }

        public void RefreshTabFrame(SpawnTab tab)
        {
            mNavBarUI.RefreshActiveFrame(tab);
        }

        public void OnUpdate()
        {
            // 현재 탭만 
            foreach (SpawnTabUI tab in mSpawnTabUIList)
            {
                if (mCurTab == tab.Tab)
                {
                    tab.OnUpdate();
                }
            }
        }

        public void InitSpawnTab<T>(SpawnTab tab) where T : SpawnTabUI
        {
            GameObject tabObj = mGameObject.Find($"{tab}", true);

            Debug.Assert(tabObj != null, $"{tab}의 SpawnTabUI가 없다.");

            T tabUI = tabObj.GetOrAddComponent<T>();

            tabUI.Init(tab);

            tabUI.SetVisible(false);

            mSpawnTabUIList.Add(tabUI);
        }

        public T GetTab<T>() where T : SpawnTabUI
        {
            return mSpawnTabUIList.FirstOrDefault(x => x is T) as T;
        }

        public void Refresh()
        {
            var curTab = mSpawnTabUIList[(int)mCurTab];

            curTab.Refresh();
        }

        public void Localize()
        {
            mNavBarUI.Localize();

            foreach (var tab in mSpawnTabUIList)
            {
                tab.Localize();
            }
        }

        int OnChangeTabButton(SpawnTab tab)
        {
            ChangeTab(tab);

            return (int)mCurTab;
        }

        public void ChangeTab(string tabName)
        {
            if (Enum.TryParse(tabName, out SpawnTab result))
            {
                ChangeTab(result);
            }
        }

        public bool ChangeTab(SpawnTab tab)
        {
            if (ContentsLockUtil.IsLockContents(tab.ChangeToContentsLockType()))
            {
                ContentsLockUtil.ShowContentsLockMessage(tab.ChangeToContentsLockType());

                SoundPlayer.PlayErrorSound();

                return false;
            }

            if (mCurTab == tab)
                return false;

            HideTab(mCurTab);

            mCurTab = tab;

            ShowTab(mCurTab);

            return true;
        }

        void ShowTab(SpawnTab tab)
        {
            SpawnTabUI showTab = mSpawnTabUIList[(int)tab];

            showTab.OnEnter();

            showTab.SetVisible(true);
        }

        void HideTab(SpawnTab tab)
        {
            SpawnTabUI hideTab = mSpawnTabUIList[(int)tab];

            hideTab.OnLeave();

            hideTab.SetVisible(false);
        }

        public void RefreshRedDots()
        {
            foreach(var tab in mSpawnTabUIList)
            {
                tab.Refresh();
            }

            for(int i = 0; i < (int)SpawnTab.Count; ++i)
            {
                SpawnTab tab = (SpawnTab)i;

                mNavBarUI.SetActiveRedDot(tab, 
                    RedDotUtil.IsActiveRedDotBySpawnTab(tab) &&
                    ContentsLockUtil.IsLockContents(tab.ChangeToContentsLockType()) == false);
            }
        }
    }

    class SpawnTabUI : MonoBehaviour
    {
        protected SpawnTab mTab;
        protected Canvas mCanvas;
        protected GraphicRaycaster mGraphicRaycaster;
        public SpawnTab Tab => mTab;
        public virtual void Init(SpawnTab tab) 
        {
            mTab = tab;
            mCanvas = GetComponent<Canvas>();
            mGraphicRaycaster = GetComponent<GraphicRaycaster>();
        }
        public virtual void OnEnter() { }
        public virtual void OnLeave() { }
        public virtual void Localize() { }
        public virtual void RefreshContentsLockUI() { }
        public virtual void Refresh() { }
        public virtual void OnUpdate() { }

        public void SetVisible(bool visible)
        {
            mCanvas.enabled = visible;
            mGraphicRaycaster.enabled = visible;
        }
    }
}