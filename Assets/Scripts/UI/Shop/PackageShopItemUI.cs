using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mosframe;
using System.Linq;

namespace Lance
{
    class PackageShopItemUI : MonoBehaviour, IDynamicScrollViewItem
    {
        bool mInit;
        PackageShopData mData;

        List<PackageRewardSlotUI> mPackageNormalRewardSlotUIList;

        List<PackageRewardSlotUI> mPackageMonthlyFeeRewardSlotUIList;       // 즉시 지급 보상
        List<PackageRewardSlotUI> mPackageMonthlyFeeDailyRewardSlotUIList;  // 매일 지급 보상
        Image mImageBackground;
        TextMeshProUGUI mTextName;
        TextMeshProUGUI mTextPurchaseCount;

        GameObject mMonthlyFeeRewardObj;
        GameObject mNormalRewardObj;
        
        TextMeshProUGUI mTextValueAmount;
        GameObject mValueMarkObj;
        GameObject mModalObj;
        GameObject mTimeLimit;
        TextMeshProUGUI mTextTimeLimit;
        Shop_PackageTabUI mParent;

        Button mButtonPurchase;
        GameObject mPurchaseRedDot;
        TextMeshProUGUI mTextPrice;
        Button mButtonPreview;

        TextMeshProUGUI mTextMonthlyFeeEndDateNum;
        public void Init()
        {
            if (mInit)
                return;

            mInit = true;

            mParent = gameObject.GetComponentInParent<Shop_PackageTabUI>();

            mValueMarkObj = gameObject.FindGameObject("ValueMark");
            mModalObj = gameObject.FindGameObject("Image_Modal");
            mImageBackground = gameObject.FindComponent<Image>("Image_Frame");
            mTextName = gameObject.FindComponent<TextMeshProUGUI>("Text_PackageName");
            mTextPurchaseCount = gameObject.FindComponent<TextMeshProUGUI>("Text_RemainPurchaseCount");
            mTextPrice = gameObject.FindComponent<TextMeshProUGUI>("Text_Price");
            mTextValueAmount = mValueMarkObj.FindComponent<TextMeshProUGUI>("Text_ValueAmount");

            mTimeLimit = gameObject.FindGameObject("TimeLimit");
            mTextTimeLimit = mTimeLimit.FindComponent<TextMeshProUGUI>("Text_TimeLimit");
            mTextMonthlyFeeEndDateNum = gameObject.FindComponent<TextMeshProUGUI>("Text_MonthlyFeeEndDateNum");

            mButtonPurchase = gameObject.FindComponent<Button>("Button_Purchase");
            mPurchaseRedDot = mButtonPurchase.gameObject.FindGameObject("RedDot");
            mButtonPurchase.SetButtonAction(OnPurchaseButton);
            mButtonPreview = gameObject.FindComponent<Button>("Button_Preview");

            InitRewardSlotUI();
        }

        void InitRewardSlotUI()
        {
            mNormalRewardObj = gameObject.FindGameObject("Reward_Normal");
            mPackageNormalRewardSlotUIList = new List<PackageRewardSlotUI>();
            for (int i = 0; i < 9; ++i)
            {
                int index = i + 1;

                var rewardSlotObj = mNormalRewardObj.FindGameObject($"RewardSlot_{index}");

                var rewardSlotUI = rewardSlotObj.GetOrAddComponent<PackageRewardSlotUI>();

                rewardSlotUI.Init();

                mPackageNormalRewardSlotUIList.Add(rewardSlotUI);
            }

            mMonthlyFeeRewardObj = gameObject.FindGameObject("Reward_MonthlyFee");

            mPackageMonthlyFeeRewardSlotUIList = new List<PackageRewardSlotUI>();
            mPackageMonthlyFeeDailyRewardSlotUIList = new List<PackageRewardSlotUI>();

            var immediatelyRewardObj = mMonthlyFeeRewardObj.FindGameObject("Reward_Immediately");
            var dailyRewardObj = mMonthlyFeeRewardObj.FindGameObject("Reward_Daily");

            for (int i = 0; i < 2; ++i)
            {
                int index = i + 1;

                var immediatelyRewardSlotObj = immediatelyRewardObj.FindGameObject($"RewardSlot_{index}");
                var immediatelyRewardSlotUI = immediatelyRewardSlotObj.GetOrAddComponent<PackageRewardSlotUI>();
                immediatelyRewardSlotUI.Init();

                mPackageMonthlyFeeRewardSlotUIList.Add(immediatelyRewardSlotUI);

                var dailyRewardSlotObj = dailyRewardObj.FindGameObject($"RewardSlot_{index}");
                var dailyRewardSlotUI = dailyRewardSlotObj.GetOrAddComponent<PackageRewardSlotUI>();
                dailyRewardSlotUI.Init();

                mPackageMonthlyFeeDailyRewardSlotUIList.Add(dailyRewardSlotUI);
            }
        }

        public void OnUpdateItem(int index)
        {
            if (mParent == null)
                return;

            mData = mParent.GetPackageData(index);
            if (mData != null)
            {
                if (Lance.Account.PackageShop.IsPurchasedMonthlyFee(mData.id))
                {
                    mTextMonthlyFeeEndDateNum.gameObject.SetActive(true);

                    StringParam endDateNumParam = new StringParam("endDateNum", TimeUtil.GetTimeStr(Lance.Account.PackageShop.GetMonthlyFeeEndDateNum(mData.id)));

                    mTextMonthlyFeeEndDateNum.text = StringTableUtil.Get("UIString_ActivityEndDateNum", endDateNumParam);
                }
                else
                {
                    mTextMonthlyFeeEndDateNum.gameObject.SetActive(false);
                }

                if (mData.startDate > 0 && mData.endDate > 0)
                {
                    mTimeLimit.SetActive(true);

                    var remainSeconds = TimeUtil.RemainSecondsToDateNum(mData.endDate);

                    var splitResult = TimeUtil.SplitTime2(remainSeconds);

                    StringParam timeParam = new StringParam("day", splitResult.day);
                    timeParam.AddParam("hour", $"{splitResult.hour:00}");
                    timeParam.AddParam("min", $"{splitResult.min:00}");
                    timeParam.AddParam("sec", $"{splitResult.sec:00}");

                    mTextTimeLimit.text = StringTableUtil.Get("UIString_RemainTime", timeParam);
                }
                else
                {
                    mTimeLimit.SetActive(false);
                }

                SetName();
                SetPurchaseCount();
                SetReward();

                mImageBackground.sprite = Lance.Atlas.GetUISprite(mData.background);
                mTextPrice.text = mData.productId.IsValid() ? Lance.IAPManager.GetPrcieString(mData.productId) : StringTableUtil.Get("UIString_Free");

                StringParam param = new StringParam("valueAmount", mData.valueAmount);
                mTextValueAmount.text = StringTableUtil.Get("UIString_ValueAmount", param);

                bool isEnoughPurchaseCount = Lance.Account.PackageShop.IsEnoughPurchaseCount(mData.id);

                mPurchaseRedDot.SetActive(mData.productId.IsValid() == false && isEnoughPurchaseCount);
                mValueMarkObj.SetActive(mData.valueAmount > 0);
                mModalObj.SetActive(!isEnoughPurchaseCount);

                void SetReward()
                {
                    bool isMonthlyFee = mData.monthlyFeeDailyReward.IsValid();

                    mNormalRewardObj.SetActive(!isMonthlyFee);
                    mMonthlyFeeRewardObj.SetActive(isMonthlyFee);

                    if (isMonthlyFee)
                    {
                        RewardData immediatelyReward = Lance.GameData.RewardData.TryGet(mData.reward);
                        if (immediatelyReward != null)
                        {
                            List<ItemInfo> itemInfos = ItemInfoUtil.CreateItemInfos(immediatelyReward);

                            if (mData.mileage > 0)
                            {
                                var mileageItemInfo = new ItemInfo(ItemType.Mileage, mData.mileage);

                                itemInfos.Add(mileageItemInfo);
                            }

                            for (int i = 0; i < mPackageMonthlyFeeRewardSlotUIList.Count; ++i)
                            {
                                if (i < itemInfos.Count())
                                {
                                    mPackageMonthlyFeeRewardSlotUIList[i].SetActive(true);
                                    mPackageMonthlyFeeRewardSlotUIList[i].Refresh(itemInfos[i]);
                                }
                                else
                                {
                                    mPackageMonthlyFeeRewardSlotUIList[i].SetActive(false);
                                }
                            }

                            itemInfos = null;
                        }

                        RewardData dailyReward = Lance.GameData.RewardData.TryGet(mData.monthlyFeeDailyReward);
                        if (dailyReward != null)
                        {
                            List<ItemInfo> itemInfos = ItemInfoUtil.CreateItemInfos(dailyReward);

                            for (int i = 0; i < mPackageMonthlyFeeDailyRewardSlotUIList.Count; ++i)
                            {
                                if (i < itemInfos.Count())
                                {
                                    mPackageMonthlyFeeDailyRewardSlotUIList[i].SetActive(true);
                                    mPackageMonthlyFeeDailyRewardSlotUIList[i].Refresh(itemInfos[i]);
                                }
                                else
                                {
                                    mPackageMonthlyFeeDailyRewardSlotUIList[i].SetActive(false);
                                }
                            }

                            itemInfos = null;
                        }
                    }
                    else
                    {
                        RewardData reward = Lance.GameData.RewardData.TryGet(mData.reward);
                        if (reward != null)
                        {
                            List<ItemInfo> itemInfos = ItemInfoUtil.CreateItemInfos(reward);

                            if (mData.mileage > 0)
                            {
                                var mileageItemInfo = new ItemInfo(ItemType.Mileage, mData.mileage);

                                itemInfos.Add(mileageItemInfo);
                            }

                            for (int i = 0; i < mPackageNormalRewardSlotUIList.Count; ++i)
                            {
                                if (i < itemInfos.Count())
                                {
                                    mPackageNormalRewardSlotUIList[i].SetActive(true);
                                    mPackageNormalRewardSlotUIList[i].Refresh(itemInfos[i]);
                                }
                                else
                                {
                                    mPackageNormalRewardSlotUIList[i].SetActive(false);
                                }
                            }

                            itemInfos = null;
                        }
                    }
                }

                void SetName()
                {
                    if (mData.type == PackageType.StepReward)
                    {
                        int step = Lance.Account.PackageShop.GetStep(mData.id);

                        mTextName.text = StringTableUtil.GetName($"{mData.id}{step}");

                        mButtonPreview.gameObject.SetActive(true);
                        mButtonPreview.SetButtonAction(() =>
                        {
                            var popup = Lance.PopupManager.CreatePopup<Popup_StepPackagePreviewUI>();

                            popup.Init(mData.id);
                        });
                    }
                    else
                    {
                        mTextName.text = StringTableUtil.GetName($"{mData.id}");

                        mButtonPreview.gameObject.SetActive(false);
                    }
                }

                void SetPurchaseCount()
                {
                    StringParam param = new StringParam("remainCount", Lance.Account.PackageShop.GetRemainPurchaseCount(mData.id));
                    param.AddParam("limitCount", mData.purchaseLimitCount);

                    string purchaseCountText = string.Empty;

                    if (mData.resetType == PackageResetType.Daily)
                    {
                        purchaseCountText = StringTableUtil.Get("UIString_DailyPurchaseLimitCount", param);
                    }
                    else if (mData.resetType == PackageResetType.Weekly)
                    {
                        purchaseCountText = StringTableUtil.Get("UIString_WeeklyPurchaseLimitCount", param);
                    }
                    else if (mData.resetType == PackageResetType.Monthly)
                    {
                        purchaseCountText = StringTableUtil.Get("UIString_MonthlyPurchaseLimitCount", param);
                    }
                    else
                    {
                        if (mData.type == PackageType.StepReward)
                        {
                            StringParam param2 = new StringParam("curStep", Lance.Account.PackageShop.GetStep(mData.id));
                            param2.AddParam("maxStep", DataUtil.GetPackageShopDataMaxStep(mData.id));

                            purchaseCountText = StringTableUtil.Get("UIString_StepPackageDeal", param2);
                        }
                        else
                        {
                            purchaseCountText = StringTableUtil.Get("UIString_PurchaseLimitCount", param);
                        }
                    }

                    mTextPurchaseCount.text = purchaseCountText;
                }
            }
        }

        void OnPurchaseButton()
        {
            if (mData != null)
            {
                Lance.GameManager.PurchasePackageItem(mData, OnFinishPurchased);

                void OnFinishPurchased()
                {
                    var parent = gameObject.GetComponentInParent<Shop_PackageTabUI>();

                    parent?.Refresh();
                    parent?.RefreshRedDots();

                    if (mData.productId.IsValid() == false)
                        Lance.Lobby.RefreshAllTabRedDots();
                }
            }
        }
    }

    class PackageRewardSlotUI : MonoBehaviour
    {
        ItemSlotUI mItemSlotUI;
        public void Init()
        {
            var itemSlotObj = gameObject.FindGameObject("ItemSlotUI");

            mItemSlotUI = itemSlotObj.GetOrAddComponent<ItemSlotUI>();
            mItemSlotUI.Init();

            SetActive(false);
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        public void Refresh(ItemInfo itemInfo)
        {
            mItemSlotUI.Refresh(itemInfo);
        }
    }


}
