using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Lance
{
    class Popup_LimitBreakUI : PopupBase
    {
        public void Init()
        {
            StartCoroutine(ShowActiveMotion());
        }

        IEnumerator ShowActiveMotion()
        {
            var limitBreakObj = gameObject.FindGameObject("Image_LimitBreak");

            var limitBreakUIManager = limitBreakObj.GetOrAddComponent<LimitBreakUIManager>();
            limitBreakUIManager.Init();

            yield return limitBreakUIManager.PlayActiveMotion(Lance.Account.ExpLevel.GetLimitBreak());

            Close();
        }
    }
}