using UnityEngine;

namespace Lance
{
    class Popup_EquipmentOptionInfoUI : PopupBase
    {
        EquipmentOptionInfoTabTabUIManager mTabUIManager;
        public void Init()
        {
            SetUpCloseAction();

            SetTitleText(StringTableUtil.Get("Title_EquipmentOptionStatInfo"));

            mTabUIManager = new EquipmentOptionInfoTabTabUIManager();
            mTabUIManager.Init(gameObject);
        }
    }
}