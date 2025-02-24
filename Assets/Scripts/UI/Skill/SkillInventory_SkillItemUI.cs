using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System;

namespace Lance
{
    class SkillInventory_SkillItemUI : MonoBehaviour
    {
        string mId;
        SkillData mSkillData;
        Image mImageItem;
        Image mImageType;
        Image mImageGrade;
        Image mImageGradeBackground;
        Image mImageModal;
        
        TextMeshProUGUI mTextLevel;
        TextMeshProUGUI mTextIsEquipped;
        TextMeshProUGUI mTextLockReason;
        GameObject mIsEquippedObj;
        GameObject mSelectedObj;
        GameObject mLockedObj;
        GameObject mRedDotObj;
        Slider mSliderCount;
        TextMeshProUGUI mTextCount;
        public RectTransform RectTm => GetComponent<RectTransform>();
        public string Id => mId;
        public void Init(string id)
        {
            mId = id;
            mSkillData = DataUtil.GetSkillData(id);

            mImageItem = gameObject.FindComponent<Image>("Image_Item");
            mImageType = gameObject.FindComponent<Image>("Image_Type");
            mImageGrade = gameObject.FindComponent<Image>("Image_Grade");
            mImageGradeBackground = gameObject.FindComponent<Image>("Image_GradeBackground");
            mTextLevel = gameObject.FindComponent<TextMeshProUGUI>("Text_Level");
            mTextIsEquipped = gameObject.FindComponent<TextMeshProUGUI>("Text_IsEquipped");
            mImageModal = gameObject.FindComponent<Image>("Image_Modal");

            mSliderCount = gameObject.FindComponent<Slider>("Slider_SkillCount");
            mTextCount = gameObject.FindComponent<TextMeshProUGUI>("Text_SkillCount");

            mIsEquippedObj = gameObject.FindGameObject("IsEquipped");
            mSelectedObj = gameObject.FindGameObject("Selected");
            mLockedObj = gameObject.FindGameObject("IsLocked");
            mTextLockReason = mLockedObj.FindComponent<TextMeshProUGUI>("Text_LockReason");
            mRedDotObj = gameObject.FindGameObject("RedDot");

            mImageType.sprite = Lance.Atlas.GetItemSlotUISprite($"Skill_{mSkillData.type}");
            mImageGrade.sprite = Lance.Atlas.GetIconGrade(mSkillData.grade);
            mImageGradeBackground.sprite = Lance.Atlas.GetFrameGrade(mSkillData.grade);
            mImageItem.sprite = Lance.Atlas.GetSkill(mSkillData.id);

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

            Refresh();
        }

        public void SetActiveRedDot(bool isActive)
        {
            mRedDotObj.SetActive(isActive);
        }

        public void RefreshRedDot()
        {
            SetActiveRedDot(RedDotUtil.IsActiveSkillRedDot(mId));
        }

        public void Localize()
        {
            if (mSkillData.requireLimitBreak > 0)
            {
                StringParam param = new StringParam("limitBreak", mSkillData.requireLimitBreak);

                mTextLockReason.text = StringTableUtil.Get("UIString_RequireLimitBreak", param);
            }
        }

        public void Refresh()
        {
            bool have = Lance.Account.HaveSkill(mSkillData.type, mId);
            bool isEquipped = Lance.Account.IsEquippedSkill(mSkillData.type, mId);

            SetHaveItem(have);

            if (have)
            {
                RefreshIsEquipped();
                RefreshLevel();
                RefreshSkillCount();
            }
            else
            {
                RefreshIsEquipped();
                RefreshSkillCount();
            }

            RefreshRedDot();

            mIsEquippedObj.SetActive(isEquipped);
            mImageModal.gameObject.SetActive(!have);
            RefreshLock();

            void SetHaveItem(bool have)
            {
                mTextLevel.gameObject.SetActive(have);
            }

            void RefreshIsEquipped()
            {
                mTextIsEquipped.gameObject.SetActive(isEquipped);
            }

            void RefreshLevel()
            {
                int level = Lance.Account.GetSkillLevel(mSkillData.type, mId);

                mTextLevel.text = $"Lv. {level}";
            }

            void RefreshSkillCount()
            {
                mTextCount.text = mSkillData.requireLimitBreak == 0 ? $"{Lance.Account.GetSkillCount(mSkillData.type, mId)}" : "";

                mSliderCount.value = have ? 1f : 0f;
            }

            void RefreshLock()
            {
                mLockedObj.SetActive(have == false && mSkillData.requireLimitBreak > 0);

                if (have == false && mSkillData.requireLimitBreak > 0)
                {
                    StringParam param = new StringParam("limitBreak", mSkillData.requireLimitBreak);

                    mTextLockReason.text = StringTableUtil.Get("UIString_RequireLimitBreak", param);
                }
            }
        }

        public void SetActiveModal(bool isAcitve)
        {
            mImageModal.gameObject.SetActive(isAcitve);
        }

        public void SetActiveIsEquipped(bool isActive)
        {
            mIsEquippedObj.SetActive(isActive);
        }

        public void SetActiveIsLocked(bool isActive)
        {
            mLockedObj.SetActive(isActive);
        }
    }
}