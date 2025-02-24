using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Lance
{
    class Popup_DescUI : PopupBase
    {
        public void Init(string title, string desc)
        {
            SetUpCloseAction();
            SetTitleText(title);

            var textDesc = gameObject.FindComponent<TextMeshProUGUI>("Text_Desc");

            textDesc.text = desc;
        }
    }
}