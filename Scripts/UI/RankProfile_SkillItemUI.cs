using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System;

namespace Lance
{
    class RankProfile_SkillItemUI : MonoBehaviour
    {
        public void Init(string id, int level)
        {
            gameObject.SetActive(true);

            var data = DataUtil.GetSkillData(id);

            var imageItem = gameObject.FindComponent<Image>("Image_Item");
            imageItem.sprite = Lance.Atlas.GetSkill(data.id);

            var imageGrade = gameObject.FindComponent<Image>("Image_Grade");
            imageGrade.sprite = Lance.Atlas.GetIconGrade(data.grade);

            var imageGradeBackground = gameObject.FindComponent<Image>("Image_GradeBackground");
            imageGradeBackground.sprite = Lance.Atlas.GetFrameGrade(data.grade);

            var textLevel = gameObject.FindComponent<TextMeshProUGUI>("Text_Level");
            textLevel.text = $"Lv. {level}";

            var sliderSkillCountCount = gameObject.FindComponent<Slider>("Slider_SkillCount");
            sliderSkillCountCount.value = 0f;

            var textEquipmentCount = gameObject.FindComponent<TextMeshProUGUI>("Text_SkillCount");
            textEquipmentCount.text = "0";

            var isEquippedObj = gameObject.FindGameObject("IsEquipped");
            isEquippedObj.SetActive(false);

            var selectedObj = gameObject.FindGameObject("Selected");
            selectedObj.SetActive(false);

            var redDotObj = gameObject.FindGameObject("RedDot");
            redDotObj.SetActive(false);

            var textIsEquipped = gameObject.FindComponent<TextMeshProUGUI>("Text_IsEquipped");
            textIsEquipped.gameObject.SetActive(false);

            var isLocked = gameObject.FindGameObject("IsLocked");
            isLocked.SetActive(false);

            var imageModal = gameObject.FindComponent<Image>("Image_Modal");
            imageModal.gameObject.SetActive(false);
        }
    }
}