using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;


namespace Lance
{
    class Popup_ConfirmPurchaseUI7 : PopupBase
    {
        public void Init(CostumeShopData shopData, Action onConfirm)
        {
            SetUpCloseAction();

            SetTitleText(StringTableUtil.Get("Title_Purchase"));

            var costumeData = DataUtil.GetCostumeData(shopData.reward);

            var itemInfo = new ItemInfo(ItemType.Costume, 1).SetId(costumeData.id);

            // 이름
            var textShopItemName = gameObject.FindComponent<TextMeshProUGUI>("Text_Name");
            textShopItemName.text = StringTableUtil.GetName(costumeData.id);

            // 가격
            var textTotalPrice = gameObject.FindComponent<TextMeshProUGUI>("Text_TotalPrice");
            textTotalPrice.text = shopData.price.ToAlphaString();

            // 구매하려는 상품
            var rewardSlotObj = gameObject.FindGameObject("PackagePopupRewardSlotUI");
            var rewardSlotUI = rewardSlotObj.GetOrAddComponent<ItemSlotUI>();
            rewardSlotUI.Init(itemInfo);

            var buttonCancle = gameObject.FindComponent<Button>("Button_Cancle");
            buttonCancle.SetButtonAction(() => OnClose());

            var buttonConfirm = gameObject.FindComponent<Button>("Button_Confirm");
            buttonConfirm.SetButtonAction(() =>
            {
                Close();

                if (Lance.GameManager.PurchaseShopCostume(shopData.id))
                {
                    onConfirm?.Invoke();
                }
            });
        }
    }
}