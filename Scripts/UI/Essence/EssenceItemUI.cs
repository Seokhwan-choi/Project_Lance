using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;


namespace Lance
{
    class EssenceItemUI : MonoBehaviour
    {
        EssenceType mType;
        Image mImageEssence_Step1;
        Image mImageEssence_Step2;
        Image mImageEssence_Step3;
        Image mImageEssence_Step4;
        Image mImageEssence_Step5;
        Image mImageSelected;
        TextMeshProUGUI mTextLevel;
        GameObject mRedDot;

        public EssenceType Type => mType;
        public void Init(EssenceType essenceType, Action<EssenceType> onSelect)
        {
            mType = essenceType;
            mImageEssence_Step1 = gameObject.FindComponent<Image>("Image_Essence_1");
            mImageEssence_Step1.sprite = Lance.Atlas.GetItemSlotUISprite($"Essence_{essenceType}_1");

            mImageEssence_Step2 = gameObject.FindComponent<Image>("Image_Essence_2");
            mImageEssence_Step2.sprite = Lance.Atlas.GetItemSlotUISprite($"Essence_{essenceType}_2");

            mImageEssence_Step3 = gameObject.FindComponent<Image>("Image_Essence_3");
            mImageEssence_Step3.sprite = Lance.Atlas.GetItemSlotUISprite($"Essence_{essenceType}_3");

            mImageEssence_Step4 = gameObject.FindComponent<Image>("Image_Essence_4");
            mImageEssence_Step4.sprite = Lance.Atlas.GetItemSlotUISprite($"Essence_{essenceType}_4");

            mImageEssence_Step5 = gameObject.FindComponent<Image>("Image_Essence_5");
            mImageEssence_Step5.sprite = Lance.Atlas.GetItemSlotUISprite($"Essence_{essenceType}_5");

            mImageSelected = gameObject.FindComponent<Image>("Image_Selected");
            mTextLevel = gameObject.FindComponent<TextMeshProUGUI>("Text_Level");
            mRedDot = gameObject.FindGameObject("RedDot");

            var button = GetComponent<Button>();
            button.SetButtonAction(() => onSelect?.Invoke(mType));

            Refresh();
        }

        public void SetSelected(bool isSelected)
        {
            mImageSelected.gameObject.SetActive(isSelected);
        }

        public void Refresh()
        {
            int level = Lance.Account.Essence.GetLevel(mType);
            int step = Lance.Account.Essence.GetStep(mType);

            mImageEssence_Step1.gameObject.SetActive(step + 1 == 1);
            mImageEssence_Step2.gameObject.SetActive(step + 1 == 2);
            mImageEssence_Step3.gameObject.SetActive(step + 1 == 3);
            mImageEssence_Step4.gameObject.SetActive(step + 1 == 4);
            mImageEssence_Step5.gameObject.SetActive(step + 1 >= 5);

            mTextLevel.text = mType == EssenceType.Central ? $"{step + 1}" : $"{level}";

            mRedDot.SetActive(Lance.Account.CanUpgradeEssence(mType));

            if (mType == EssenceType.Central)
            {
                var activeObj = gameObject.FindGameObject("Active");

                activeObj.SetActive(Lance.Account.Essence.IsActiveCentralEssence());
            }
        }

        public void PlayMotion()
        {
            Lance.ParticleManager.AquireUI($"UpgradeEssence_{mType}", transform as RectTransform);
        }
    }
}
