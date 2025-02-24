using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;


namespace Lance
{
    class Popup_ConfirmPurchaseUI6 : PopupBase
    {
        int mPurchaseCount;
        DemonicRealmSpoilsData mData;
        TextMeshProUGUI mTextPurchaseCount;
        TextMeshProUGUI mTextTotalPrice;
        public void Init(DemonicRealmSpoilsData data, Action onConfirm)
        {
            SetUpCloseAction();

            SetTitleText(StringTableUtil.Get("Title_Purchase"));

            mData = data;

            var reward = Lance.GameData.RewardData.TryGet(data.reward);
            var itemInfo = new ItemInfo(reward);

            // 이름
            var textShopItemName = gameObject.FindComponent<TextMeshProUGUI>("Text_Name");
            textShopItemName.text = itemInfo.GetName();

            // 구매하려는 상품
            var rewardSlotObj = gameObject.FindGameObject("PackagePopupRewardSlotUI");
            var rewardSlotUI = rewardSlotObj.GetOrAddComponent<ItemSlotUI>();
            rewardSlotUI.Init(itemInfo);

            var buttonCancle = gameObject.FindComponent<Button>("Button_Cancle");
            buttonCancle.SetButtonAction(() => OnClose());

            var buttonConfirm = gameObject.FindComponent<Button>("Button_Confirm");
            buttonConfirm.SetButtonAction(() =>
            {
                if (Lance.GameManager.PurchaseDemonicRealmSpoilsItem(data.id, mPurchaseCount))
                {
                    onConfirm?.Invoke();

                    Close();
                }
            });

            mTextPurchaseCount = gameObject.FindComponent<TextMeshProUGUI>("Text_PurchaseCount");
            mTextTotalPrice = gameObject.FindComponent<TextMeshProUGUI>("Text_TotalPrice");

            var imageCurrency = buttonConfirm.FindComponent<Image>("Image_Currency");
            imageCurrency.sprite = Lance.Atlas.GetItemSlotUISprite($"Currency_SoulStone");

            // 구매 갯수 지정
            var buttonMinus = gameObject.FindComponent<Button>("Button_Minus");
            buttonMinus.SetButtonAction(() => OnChangePurchaseCount(minus: true));
            var buttonPlus = gameObject.FindComponent<Button>("Button_Plus");
            buttonPlus.SetButtonAction(() => OnChangePurchaseCount(minus: false));

            var buttonMin = gameObject.FindComponent<Button>("Button_Min");
            buttonMin.SetButtonAction(() => SetPurchaseCount(1));

            var buttonMax = gameObject.FindComponent<Button>("Button_Max");
            buttonMax.SetButtonAction(() => SetPurchaseCount(mData.purchaseDailyCount));

            SetPurchaseCount(1);
        }

        void OnChangePurchaseCount(bool minus)
        {
            mPurchaseCount = mPurchaseCount + (minus ? -1 : 1);

            SetPurchaseCount(mPurchaseCount);
        }

        void SetPurchaseCount(int purchaseCount)
        {
            int maxCount = mData.purchaseDailyCount > 0 ? Math.Min(mData.purchaseDailyCount, Lance.Account.DemonicRealmSpoils.GetRemainPurchaseCount(mData.id)) : 1;

            mPurchaseCount = Mathf.Clamp(purchaseCount, 1, maxCount);

            mTextPurchaseCount.text = $"{mPurchaseCount}";
            mTextTotalPrice.text = $"{mPurchaseCount * mData.price}";
        }
    }
}