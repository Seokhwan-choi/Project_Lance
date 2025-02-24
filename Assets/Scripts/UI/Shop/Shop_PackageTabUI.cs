using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mosframe;
using System.Linq;


namespace Lance
{
    public enum PackageShopTab
    {
        Special,
        Daily,
        Weekly,
        Monthly,
        MonthlyFee,
        Step,

        Count,
    }

    class Shop_PackageTabUI : ShopTabUI
    {
        const float RefreshTime = 1f;
        float mRefreshTime;
        DynamicVScrollView mDynamicScrollView;
        PackageShopData[] mPackageShopDatas;

        GameObject mSoldOutObj;

        PackageShopTab mCurTab;
        TabNavBarUIManager<PackageShopTab> mNavBarUI;
        public override void Init(ShopTab tab)
        {
            base.Init(tab);

            mRefreshTime = RefreshTime;

            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());

            mSoldOutObj = gameObject.FindGameObject("SoldOut");

            bool isKorean = Lance.LocalSave.LangCode == LangCode.KR;
            var imageSoldOut = mSoldOutObj.FindComponent<Image>("Image_SoldOut");
            imageSoldOut.sprite = Lance.Atlas.GetUISprite(isKorean ? "Image_Shop_Sorry" : "Image_Shop_Sorry_Eng");

            GameObject navBar = gameObject.FindGameObject("PackageShop_NavBar");

            mNavBarUI = new TabNavBarUIManager<PackageShopTab>();
            mNavBarUI.Init(navBar, OnChangeTabButton);
            mNavBarUI.RefreshActiveFrame(PackageShopTab.Special);

            mPackageShopDatas = Lance.Account.PackageShop.GetInStockDatas(PackageCategory.Special).ToArray();

            mDynamicScrollView = gameObject.FindComponent<DynamicVScrollView>("DynamicVerticalScrollView");
            mDynamicScrollView.totalItemCount = mPackageShopDatas?.Count() ?? 0;
            mDynamicScrollView.SelfOnSeedData();
        }

        public override void OnUpdate()
        {
            float dt = Time.unscaledDeltaTime;

            mRefreshTime -= dt;
            if (mRefreshTime <= 0f)
            {
                mRefreshTime = RefreshTime;

                Refresh();
            }
        }

        public override void Localize()
        {
            bool isKorean = Lance.LocalSave.LangCode == LangCode.KR;
            var imageSoldOut = mSoldOutObj.FindComponent<Image>("Image_SoldOut");
            imageSoldOut.sprite = Lance.Atlas.GetUISprite(isKorean ? "Image_Shop_Sorry" : "Image_Shop_Sorry_Eng");

            mNavBarUI.Localize();

            Refresh();
        }

        public override void Refresh()
        {
            mPackageShopDatas = Lance.Account.PackageShop.GetInStockDatas(mCurTab.ChangeToCategory()).ToArray();

            mDynamicScrollView.totalItemCount = mPackageShopDatas?.Count() ?? 0;

            mDynamicScrollView.refresh();

            mSoldOutObj.SetActive(mPackageShopDatas == null || mPackageShopDatas.Length == 0);
        }

        public override void RefreshRedDots()
        {
            for (int i = 0; i < (int)PackageShopTab.Count; ++i)
            {
                PackageShopTab tab = (PackageShopTab)i;

                mNavBarUI.SetActiveRedDot(tab, Lance.Account.PackageShop.CanPurchaseFreePackage(tab.ChangeToCategory()));
            }
        }

        public override void OnEnter()
        {
            Refresh();
            RefreshRedDots();
        }

        int OnChangeTabButton(PackageShopTab tab)
        {
            ChangeTab(tab);

            return (int)mCurTab;
        }

        public bool ChangeTab(PackageShopTab tab)
        {
            if (mCurTab == tab)
                return false;

            mCurTab = tab;

            Refresh();

            if (mDynamicScrollView.totalItemCount > 0)
                mDynamicScrollView.scrollByItemIndex(0);
            
            //StartCoroutine(DelayedScrollToTop());

            return true;
        }

        IEnumerator DelayedScrollToTop()
        {
            yield return null;

            if (mDynamicScrollView.totalItemCount > 0)
                mDynamicScrollView.scrollByItemIndex(0);
        }

        public PackageShopData GetPackageData(int index)
        {
            if (mPackageShopDatas == null)
                return null;

            if (mPackageShopDatas.Length <= index || index < 0)
                return null;

            return mPackageShopDatas[index];
        }
    }


    static class PackageShopTabExt
    {
        public static PackageCategory ChangeToCategory(this PackageShopTab tab)
        {
            switch (tab)
            {
                case PackageShopTab.Special:
                    return PackageCategory.Special;
                case PackageShopTab.Daily:
                    return PackageCategory.Daily;
                case PackageShopTab.Weekly:
                    return PackageCategory.Weekly;
                case PackageShopTab.Monthly:
                    return PackageCategory.Monthly;
                case PackageShopTab.MonthlyFee:
                    return PackageCategory.MonthlyFee;
                case PackageShopTab.Step:
                default:
                    return PackageCategory.Step;
            }
        }
    }
}