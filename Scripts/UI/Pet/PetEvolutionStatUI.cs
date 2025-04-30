using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;


namespace Lance
{
    class PetEvolutionStatUI : MonoBehaviour
    {
        string mPetId;
        int mSlot;

        Image mImageGrade;
        TextMeshProUGUI mTextStatName;
        TextMeshProUGUI mTextStatValue;
        Image mImageChangeLock;
        Image mImageModal;
        GameObject mLockObj;
        public void Init(string petId, int slot, Action onToggleChangeLock)
        {
            mPetId = petId;
            mSlot = slot;

            var buttonChange = gameObject.GetComponent<Button>();
            buttonChange.SetButtonAction(() =>
            {
                var statInfo = Lance.Account.Pet.GetEvolutionStat(mPetId, mSlot);
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

            var lockReason = gameObject.FindComponent<TextMeshProUGUI>("Text_LockReason");

            StringParam param = new StringParam("step", Lance.GameData.PetEvolutionStatUnlockData.unlockSlot[slot]);

            lockReason.text = StringTableUtil.Get("UIString_StepUnlock", param);
        }

        public void Refresh()
        {
            var statInfo = Lance.Account.Pet.GetEvolutionStat(mPetId, mSlot);
            if (statInfo != null)
            {
                StatType statType = statInfo.GetStatType();
                bool isPercentType = statType.IsPercentType();
                bool changeLocked = statInfo.GetLocked();
                bool isActiveStat = DataUtil.IsActivePetEvolutionStat(Lance.Account.Pet.GetStep(mPetId), mSlot);

                if (isActiveStat)
                {
                    mImageGrade.sprite = Lance.Atlas.GetIconGrade(statInfo.GetGrade());
                    mImageModal.gameObject.SetActive(changeLocked);
                    mImageChangeLock.SetColor(changeLocked ? "E9CFAA" : "FFFFFF");
                    mImageChangeLock.sprite = Lance.Atlas.GetItemSlotUISprite(changeLocked ? "Icon_Lock" : "Icon_LockOff");
                    mTextStatName.text = StringTableUtil.GetName($"{statType}");
                    mTextStatValue.text = isPercentType ? $"{statInfo.GetStatValue() * 100f:F2}%" : $"{statInfo.GetStatValue()}";
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
        }
    }
}