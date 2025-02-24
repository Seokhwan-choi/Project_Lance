using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BackEnd;
using System;

namespace Lance
{
    class Lobby_UserInfoUI : MonoBehaviour
    {
        TextMeshProUGUI mText_Level;
        Slider mSliderExp;
        TextMeshProUGUI mText_Nickname;
        TextMeshProUGUI mText_PowerLevel;

        Image mImagePortrait;
        Image mImagePortraitFrame;
        GameObject mRedDot;
        public void Init()
        {
            mText_Level = gameObject.FindComponent<TextMeshProUGUI>("Text_LevelValue");
            mSliderExp = gameObject.FindComponent<Slider>("Slider_Exp");
            mText_Nickname = gameObject.FindComponent<TextMeshProUGUI>("Text_Nickname");
            mText_PowerLevel = gameObject.FindComponent<TextMeshProUGUI>("Text_PowerLevel");
            mImagePortrait = gameObject.FindComponent<Image>("Image_Portrait");
            mImagePortraitFrame = gameObject.FindComponent<Image>("Image_Portrait_Frame");
            mRedDot = gameObject.FindGameObject("RedDot");

            var buttonUserInfo = GetComponent<Button>();
            buttonUserInfo.SetButtonAction(OnUserInfoButton);

            UpdateUserInfos();
        }

        void OnUserInfoButton()
        {
            var popup = Lance.PopupManager.CreatePopup<Popup_UserInfoUI>();

            popup.Init();
        }

        public void UpdateUserInfos()
        {
            UpdateExp();
            UpdateLevel();
            UpdateNickname();
            UpdatePowerLevel();
            UpdatePortrait();
            UpdateAchievement();
        }

        public void UpdateExp()
        {
            double curExp = Lance.Account.ExpLevel.GetExp();
            double requipreExp = Math.Max(1, Lance.Account.ExpLevel.GetCurrentRequireExp());

            mSliderExp.value = (float)(curExp / requipreExp);
        }

        public void UpdateLevel()
        {
            mText_Level.text = $"{Lance.Account.ExpLevel.GetLevel()}";
        }

        public void UpdateNickname()
        {
            mText_Nickname.text = $"{Backend.UserNickName}";
        }

        public void UpdatePowerLevel()
        {
            mText_PowerLevel.text = Lance.Account.UserInfo.GetPowerLevel().ToAlphaString();
        }

        public void UpdatePortrait()
        {
            var costume = Lance.Account.Costume.GetEquippedCostumeId(CostumeType.Body);
            var costumeData = Lance.GameData.BodyCostumeData.TryGet(costume);
            if (costumeData != null)
            {
                mImagePortrait.sprite = Lance.Atlas.GetPlayerSprite(costumeData.uiSprite);
            }
        }

        public void UpdateAchievement()
        {
            var achievement = Lance.Account.Achievement.GetEquippedAchievement();
            var achievementData = Lance.GameData.AchievementData.TryGet(achievement);
            if (achievementData != null)
            {
                mImagePortraitFrame.sprite = Lance.Atlas.GetUISprite(achievementData.uiSprite);
            }
            else
            {
                mImagePortraitFrame.sprite = Lance.Atlas.GetUISprite(Lance.GameData.AchievementCommonData.defaultFrame);
            }

            RefreshFrameRedDot();
        }

        public void RefreshFrameRedDot()
        {
            bool anyCanReceive = Lance.Account.Achievement.AnyCanReceive();
            if (mRedDot.activeSelf != anyCanReceive)
                mRedDot.SetActive(anyCanReceive);
        }


        public void OnStartStage(StageData stageData)
        {
            gameObject.SetActive(!stageData.type.IsJousting());
        }
    }
}