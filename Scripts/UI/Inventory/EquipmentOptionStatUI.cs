using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;


namespace Lance
{
    class EquipmentOptionStatUI : MonoBehaviour
    {
        string mEquipmentId;
        int mSlot;

        Image mImageGrade;
        TextMeshProUGUI mTextStatName;
        TextMeshProUGUI mTextStatValue;
        TextMeshProUGUI mTextLockReason;
        Image mImageChangeLock;
        Image mImageModal;
        GameObject mLockObj;
        public void Init(string equipmentId, int slot, Action onToggleChangeLock)
        {
            mEquipmentId = equipmentId;
            mSlot = slot;

            var buttonChange = gameObject.GetComponent<Button>();
            buttonChange.SetButtonAction(() =>
            {
                var statInfo = Lance.Account.GetEquipmentOptionStat(mEquipmentId, mSlot);
                if (statInfo != null)
                {
                    statInfo.ToggleChangeLock();

                    Refresh();

                    onToggleChangeLock?.Invoke();
                }
            });

            mImageGrade = gameObject.FindComponent<Image>("Image_Grade");
            mImageChangeLock = gameObject.FindComponent<Image>("Image_ChangeLock");
            mImageModal = gameObject.FindComponent<Image>("Image_Modal");
            mTextStatName = gameObject.FindComponent<TextMeshProUGUI>("Text_StatName");
            mTextStatValue = gameObject.FindComponent<TextMeshProUGUI>("Text_StatValue");

            mLockObj = gameObject.FindGameObject("Lock");

            mTextLockReason = gameObject.FindComponent<TextMeshProUGUI>("Text_LockReason");
        }

        public void Refresh()
        {
            var statInfo = Lance.Account.GetEquipmentOptionStat(mEquipmentId, mSlot);
            if (statInfo != null)
            {
                StatType statType = statInfo.GetStatType();
                bool isPercentType = statType.IsPercentType();
                bool changeLocked = statInfo.GetLocked();
                bool isActiveStat = Lance.Account.IsActiveEquipmentOptionStat(mEquipmentId, mSlot);

                if (isActiveStat)
                {
                    var data = DataUtil.GetEquipmentData(mEquipmentId);

                    double statValue = statInfo.GetStatValue(data?.grade ?? Grade.D, true);

                    mImageGrade.sprite = Lance.Atlas.GetIconGrade(statInfo.GetGrade());
                    mImageModal.gameObject.SetActive(changeLocked);
                    mImageChangeLock.SetColor(changeLocked ? "E9CFAA" : "FFFFFF");
                    mImageChangeLock.sprite = Lance.Atlas.GetItemSlotUISprite(changeLocked ? "Icon_Lock" : "Icon_LockOff");
                    mTextStatName.text = StringTableUtil.GetName($"{statType}");
                    mTextStatValue.text = isPercentType ? $"{statValue * 100f:F2}%" : $"{statValue}";
                }
                else
                {
                    mImageGrade.sprite = Lance.Atlas.GetIconGrade(Grade.D);
                    mTextStatName.text = "---";
                    mTextStatValue.text = "---";
                }

                mLockObj.SetActive(isActiveStat == false);
            }
            else
            {
                mImageGrade.sprite = Lance.Atlas.GetIconGrade(Grade.D);
                mTextStatName.text = "---";
                mTextStatValue.text = "---";
                mLockObj.SetActive(true);
            }

            bool haveEquipment = Lance.Account.HaveEquipment(mEquipmentId);
            if (haveEquipment)
                mTextLockReason.text = StringTableUtil.Get("UIString_NotEnoughEquipmentGrade");
            else
                mTextLockReason.text = StringTableUtil.Get("UIString_HaveNotEquipment");
        }
    }
}