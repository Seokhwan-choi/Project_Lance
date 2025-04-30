using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Lance
{
    class GradeProbItemUI : MonoBehaviour
    {
        Grade mGrade;
        TextMeshProUGUI mTextProb;
        Image mImageModal;
        public Grade Grade => mGrade;
        public void Init(Grade grade)
        {
            mGrade = grade;
            var imageGrade = gameObject.FindComponent<Image>("Image_Item");
            imageGrade.sprite = Lance.Atlas.GetIconGrade(grade);

            mTextProb = gameObject.FindComponent<TextMeshProUGUI>("Text_Prob");
            mImageModal = gameObject.FindComponent<Image>("Image_Modal");
        }

        public void Refresh(float prob)
        {
            mTextProb.text = $"{prob * 100f:F3}%";

            mImageModal.gameObject.SetActive(prob == 0);
        }
    }
}