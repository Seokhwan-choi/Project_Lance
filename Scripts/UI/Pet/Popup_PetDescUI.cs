using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Lance
{
    class Popup_PetDescUI : PopupBase
    {
        public void Init()
        {
            SetUpCloseAction();

            SetTitleText(StringTableUtil.Get("Title_Pet"));

            var petDesc = gameObject.FindComponent<TextMeshProUGUI>("Text_Desc");
            petDesc.text = StringTableUtil.GetDesc("Pet");

            var elementalSystem1 = gameObject.FindGameObject("Image_ElementalSystem_1");
            var textBonusValue = elementalSystem1.FindComponent<TextMeshProUGUI>("Text_BonusValue");

            StringParam param = new StringParam("valueAmount", $"{Lance.GameData.PetCommonData.strongTypeAtkValue:F2}");

            textBonusValue.text = StringTableUtil.Get("UIString_ValueAmount", param);

            var elementalSystem2 = gameObject.FindGameObject("Image_ElementalSystem_2");
            var textBonusValue2 = elementalSystem2.FindComponent<TextMeshProUGUI>("Text_BonusValue");
            StringParam param2 = new StringParam("valueAmount", $"{Lance.GameData.PetCommonData.weakTypeAtkValue:F2}");

            textBonusValue2.text = StringTableUtil.Get("UIString_ValueAmount", param2);
        }
    }
}