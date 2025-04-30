using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Lance
{
    class DemonicRealmItemUI : MonoBehaviour
    {
        DemonicRealmData mData;
        GameObject mRedDotObj;
        GameObject mLockObj;
        Image mImageDemonicRealm;
        TextMeshProUGUI mTextMyTicket;
        public void Init(DemonicRealmData data)
        {
            mData = data;

            Button button = GetComponentInChildren<Button>();
            button.SetButtonAction(OnButtonAction);

            var guideActionTag = button.GetOrAddComponent<GuideActionTag>();
            guideActionTag.Tag = data.type.ChangeToGuideActionType();

            mImageDemonicRealm = gameObject.FindComponent<Image>("Image_DemonicRealm");

            var imageTicket = gameObject.FindComponent<Image>("Image_Ticket");
            imageTicket.sprite = Lance.Atlas.GetDemonicRealmStone(data.type);

            mTextMyTicket = gameObject.FindComponent<TextMeshProUGUI>("Text_MyTicket");

            TextMeshProUGUI textName = gameObject.FindComponent<TextMeshProUGUI>("Text_DemonicRealmName");
            textName.text = StringTableUtil.Get($"Title_{data.type}DemonicRealm");

            TextMeshProUGUI textDesc = gameObject.FindComponent<TextMeshProUGUI>("Text_DemonicRealmDesc");
            textDesc.text = StringTableUtil.Get($"Desc_{data.type}DemonicRealm");

            mRedDotObj = gameObject.FindGameObject("RedDot");
            mLockObj = gameObject.FindGameObject("Lock");

            var textUnlock = mLockObj.FindComponent<TextMeshProUGUI>("Text_Unlock");
            if (data.type == StageType.Ancient)
            {
                textUnlock.text = StringTableUtil.Get("UIString_RequireAllArtifactMaxLevel");
            }
            else
            {
                textUnlock.text = ContentsLockUtil.GetContentsLockMessage(mData.type.ChangeToContentsLockType());
            }
            
            Refresh();
        }

        public void RefreshContentsLockUI()
        {
            mLockObj.SetActive(ContentsLockUtil.IsLockContents(mData.type.ChangeToContentsLockType()));
        }

        public void Localize()
        {
            TextMeshProUGUI textName = gameObject.FindComponent<TextMeshProUGUI>("Text_DemonicRealmName");
            textName.text = StringTableUtil.Get($"Title_{mData.type}DemonicRealm");

            TextMeshProUGUI textDesc = gameObject.FindComponent<TextMeshProUGUI>("Text_DemonicRealmDesc");
            textDesc.text = StringTableUtil.Get($"Desc_{mData.type}DemonicRealm");

            var textUnlock = mLockObj.FindComponent<TextMeshProUGUI>("Text_Unlock");
            textUnlock.text = ContentsLockUtil.GetContentsLockMessage(mData.type.ChangeToContentsLockType());
        }

        public void Refresh()
        {
            mImageDemonicRealm.sprite = Lance.Atlas.GetDemonicRealmThumbnail(mData.type);

            mTextMyTicket.text = $"{Lance.Account.Currency.GetDemonicRealmStone(mData.type)}";
        }

        void OnButtonAction()
        {
            var popup = Lance.PopupManager.CreatePopup<Popup_DemonicRealmUI>();

            popup.Init(this, mData);
        }
    }
}