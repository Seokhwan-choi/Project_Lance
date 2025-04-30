using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mosframe;
using System.Linq;

namespace Lance
{
    enum CollectionTab
    {
        Weapon,
        Armor,
        Gloves,
        Shoes,
        Skill,
        Necklace,
        Earring,
        Ring,

        Count
    }

    class Popup_CollectionUI : PopupBase
    {
        DynamicVScrollView mScrollView;
        Button mButtonAllComplete;
        GameObject mRedDotObj;
        CollectionData[] mCurrentDatas;
        CollectionTab mCurTab;
        TabNavBarUIManager<CollectionTab> mNavBarUI;
        public void Init()
        {
            SetUpCloseAction();

            SetTitleText(StringTableUtil.Get("Title_Collection"));

            mButtonAllComplete = gameObject.FindComponent<Button>("Button_AllComplete");
            mButtonAllComplete.SetButtonAction(OnAllCompleteButton);
            mRedDotObj = mButtonAllComplete.gameObject.FindGameObject("RedDot");

            mNavBarUI = new TabNavBarUIManager<CollectionTab>();
            mNavBarUI.Init(gameObject.FindGameObject("NavBar"), OnChangeTabButton);
            mNavBarUI.RefreshActiveFrame(CollectionTab.Weapon);

            mCurTab = CollectionTab.Weapon;

            mScrollView = gameObject.FindComponent<DynamicVScrollView>("ScrollView");

            RefreshRedDots();

            RefreshCurrentTabDatas();
        }

        public override void OnClose()
        {
            base.OnClose();

            Lance.Lobby.RefreshCollectionRedDot();
        }

        void OnAllCompleteButton()
        {
            var result = Lance.GameManager.AllCompleteCollection(mCurTab.ChangeToItemType());
            if (result.Count() > 0)
            {
                SoundPlayer.PlayShowReward();

                RefreshRedDots();

                RefreshAllCompleteButton();

                var collectionItemUIs = gameObject.GetComponentsInChildren<CollectionItemUI>();

                foreach (var collectionItemUI in collectionItemUIs)
                {
                    foreach (var id in result)
                    {
                        if (collectionItemUI.Id == id)
                        {
                            collectionItemUI.Refresh();
                            collectionItemUI.PlayReceiveMotion();
                        }
                    }
                }
            }
        }

        int OnChangeTabButton(CollectionTab tab)
        {
            ChangeTab(tab);

            return (int)mCurTab;
        }

        void ChangeTab(CollectionTab tab)
        {
            if (mCurTab == tab)
                return;

            mCurTab = tab;

            RefreshCurrentTabDatas();
        }

        void RefreshCurrentTabDatas()
        {
            var itemType = mCurTab.ChangeToItemType();

            mCurrentDatas = Lance.GameData.CollectionData.Values
                .Where(x => x.itemType == itemType)
                .OrderByDescending(x => Lance.Account.Collection.IsAlreadyCollect(x.id) ? 0 : 1)
                .ToArray();

            mScrollView.totalItemCount = mCurrentDatas.Length;

            Refresh();
        }

        public void Refresh()
        {
            mScrollView.refresh();

            RefreshAllCompleteButton();
        }

        public void RefreshAllCompleteButton()
        {
            var itemType = mCurTab.ChangeToItemType();

            bool anyCompleteCollect = Lance.Account.AnyCompleteCollect(itemType);

            mButtonAllComplete.SetActiveFrame(anyCompleteCollect);

            mRedDotObj.SetActive(anyCompleteCollect);
        }

        public CollectionData GetCollectionData(int index)
        {
            return mCurrentDatas[index];
        }

        public void PlayQuestItemReceiveMotion(string id)
        {
            var collectionItemUIs = gameObject.GetComponentsInChildren<CollectionItemUI>();

            foreach (var collectionItemUI in collectionItemUIs)
            {
                if (collectionItemUI.Id == id)
                {
                    collectionItemUI.PlayReceiveMotion();
                }
            }
        }

        public override void RefreshRedDots()
        {
            for (int i = 0; i < (int)CollectionTab.Count; ++i)
            {
                CollectionTab tab = (CollectionTab)i;

                ItemType type = tab.ChangeToItemType();

                mNavBarUI.SetActiveRedDot(tab, Lance.Account.AnyCompleteCollect(type));
            }
        }
    }

    static class CollectionTabExt
    {
        public static ItemType ChangeToItemType(this CollectionTab tab)
        {
            switch (tab)
            {
                case CollectionTab.Weapon:
                    return ItemType.Weapon;
                case CollectionTab.Armor:
                    return ItemType.Armor;
                case CollectionTab.Gloves:
                    return ItemType.Gloves;
                case CollectionTab.Shoes:
                    return ItemType.Shoes;
                case CollectionTab.Skill:
                    return ItemType.Skill;
                case CollectionTab.Necklace:
                    return ItemType.Necklace;
                case CollectionTab.Earring:
                    return ItemType.Earring;
                case CollectionTab.Ring:
                default:
                    return ItemType.Ring;
            }
        }
    }
}