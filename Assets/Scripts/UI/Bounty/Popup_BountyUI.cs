using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Lance
{
    class Popup_BountyUI : PopupBase
    {
        List<BountyQuestItemUI> mQuestItemUIList;
        public void Init()
        {
            SetUpCloseAction();
            SetTitleText(StringTableUtil.Get("Title_Bounty"));

            var textDesc = gameObject.FindComponent<TextMeshProUGUI>("Text_Desc");
            textDesc.text = StringTableUtil.Get("Desc_Bounty");

            mQuestItemUIList = new List<BountyQuestItemUI>();

            int index = 0;

            foreach (BountyQuestInfo questInfo in Lance.Account.GetBountyQuestInfos())
            {
                var questItemObj = gameObject.FindGameObject($"BountyItemUI_{index + 1}");

                var questItemUI = questItemObj.GetOrAddComponent<BountyQuestItemUI>();
                questItemUI.Init(questInfo.GetId(), onMove:()=> Close());

                mQuestItemUIList.Add(questItemUI);

                index++;
            }
        }
    }

}