using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using BackEnd;


namespace Lance
{
    class CostumeItemUI : MonoBehaviour
    {
        string mId;
        Image mImageFrame;
        GameObject mSelectedObj;
        GameObject mEquippedObj;
        GameObject mInActive;
        GameObject mRedDotObj;
        GameObject mLockObj;
        TextMeshProUGUI mTextLockReason;
        public string Id => mId;
        public void Init(CostumeData data, Action<string> onSelect)
        {
            mId = data.id;
            mImageFrame = GetComponent<Image>();

            var bodyObj = gameObject.FindGameObject("Body");
            var weaponObj = gameObject.FindGameObject("Weapon");
            var etcObj = gameObject.FindGameObject("Etc");

            bodyObj.SetActive(data.type == CostumeType.Body);
            weaponObj.SetActive(data.type == CostumeType.Weapon);
            etcObj.SetActive(data.type == CostumeType.Etc);

            if (data.type == CostumeType.Body)
            {
                var imageBody = bodyObj.FindComponent<Image>("Image_Portrait");
                imageBody.sprite = Lance.Atlas.GetPlayerSprite(data.uiSprite);
            }
            else if(data.type == CostumeType.Etc)
            {
                var imageEtc = etcObj.FindComponent<Image>("Image_Etc");
                imageEtc.sprite = Lance.Atlas.GetPlayerSprite(data.uiSprite);
            }
            else
            {
                var imageWeapon = weaponObj.FindComponent<Image>("Image_Weapon");
                imageWeapon.sprite = Lance.Atlas.GetPlayerSprite(data.uiSprite);
            }

            var imageGrade = gameObject.FindComponent<Image>("Image_Grade");
            imageGrade.sprite = Lance.Atlas.GetIconGrade(data.grade);

            mSelectedObj = gameObject.FindGameObject("Selected");
            mEquippedObj = gameObject.FindGameObject("Equipped");
            mInActive = gameObject.FindGameObject("InActive");
            mRedDotObj = gameObject.FindGameObject("RedDot");
            mLockObj = gameObject.FindGameObject("Lock");
            mTextLockReason = mLockObj.FindComponent<TextMeshProUGUI>("Text_LockReason");

            var button = GetComponent<Button>();
            button.SetButtonAction(() =>
            {
                onSelect?.Invoke(mId);
            });

            Refresh();
        }

        public void SetSelect(bool isSelect)
        {
            mSelectedObj.SetActive(isSelect);

            mImageFrame.sprite = Lance.Atlas.GetUISprite(isSelect ? "Frame_Costume_Active" : "Frame_Costume_Inactive");
        }

        public void Localize()
        {
            var costumeData = DataUtil.GetCostumeData(mId);

            if (costumeData.requireLimitBreak > 0)
            {
                StringParam param = new StringParam("limitBreak", costumeData.requireLimitBreak);

                mTextLockReason.text = StringTableUtil.Get("UIString_RequireLimitBreak", param);
            }
            else if (costumeData.eventId.IsValid())
            {
                mTextLockReason.text = StringTableUtil.GetName(costumeData.eventId);
            }
        }

        public void Refresh()
        {
            bool haveCostume = Lance.Account.Costume.HaveCostume(mId);
            bool isEquipped = Lance.Account.Costume.IsEquipped(mId);

            var costumeData = DataUtil.GetCostumeData(mId);

            mEquippedObj.SetActive(isEquipped && haveCostume);
            mInActive.SetActive(!haveCostume);
            mLockObj.SetActive(haveCostume == false && 
                (costumeData.requireLimitBreak > Lance.Account.ExpLevel.GetLimitBreak() || costumeData.eventId.IsValid()));

            if (costumeData.requireLimitBreak > 0)
            {
                StringParam param = new StringParam("limitBreak", costumeData.requireLimitBreak);

                mTextLockReason.text = StringTableUtil.Get("UIString_RequireLimitBreak", param);
            }
            else if (costumeData.eventId.IsValid())
            {
                mTextLockReason.text = StringTableUtil.GetName(costumeData.eventId);
            }

            RefreshRedDot();
        }

        public void RefreshRedDot()
        {
            mRedDotObj.SetActive(RedDotUtil.IsActiveCostumeRedDot(mId));
        }
    }
}

