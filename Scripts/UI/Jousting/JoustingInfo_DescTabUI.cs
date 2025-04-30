using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Lance
{
    class JoustingInfo_DescTabUI : JoustingInfoTabUI 
    {
        public override void Init(JoustingInfoTabUIManager parent, JoustingInfoTab tab)
        {
            base.Init(parent, tab);

            var textDesc = gameObject.FindComponent<TextMeshProUGUI>("Text_Desc");
            textDesc.text = StringTableUtil.GetDesc("Jousting");
        }
    }
}