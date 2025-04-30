using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;


namespace Lance
{
    class PetEvolutionStatUI2 : MonoBehaviour
    {
        string mPetId;
        int mSlot;

        Image mImageGrade;
        TextMeshProUGUI mTextStatName;
        TextMeshProUGUI mTextStatValue;
        public void Init(string petId, int slot)
        {
            mPetId = petId;
            mSlot = slot;
            mImageGrade = gameObject.FindComponent<Image>("Image_Grade");
            mTextStatName = gameObject.FindComponent<TextMeshProUGUI>("Text_StatName");
            mTextStatValue = gameObject.FindComponent<TextMeshProUGUI>("Text_StatValue");
        }

        public void Refresh()
        {
            var statInfo = Lance.Account.Pet.GetEvolutionStat(mPetId, mSlot);
            if (statInfo != null)
            {
                StatType statType = statInfo.GetStatType();
                bool isPercentType = statType.IsPercentType();
                bool isActiveStat = DataUtil.IsActivePetEvolutionStat(Lance.Account.Pet.GetStep(mPetId), mSlot);

                if (isActiveStat)
                {
                    mImageGrade.sprite = Lance.Atlas.GetIconGrade(statInfo.GetGrade());
                    mTextStatName.text = StringTableUtil.GetName($"{statType}");
                    mTextStatValue.text = isPercentType ? $"{statInfo.GetStatValue() * 100f:F2}%" : $"{statInfo.GetStatValue()}";

                    gameObject.SetActive(true);
                }
                else
                {
                    mImageGrade.sprite = Lance.Atlas.GetIconGrade(Grade.D);
                    mTextStatName.text = "---";
                    mTextStatValue.text = "---";

                    gameObject.SetActive(false);
                }
            }
            else
            {
                mImageGrade.sprite = Lance.Atlas.GetIconGrade(Grade.D);
                mTextStatName.text = "---";
                mTextStatValue.text = "---";

                gameObject.SetActive(false);
            }
        }
    }
}