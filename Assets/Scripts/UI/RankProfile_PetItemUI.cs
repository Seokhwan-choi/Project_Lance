using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace Lance
{
    class RankProfile_PetItemUI : MonoBehaviour
    {
        public void Init(string id, int level, int step)
        {
            var petData = Lance.GameData.PetData.TryGet(id);

            var imageElementalType = gameObject.FindComponent<Image>("Image_ElementalType");
            imageElementalType.sprite = Lance.Atlas.GetUISprite($"Icon_Ele_{petData.type}");

            var imagePet = gameObject.FindComponent<Image>("Image_Pet");
            var petUIMotion = imagePet.GetOrAddComponent<PetUIMotion>();
            petUIMotion.Init();

            petUIMotion.RefreshSprites(petData.type, step);

            // 레벨
            var textLevel = gameObject.FindComponent<TextMeshProUGUI>("Text_Level");
            textLevel.text = $"Lv.{level}";

            var isEquippedObj = gameObject.FindGameObject("IsEquipped");
            isEquippedObj.SetActive(false);

            var redDotObj = gameObject.FindGameObject("RedDot");
            redDotObj.SetActive(false);

            var subGradeManager = new SubGradeManager();
            subGradeManager.Init(gameObject.FindGameObject("SubGrade"));
            // 등급
            if (step > 0)
            {
                subGradeManager.SetActive(true);
                subGradeManager.SetSubGrade((SubGrade)step - 1);
            }
            else
            {
                subGradeManager.SetActive(false);
            }
        }
    }
}