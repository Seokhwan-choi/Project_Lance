using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System;

namespace Lance
{
    class RankProfile_EquipmentItemUI : MonoBehaviour
    {
        public void Init(string id, int level, int reforgeStep)
        {
            gameObject.SetActive(true);

            var data = DataUtil.GetEquipmentData(id);

            var subGradeManager = new SubGradeManager();
            subGradeManager.Init(gameObject.FindGameObject("SubGrade"));
            subGradeManager.SetSubGrade(data.subGrade);

            var imageItem = gameObject.FindComponent<Image>("Image_Item");
            imageItem.sprite = Lance.Atlas.GetItemSlotUISprite(data.sprite);

            var imageGrade = gameObject.FindComponent<Image>("Image_Grade");
            imageGrade.sprite = Lance.Atlas.GetIconGrade(data.grade);

            var imageGradeBackground = gameObject.FindComponent<Image>("Image_GradeBackground");
            imageGradeBackground.sprite = Lance.Atlas.GetFrameGrade(data.grade);

            var textLevel = gameObject.FindComponent<TextMeshProUGUI>("Text_Level");
            textLevel.text = $"Lv. {level}";

            var textReforgeStep = gameObject.FindComponent<TextMeshProUGUI>("Text_ReforgeStep");
            textReforgeStep.gameObject.SetActive(reforgeStep > 0);
            textReforgeStep.text = $"+{reforgeStep}";

            var sliderEquipmentCount = gameObject.FindComponent<Slider>("Slider_EquipmentCount");
            sliderEquipmentCount.value = 0f;

            var textEquipmentCount = gameObject.FindComponent<TextMeshProUGUI>("Text_EquipmentCount");
            textEquipmentCount.text = $"{0} / {data.combineCount}";

            var isEquippedObj = gameObject.FindGameObject("IsEquipped");
            isEquippedObj.SetActive(false);

            var selectedObj = gameObject.FindGameObject("Selected");
            selectedObj.SetActive(false);

            var redDotObj = gameObject.FindGameObject("RedDot");
            redDotObj.SetActive(false);

            var textIsEquipped = gameObject.FindComponent<TextMeshProUGUI>("Text_IsEquipped");
            textIsEquipped.gameObject.SetActive(false);

            var imageModal = gameObject.FindComponent<Image>("Image_Modal");
            imageModal.gameObject.SetActive(false);
        }
    }
}