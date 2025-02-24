using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Lance
{
    class Lobby_NormalStageInfoUI : MonoBehaviour
    {
        GameObject mMonsterBattleObj;
        TextMeshProUGUI mTextMonsterStage;
        Slider mSliderMonsterKillCount;
        TextMeshProUGUI mTextMonsterKillCount;
        Button mButtonBossChallenge;
        Button mButtonBossAutoChallenge;
        Image mImageBossChallenge;
        Image mImageBossIcon;
        TextMeshProUGUI mTextBossChallenge;
        
        Image mImageBossAutoChallengeCheck;

        GameObject mBossBattleObj;
        Slider mSliderBossHp;
        TextMeshProUGUI mTextBossHp;
        Slider mSliderBossTime;
        StageManager StageManager => Lance.GameManager.StageManager;
        public void Init()
        {
            mMonsterBattleObj = gameObject.FindGameObject("MonsterBattle");
            mTextMonsterStage = mMonsterBattleObj.FindComponent<TextMeshProUGUI>("Text_Stage");
            mSliderMonsterKillCount = mMonsterBattleObj.FindComponent<Slider>("Slider_MonsterKillCount");
            mTextMonsterKillCount = mMonsterBattleObj.FindComponent<TextMeshProUGUI>("Text_MonsterKillCount");

            mButtonBossChallenge = mMonsterBattleObj.FindComponent<Button>("Button_BossChallenge");
            mButtonBossChallenge.SetButtonAction(OnButtonBossChallenge);
            mImageBossIcon = mMonsterBattleObj.FindComponent<Image>("Image_BossIcon");
            mImageBossChallenge = mButtonBossChallenge.GetComponent<Image>();
            mTextBossChallenge = mMonsterBattleObj.FindComponent<TextMeshProUGUI>("Text_Boss");

            var bossAutoChallengeObj = mMonsterBattleObj.FindGameObject("BossAutoChallenge");
            mImageBossAutoChallengeCheck = bossAutoChallengeObj.FindComponent<Image>("Image_Check");
            mImageBossAutoChallengeCheck.gameObject.SetActive(SaveBitFlags.BossBreakThrough.IsOn());
            mButtonBossAutoChallenge = bossAutoChallengeObj.FindComponent<Button>("Button_BossAutoChallenge");
            mButtonBossAutoChallenge.SetButtonAction(OnButtonBossAutoChallenge);

            mBossBattleObj = gameObject.FindGameObject("BossBattle");
            mSliderBossHp = mBossBattleObj.FindComponent<Slider>("Slider_BossHp");
            mTextBossHp = mBossBattleObj.FindComponent<TextMeshProUGUI>("Text_BossHp");
            mSliderBossTime = mBossBattleObj.FindComponent<Slider>("Slider_BossTime");

            var buttonGiveup = mBossBattleObj.FindComponent<Button>("Button_Giveup");
            buttonGiveup.SetButtonAction(OnButtonGiveup);
        }

        public void RefreshContentsLockUI()
        {
            mButtonBossAutoChallenge.gameObject.SetActive(ContentsLockUtil.IsLockContents(ContentsLockType.BossAutoChallege) == false);
            mButtonBossChallenge.gameObject.SetActive(ContentsLockUtil.IsLockContents(ContentsLockType.BossChallenge) == false);
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        public void Localize()
        {
            mTextMonsterStage.text = StageRecordsUtil.GetCurStageInfoToString();
        }

        public void Refresh()
        {
            bool isBossStage = StageManager.IsBossStage;

            mMonsterBattleObj.SetActive(!isBossStage);
            mBossBattleObj.SetActive(isBossStage);

            if (isBossStage)
            {
                UpdateBossHp();
                UpdateBossTime();
            }
            else
            {
                UpdateMonsterKillCount();

                mTextMonsterStage.text = StageRecordsUtil.GetCurStageInfoToString();
            }

            mImageBossAutoChallengeCheck.gameObject.SetActive(SaveBitFlags.BossBreakThrough.IsOn());
        }

        public void UpdateMonsterKillCount()
        {
            int killMonster = StageManager.MonsterKillCount;
            int challengeCount = Lance.GameData.StageCommonData.bossChallengeKillCount;

            challengeCount = Math.Max(1, challengeCount);
            killMonster = Math.Min(challengeCount, killMonster);

            mSliderMonsterKillCount.value = (float)killMonster / (float)challengeCount;
            mTextMonsterKillCount.text = $"{killMonster} / {challengeCount}";

            bool canTryBossChallenge = StageManager.CanTryBossChallenge();

            mImageBossIcon.sprite = Lance.Atlas.GetUISprite(canTryBossChallenge ? "Icon_Boss2" : "Icon_Boss2_Inactive");
            mTextBossChallenge.SetColor(canTryBossChallenge ? "FFFFFF" : "525252");
            mImageBossChallenge.sprite = Lance.Atlas.GetUISprite(canTryBossChallenge ? "Button_Boss_Summons_Active" : "Button_Boss_Summons_InActive");
        }

        public void UpdateBossHp()
        {
            double bossCurHp = StageManager.Boss?.Stat.CurHp ?? 0;
            double bossMaxHp = StageManager.Boss?.Stat.MaxHp ?? 1;

            mSliderBossHp.value = (float)(bossCurHp / bossMaxHp);
            mTextBossHp.text = $"{bossCurHp.ToAlphaString()} / {bossMaxHp.ToAlphaString()}";
        }

        public void UpdateBossTime()
        {
            float currBossTime = StageManager.Timeout;
            float bossTimeout = StageManager.StageData?.stageTimeout ?? 1;

            mSliderBossTime.value = currBossTime / bossTimeout;
        }

        void OnButtonBossChallenge()
        {
            if (ContentsLockUtil.IsLockContents(ContentsLockType.BossChallenge))
                return;

            // 보스 도전
            StageManager.OnBossChallengeButton();
        }

        void OnButtonBossAutoChallenge()
        {
            if (ContentsLockUtil.IsLockContents(ContentsLockType.BossAutoChallege))
                return;

            SaveBitFlags.BossBreakThrough.Toggle();

            Lance.GameManager.CheckQuest(QuestType.ActiveAutoChallenge, 1);

            mImageBossAutoChallengeCheck.gameObject.SetActive(SaveBitFlags.BossBreakThrough.IsOn());

            Lance.GameManager.OnBossAutoChallenge();
        }

        void OnButtonGiveup()
        {
            StageManager.OnGiveupButton();
        }
    }
}