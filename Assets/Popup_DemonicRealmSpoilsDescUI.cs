using UnityEngine;


namespace Lance
{
    class Popup_DemonicRealmSpoilsDescUI : PopupBase
    {
        public void Init()
        {
            SetUpCloseAction();

            SetTitleText(StringTableUtil.Get("Title_DemonicRealmSpoils"));
        }
    }
}