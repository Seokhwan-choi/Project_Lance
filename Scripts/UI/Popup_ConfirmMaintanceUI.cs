using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using DG.Tweening;

namespace Lance
{
    class Popup_ConfirmMaintanceUI : PopupBase
    {
        public void Init(string title, string desc, Action onConfirm)
        {
            SetTitleText(title);

            var textDesc = gameObject.FindComponent<TextMeshProUGUI>("Text_Desc");
            textDesc.text = desc;

            Button buttonConfirm = gameObject.FindComponent<Button>("Button_Confirm");
            buttonConfirm.SetButtonAction(() =>
            {
                onConfirm?.Invoke();

                Close();
            });
        }

        public override void OnBackButton(bool immediate = false, bool hideMotion = true)
        {
            
        }
    }
}


