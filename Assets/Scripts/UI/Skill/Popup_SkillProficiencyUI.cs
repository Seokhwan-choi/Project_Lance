using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mosframe;

namespace Lance
{
    class Popup_SkillProficiencyUI : PopupBase
    {
        public void Init()
        {
            SetUpCloseAction();

            SetTitleText(StringTableUtil.GetName("SkillProficiency"));

            StartCoroutine(DelayedInit());
        }

        IEnumerator DelayedInit()
        {
            var scrollView = gameObject.FindComponent<DynamicVScrollView>("ScrollView");
            scrollView.totalItemCount = Lance.GameData.SkillProficiencyData.Count;
            scrollView.refresh();

            yield return null;

            yield return null;

            int currentLevel = Lance.Account.SkillInventory.GetSkillProficiencyLevel();
            int currentIndex = currentLevel - 1;

            scrollView.scrollByItemIndex(currentIndex);
        }
    }

    
}