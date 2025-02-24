using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace Lance
{
    class EquipmentOption_StatToOwnStatTabUI : EquipmentOptionInfoTabUI
    {
        //List<GradeOwnStatValueItemUI> mOwnStatValueTableItemUIList;
        public override void Init(EquipmentOptionInfoTab tab)
        {
            base.Init(tab);

            //mOwnStatValueTableItemUIList = new List<GradeOwnStatValueItemUI>();

            for (int i = 0; i < (int)Grade.Count; ++i)
            {
                Grade grade = (Grade)i;

                var statValueTableItemUIObj = gameObject.FindGameObject($"GradeChangeValueItemUI_{i + 1}");
                var statValueTableItemUI = statValueTableItemUIObj.GetOrAddComponent<GradeOwnStatValueItemUI>();
                statValueTableItemUI.Init(grade);

                var data = Lance.GameData.EquipmentOptionStatToOwnStatData.TryGet(grade);
                if (data != null)
                {
                    statValueTableItemUI.Refresh((float)data.changeValue);
                }

                //mOwnStatValueTableItemUIList.Add(statValueTableItemUI);
            }
        }
    }

    class GradeOwnStatValueItemUI : MonoBehaviour
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

            // 프리팹 재활용해서 이름이 prob임
            mTextProb = gameObject.FindComponent<TextMeshProUGUI>("Text_Prob");
            mImageModal = gameObject.FindComponent<Image>("Image_Modal");
        }

        public void Refresh(double changeValue)
        {
            mTextProb.text = $"{changeValue * 100f:F2}%";

            mImageModal.gameObject.SetActive(changeValue == 0);
        }
    }
}