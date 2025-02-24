using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System;

namespace Lance
{
    class CostumeShopItemUI : MonoBehaviour
    {
        CostumeShopData mShopData;
        CostumeData mData;
        CostumeShopTabUI mParent;
        GameObject mModalObj;
        public void Init(CostumeShopTabUI parent, string id)
        {
            mParent = parent;
            mShopData = Lance.GameData.CostumeShopData.TryGet(id);
            mData = DataUtil.GetCostumeData(mShopData.reward);

            var imageCostume = gameObject.FindComponent<Image>("Image_Costume");
            var imageWeaonCostume = gameObject.FindComponent<Image>("Image_WeaponCostume");
            var imageGrade = gameObject.FindComponent<Image>("Image_Grade");
            imageGrade.sprite = Lance.Atlas.GetIconGrade(mData.grade);

            if (mData.type == CostumeType.Weapon)
            {
                imageCostume.gameObject.SetActive(false);
                imageWeaonCostume.gameObject.SetActive(true);

                imageWeaonCostume.sprite = Lance.Atlas.GetPlayerSprite(mData.uiSprite);
            }
            else
            {
                imageCostume.gameObject.SetActive(true);
                imageWeaonCostume.gameObject.SetActive(false);

                imageCostume.sprite = Lance.Atlas.GetPlayerSprite(mData.uiSprite);
            }

            var textPrice = gameObject.FindComponent<TextMeshProUGUI>("Text_Price");
            textPrice.text = mShopData.price.ToAlphaString();

            var textName = gameObject.FindComponent<TextMeshProUGUI>("Text_ItemName");
            textName.text = StringTableUtil.GetName(mData.id);

            var buttonPurchase = gameObject.FindComponent<Button>("Button_Purchase");
            buttonPurchase.SetButtonAction(OnPurchaseButton);

            mModalObj = gameObject.FindGameObject("Image_Modal");

            Refresh();
        }

        public void Refresh()
        {
            mModalObj.SetActive(Lance.Account.Costume.HaveCostume(mData.id));
        }

        void OnPurchaseButton()
        {
            var popup = Lance.PopupManager.CreatePopup<Popup_ConfirmPurchaseUI7>("Popup_ConfirmPurchaseUI3");

            popup.Init(mShopData, () =>
            {
                Refresh();

                mParent.Refresh();

                mParent.Parent.RefreshMyCostumeUpgrade();
            });
        }

        public void Localize()
        {
            var textName = gameObject.FindComponent<TextMeshProUGUI>("Text_ItemName");
            textName.text = StringTableUtil.GetName(mData.id);
        }
    }
}

