using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

namespace Lance
{
    class Lobby_SettingMenuUI : MonoBehaviour
    {
        bool mInMotion;
        bool mIsActiveSettingMenu;
        GameObject mSettingMenuOpenObj;
        GameObject mSettingMenuRedDot;
        GameObject mAttendanceObj;
        GameObject mAttendanceRedDot;
        GameObject mPostRedDot;
        public void Init()
        {
            GameObject burgerMenuObj = gameObject.FindGameObject("SettingMenu");
            mSettingMenuOpenObj = burgerMenuObj.FindGameObject("SettingMenu_Open");
            mSettingMenuRedDot = burgerMenuObj.FindGameObject("RedDot");

            var buttonSettingMenu = burgerMenuObj.FindComponent<Button>("Button_SettingMenu");
            buttonSettingMenu.SetButtonAction(OnButtonSettingMenu);

            var buttonPost = mSettingMenuOpenObj.FindComponent<Button>("Post");
            buttonPost.SetButtonAction(() =>
            {
                var popup = Lance.PopupManager.CreatePopup<Popup_PostUI>();
                popup.Init();

                OnButtonSettingMenu();
            });

            mPostRedDot = buttonPost.gameObject.FindGameObject("RedDot");

            var buttonSetting = mSettingMenuOpenObj.FindComponent<Button>("Setting");
            buttonSetting.SetButtonAction(() =>
            {
                var popup = Lance.PopupManager.CreatePopup<Popup_SettingUI>();
                popup.Init();

                OnButtonSettingMenu();
            });

            mAttendanceObj = mSettingMenuOpenObj.FindGameObject("Attendance");

            var buttonAttendance = mAttendanceObj.GetComponent<Button>();
            buttonAttendance.SetButtonAction(() =>
            {
                var popup = Lance.PopupManager.CreatePopup<Popup_AttendanceUI>();
                popup.Init();
                popup.SetOnCloseAction(() => Lance.GameManager.CheckGuideQuestReceiveReward(QuestType.ConfirmAttendance));

                OnButtonSettingMenu();
            });

            mAttendanceRedDot = buttonAttendance.gameObject.FindGameObject("RedDot");

            var buttonSleepMode = mSettingMenuOpenObj.FindComponent<Button>("SleepMode");
            buttonSleepMode.SetButtonAction(() =>
            {
                Lance.GameManager.SetSleepMode(true);

                OnButtonSettingMenu();
            });

            var buttonNotice = mSettingMenuOpenObj.FindComponent<Button>("Notice");
            buttonNotice.SetButtonAction(() =>
            {
                var noticeList = Lance.Account.Notice.GetNoticeList();
                if (noticeList != null)
                {
                    var popup = Lance.PopupManager.CreatePopup<Popup_NoticeListUI>();

                    popup.Init(noticeList);
                }

                OnButtonSettingMenu();
            });

            RefreshSettingMenu();
        }

        public void RefreshRedDots()
        {
            bool anyPostCanReceiveReward = Lance.Account.Post.AnyCanReceiveReward();
            bool anyAttendanceCanReceiveReward = Lance.Account.Attendance.AnyCanReceiveReward() && ContentsLockUtil.IsLockContents(ContentsLockType.Attendance) == false;

            mSettingMenuRedDot.SetActive(anyPostCanReceiveReward || anyAttendanceCanReceiveReward);
            mPostRedDot.SetActive(anyPostCanReceiveReward);
            mAttendanceRedDot.SetActive(anyAttendanceCanReceiveReward);
        }

        void OnButtonSettingMenu()
        {
            if (mInMotion)
                return;

            mIsActiveSettingMenu = !mIsActiveSettingMenu;

            RefreshSettingMenu();
        }

        void RefreshSettingMenu()
        {
            if (mIsActiveSettingMenu)
            {
                mSettingMenuOpenObj.SetActive(true);

                mInMotion = true;

                UIUtil.SpeechBubbleShow(mSettingMenuOpenObj.transform, () => mInMotion = false);
            }
            else
            {
                mInMotion = true;

                UIUtil.SpeechBubbleHide(mSettingMenuOpenObj.transform, () =>
                {
                    mSettingMenuOpenObj.SetActive(false);

                    mInMotion = false;
                });
            }
        }

        public void RefreshContentsLockUI()
        {
            mAttendanceObj.SetActive(ContentsLockUtil.IsLockContents(ContentsLockType.Attendance) == false);
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        public void OnStartStage(StageData stageData)
        {
            SetActive(!stageData.type.IsJousting());
        }
    }
}