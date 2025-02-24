using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System;

namespace Lance
{
    class Inventory_AccessoryItemUI : MonoBehaviour
    {
        string mId;
        AccessoryData mData;
        RectTransform mItemTm;
        Image mImageItem;
        Image mImageGradeBackground;
        Image mImageGrade;
        Image mImageModal;
        TextMeshProUGUI mTextLevel;
        TextMeshProUGUI mTextReforgeStep;
        TextMeshProUGUI mTextIsEquipped;

        GameObject mIsEquippedObj;
        GameObject mSelectedObj;
        GameObject mRedDotObj;
        SubGradeManager mSubGradeManager;
        Slider mSliderEquipmentCount;
        TextMeshProUGUI mTextEquipmentCount;
        public RectTransform ItemTm => mItemTm;
        public string Id => mId;
        public void Init()
        {
            mSubGradeManager = new SubGradeManager();
            mSubGradeManager.Init(gameObject.FindGameObject("SubGrade"));

            mItemTm = gameObject.FindGameObject("Item").GetComponent<RectTransform>();
            mImageItem = gameObject.FindComponent<Image>("Image_Item");
            mTextIsEquipped = gameObject.FindComponent<TextMeshProUGUI>("Text_IsEquipped");
            mImageGradeBackground = gameObject.FindComponent<Image>("Image_GradeBackground");
            mImageGrade = gameObject.FindComponent<Image>("Image_Grade");
            mImageModal = gameObject.FindComponent<Image>("Image_Modal");
            mTextLevel = gameObject.FindComponent<TextMeshProUGUI>("Text_Level");
            mTextReforgeStep = gameObject.FindComponent<TextMeshProUGUI>("Text_ReforgeStep");
            mSliderEquipmentCount = gameObject.FindComponent<Slider>("Slider_AccessoryCount");
            mTextEquipmentCount = gameObject.FindComponent<TextMeshProUGUI>("Text_AccessoryCount");
            mIsEquippedObj = gameObject.FindGameObject("IsEquipped");
            mSelectedObj = gameObject.FindGameObject("Selected");
            mRedDotObj = gameObject.FindGameObject("RedDot");
        }

        public void SetId(string id)
        {
            mId = id;
            mData = DataUtil.GetAccessoryData(id);

            mImageGrade.sprite = Lance.Atlas.GetIconGrade(mData.grade);
            mImageGradeBackground.sprite = Lance.Atlas.GetFrameGrade(mData.grade);
            mImageItem.sprite = Lance.Atlas.GetItemSlotUISprite(mData.sprite);
            mSubGradeManager.SetSubGrade(mData.subGrade);

            Refresh();
        }

        public void SetButtonAction(Action<string> buttonAction)
        {
            var button = GetComponent<Button>();
            button.SetButtonAction(() =>
            {
                if (mId.IsValid())
                    buttonAction.Invoke(mId);
            });
        }

        public void SetSelected(bool isSelected)
        {
            mImageModal.gameObject.SetActive(isSelected);
            mSelectedObj.SetActive(isSelected);

            Refresh(isSelected);
        }

        void RefreshRedDot()
        {
            mRedDotObj.SetActive(mIsIgnoreRedDot ? false : RedDotUtil.IsActiveAccessoryRedDot(mId));
        }

        bool mIsIgnoreRedDot;
        public void SetIgnoreRedDot(bool ignore)
        {
            mIsIgnoreRedDot = ignore;
        }

        public void Refresh(bool isSelected = false)
        {
            if (mId.IsValid() == false)
                return;

            var accessoryInst = Lance.Account.GetAccessory(mId);

            bool have = accessoryInst != null;
            bool isEquipped = accessoryInst?.IsEquipped() ?? false;

            if (have)
            {
                mTextLevel.text = $"Lv. {accessoryInst.GetLevel()}";

                var reforgeStep = accessoryInst.GetReforgeStep();

                mTextReforgeStep.gameObject.SetActive(reforgeStep > 0);

                mTextReforgeStep.text = $"+{reforgeStep}";
            }
            else
            {
                mTextReforgeStep.gameObject.SetActive(false);
            }

            RefreshRedDot();

            int count = accessoryInst?.GetCount() ?? 0;
            int combineCount = mData.combineCount;

            combineCount = Math.Max(1, combineCount);

            mTextEquipmentCount.text = $"{count} / {combineCount}";

            mSliderEquipmentCount.value = (float)count / (float)combineCount;

            mTextIsEquipped.gameObject.SetActive(isEquipped);
            mTextLevel.gameObject.SetActive(have);
            mIsEquippedObj.SetActive(isEquipped);
            mImageModal.gameObject.SetActive(isSelected || !have);
        }

        public void SetActiveModal(bool isAcitve)
        {
            mImageModal.gameObject.SetActive(isAcitve);
        }

        public void SetActiveIsEquipped(bool isActive)
        {
            mIsEquippedObj.SetActive(isActive);
        }
    }
}