using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using BackEnd;

namespace Lance
{
    class Popup_ThanksToUI : PopupBase
    {
        public void Init()
        {
            SetUpCloseAction();

            SetTitleText(StringTableUtil.Get("Title_Thanksto"));
        }
    }
}