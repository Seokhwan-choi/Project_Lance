using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;


namespace Lance
{
    class Popup_DemonicRealmSpoilsUI : PopupBase
    {
        bool mFirstMessage;
        TextMeshProUGUI mTextSoulStoneAmount;
        Image mImageCharacter;
        List<Image> mImageFriendShipIcons;
        TextMeshProUGUI mTextFriendShipValue;
        TextMeshProUGUI mTextMessage;
        List<DemonicRealmSpoilsItemUI> mSpoilsItemUIList;
        public void Init()
        {
            mFirstMessage = true;

            SetUpCloseAction();

            SetTitleText(StringTableUtil.Get("Title_DemonicRealmSpoils"));

            mTextSoulStoneAmount = gameObject.FindComponent<TextMeshProUGUI>("Text_SoulStoneAmount");
            mImageCharacter = gameObject.FindComponent<Image>("Image_Character");

            mImageFriendShipIcons = new List<Image>();
            mTextFriendShipValue = gameObject.FindComponent<TextMeshProUGUI>("Text_FriendShipCurrentValue");

            var friendShipIconsObj = gameObject.FindGameObject("FriendShipIcons");

            for(int i = 0; i < 10; ++i)
            {
                int index = i + 1;

                var friendShipIcon = friendShipIconsObj.FindComponent<Image>($"FriendShip_{index}");

                mImageFriendShipIcons.Add(friendShipIcon);
            }

            mTextMessage = gameObject.FindComponent<TextMeshProUGUI>("Text_Message");

            var itemListObj = gameObject.FindGameObject("ItemList");

            itemListObj.AllChildObjectOff();

            mSpoilsItemUIList = new List<DemonicRealmSpoilsItemUI>();

            foreach (var data in Lance.GameData.DemonicRealmSpoilsData.Values)
            {
                var spoilsItemObj = Util.InstantiateUI("DemonicRealmSpoilsItemUI", itemListObj.transform);

                var spoilsItemUI = spoilsItemObj.GetOrAddComponent<DemonicRealmSpoilsItemUI>();

                spoilsItemUI.Init(this, data.id);

                mSpoilsItemUIList.Add(spoilsItemUI);
            }

            var buttonRefreshMessage = gameObject.FindComponent<Button>("Character_Background");
            buttonRefreshMessage.SetButtonAction(RefreshMessage);

            if ( Lance.Account.DemonicRealmSpoils.Visit() )
            {
                PlayHeartExplosion();
            }

            var buttonInfo = gameObject.FindComponent<Button>("Button_Info");
            buttonInfo.SetButtonAction(() =>
            {
                var popup = Lance.PopupManager.CreatePopup<Popup_DemonicRealmSpoilsDescUI>();

                popup.Init();
            });

            Refresh();
            RefreshMessage();
        }

        public void Refresh()
        {
            mTextSoulStoneAmount.text = $"{Lance.Account.Currency.GetSoulStone()}";

            RefreshCharacter();
            RefreshFriendShip();

            foreach (var itemUI in mSpoilsItemUIList)
            {
                itemUI.Refresh();
            }
        }

        void RefreshCharacter()
        {
            int level = Lance.Account.DemonicRealmSpoils.GetFriendShipLevel();

            var speechData = Lance.GameData.FalanSpeechData.TryGet(level);
            if (speechData != null)
            {
                mImageCharacter.sprite = Lance.Atlas.GetUISprite(speechData.sprite);
            }
        }

        void RefreshFriendShip()
        {
            int myFriendShipLevel = Lance.Account.DemonicRealmSpoils.GetFriendShipLevel();

            for(int i = 0; i < 10; ++ i)
            {
                int level = i + 1;

                if (myFriendShipLevel >= level)
                {
                    mImageFriendShipIcons[i].sprite = Lance.Atlas.GetUISprite("Icon_Spoils_Friendship_Heart");
                }
                else
                {
                    mImageFriendShipIcons[i].sprite = Lance.Atlas.GetUISprite("Icon_Spoils_Friendship_HeartEmpty");
                }
            }

            mTextFriendShipValue.text = $"{Lance.Account.DemonicRealmSpoils.GetStackedFriendShipExp()}";
        }

        void RefreshMessage()
        {
            string message = string.Empty;
            int level = Lance.Account.DemonicRealmSpoils.GetFriendShipLevel();

            var falanData = Lance.GameData.FalanSpeechData.TryGet(level);
            if (falanData != null)
            {
                if (mFirstMessage)
                {
                    mFirstMessage = false;
                    // 첫번째 메세지
                    message = StringTableUtil.Get($"UIString_{falanData.firstSpeech}");
                }
                else
                {
                    // 랜덤 메세지
                    string[] speechList = falanData.speechList.SplitByDelim();

                    message = StringTableUtil.Get($"UIString_{Util.RandomSelect(speechList)}");
                }

                mTextMessage.text = string.Empty;
                mTextMessage.DOText(message, 1f)
                    .SetUpdate(isIndependentUpdate: true)
                    .OnComplete(() =>
                    {
                        mTextMessage.DOKill();
                        mTextMessage.text = message;
                    }).timeScale = 1f;
            }
        }

        public void PlayHeartExplosion()
        {
            SoundPlayer.PlayHeartExplostion();

            Lance.ParticleManager.AquireUI("HeartExplosion", mImageCharacter.rectTransform);
        }
    }
}