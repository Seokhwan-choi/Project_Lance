using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Lance
{
    class LobbyTabNavBarUI
    {
        LobbyTabUIManager mParent;
        List<LobbyNavBarItemUI> mNavBarItemList;
        public void Init(LobbyTabUIManager parent)
        {
            mParent = parent;
            mNavBarItemList = new List<LobbyNavBarItemUI>();
            GameObject navBarParent = Lance.Lobby.Find("Lobby_NavBar");

            for (int i = 0; i < (int)LobbyTab.Count; ++i)
            {
                LobbyTab tab = (LobbyTab)i;

                string tabName = $"{tab}";

                GameObject navBarItemObj = navBarParent.FindGameObject(tabName);

                var navBarItemUI = navBarItemObj.GetOrAddComponent<LobbyNavBarItemUI>();

                navBarItemUI.Init(tab, OnButtonAction);

                mNavBarItemList.Add(navBarItemUI);
            }

            RefreshTab();
        }

        void OnButtonAction(LobbyTab tab)
        {
            if (ContentsLockUtil.IsLockContents(tab.ChangeToContentsLockType()))
            {
                ContentsLockUtil.ShowContentsLockMessage(tab.ChangeToContentsLockType());

                SoundPlayer.PlayErrorSound();

                return;
            }

            if (mParent.ChangeTab(tab))
            {
                RefreshTab();
            }
        }

        public void Localize()
        {
            foreach (var item in mNavBarItemList)
            {
                item.Localize();
            }
        }

        public void RefreshTab()
        {
            foreach (var item in mNavBarItemList)
            {
                item.SetActiveTabIcon(mParent.CurTab == item.Tab);
            }
        }

        public void RefreshContentsLockUI()
        {
            foreach (var item in mNavBarItemList)
            {
                item.SetLock(ContentsLockUtil.IsLockContents(item.Tab.ChangeToContentsLockType()));
            }
        }

        public void RefreshRedDots()
        {
            foreach (var item in mNavBarItemList)
            {
                item.SetActiveRedDot(RedDotUtil.IsAcitveRedDotByLobbyTab(item.Tab));
            }
        }
    }

    class LobbyNavBarItemUI : MonoBehaviour
    {
        LobbyTab mTab;
        GameObject mRedDot;

        GameObject mLockObj;
        GameObject mUnlockObj;
        Image mImageTabIcon;
        public LobbyTab Tab => mTab;
        public void Init(LobbyTab tab, Action<LobbyTab> onButtonAction)
        {
            mTab = tab;
            mImageTabIcon = gameObject.FindComponent<Image>("Image_TabIcon");

            var textTabName = gameObject.FindComponent<TextMeshProUGUI>("Text_TabName");
            textTabName.text = StringTableUtil.Get($"TabName_{tab}");

            var button = gameObject.GetComponentInChildren<Button>();
            button.SetButtonAction(() => onButtonAction.Invoke(tab));

            mRedDot = gameObject.FindGameObject("RedDot");
            mLockObj = gameObject.FindGameObject("Lock");
            mUnlockObj = gameObject.FindGameObject("Unlock");
        }

        public void Localize()
        {
            var textTabName = gameObject.FindComponent<TextMeshProUGUI>("Text_TabName");
            textTabName.text = StringTableUtil.Get($"TabName_{mTab}");
        }

        public void SetActiveTabIcon(bool isActive)
        {
            string prefix = isActive ? "Active" : "Inactive";

            mImageTabIcon.sprite = Lance.Atlas.GetUISprite($"Icon_BottomMenu_{mTab}_{prefix}");
        }

        public void SetLock(bool isLock)
        {
            mLockObj.SetActive(isLock);
            mUnlockObj.SetActive(!isLock);
        }

        public void SetActiveRedDot(bool isActive)
        {
            mRedDot.SetActive(isActive);
        }
    }
}


