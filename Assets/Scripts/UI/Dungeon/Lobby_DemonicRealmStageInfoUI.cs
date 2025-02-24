using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Lance
{
    class Lobby_DemonicRealmStageInfoUI : MonoBehaviour
    {
        Image mImageGradation;

        Slider mSliderScore;
        TextMeshProUGUI mTextScore;
        Slider mSliderTimer;

        GameObject mBestScoreObj;
        TextMeshProUGUI mTextBestScore;
        Image mImageContentsIcon;
        StageManager StageManager => Lance.GameManager.StageManager;
        StageData StageData => StageManager.StageData;
        public void Init()
        {
            mImageGradation = gameObject.FindComponent<Image>("Image_Gradation");
            mImageContentsIcon = gameObject.FindComponent<Image>("Image_ContentsIcon");

            mSliderScore = gameObject.FindComponent<Slider>("Slider_Score");
            mTextScore = gameObject.FindComponent<TextMeshProUGUI>("Text_Score");
            mSliderTimer = gameObject.FindComponent<Slider>("Slider_Timer");
            mBestScoreObj = gameObject.FindGameObject("BestScore");
            mTextBestScore = mBestScoreObj.FindComponent<TextMeshProUGUI>("Text_BestScore");
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        public void Refresh()
        {
            RefreshContentsImage();

            UpdateScore();
            UpdateTimer();
        }

        public void RefreshContentsImage()
        {
            mImageContentsIcon.sprite = Lance.Atlas.GetUISprite($"Image_DemonicRealm_Gaugebar_{StageData.type}");
            mImageGradation.sprite = Lance.Atlas.GetUISprite("Gradation_Red");
        }
        
        public void UpdateScore()
        {
            if (StageManager.IsDemonicRealm)
            {
                float sliderValue = 1f;
                string scoreStr = string.Empty;

                mBestScoreObj.SetActive(!StageData.type.HaveNextStepDemonicRealm());

                int killMonster = StageManager.MonsterKillCount;
                int limitCount = StageData.monsterLimitCount;

                killMonster = Math.Min(limitCount, killMonster);

                sliderValue = (float)killMonster / (float)limitCount;
                scoreStr = $"{killMonster} / {limitCount}";

                //if (StageData.type == StageType.Pet)
                //{
                //    int bestStep = Lance.Account.DemonicRealm.GetBestStep(StageData.type);

                //    StringParam param = new StringParam("step", bestStep);

                //    mTextBestScore.text = StringTableUtil.Get("UIString_Step", param);
                //}

                mSliderScore.value = sliderValue;
                mTextScore.text = scoreStr;
            }
        }

        public void UpdateTimer()
        {
            float curTimeout = StageManager.Timeout;
            float dataTimeout = Math.Max(1, StageData.stageTimeout);

            mSliderTimer.value = curTimeout / dataTimeout;
        }
    }
}