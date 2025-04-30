using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Lance
{
    class DungeonItemUI : MonoBehaviour
    {
        DungeonData mData;
        GameObject mRedDotObj;
        GameObject mLockObj;
        Image mImageDungeon;
        TextMeshProUGUI mTextMyTicket;
        public void Init(DungeonData data)
        {
            mData = data;

            Button button = GetComponentInChildren<Button>();
            button.SetButtonAction(OnButtonAction);

            var guideActionTag = button.GetOrAddComponent<GuideActionTag>();
            guideActionTag.Tag = data.type.ChangeToGuideActionType();

            mImageDungeon = gameObject.FindComponent<Image>("Image_Dungeon");

            var imageTicket = gameObject.FindComponent<Image>("Image_Ticket");
            imageTicket.sprite = Lance.Atlas.GetTicket(data.type);

            mTextMyTicket = gameObject.FindComponent<TextMeshProUGUI>("Text_MyTicket");

            TextMeshProUGUI textName = gameObject.FindComponent<TextMeshProUGUI>("Text_DungeonName");
            textName.text = StringTableUtil.Get($"Title_{data.type}Dungeon");

            TextMeshProUGUI textDesc = gameObject.FindComponent<TextMeshProUGUI>("Text_DungeonDesc");
            textDesc.text = StringTableUtil.Get($"Desc_{data.type}Dungeon");

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
            if (mData.type == StageType.Ancient)
            {
                mLockObj.SetActive(Lance.Account.Artifact.IsAllArtifactMaxLevel() == false);
            }
            else
            {
                mLockObj.SetActive(ContentsLockUtil.IsLockContents(mData.type.ChangeToContentsLockType()));
            }
        }

        public void Localize()
        {
            TextMeshProUGUI textName = gameObject.FindComponent<TextMeshProUGUI>("Text_DungeonName");
            textName.text = StringTableUtil.Get($"Title_{mData.type}Dungeon");

            TextMeshProUGUI textDesc = gameObject.FindComponent<TextMeshProUGUI>("Text_DungeonDesc");
            textDesc.text = StringTableUtil.Get($"Desc_{mData.type}Dungeon");

            var textUnlock = mLockObj.FindComponent<TextMeshProUGUI>("Text_Unlock");
            if (mData.type == StageType.Ancient)
            {
                textUnlock.text = StringTableUtil.Get("UIString_RequireAllArtifactMaxLevel");
            }
            else
            {
                textUnlock.text = ContentsLockUtil.GetContentsLockMessage(mData.type.ChangeToContentsLockType());
            }
        }

        public void Refresh()
        {
            mImageDungeon.sprite = Lance.Atlas.GetDungeonThumbnail(mData.type);

            mTextMyTicket.text = $"{Lance.Account.GetDungeonTicket(mData.type)}";
        }

        void OnButtonAction()
        {
            if (mData.type == StageType.Raid)
            {
                var popup = Lance.PopupManager.CreatePopup<Popup_RaidDungeonUI>();

                popup.Init(this, mData);
            }
            else
            {
                string popupName = mData.type.HaveNextStepDungeon() ? "Popup_DungeonUI" : "Popup_DungeonUI2";

                var popup = Lance.PopupManager.CreatePopup<Popup_DungeonUI>(popupName);

                popup.Init(this, mData);
            }
        }
    }
}