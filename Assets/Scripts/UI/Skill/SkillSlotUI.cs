using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

namespace Lance
{
    class SkillSlotUI : MonoBehaviour
    {
        SkillType mType;
        string mId;
        int mSlot;
        GameObject mUnlockObj;
        GameObject mEquippedObj;
        
        Image mImageSkill;
        Image mImageFrame;
        Image mImageInCoolTime;
        TextMeshProUGUI mTextCoolTime;

        GameObject mRedDotObj;
        GameObject mEmptyObj;
        GameObject mLockObj;

        public string Id => mId;
        public void Init(SkillType skillType, int slot, Action<int> onButton = null)
        {
            mType = skillType;
            mSlot = slot;

            if (onButton != null)
            {
                var button = gameObject.GetComponentInChildren<Button>();
                button.SetButtonAction(() =>
                {
                    onButton?.Invoke(mSlot);

                    Refresh();
                });
            }

            mUnlockObj = gameObject.FindGameObject("Unlock");
            mEquippedObj = mUnlockObj.FindGameObject("Equipped");
            mImageSkill = mEquippedObj.FindComponent<Image>("Image_Skill");
            mImageFrame = gameObject.FindComponent<Image>("Image_Frame");
            mImageInCoolTime = mEquippedObj.FindComponent<Image>("Image_InCoolTime");
            mTextCoolTime = mEquippedObj.FindComponent<TextMeshProUGUI>("Text_CoolTime");

            mRedDotObj = gameObject.FindGameObject("RedDot");
            mEmptyObj = mUnlockObj.FindGameObject("Empty");
            mLockObj = gameObject.FindGameObject("Lock");

            int unLockLevel = DataUtil.GetSkillSlotUnlockLevel(mSlot);

            var textUnlockLevel = mLockObj.FindComponent<TextMeshProUGUI>("Text_UnlockLevel");
            textUnlockLevel.text = $"Lv. {unLockLevel}";

            Refresh();

            UpdateCoolTime(0, 0);
        }

        public void UpdateCoolTime(float coolTime, float coolTimeValue)
        {
            if (coolTime > 0)
            {
                mImageInCoolTime.gameObject.SetActive(true);
                mTextCoolTime.gameObject.SetActive(true);

                mImageInCoolTime.fillAmount = coolTimeValue;
                mTextCoolTime.text = $"{coolTime:F1}";
            }
            else
            {
                mImageInCoolTime.gameObject.SetActive(false);
                mTextCoolTime.gameObject.SetActive(false);
            }
        }

        public void Refresh()
        {
            var curPresetIds = Lance.Account.SkillInventory.GetCurSkillsetIds(mType);

            mId = curPresetIds[mSlot];

            bool isUnlock = Lance.Account.IsUnlockSkillSlot(mSlot);
            bool anyCanEquipSkill = Lance.Account.SkillInventory.AnyCanEquipSkill(mType);

            mUnlockObj.SetActive(isUnlock);
            mLockObj.SetActive(!isUnlock);

            if (mId.IsValid())
            {
                mEquippedObj.SetActive(true);
                mEmptyObj.SetActive(false);
                mRedDotObj.SetActive(false);

                var data = DataUtil.GetSkillData(mId);

                Debug.Assert(data != null, $"{mId}의 SkillData가 존재하지 않음");

                Grade grade = isUnlock ? data.grade : Grade.D;

                mImageSkill.sprite = Lance.Atlas.GetSkill(mId);
                mImageFrame.sprite = Lance.Atlas.GetFrameGrade(grade);
            }
            else
            {
                mEquippedObj.SetActive(false);
                mEmptyObj.SetActive(true);
                mRedDotObj.SetActive(isUnlock && anyCanEquipSkill);

                mImageFrame.sprite = Lance.Atlas.GetFrameGrade(Grade.D);
            }
        }
    }
}