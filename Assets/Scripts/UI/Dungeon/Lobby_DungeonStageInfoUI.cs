using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Lance
{
    class Lobby_DungeonStageInfoUI : MonoBehaviour
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
            mImageContentsIcon.sprite = Lance.Atlas.GetUISprite($"Image_Dungeon_Gaugebar_{StageData.type}");
            mImageGradation.sprite = GetGradation();

            Sprite GetGradation()
            {
                if (StageData.type == StageType.Gold)
                    return Lance.Atlas.GetUISprite("Gradation_Yellow");
                else if (StageData.type == StageType.Stone)
                    return Lance.Atlas.GetUISprite("Gradation_Magenta");
                else
                    return Lance.Atlas.GetUISprite("Gradation_Red");
            }
        }
        
        public void UpdateScore()
        {
            if (StageManager.IsDungeon)
            {
                float sliderValue = 1f;
                string scoreStr = string.Empty;

                mBestScoreObj.SetActive(!StageData.type.HaveNextStepDungeon());

                if (StageData.type == StageType.Raid)
                {
                    double curScore = StageManager.StackedDamage;
                    double bestScore = StageManager.GetRaidBossBestScore();

                    scoreStr = curScore.ToAlphaString();

                    mTextBestScore.text = curScore > bestScore ? curScore.ToAlphaString() : bestScore.ToAlphaString();
                }
                else if (StageData.type == StageType.Ancient)
                {
                    double bossCurHp = StageManager.Boss?.Stat.CurHp ?? 0;
                    double bossMaxHp = StageManager.Boss?.Stat.MaxHp ?? 1;

                    sliderValue = (float)(bossCurHp / bossMaxHp);
                    scoreStr = $"{bossCurHp.ToAlphaString()} / {bossMaxHp.ToAlphaString()}";

                    int bestStep = Lance.Account.Dungeon.GetBestStep(StageData.type);

                    StringParam param = new StringParam("step", bestStep);

                    mTextBestScore.text = StringTableUtil.Get("UIString_Step", param);
                }
                else if (StageData.type == StageType.Pet)
                {
                    double bossCurHp = StageManager.Boss?.Stat.CurHp ?? 0;
                    double bossMaxHp = StageManager.Boss?.Stat.MaxHp ?? 1;

                    sliderValue = (float)(bossCurHp / bossMaxHp);
                    scoreStr = $"{bossCurHp.ToAlphaString()} / {bossMaxHp.ToAlphaString()}";

                    int bestStep = Lance.Account.Dungeon.GetBestStep(StageData.type);

                    StringParam param = new StringParam("step", bestStep);

                    mTextBestScore.text = StringTableUtil.Get("UIString_Step", param);
                }
                else
                {
                    int killMonster = StageManager.MonsterKillCount;
                    int limitCount = StageData.monsterLimitCount;

                    killMonster = Math.Min(limitCount, killMonster);

                    sliderValue = (float)killMonster / (float)limitCount;
                    scoreStr = $"{killMonster} / {limitCount}";

                    if (StageData.type == StageType.Pet)
                    {
                        int bestStep = Lance.Account.Dungeon.GetBestStep(StageData.type);

                        StringParam param = new StringParam("step", bestStep);

                        mTextBestScore.text = StringTableUtil.Get("UIString_Step", param);
                    }
                }

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