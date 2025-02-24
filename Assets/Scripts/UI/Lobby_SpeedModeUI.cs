using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Lance
{
    class Lobby_SpeedModeUI : MonoBehaviour
    {
        Button mButtonSpeedMode;
        Image mImageFrameGlow;
        Image mImageFrame;
        Image mImageSpeedValue;
        TextMeshProUGUI mTextDuration;
        public void Init()
        {
            mButtonSpeedMode = gameObject.FindComponent<Button>("Button_SpeedMode");
            mButtonSpeedMode.SetButtonAction(OnSpeedModeButton);

            mImageFrame = mButtonSpeedMode.GetComponent<Image>();
            mImageFrameGlow = gameObject.FindComponent<Image>("Image_FrameGlow");
            mImageSpeedValue = gameObject.FindComponent<Image>("Image_SpeedValue");
            mTextDuration = gameObject.FindComponent<TextMeshProUGUI>("Text_Duration");

            bool purchasedRemovedAD = Lance.Account.PackageShop.IsPurchasedRemoveAD() || Lance.IAPManager.IsPurchasedRemoveAd();

            RefreshSpeedModeUI(purchasedRemovedAD);
        }

        public void RefreshContentsLockUI()
        {
            SetActive(ContentsLockUtil.IsLockContents(ContentsLockType.SpeedMode) == false);
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        void OnSpeedModeButton()
        {
            Lance.GameManager.ActiveSpeedMode();
        }

        public void RefreshSpeedModeUI(bool purchasedRemovedAD)
        {
            if (Lance.Account.SpeedMode.InSpeedMode())
            {
                mImageFrameGlow.gameObject.SetActive(true);
                mImageFrame.sprite = Lance.Atlas.GetUISprite("Frame_Auto_Active");
                mImageSpeedValue.sprite = Lance.Atlas.GetUISprite("Icon_Speed_Active");

                if (purchasedRemovedAD)
                {
                    // 광고 제한을 구매했기 때문에 제한 시간을 보여줄 필요가 없다.
                    if (mTextDuration.gameObject.activeSelf)
                        mTextDuration.gameObject.SetActive(false);
                }
                else
                {
                    if (mTextDuration.gameObject.activeSelf == false)
                        mTextDuration.gameObject.SetActive(true);

                    mTextDuration.text = TimeUtil.GetTimeStr(Mathf.RoundToInt(Lance.Account.SpeedMode.GetDurationTime()), ignoreHour:true);
                }
            }
            else
            {
                mImageFrameGlow.gameObject.SetActive(false);
                mImageFrame.sprite = Lance.Atlas.GetUISprite("Frame_Auto_Inactive");
                mImageSpeedValue.sprite = Lance.Atlas.GetUISprite("Icon_Speed_Inactive");

                mTextDuration.text = "SPEED";
            }
        }

        public void OnStartStage(StageData stageData)
        {
            SetActive(!stageData.type.IsJousting() && ContentsLockUtil.IsLockContents(ContentsLockType.SpeedMode) == false);
        }
    }
}