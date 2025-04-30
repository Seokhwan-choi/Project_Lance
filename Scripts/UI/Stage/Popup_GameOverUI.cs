using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Lance
{
    class Popup_GameOverUI : PopupBase
    {
        const float TimeUpdateInterval = 1f;
        float mTimeUpdateInterval;
        int mCloseTime;

        TextMeshProUGUI mTextCloseTime;
        public void Init()
        {
            mCloseTime = 20;       // 약 20초 뒤에 자동 종료
            mTimeUpdateInterval = 1f;

            var buttonClose = gameObject.FindComponent<Button>("Button_Close");
            buttonClose.SetButtonAction(() => Close());

            mTextCloseTime = gameObject.FindComponent<TextMeshProUGUI>("Text_PopupClose");

            var buttonMoveToSpawn = gameObject.FindComponent<Button>("Button_MoveToSpawn");
            buttonMoveToSpawn.SetButtonAction(() =>
            {
                Close();

                Lance.Lobby.ChangeTab(LobbyTab.Spawn);
            });

            var buttonMoveToTrain = gameObject.FindComponent<Button>("Button_MoveToTrain");
            buttonMoveToTrain.SetButtonAction(() =>
            {
                Close();

                Lance.Lobby.ChangeTab(LobbyTab.Stature);

                var tab = Lance.Lobby.GetLobbyTabUI<Lobby_StatureUI>();

                tab.ChangeTab(StatureTab.GoldTrain);
            });

            var buttonMoveToSkill = gameObject.FindComponent<Button>("Button_MoveToSkill");
            buttonMoveToSkill.SetButtonAction(() =>
            {
                Close();

                Lance.Lobby.ChangeTab(LobbyTab.Skill);
            });

            var buttonMoveToInventory = gameObject.FindComponent<Button>("Button_MoveToInventory");
            buttonMoveToInventory.SetButtonAction(() =>
            {
                Close();

                Lance.Lobby.ChangeTab(LobbyTab.Inventory);
            });

            StringParam param = new StringParam("sec", (int)mCloseTime);
            mTextCloseTime.text = StringTableUtil.Get("UIString_ClosePopup", param);
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            mTimeUpdateInterval -= dt;
            if (mTimeUpdateInterval <= 0f)
            {
                mTimeUpdateInterval = TimeUpdateInterval;
                mCloseTime -= (int)TimeUpdateInterval;

                StringParam param = new StringParam("sec", (int)mCloseTime);
                mTextCloseTime.text = StringTableUtil.Get("UIString_ClosePopup", param);

                if (mCloseTime <= 0 && mClosing == false)
                {
                    Close();
                }
            }
        }
    }
}