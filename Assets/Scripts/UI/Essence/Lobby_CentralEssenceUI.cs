using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Lance
{
    class Lobby_CentralEssenceUI : MonoBehaviour
    {
        Button mButtonCentralEssence;
        GameObject mActiveFX;
        Image mImageFrame;
        Image mImageCentralEssence;
        TextMeshProUGUI mTextDuration;
        public void Init()
        {
            mButtonCentralEssence = gameObject.FindComponent<Button>("Button_CentralEssence");
            mButtonCentralEssence.SetButtonAction(OnCentralEssence);

            mImageFrame = mButtonCentralEssence.GetComponent<Image>();
            mActiveFX = gameObject.FindGameObject("Active");
            mImageCentralEssence = gameObject.FindComponent<Image>("Image_CentralEssence");
            mTextDuration = gameObject.FindComponent<TextMeshProUGUI>("Text_CentralEssence");

            RefreshCentralEssenceUI();
        }

        public void RefreshContentsLockUI()
        {
            gameObject.SetActive(ContentsLockUtil.IsLockContents(ContentsLockType.Essence) == false);
        }

        void OnCentralEssence()
        {
            var popup = Lance.PopupManager.CreatePopup<Popup_CentralEssenceActiveUI>();

            popup.Init();
        }

        public void RefreshCentralEssenceUI()
        {
            if (Lance.Account.Essence.IsActiveCentralEssence())
            {
                mActiveFX.SetActive(true);
                mImageFrame.sprite = Lance.Atlas.GetUISprite("Frame_Auto_Active");
                mImageCentralEssence.sprite = Lance.Atlas.GetUISprite("Icon_Essence_Active");

                if (mTextDuration.gameObject.activeSelf == false)
                    mTextDuration.gameObject.SetActive(true);

                mTextDuration.text = TimeUtil.GetTimeStr(Mathf.RoundToInt(Lance.Account.Essence.GetDurationTime()), ignoreHour: true);
            }
            else
            {
                mActiveFX.SetActive(false);
                mImageFrame.sprite = Lance.Atlas.GetUISprite("Frame_Auto_Inactive");
                mImageCentralEssence.sprite = Lance.Atlas.GetUISprite("Icon_Essence_Inactive");

                mTextDuration.text = StringTableUtil.Get("UIString_CentralEssenceActive");
            }
        }

        public void OnStartStage(StageData stageData)
        {
            gameObject.SetActive(!stageData.type.IsJousting() && ContentsLockUtil.IsLockContents(ContentsLockType.Essence) == false);
        }
    }
}