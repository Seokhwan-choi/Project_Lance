using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Lance
{
    class Lobby_JoustingStageInfoUI : MonoBehaviour
    {
        const float StartDistance = 50f;

        Slider mSliderPlayerPos;
        TextMeshProUGUI mTextMyNickname;
        TextMeshProUGUI mTextMyPowerLevel;

        Slider mSliderOpponentPos;
        TextMeshProUGUI mTextOpponentNickname;
        TextMeshProUGUI mTextOpponentPowerLevel;
        StageManager StageManager => Lance.GameManager.StageManager;

        public void Init()
        {
            var myBattleInfoObj = gameObject.FindGameObject("MyBattleInfo");
            mTextMyNickname = myBattleInfoObj.FindComponent<TextMeshProUGUI>("Text_MyNickname");
            mTextMyPowerLevel = myBattleInfoObj.FindComponent<TextMeshProUGUI>("Text_MyPowerLevel");

            mSliderPlayerPos = gameObject.FindComponent<Slider>("Slider_PlayerPos");

            var opponentBattlInfo = gameObject.FindGameObject("OpponentBattleInfo");
            mTextOpponentNickname = opponentBattlInfo.FindComponent<TextMeshProUGUI>("Text_OpponentNickname");
            mTextOpponentPowerLevel = opponentBattlInfo.FindComponent<TextMeshProUGUI>("Text_OpponentPowerLevel");

            mSliderOpponentPos = gameObject.FindComponent<Slider>("Slider_OpponentPos");
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        public void RefreshInfos(JoustBattleInfo myBattleInfo, JoustBattleInfo opponentBattleInfo)
        {
            if (myBattleInfo != null)
            {
                mTextMyNickname.text = myBattleInfo.GetNickName();
                mTextMyPowerLevel.text = myBattleInfo.GetPowerLevel().ToAlphaString();

                //var myBodyCostume = myBattleInfo.GetCostume(CostumeType.Body);
                //var myCostumeData = Lance.GameData.BodyCostumeData.TryGet(myBodyCostume);
                //if (myCostumeData != null)
                //{
                //    mImagePlayerPortrait.sprite = Lance.Atlas.GetPlayerSprite(myCostumeData.uiSprite);
                //}
            }
            
            if (opponentBattleInfo != null)
            {
                mTextOpponentNickname.text = opponentBattleInfo.GetNickName();
                mTextOpponentPowerLevel.text = opponentBattleInfo.GetPowerLevel().ToAlphaString();

                //var opponentBodyCostume = opponentBattleInfo.GetCostume(CostumeType.Body);
                //var opponentCostumeData = Lance.GameData.BodyCostumeData.TryGet(opponentBodyCostume);
                //if (opponentCostumeData != null)
                //{
                //    mImageOpponentPortrait.sprite = Lance.Atlas.GetPlayerSprite(opponentCostumeData.uiSprite);
                //}
            }
        }
        
        public void UpdateDistance()
        {
            Vector2 playerPos = StageManager.Player.GetPosition();
            Vector2 opponentPos = StageManager.JoustOpponent?.GetPosition() ?? Vector2.zero;

            float distance = Vector2.Distance(playerPos, opponentPos);

            distance = Mathf.Max(0f, distance);

            mSliderPlayerPos.value = distance / StartDistance;
            mSliderOpponentPos.value = distance / StartDistance;
        }
    }
}