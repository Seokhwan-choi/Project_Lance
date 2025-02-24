using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BackEnd;

namespace Lance
{
    class Shop_CurrencyTabUI : ShopTabUI
    {
        List<GemShopItemUI> mGemShopItemUI;
        public override void Init(ShopTab tab)
        {
            base.Init(tab);

            mGemShopItemUI = new List<GemShopItemUI>();

            var contents = gameObject.FindGameObject("Contents");
            contents.AllChildObjectOff();

            foreach(GemShopData gemShopData in Lance.GameData.GemShopData.Values)
            {
                var gemShopItemObj = Util.InstantiateUI("GemItemUI", contents.transform);

                var gemShopItemUI = gemShopItemObj.GetOrAddComponent<GemShopItemUI>();

                gemShopItemUI.Init(gemShopData);

                mGemShopItemUI.Add(gemShopItemUI);
            }
        }

        public override void OnEnter()
        {
            Refresh();
        }

        public override void Localize()
        {
            foreach (var gemShopItemUI in mGemShopItemUI)
            {
                gemShopItemUI.Localize();
            }
        }

        public override void Refresh()
        {
            foreach (var gemShopItemUI in mGemShopItemUI)
            {
                gemShopItemUI.Refresh();
            }
        }
    }

    class GemShopItemUI : MonoBehaviour
    {

        GameObject mModalObj;
        GemShopData mData;
        GameObject mRedDotObj;
        GameObject mBonusObj;
        GameObject mOrgObj;
        TextMeshProUGUI mTextBonusPurchaseCount;
        TextMeshProUGUI mTextDailyPurchaseCount;
        TextMeshProUGUI mTextPrice;
        TextMeshProUGUI mTextValueAmount;

        TextMeshProUGUI mTextOriginalAmount;
        TextMeshProUGUI mTextBonusOriginalAmount;
        TextMeshProUGUI mTextBonusTotalAmount;
        public void Init(GemShopData data)
        {
            mData = data;
            
            mModalObj = gameObject.FindGameObject("Image_Modal");
            mBonusObj = gameObject.FindGameObject("Bonus");
            mOrgObj = gameObject.FindGameObject("Original");
            bool isAdGem = data.watchAdDailyCount > 0;

            mTextBonusOriginalAmount = gameObject.FindComponent<TextMeshProUGUI>("Text_BonusOrgAmount");
            mTextBonusTotalAmount = gameObject.FindComponent<TextMeshProUGUI>("Text_BonusTotalAmount");
            mTextOriginalAmount = gameObject.FindComponent<TextMeshProUGUI>("Text_OrgAmount");
            mTextValueAmount = gameObject.FindComponent<TextMeshProUGUI>("Text_ValueAmount");
            mTextBonusPurchaseCount = gameObject.FindComponent<TextMeshProUGUI>("Text_BonusLimtCount");
            mTextDailyPurchaseCount = gameObject.FindComponent<TextMeshProUGUI>("Text_DailyPurchaseCount");

            bool haveMileage = mData.mileage > 0;

            var mileageObj = gameObject.FindGameObject("MileageBonus");
            mileageObj.SetActive(haveMileage);

            TextMeshProUGUI textMileage = mileageObj.FindComponent<TextMeshProUGUI>("Text_Mileage");
            textMileage.text = $"+{data.mileage}";

            Image imageFrame = gameObject.FindComponent<Image>("Image_Frame");
            imageFrame.sprite = isAdGem ?
                Lance.Atlas.GetUISprite("Frame_Shop_Brown_Back") :
                Lance.Atlas.GetUISprite("Frame_Shop_Navy_Back");

            Image imageGem = gameObject.FindComponent<Image>("Image_Gem");
            imageGem.sprite = Lance.Atlas.GetUISprite(data.spriteImg);

            InitButtons(isAdGem);
        }

        void InitButtons(bool isAdGem)
        {
            var buttonsObj = gameObject.FindGameObject("Buttons");

            var buttonWatchAd = buttonsObj.FindComponent<Button>("Button_WatchAd");
            var buttonPurchase = buttonsObj.FindComponent<Button>("Button_Purchase");

            buttonWatchAd.gameObject.SetActive(isAdGem);
            buttonPurchase.gameObject.SetActive(!isAdGem);

            mRedDotObj = buttonWatchAd.gameObject.FindGameObject("RedDot");

            if (isAdGem)
            {
                buttonWatchAd.SetButtonAction(() => 
                {
                    if (mData.watchAdDailyCount <= 0)
                    {
                        UIUtil.ShowSystemDefaultErrorMessage();

                        return;
                    }

                    // 광고 횟수가 충분한지 확인한다.
                    if ( Lance.Account.Shop.IsEnoughFreeGemWatchAdCount(mData.watchAdDailyCount) == false )
                    {
                        UIUtil.ShowSystemErrorMessage("IsNotEnoughFreeGemCount");

                        return;
                    }

                    Lance.GameManager.ShowRewardedAd(AdType.DailyGem, () =>
                    {
                        // 광고를 보고난 다음에
                        if (Lance.Account.Shop.StackWatchAdFreeGemCount(mData.watchAdDailyCount))
                        {
                            // 로그 기록
                            Param param = new Param();
                            param.Add("id", mData.id);
                            param.Add("gem", mData.gemAmount);

                            Lance.BackEnd.InsertLog("WatchAdFreeGem", param, 7);

                            Lance.GameManager.GiveReward(new RewardResult()
                            {
                                gem = mData.gemAmount,
                                mileage = mData.mileage,
                            }, ShowRewardType.Popup);

                            Refresh();

                            Lance.Lobby.RefreshTabRedDot(LobbyTab.Shop);
                        }
                        else
                        {
                            UIUtil.ShowSystemErrorMessage("NotEnoughWatchAdCount");

                            return;
                        }
                    });
                }); 
            }
            else
            {
                mTextPrice = buttonPurchase.gameObject.FindComponent<TextMeshProUGUI>("Text_Price");

                buttonPurchase.SetButtonAction(() => 
                {
                    bool isEnoughBonusCount = Lance.Account.Shop.IsEnoughBonusCount(mData.id);

                    string name = isEnoughBonusCount ? StringTableUtil.GetName($"{mData.id}_Bonus") : StringTableUtil.GetName(mData.id);

                    var gemReward = new ItemInfo(ItemType.Gem, isEnoughBonusCount ? mData.gemAmount + mData.bonusAmount : mData.gemAmount);
                    var mileageReward = new ItemInfo(ItemType.Mileage, mData.mileage);
                    ItemInfo[] rewards = new ItemInfo[] { gemReward, mileageReward };

                    var popup = Lance.PopupManager.CreatePopup<Popup_ConfirmPurchaseUI>();
                    popup.Init(name, rewards, OnConfirm);

                    void OnConfirm()
                    {
                        // 구매를 하고
                        Lance.IAPManager.Purchase(mData.productId, OnFinishPurchase);

                        // 구매 완료되면
                        void OnFinishPurchase()
                        {
                            Lance.Account.Shop.PurchaseGem(mData.id);

                            Lance.Account.UserInfo.StackPayments(mData.price);

                            Lance.GameManager.CheckQuest(QuestType.Payments, mData.price);

                            Lance.Lobby.RefreshEventRedDot();
                            Lance.Lobby.RefreshQuestRedDot();

                            // 로그 기록
                            Param param = new Param();
                            param.Add("id", mData.id);
                            param.Add("gem", mData.gemAmount);
                            param.Add("bonusGem", mData.bonusAmount);
                            param.Add("prevTotalGem", Lance.Account.Currency.GetGem());
                            param.Add("stackedPayments", Lance.Account.UserInfo.GetStackedPayments());
                            param.Add("mileage", mData.mileage);

                            Lance.BackEnd.InsertLog("PurchaseGem", param, 7);

                            Lance.GameManager.GiveReward(new RewardResult()
                            {
                                gem = isEnoughBonusCount ? mData.gemAmount + mData.bonusAmount : mData.gemAmount,
                                mileage = mData.mileage,
                            }, ShowRewardType.Popup);

                            // 보상을 다 얻고 난 다음에는 저장
                            Lance.BackEnd.UpdateAllAccountInfos();

                            Refresh();
                        }
                    }
                });
            }
        }

        public void Localize()
        {
            Refresh();
        }

        public void Refresh()
        {
            if (mData.productId.IsValid())
                mTextPrice.text = Lance.IAPManager.GetPrcieString(mData.productId);

            StringParam orgAmountParam = new StringParam("amount", mData.gemAmount);

            mTextOriginalAmount.text = StringTableUtil.Get("UIString_Amount", orgAmountParam);

            StringParam valueAmountParam = new StringParam("valueAmount", 2);

            mTextValueAmount.text = StringTableUtil.Get("UIString_ValueAmount", valueAmountParam);

            bool isAd = mData.watchAdDailyCount > 0;
            if (isAd)
            {
                mBonusObj.SetActive(false);
                mTextBonusPurchaseCount.gameObject.SetActive(false);
                mOrgObj.SetActive(true);

                bool isEnoughWatchAdCount = Lance.Account.Shop.IsEnoughFreeGemWatchAdCount(mData.watchAdDailyCount);

                StringParam param = new StringParam("remainCount", Lance.Account.Shop.GetFreeGemWatchAdRemainCount(mData.watchAdDailyCount));
                param.AddParam("limitCount", mData.watchAdDailyCount);

                mTextDailyPurchaseCount.text = StringTableUtil.Get("UIString_DailyLimitCount", param);

                mModalObj.SetActive(!isEnoughWatchAdCount);
                mRedDotObj.SetActive(isEnoughWatchAdCount);
            }
            else
            {
                int remainBonusCount = Lance.Account.Shop.GetRemainBonusCount(mData.id);

                mBonusObj.SetActive(remainBonusCount > 0);
                mTextBonusPurchaseCount.gameObject.SetActive(remainBonusCount > 0);
                mOrgObj.SetActive(remainBonusCount <= 0);

                if (remainBonusCount > 0)
                {
                    StringParam param = new StringParam("amount", mData.gemAmount + mData.bonusAmount);
                    mTextBonusTotalAmount.text = StringTableUtil.Get("UIString_Amount", param);

                    StringParam param2 = new StringParam("amount", mData.gemAmount);
                    mTextBonusOriginalAmount.text = StringTableUtil.Get("UIString_Amount", param2);
                    
                    StringParam param3 = new StringParam("remainCount", remainBonusCount);
                    param3.AddParam("limitCount", mData.bonusCount);

                    mTextBonusPurchaseCount.text = StringTableUtil.Get("UIString_BonusLimitCount", param3);
                }

                mModalObj.SetActive(false);
                mRedDotObj.SetActive(false);
            }
        }
    }
}